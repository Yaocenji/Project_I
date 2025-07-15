using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class CameraController : MonoBehaviour
{
    public Transform player;
    public EjectorManager playerEjectorManager;

    private Camera cam;

    private bool isAiming;

    private Vector2 normalPosition;
    private Vector2 aimingPosition;

    private Vector2 targetPosition;
    private float targetCameraSize;

    private void Awake()
    {
        normalPosition = Vector2.zero;
        cam = GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        if (isAiming)
        {
            aimingPosition = playerEjectorManager.AimingPos;
            targetPosition = aimingPosition;
            targetCameraSize = playerEjectorManager.AimingPos.z;
        }
        else
        {
            normalPosition = new Vector2(player.position.x, player.position.y);
            targetPosition = normalPosition;
            targetCameraSize = 20;
        }
        transform.position = new Vector3((4.0f * transform.position.x + targetPosition.x) / 5,
                                        (4.0f * transform.position.y + targetPosition.y) / 5, -10);
        cam.orthographicSize = (9.0f * cam.orthographicSize + targetCameraSize) / 10.0f;
    }

    public void BeginAiming()
    {
        isAiming = true;
    }public void EndAiming()
    {
        isAiming = false;
    }

    
}
    
}
