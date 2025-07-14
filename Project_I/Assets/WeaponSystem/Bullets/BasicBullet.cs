using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class BasicBullet : MonoBehaviour
{
    [Header("生命周期")]
    public float lifeTime = 2.0f;
    [Header("伤害")]
    public float damage = 1.0f;

    private float lifeTimer;

    private void Start()
    {
        lifeTimer = 0;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime) Destroy(gameObject);
    }
}
    
}
