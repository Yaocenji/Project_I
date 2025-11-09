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
        // 对方unit是碰撞箱所在object的父
        GameObject otherUnit = other.transform.parent.gameObject;
        
        // 敌方子弹撞到玩家
        if (gameObject.layer == LayerDataManager.Instance.enemyBulletLayer &&
            otherUnit.gameObject.layer == LayerDataManager.Instance.playerLayer)
        {
            GameSceneManager.Instance.Player.GetComponent<UnitHPController>().Hit(damage);
            
            EventSystem.EventBus.Publish(new EventSystem.PlayerAttackedEvent(damage,
                GetComponent<Rigidbody2D>().velocity.normalized ));
        }

        // 敌方子弹撞到友方
        if (gameObject.layer == LayerDataManager.Instance.enemyBulletLayer &&
            otherUnit.gameObject.layer == LayerDataManager.Instance.friendlyLayer)
        {
            otherUnit.GetComponent<UnitHPController>().Hit(damage);
        }
        
        // 友方子弹撞到敌人
        if (gameObject.layer == LayerDataManager.Instance.friendlyBulletLayer &&
            otherUnit.gameObject.layer == LayerDataManager.Instance.enemyLayer)
        {
            otherUnit.gameObject.GetComponent<UnitHPController>().Hit(damage);
        }
        
        Destroy(gameObject);
    }
}
    
}
