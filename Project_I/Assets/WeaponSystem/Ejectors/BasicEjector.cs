using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project_I
{
public class BasicEjector : MonoBehaviour
{
    [Header("瞄准时的视野长度")]
    public float aimingCameraDistance = 25.0f;
    
    [Header("瞄准时的摄像机尺寸")]
    public float aimingCameraSize = 25.0f;
    
    [Header("瞄准时的鼠标灵敏度倍率")]
    public float aimingMouseSensitivity = 0.5f;
    
    [Header("瞄准时的时间流速倍率")]
    public float aimingTimeScale = 0.5f;
    
    public virtual void BeginEject()
    {
        return;
    }
    public virtual void Ejecting()
    {
        return;
    }
    public virtual void EndEject()
    {
        return;
    }

    public virtual Vector2 AimingCameraPos(Vector2 aircraftPos, Vector2 mouseTargetPos, Vector2 targetAircraftPos)
    {
        return Vector2.zero;
    }

    public virtual void BeginAiming()
    {
        
    }
    public virtual void EndAiming()
    {
        
    }

    public virtual void Test()
    {
        Debug.Log("Basic Class.");
    }
}
    
}
