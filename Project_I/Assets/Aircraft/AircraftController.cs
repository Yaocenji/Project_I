using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Project_I
{
public class AircraftController : MonoBehaviour
{
    // 当前目标点的世界空间坐标
    private Vector2 _targetPositionWs;
    
    // 重力常数
    private const float g = 9.81f;
    // 物理帧帧周期
    private const float fixedFrameTime = 0.02f;
    
    // 飞机参数
    [LabelText("慢车出力")]
    public float engineF_0 = 1.0f;
    [LabelText("满车出力")]
    public float engineF_1 = 15.5f;
    [LabelText("加力出力")]
    public float engineF_2 = 30.2f;
    [LabelText("每秒引擎出力最大变化量")]
    public float maxChangePerSecondEngineF = 23.0f;
    
    [LabelText("飞机质量")]
    public float mass = 1.0f;
    
    [LabelText("最小回转角速度（°/s）")]
    public float minRotateRatio = 20f;
    [LabelText("最大回转角速度（°/s）")]
    public float maxRotateRatio = 75f;
    
    [LabelText("最佳回转速度起点")]
    public float minBestRotateSpeed = 5.0f;
    [LabelText("最佳回转速度终点")]
    public float maxBestRotateSpeed = 20.0f;
    [LabelText("锁舵速度")]
    public float rudderLockSpeed = 40.0f;

    [LabelText("水平安定力")]
    public float horizonalStability = 0.035f;
    [LabelText("存能系数")]
    public float energyRetention = 0.5f;
    [LabelText("到达最大翼面效应的速度")]
    public float maxWingStablilitySpeed = 7.5f;
    
    // 是否使用推进
    private bool useStandardThrust;
    // 是否使用加力
    private bool useAugmentationThrust;

    public bool UseStandardThrust
    {
        get { return useStandardThrust; }
    }

    public bool UseAugmentationThrust
    {
        get { return useAugmentationThrust; }
    }
        
    // 几个关键力
    private float targetEngineF;
    private float currEngineF;
    private float maxChangePerFrameEngineF;
    private Vector2 engineF;
    private Vector2 liftF;
    private Vector2 fricF;
    private Vector2 gravityF;
    
    // 一个物理帧中的合力
    private Vector2 sumF;

    // 运动状态
    private Vector2 velocity;
    private float speed;
    private float angularVelocity;
    
    // 获取刚体
    private Rigidbody2D _rigidbody2D;

    public Vector2 getVelocity
    {
        get
        {
            return velocity;
        }
    }

    public Vector2 getAcceleration()
    {
        return sumF / _rigidbody2D.mass / fixedFrameTime;
    }
    
    void Start()
    {
        _targetPositionWs = transform.position + transform.right;
        speed = 0;
        useStandardThrust = false;
        useAugmentationThrust = false;
        velocity = Vector2.zero;
        speed = 0;
        angularVelocity = 0;
        
        currEngineF = engineF_0;
        
        maxChangePerFrameEngineF = maxChangePerSecondEngineF * fixedFrameTime;
        
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.mass = mass;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 计算物理量
        velocity = _rigidbody2D.velocity;
        speed = velocity.magnitude;
        
        // 初始化
        {
            sumF = Vector2.zero;
        }

        // 飞行动力
        {
            // 重力
            gravityF = mass * g * Vector2.down;
            // 引擎
            if (!useStandardThrust)
                targetEngineF = engineF_0;
            else if (useStandardThrust && !useAugmentationThrust)
                targetEngineF = engineF_1;
            else
                targetEngineF = engineF_2;
            
            // 引擎变力
            if (Mathf.Abs(currEngineF - targetEngineF) <= maxChangePerFrameEngineF)
                currEngineF = targetEngineF;
            else if (targetEngineF >  currEngineF)
                currEngineF += maxChangePerFrameEngineF;
            else
                currEngineF -= maxChangePerFrameEngineF;
            
            engineF = currEngineF * transform.right;
            
            
            // 升力
            if (!useStandardThrust)
                liftF = -gravityF * 0.25f;
            else
                liftF = -gravityF;
            // 阻力
            fricF = -velocity.normalized * (speed * speed) / 15.0f;

            // 合力
            sumF = gravityF + engineF + liftF + fricF;
        }

        // 旋转
        {
            Vector2 targetDir = (_targetPositionWs - (Vector2)transform.position).normalized;
            float targetAngle = targetDir.y >= 0 ? Mathf.Acos(targetDir.x) : -Mathf.Acos(targetDir.x);
            Vector3 cross = Vector3.Cross(transform.right, targetDir);
            float dot = Vector2.Dot(transform.right, targetDir);
            // 计算要变化的弧度（逆正顺负）
            float deltaAngle = dot >= 0 ? Mathf.Asin(cross.z) : ( cross.z >= 0 ? Mathf.PI - Mathf.Asin(cross.z) : - Mathf.PI - Mathf.Asin(cross.z) );
            // 计算当前的最大角速度
            float currMaxAngularVelocity;
            if (speed >= minBestRotateSpeed && speed <= maxBestRotateSpeed)
            {
                currMaxAngularVelocity = maxRotateRatio;
            }else if (speed <= minBestRotateSpeed)
            {
                currMaxAngularVelocity = Mathf.Lerp(minRotateRatio, maxRotateRatio, speed / minBestRotateSpeed);
            }else if (speed >= maxBestRotateSpeed && speed <= rudderLockSpeed)
            {
                currMaxAngularVelocity = Mathf.Lerp(maxRotateRatio, 0,
                    (speed - maxBestRotateSpeed) / (rudderLockSpeed - maxBestRotateSpeed));
            }else
            {
                currMaxAngularVelocity = 0;
            }
            
            // 负反馈控制（这里是魔法数）
            angularVelocity = (deltaAngle / Mathf.PI * 180.0f) * 0.5f;
            
            if (Mathf.Abs(angularVelocity) <= 0.05f)
                angularVelocity = 0.05f * Mathf.Sign(deltaAngle);
            if (Mathf.Abs(angularVelocity) >= currMaxAngularVelocity * fixedFrameTime)
                angularVelocity = Mathf.Sign(angularVelocity) * currMaxAngularVelocity * fixedFrameTime;

            angularVelocity += 0.2f;
            
            // 旋转
            if (Mathf.Abs(transform.rotation.eulerAngles.z + angularVelocity) > Mathf.Epsilon)
                _rigidbody2D.MoveRotation(Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + angularVelocity));
            else
            {
                // 当旋转值等于零的时候，啥也不干，否则错误
            }
        }
        
        // 水平安定与存能
        {
            // 正交分解
            Vector2 parallelVelocity = Vector2.Dot(velocity, transform.right) / Vector2.Dot(transform.right, transform.right) 
                                       * (Vector2)transform.right;
            Vector2 verticalVelocity = velocity - parallelVelocity;
            // 计算当前水平安定系数
            float currStb = speed >= maxWingStablilitySpeed
                ? horizonalStability
                : Mathf.Lerp(0, horizonalStability, speed / maxWingStablilitySpeed);
            
            velocity = (parallelVelocity + verticalVelocity.magnitude * currStb * energyRetention * parallelVelocity.normalized) + verticalVelocity * (1 - currStb);
            
            // 加入平行方向的增力
            sumF += verticalVelocity.magnitude * currStb * energyRetention * mass / fixedFrameTime * parallelVelocity.normalized;
            // 加入垂直方向的阻力
            sumF -= currStb * mass / fixedFrameTime  * verticalVelocity;
        }

        // 物理计算启动
        {
            // 开始作用 （动量定理）
            // velocity += sumF * fixedFrameTime / mass;

            // 开始作用 （产生位移）
            // transform.position += (Vector3)(velocity * fixedFrameTime);
            
            _rigidbody2D.AddForce(sumF * Time.deltaTime / Time.fixedDeltaTime, ForceMode2D.Force);
        }
        
        //Debug.Log(speed);
    }

    public void SetTargetPosition(Vector2 tar)
    {
        _targetPositionWs = tar;
    }
    public Vector2 GetTargetPosition()
    {
        return _targetPositionWs;
    }
    
    public void StartStandardThrust()
    {
        useStandardThrust = true;
    }
    public void EndStandardThrust()
    {
        useStandardThrust = false;
    }

    public void StartAugmentationThrust()
    {
        useAugmentationThrust = true;
    }
    public void EndAugmentationThrust()
    {
        useAugmentationThrust = false;
    }
}
    
}
