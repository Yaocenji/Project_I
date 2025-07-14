using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerDataManager : MonoBehaviour
{
    public static LayerDataManager Instance;
    
    // 提前获取layer
    public int playerLayer;
    public int friendlyLayer;
    public int enemyLayer;
    public int friendlyBulletLayer;
    public int enemyBulletLayer;
    void Awake()
    {
        Instance = this;
        
        playerLayer = LayerMask.NameToLayer("Player");
        friendlyLayer = LayerMask.NameToLayer("Friendly");
        enemyLayer = LayerMask.NameToLayer("Enemy");
        friendlyBulletLayer = LayerMask.NameToLayer("FriendlyBullet");
        enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
