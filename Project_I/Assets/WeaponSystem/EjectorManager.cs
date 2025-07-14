using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Project_I
{
public class EjectorManager : MonoBehaviour
{
    // 四个发射器
    public BasicEjector ejectorsUp;
    public BasicEjector ejectorsDown;
    public BasicEjector ejectorsLeft;
    public BasicEjector ejectorsRight;

    // 当前活跃发射器
    private BasicEjector currEjector;    
    
    // 是否正在瞄准
    [NonSerialized]
    public bool isAiming;

    private void Start()
    {
        // 进行一些检查

        currEjector = null;
        isAiming = false;
        
        ejectorsUp = GetComponent<GunAndCannonEjector>();
        // Debug
        // ejectorsUp.Test();
        currEjector = ejectorsUp;
    }

    public void SwitchEjector(int t)
    {
        if (t < 0 || t > 4)
        {
            throw new Exception("Error: switch param error;");
        }

        if (t == 0)
        {
            currEjector = ejectorsUp;
        }if (t == 1)
        {
            currEjector = ejectorsDown;
        }if (t == 2)
        {
            currEjector = ejectorsLeft;
        }if (t == 3)
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

    public void AimingCameraPos(Vector2 aircraftPos, Vector2 mouseTargetPos, Vector2 targetAircraftPos)
    {
        if (currEjector is not null)
        {
            currEjector.AimingCameraPos(aircraftPos, mouseTargetPos, targetAircraftPos);
        }
    }
}
    
}
