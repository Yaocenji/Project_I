using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

namespace Test
{
public class BasicBullet : MonoBehaviour
{
    [Header("出膛速度")]
    public float initialSpeed = 75;
    [Header("射速（秒/发）")]
    public float fireRate = 0.2f;
    [Header("生存时间")]
    public float surviveTime = 5.0f;

    private Rigidbody2D _rigidbody2D;

    private Animator _animator;

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        surviveTime -= Time.deltaTime;
        if (surviveTime <= 0) Boom();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Boom();
    }

    private void Boom()
    {
        
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        _animator.SetTrigger("Boom");
    }

    // 在动画最后一针调用
    public void DestroyAfterBoom()
    {
        Destroy(gameObject);
    }
}
    
}
