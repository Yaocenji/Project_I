using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class EjectorController : MonoBehaviour
{
    // 玩家机体控制器
    private AircraftController aircraftController;
    
    // 四个发射器
    public BasicEjector ejectorsUp;
    public BasicEjector ejectorsDown;
    public BasicEjector ejectorsLeft;
    public BasicEjector ejectorsRight;

    // 当前活跃发射器
    private BasicEjector currEjector;    
    
    // 是否正在瞄准
    private bool isAiming;
    public bool getAiming { get => isAiming; }
    
    // 瞄准的位置
    private Vector2 _aimingPosition;
    public Vector2 AimingPos
    {
        get => _aimingPosition;
    }
    // 瞄准时的摄像机大小
    public float AimingCameraSize
    {
        get => currEjector.aimingCameraSize;
    }
    // 瞄准时的鼠标灵敏度倍率
    public float AimingMouseSensitivity
    {
        get => currEjector.aimingMouseSensitivity;
    }
    // 瞄准时的时间流速倍率
    public float AimingTimeScale
    {
        get => currEjector.aimingTimeScale;
    }

    private void Start()
    {
        _aimingPosition = Vector3.zero;
        // 进行一些检查

        currEjector = null;
        isAiming = false;
        
        // 测试武备代码
        ejectorsUp = GetComponent<GunAndCannonEjector>();
        ejectorsDown = GetComponent<SmallBombEjector>();
        
        // Debug 
        // ejectorsUp.Test();
        currEjector = ejectorsUp;
        
        // 要获取的其他组件
        aircraftController = GetComponent<AircraftController>();
    }

    private void Update()
    {
        if (isAiming && currEjector is not null)
        {
            _aimingPosition = AimingCameraPos(transform.position, aircraftController.GetTargetPosition(), Vector2.zero);
        }
    }

    public void SwitchEjector(int t)
    {
        if (t < 0 || t > 4)
        {
            throw new Exception("Error: switch param error;");
        }

        if (t == 0 && ejectorsUp is not null)
        {
            currEjector = ejectorsUp;
        }
        if (t == 1 && ejectorsDown is not null)
        {
            currEjector = ejectorsDown;
        }
        if (t == 2 && ejectorsLeft is not null)
        {
            currEjector = ejectorsLeft;
        }
        if (t == 3 && ejectorsRight is not null)
        {
            currEjector = ejectorsRight;
        }
    }
    
    public void BeginEject()
    {
        if (currEjector is not null)
        {
            currEjector.BeginEject();
        }
    }
    /*public void Ejecting()
    {
        if (currEjector is not null)
        {
            currEjector.Ejecting();
        }
    }*/
    public void EndEject()
    {
        if (currEjector is not null)
        {
            currEjector.EndEject();
        }
    }

    public void BeginAiming()
    {
        isAiming = true;
    }
    public void EndAiming()
    {
        isAiming = false;
    }

    public Vector2 AimingCameraPos(Vector2 aircraftPos, Vector2 mouseTargetPos, Vector2 targetAircraftPos)
    {
        if (currEjector is not null)
        {
            return currEjector.AimingCameraPos(aircraftPos, mouseTargetPos, targetAircraftPos);
        }
        else return Vector2.zero;
    }
}
    
}
