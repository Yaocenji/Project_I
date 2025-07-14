using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class CameraController : MonoBehaviour
{
    public Transform player;

    public bool isAiming;

    private Vector2 normalPosition;
    private Vector2 aimingPosition;

    private void Awake()
    {
        normalPosition = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        normalPosition = new Vector2(player.position.x, player.position.y);
        
        transform.position = new Vector3(normalPosition.x, normalPosition.y, -10);
    }

    public void SetAimingPos(Vector2 ap)
    {
        aimingPosition = ap;
    }
}
    
}
