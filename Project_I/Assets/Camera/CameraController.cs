using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class CameraController : MonoBehaviour
{
    [Header("玩家位姿")]
    private Transform playerTransform;
    [Header("玩家飞行控制器")]
    private AircraftController playerAircraftController;
    [Header("玩家发射器")]
    private EjectorController playerEjectorController;

    private Camera cam;

    private Vector2 normalPosition;
    private Vector2 aimingPosition;

    private Vector2 targetPosition;
    private float targetCameraSize;

    private void Awake()
    {
        // 注册为主摄像机
        GameSceneManager.Instance.RegisterMainCamera(GetComponent<Camera>());
        
    }

    private void Start()
    {
        normalPosition = Vector2.zero;
        
        cam = GetComponent<Camera>();
        
        // 要用到的场景中的其他脚本，通过GameSceneManager获取
        playerTransform = GameSceneManager.Instance.player.transform;
        playerEjectorController = GameSceneManager.Instance.player.GetComponent<EjectorController>();
        playerAircraftController = GameSceneManager.Instance.player.GetComponent<AircraftController>();
    }

    void FixedUpdate()
    {
        if (playerEjectorController.getAiming)   // 瞄准模式
        {
            aimingPosition = playerEjectorController.AimingPos;
            targetPosition = aimingPosition;
            targetCameraSize = playerEjectorController.AimingCameraSize;
        }
        else
        {
            normalPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
            normalPosition += playerAircraftController.getVelocity * 0.25f;
            targetPosition = normalPosition;
            targetCameraSize = 20;
        }
        transform.position = new Vector3((4.0f * transform.position.x + targetPosition.x) / 5,
                                        (4.0f * transform.position.y + targetPosition.y) / 5, -10);
        cam.orthographicSize = (9.0f * cam.orthographicSize + targetCameraSize) / 10.0f;
    }


}
    
}
