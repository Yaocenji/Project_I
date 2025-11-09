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
    
    // 溅射范围控制器
    private SputterController _sputterController;
    // 刚体
    private Rigidbody2D _rigidbody2D;

    public SputterController sputterController
    {
        get => _sputterController;
    }

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _sputterController = GetComponentInChildren<SputterController>();
        _sputterController.gameObject.layer = gameObject.layer;
    }

    void FixedUpdate()
    {
        // 加入空阻
        float speed = _rigidbody2D.velocity.magnitude;
        Vector2 fricF = -_rigidbody2D.velocity.normalized * (speed * speed) / 45.0f;
        // 施加空阻
        _rigidbody2D.AddForce(fricF, ForceMode2D.Force);
    }

    private new void OnTriggerEnter2D(Collider2D other)
    {
        // 撞到敌人或者地板
        if (other != null && 
            (other.gameObject.layer == LayerDataManager.Instance.enemyLayer 
             || other.gameObject.layer == LayerDataManager.Instance.groundLayer))
        {
            // 对爆炸范围内的所有目标都施加伤害
            foreach (var enemy in _sputterController.sputterEnemies)
            {
                enemy.GetComponent<UnitHPController>().Hit(damage);
            }
        }
        Destroy(gameObject);
    }
}
    
}
