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
        // 敌方子弹撞到我方
        // TODO 友方现在未加入
        if (gameObject.layer == LayerDataManager.Instance.enemyBulletLayer &&
            (other.gameObject.layer == LayerDataManager.Instance.playerLayer /*||
             other.gameObject.layer == LayerDataManager.Instance.friendlyLayer*/))
        {
            GameSceneManager.Instance.player.GetComponent<UnitHPController>().Hit(damage);
        }
        
        // 友方子弹撞到敌人
        if (gameObject.layer == LayerDataManager.Instance.friendlyBulletLayer &&
            other.gameObject.layer == LayerDataManager.Instance.enemyLayer)
        {
            other.gameObject.GetComponent<BasicEnemy>().GetComponent<UnitHPController>().Hit(damage);
        }
        
        Destroy(gameObject);
    }
}
    
}
