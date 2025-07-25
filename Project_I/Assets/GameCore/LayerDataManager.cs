using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{

public class LayerDataManager : MonoBehaviour
{
    public static LayerDataManager Instance;
    
    // 提前获取layer
    public int playerLayer;
    public int friendlyLayer;
    public int enemyLayer;
    public int friendlyBulletLayer;
    public int enemyBulletLayer;
    public int groundLayer;

    public int playerLayerMask;
    public int friendlyLayerMask;
    public int enemyLayerMask;
    public int friendlyBulletLayerMask;
    public int enemyBulletLayerMask;
    public int groundLayerMask;
    void Awake()
    {
        Instance = this;
        
        playerLayer = LayerMask.NameToLayer("Player");
        friendlyLayer = LayerMask.NameToLayer("Friendly");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        friendlyBulletLayer = LayerMask.NameToLayer("FriendlyBullet");
        enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");
        groundLayer = LayerMask.NameToLayer("Ground");

        playerLayerMask = LayerMask.GetMask("Player");
        friendlyLayerMask = LayerMask.GetMask("Friendly");
        enemyLayerMask = LayerMask.GetMask("Enemy");
        friendlyBulletLayerMask = LayerMask.GetMask("FriendlyBullet");
        enemyBulletLayerMask = LayerMask.GetMask("EnemyBullet");
        groundLayerMask = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
        

    
}
