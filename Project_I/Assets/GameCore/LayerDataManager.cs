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
    void Awake()
    {
        Instance = this;
        
        playerLayer = LayerMask.NameToLayer("Player");
        friendlyLayer = LayerMask.NameToLayer("Friendly");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        friendlyBulletLayer = LayerMask.NameToLayer("FriendlyBullet");
        enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");
        groundLayer = LayerMask.NameToLayer("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
        

    
}
