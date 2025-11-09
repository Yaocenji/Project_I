using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project_I
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance;
        
        [LabelText("速度的摄像机偏移系数")] [Range(0, 1)] public float speedPosParam = 0.25f;
        [LabelText("加速度的摄像机偏移系数")] [Range(0, 0.01f)] public float accelerationPosParam = 0.000f;
        [LabelText("速度的摄像机大小系数")] [Range(0, 0.5f)] public float speedScaleParam = 0.15f;
        [LabelText("最大摄像机大小")] public float maxCameraSize = 12.0f;
        
        [Header("玩家位姿")]
        private Transform playerTransform;
        [Header("玩家飞行控制器")]
        private AircraftController playerAircraftController;
        [Header("玩家发射器")]
        private EjectorController playerEjectorController;

        private Camera cam;

        private Vector2 normalPosition;
        private float normalScale;
        private Vector2 aimingPosition;

        private Vector2 targetPosition;
        private float targetCameraSize;
        private float rawCameraSize;
        
        // 存储一组：无特效的cameraPos和cameraSize
        private Vector2 NoEffectCameraPosition;
        private float NoEffectCameraSize;

        public float RawCameraSize
        {
            get { return rawCameraSize; }
        }

        private void Awake()
        {
            Instance = this;
            
            // 注册为主摄像机
            GameSceneManager.Instance.RegisterMainCamera(GetComponent<Camera>());
            
        }

        private void Start()
        {
            normalPosition = Vector2.zero;
            normalScale = 1;
            
            cam = GetComponent<Camera>();
            rawCameraSize = cam.orthographicSize;
            
            // 要用到的场景中的其他脚本，通过GameSceneManager获取
            playerTransform = GameSceneManager.Instance.Player.transform;
            playerEjectorController = GameSceneManager.Instance.Player.GetComponent<EjectorController>();
            playerAircraftController = GameSceneManager.Instance.Player.GetComponent<AircraftController>();
            
            NoEffectCameraPosition = transform.position;
            NoEffectCameraSize = cam.orthographicSize;
        }

        void FixedUpdate()
        {
            normalPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
            normalPosition += playerAircraftController.getVelocity * speedPosParam;
            normalPosition += playerAircraftController.getAcceleration().magnitude * accelerationPosParam * playerAircraftController.getVelocity.normalized;

            var tmpScale = playerAircraftController.getVelocity.magnitude * speedScaleParam;
            normalScale = 1;// - Mathf.Exp(-tmpScale);
            
            
            if (playerEjectorController.getAiming)   // 瞄准模式
            {
                aimingPosition = playerEjectorController.AimingPos;
                targetPosition = (aimingPosition + normalPosition) / 2.0f;
                targetCameraSize = (playerEjectorController.AimingCameraSize + rawCameraSize * normalScale) / 2.0f;
            }
            else    // 正常模式
            {
                targetPosition = normalPosition;
                targetCameraSize = rawCameraSize * normalScale;
            }
            
            // 弹簧趋近
            var newPosition = Utilities.GenericMath.SpringApproach<Vector2>(NoEffectCameraPosition, targetPosition, 0.1f, Time.deltaTime / Time.fixedDeltaTime);
            NoEffectCameraPosition = new Vector3(newPosition.x, newPosition.y, -10);
            var newCameraSize = Utilities.GenericMath.SpringApproach(NoEffectCameraSize, targetCameraSize, 0.1f, Time.deltaTime / Time.fixedDeltaTime);
            NoEffectCameraSize = newCameraSize;
            
            /*transform.position = new Vector3(NoEffectCameraPosition.x, NoEffectCameraPosition.y, -10);
            cam.orthographicSize = NoEffectCameraSize;*/
            
            
            
            // 以下加入镜头特效
            // 镜头抖动：硬弹簧
            float k = 10.0f;
            float posDist = Vector2.Distance(NoEffectCameraPosition, transform.position);
            Vector2 posDir = (NoEffectCameraPosition - (Vector2)transform.position).normalized;
            float sizeDist = Mathf.Abs(cam.orthographicSize - NoEffectCameraSize);
            // 胡克定律
            Vector2 posForce = k * posDist * posDir;
            Vector2 newPos = (Vector2)transform.position + posForce * Time.deltaTime;
            
            
            
            transform.position = new Vector3(newPos.x, newPos.y, -10);
            cam.orthographicSize = NoEffectCameraSize;
        }

        private void Update()
        {
        }

        public void AddSingleShake(Vector2 offset)
        {
            transform.position += (Vector3)offset;
        }
    }
        
}
