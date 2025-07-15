using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class BasicBullet : MonoBehaviour
{
    [Header("生命周期")]
    public float lifeTime = 0.2f;
    [Header("伤害")]
    public float damage = 1.0f;

    private float lifeTimer;

    private Vector2 velocity;

    private void Start()
    {
        lifeTimer = 0;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= lifeTime) Destroy(gameObject);
    }
    
    public virtual void OnTriggerEnter2D(Collider2D other)
    {
        // 撞到敌人
        if (other is not null && other.gameObject.layer == LayerDataManager.Instance.enemyLayer)
            other.gameObject.GetComponent<BasicEnemy>().Hit(damage);
        Destroy(gameObject);
    }
}
    
}
