using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Project_I
{
public class BasicEnemy : MonoBehaviour
{
    private AircraftController aircraftController;
    
    [Header("血量")]
    public float hp = 30f;

    [NonSerialized]
    public Vector2 targetPos;

    private bool inited = false;

    private void Awake()
    {
        // 注册敌人
        GameSceneManager.Instance.RegisterEnemy(gameObject);
    }

    private void Start()
    {
        // 要获取的组件
        aircraftController = GetComponent<AircraftController>();
        targetPos = Vector2.zero;
    }

    private void Update()
    {
        if (!inited)
        {
            aircraftController.SetTargetPosition(targetPos);
            aircraftController.StartStandardThrust();
            inited = true;
        }
        
        // 可能的死亡
        if (hp <= 0)
        {
            Die();
        }
    }

    public void Hit(float dmg)
    {
        hp -= dmg;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
    }
}
    
}
