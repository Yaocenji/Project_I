using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{

public class LayerDataManager : MonoBehaviour
{
    public static LayerDataManager Instance;
    
    // 提前获取layer
    [HideInInspector]
    public int playerLayer;
    [HideInInspector]
    public int friendlyLayer;
    [HideInInspector]
    public int enemyLayer;
    [HideInInspector]
    public int friendlyBulletLayer;
    [HideInInspector]
    public int enemyBulletLayer;
    [HideInInspector]
    public int groundLayer;

    [HideInInspector]
    public int playerLayerMask;
    [HideInInspector]
    public int friendlyLayerMask;
    [HideInInspector]
    public int enemyLayerMask;
    [HideInInspector]
    public int friendlyBulletLayerMask;
    [HideInInspector]
    public int enemyBulletLayerMask;
    [HideInInspector]
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
