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
        // 敌方子弹撞到玩家
        if (gameObject.layer == LayerDataManager.Instance.enemyBulletLayer &&
            other.gameObject.layer == LayerDataManager.Instance.playerLayer)
        {
            GameSceneManager.Instance.Player.GetComponent<UnitHPController>().Hit(damage);
            
            EventSystem.EventBus.Publish(new EventSystem.PlayerAttackedEvent(damage,
                GetComponent<Rigidbody2D>().velocity.normalized ));
        }

        // 敌方子弹撞到友方
        if (gameObject.layer == LayerDataManager.Instance.enemyBulletLayer &&
            other.gameObject.layer == LayerDataManager.Instance.friendlyLayer)
        {
            other.GetComponent<UnitHPController>().Hit(damage);
        }
        
        // 友方子弹撞到敌人
        if (gameObject.layer == LayerDataManager.Instance.friendlyBulletLayer &&
            other.gameObject.layer == LayerDataManager.Instance.enemyLayer)
        {
            other.gameObject.GetComponent<UnitHPController>().Hit(damage);
        }
        
        Destroy(gameObject);
    }
}
    
}
