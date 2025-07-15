using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Project_I
{
public class BasicEnemy : MonoBehaviour
{
    public AircraftController aircraftController;
    
    [Header("血量")]
    public float hp = 30f;

    [NonSerialized]
    public Vector2 targetPos;

    private void Awake()
    {
        targetPos = Vector2.zero;
    }

    private void Start()
    {
        aircraftController.SetTargetPosition(targetPos);
        aircraftController.StartStandardThrust();
    }

    private void Update()
    {
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
