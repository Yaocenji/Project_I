using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class BasicEjector : MonoBehaviour
{
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

    public virtual void Test()
    {
        Debug.Log("Basic Class.");
    }
}
    
}
