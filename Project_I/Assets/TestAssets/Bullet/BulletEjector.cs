using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Test
{
public class BulletEjector : MonoBehaviour
{
    public GameObject Bullet;

    private Rigidbody2D _rigidbody2D;

    // 提前获取layer
    private int _playerLayer;
    private int _friendlyLayer;
    private int _enemyLayer;
    private int _friendlyBulletLayer;
    private int _enemyBulletLayer;
    
    // 发射状态
    private bool isEjecting;
    // 冷却时间
    private float coldDownTime;

    private void Awake()
    {
        _playerLayer = LayerMask.NameToLayer("Player");
        _friendlyLayer = LayerMask.NameToLayer("Friendly");
        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _friendlyBulletLayer = LayerMask.NameToLayer("FriendlyBullet");
        _enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");

        isEjecting = false;
        coldDownTime = 0;

        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    
    private void Update()
    {
        if (coldDownTime > 0)
            coldDownTime -= Time.deltaTime;

        if (isEjecting && coldDownTime <= 0)
        {
            while (coldDownTime <= 0)
            {
                var newBullet = Eject().GetComponent<BasicBullet>();
                coldDownTime += newBullet.fireRate;
            }
        }
    }

    public void BeginEject()
    {
        isEjecting = true;
    }

    public void EndEject()
    {
        isEjecting = false;
    }

    public GameObject Eject()
    {
        GameObject newBulletObject = Instantiate(Bullet);
        if (gameObject.layer == _playerLayer || gameObject.layer == _friendlyLayer)
            newBulletObject.layer = _friendlyBulletLayer;
        else if (gameObject.layer == _enemyLayer)
            newBulletObject.layer = _enemyBulletLayer;

        newBulletObject.transform.position = transform.position;
        newBulletObject.transform.rotation = transform.rotation;

        BasicBullet newBullet = newBulletObject.GetComponent<BasicBullet>();

        Rigidbody2D newRB = newBulletObject.GetComponent<Rigidbody2D>();
        newRB.AddForce(_rigidbody2D.velocity * _rigidbody2D.mass, ForceMode2D.Impulse);
        newRB.AddForce(newBulletObject.transform.right * newRB.mass * newBullet.initialSpeed, ForceMode2D.Impulse);

        return newBulletObject;
    }
}

}
