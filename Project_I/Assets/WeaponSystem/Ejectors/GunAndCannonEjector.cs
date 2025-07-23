using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = System.Numerics.Quaternion;
using Random = UnityEngine.Random;

namespace Project_I
{
public class GunAndCannonEjector : BasicEjector
{
    public GameObject gunBullet;

    [Header("机枪数据")]
    [Header("冷却")]
    public float gunColdTime = 0.05f;
    [Header("出膛速度")]
    public float speed = 100.0f;
    [Header("散布")]
    public float sigma = 5.0f;
    
    // 当前飞机的参数
    private AircraftController _aircraftController;

    // 当前是否正在开火？
    private bool isEjecting;
    
    // 计时器
    private float timerGun;
    

    private void Start()
    {
        isEjecting = false;
        timerGun = 0;

        _aircraftController = GetComponent<AircraftController>();
    }

    private void Update()
    {
        if (isEjecting)
        {
            timerGun += Time.deltaTime;

            // 机枪发射
            while (timerGun > gunColdTime)
            {
                timerGun -= gunColdTime;
                Ejecting();
            }
        }
    }

    public override void BeginEject()
    {
        isEjecting = true;
    }

    public override void EndEject()
    {
        isEjecting = false;
    }

    public override void Ejecting()
    {
        GameObject newBulletObject = Instantiate(gunBullet);
        if (gameObject.layer == LayerDataManager.Instance.playerLayer || gameObject.layer == LayerDataManager.Instance.friendlyLayer)
            newBulletObject.layer = LayerDataManager.Instance.friendlyBulletLayer;
        else if (gameObject.layer == LayerDataManager.Instance.enemyLayer)
            newBulletObject.layer = LayerDataManager.Instance.enemyBulletLayer;

        newBulletObject.transform.position = transform.position;
        newBulletObject.transform.rotation = UnityEngine.Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + Random.Range(-sigma, sigma));

        BasicBullet newBullet = newBulletObject.GetComponent<BasicBullet>();

        Rigidbody2D newRB = newBulletObject.GetComponent<Rigidbody2D>();
        
        newRB.AddForce(_aircraftController.getVelocity * newRB.mass, ForceMode2D.Impulse);
        newRB.AddForce(newRB.mass * speed * newBulletObject.transform.right, ForceMode2D.Impulse);
    }

    public override Vector2 AimingCameraPos(Vector2 aircraftPos, Vector2 mouseTargetPos, Vector2 targetAircraftPos)
    {
        return aircraftPos + (mouseTargetPos - aircraftPos).normalized * aimingCameraDistance / 2.0f;
    }

    public override void Test()
    {
        Debug.Log("Derived Class.");
    }
}
    
}
