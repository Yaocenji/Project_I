using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class GunBullet : BasicBullet
{
    /*public new void OnTriggerEnter2D(Collider2D other)
    {
        /#1#/ 敌方子弹撞到我方
        // TODO 友方现在未加入
        if (gameObject.layer == LayerDataManager.Instance.enemyBulletLayer &&
            (other.gameObject.layer == LayerDataManager.Instance.playerLayer /*|| 
             other.gameObject.layer == LayerDataManager.Instance.friendlyLayer#2#))
        {
            GameSceneManager.Instance.player.GetComponent<UnitHPController>().Hit(damage);
        }
        
        // 友方子弹撞到敌人
        if (gameObject.layer == LayerDataManager.Instance.friendlyBulletLayer &&
            other.gameObject.layer == LayerDataManager.Instance.enemyLayer)
        {
            other.gameObject.GetComponent<BasicEnemy>().GetComponent<UnitHPController>().Hit(damage);
        }#1#
        Destroy(gameObject);
    }*/
}
    
}
