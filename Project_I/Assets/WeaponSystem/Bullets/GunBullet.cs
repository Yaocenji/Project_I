using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class GunBullet : BasicBullet
{
    public new void OnTriggerEnter2D(Collider2D other)
    {
        // 撞到敌人
        if (other is not null && other.gameObject.layer == LayerDataManager.Instance.enemyLayer)
            other.gameObject.GetComponent<BasicEnemy>().Hit(damage);
        Destroy(gameObject);
    }
}
    
}
