using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class SputterController : MonoBehaviour
{
    private CircleCollider2D _collider2D;
    
    private List<BasicEnemy> _sputterEnemies;
    public List<BasicEnemy> sputterEnemies
    {
        get => _sputterEnemies;
    }
    
    void Start()
    {
        _collider2D = GetComponent<CircleCollider2D>();
        _collider2D.radius = transform.parent.GetComponent<SmallBomb>().sputterRadius;
        
        _sputterEnemies = new List<BasicEnemy>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other is not null && other.gameObject.layer == LayerDataManager.Instance.enemyLayer)
        {
            _sputterEnemies.Add(other.gameObject.GetComponent<BasicEnemy>());
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other is not null && other.gameObject.layer == LayerDataManager.Instance.enemyLayer)
        {
            _sputterEnemies.Remove(other.gameObject.GetComponent<BasicEnemy>());
        }
    }
}
    
}
