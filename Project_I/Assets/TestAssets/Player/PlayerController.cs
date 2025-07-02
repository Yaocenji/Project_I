using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Test
{
    public class PlayerController : MonoBehaviour
    {
        public Camera mainCamera;

        public Transform rearControl;

        public SpriteRenderer fireTemp0;
        public SpriteRenderer fireTemp1;

        [Header("初速度")]
        public float iniVel = 15;

        [Header("慢车推重比")]
        public float SlowThrustByWeight = 0.1f;
        [Header("军推推重比")]
        public float BoostThrustByWeight = 0.56f;
        [Header("加力推重比")]
        public float ThrustAugThrustByWeight = 1.2f;
        [Header("最回转速率系数小（速度为0时）")]
        public float MinRotationRatio = 0.15f;
        [Header("最大回转速率系数")]
        public float MaxRotationRatio = 1.0f;
        [Header("达到最大回转速率的速度")]
        public float MaxRotSpeed = 15f;
        [Header("风阻系数")]
        public float WindDragFric = 1.0f;
        [Header("起飞速度门限速度")]
        public float TakeOffThreshold = 5.0f;
        
        // 输入系统
        private TestInput _testInput;
        // 鼠标世界空间
        private Vector2 _mousePositionWs;
        // 刚体
        private Rigidbody2D _rigidbody2D;
        
        // 机型重力
        private float _gravity;
        
        // 关键数量
        private float sinAOA;// 攻角余弦
        private float speed;

        // 是否使用推进
        private bool useBoost;
        // 是否使用加力
        private bool useThrustAug;
        
        // 几个关键力
        private Vector2 engineF;
        private Vector2 liftF;
        private Vector2 fricF;
        private Vector2 rotatF;

        private Vector2 gravityF;

        private float tailWing; // 尾翼位置，[-1,1]
        
        void Awake()
        {
            _testInput = new TestInput();
            _testInput.Enable();
            // 鼠标移动事件
            _testInput.Player.MousePos.performed += OnMouseMoving;
            // 军推按住事件
            _testInput.Player.Boost.started += OnBoostStarting;
            _testInput.Player.Boost.canceled += OnBoostCancelling;
            // 加力按住事件
            _testInput.Player.ThrustAug.started += OnThrustAugStarting;
            _testInput.Player.ThrustAug.canceled += OnThrustAugCancelling;
        }

        private void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _rigidbody2D.velocity = transform.right * iniVel;

            _gravity = _rigidbody2D.mass * 9.81f;

            sinAOA = 0;
            speed = 0;

            useBoost = false;
            useThrustAug = false;
            
            engineF = Vector2.zero;
            liftF = Vector2.zero;
            fricF = Vector2.zero;
            rotatF = Vector2.zero;
            
            gravityF = Vector2.down * _gravity;

            tailWing = 0;
        }

        // Update is called once per frame
        void Update()
        {
            _mousePositionWs = mainCamera.ScreenToWorldPoint(_testInput.Player.MousePos.ReadValue<Vector2>());
            Calculate();
        }

        private void FixedUpdate()
        {
            engineF = Engine();
            liftF = Lift();
            fricF = Fric();
            rotatF = Rotat();
            
            // 质心力（平动力）
            Vector2 centricResultForce = Vector2.zero;
            centricResultForce += liftF;
            centricResultForce += fricF;
            
            // 尾部力
            Vector2 rearResultForce = Vector2.zero;
            rearResultForce += engineF;
            rearResultForce += rotatF; 
            
            // 质心力作用
            _rigidbody2D.AddForce(centricResultForce, ForceMode2D.Force);
            // 尾部力作用
            _rigidbody2D.AddForceAtPosition(rearResultForce, rearControl.position, ForceMode2D.Force);
        }

        // 引擎
        private Vector2 Engine()
        {
            float coef;
            if (!useBoost) coef = SlowThrustByWeight;
            else if (useBoost && !useThrustAug) coef = BoostThrustByWeight;
            else/* if (useBoost && useThrustAug)*/ coef = ThrustAugThrustByWeight;
            return transform.right * _rigidbody2D.mass * coef * 10.0f;
        }
        
        // 升力体
        private Vector2 Lift()
        {
            float liftCoef = LiftCoef(TakeOffThreshold);

            float angleCoef = 1;
            
            // 机头倾斜角影响 竖直飞时减小升力
            float cosElevAngle = Mathf.Abs(Vector2.Dot(transform.right, Vector2.right));
            cosElevAngle *= 1.25f;
            angleCoef *= Mathf.Lerp(0.35f, 1.0f, cosElevAngle);
            
            // 攻角影响 攻角越接近90度，升力越小
            float cosAOA = Mathf.Sqrt(1 - sinAOA * sinAOA);
            angleCoef *= Mathf.Abs(cosAOA);

            return _gravity * Vector2.up * liftCoef * angleCoef;
        }
        
        // 空气阻力
        private Vector2 Fric()
        {
            float fric = 1;
            
            // 速度系数
            fric *= (speed * speed / 255.0f * 5 + .03f);
            
            // 攻角系数[0,1]
            float aoaCeof = Mathf.Abs(sinAOA);
            // 攻角速度系数（速度越小，攻角变化对阻力的影响越不明显）[1,2.5]
            aoaCeof *= 1.5f * (1 - Mathf.Exp(-speed / 15.0f));
            aoaCeof += 1;
            // 乘上fric
            fric *= aoaCeof;
            
            return - fric * _rigidbody2D.velocity.normalized * WindDragFric;
        }
        
        // 转向
        private Vector2 Rotat()
        {
            Vector2 mouseDir = (_mousePositionWs - (Vector2)transform.position).normalized;
            
            Vector3 cross = Vector3.Cross(transform.right, mouseDir);
            float dot = Vector2.Dot(transform.right, mouseDir);
            
            // 计算要变化的弧度（逆正顺负）
            float deltaAngle = dot >= 0 ? Mathf.Asin(cross.z) : ( cross.z >= 0 ? Mathf.PI - Mathf.Asin(cross.z) : - Mathf.PI - Mathf.Asin(cross.z) );

            // 计算目标的tailWing
            float targetTailWing = -(deltaAngle - 0.02f * _rigidbody2D.angularVelocity / Mathf.PI);

            if (targetTailWing > tailWing)
                tailWing += targetTailWing - tailWing >= .3f ? .3f : targetTailWing - tailWing;
            else if (targetTailWing < tailWing)
                tailWing -= tailWing - targetTailWing>= .3f ? .3f : tailWing - targetTailWing;

            //tailWing = targetTailWing;

            // 力臂长度
            tailWing *= 1.0f;
            
            // 乘以回转系数
            tailWing *= RotRatio();
            
            /*if (tailWing >= 10.0f) tailWing = 10.0f;
            if (tailWing <= -10.0f) tailWing = -10.0f;*/

            //return Vector2.zero;
            return rearControl.up * tailWing;
        }
        
        // 计算升力系数
        private float LiftCoef(float threshold)
        {
            if (speed >= threshold) return 1.0f;

            return 1 - Mathf.Pow(1 - speed / threshold, 3.0f);
        }

        // 计算回转速度系数
        private float RotRatio()
        {
            return Mathf.Lerp(MinRotationRatio, MaxRotationRatio, speed / MaxRotSpeed);
        }


        // 计算关键数值
        private void Calculate()
        {
            Vector2 mouseDir = (_mousePositionWs - (Vector2)transform.position).normalized;
            sinAOA = Vector3.Cross(transform.right, mouseDir).z;
            speed = _rigidbody2D.velocity.magnitude;
            
            Debug.Log(speed);
        }

        private void OnDrawGizmos()
        {
            Vector2 centerPos = transform.position;
            Vector2 rearPos = rearControl.position;
            
            Vector2 gravityPos = centerPos + 3 * gravityF;
            Vector2 enginePos  = rearPos   + 3 * engineF;
            Vector2 fricPos    = centerPos + 3 * fricF;
            Vector2 liftPos    = centerPos + 3 * liftF;
            Vector2 rotatPos   = rearPos   + 3 * rotatF;
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(centerPos, gravityPos);
            Gizmos.DrawLine(centerPos, fricPos);
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rearPos, enginePos);
            Gizmos.DrawLine(centerPos, liftPos);
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(rearPos, rotatPos);
        }

        // 鼠标移动
        private void OnMouseMoving(InputAction.CallbackContext context)
        {
            //_mousePositionWs = mainCamera.ScreenToWorldPoint(context.ReadValue<Vector2>());
        }
        
        // 推进
        private void OnBoostStarting(InputAction.CallbackContext context)
        {
            useBoost = true;
            fireTemp0.enabled = true;
        }
        // 加力结束
        private void OnBoostCancelling(InputAction.CallbackContext context)
        {
            useBoost = false;
            fireTemp0.enabled = false;
        }
        // 加力
        private void OnThrustAugStarting(InputAction.CallbackContext context)
        {
            useThrustAug = true;
            fireTemp1.enabled = true;
        }
        // 加力结束
        private void OnThrustAugCancelling(InputAction.CallbackContext context)
        {
            useThrustAug = false;
            fireTemp1.enabled = false;
        }
    }
}
