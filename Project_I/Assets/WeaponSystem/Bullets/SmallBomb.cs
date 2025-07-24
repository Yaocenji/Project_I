using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Project_I
{
public class SmallBomb : BasicBullet
{
    [Header("爆炸范围")]
    public float sputterRadius = 5.0f;
    
    private SputterController _sputterController;

    public SputterController sputterController
    {
        get => _sputterController;
    }

    void Start()
    {
        _sputterController = GetComponentInChildren<SputterController>();
        _sputterController.gameObject.layer = gameObject.layer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private new void OnTriggerEnter2D(Collider2D other)
    {
        // 撞到敌人或者地板
        if (other is not null && 
            (other.gameObject.layer == LayerDataManager.Instance.enemyLayer 
             || other.gameObject.layer == LayerDataManager.Instance.groundLayer))
        {
            // 对爆炸范围内的所有目标都施加伤害
            foreach (var enemy in _sputterController.sputterEnemies)
            {
                enemy.Hit(damage);
            }
        }
        Destroy(gameObject);
    }
}
    
}
