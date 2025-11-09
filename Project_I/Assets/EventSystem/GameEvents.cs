using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.EventSystem
{
    public struct EnemyRegisteredEvent
    {
        public GameObject Enemy;
        public EnemyRegisteredEvent(GameObject enemy) => Enemy = enemy;
    }
    public struct EnemyDiedEvent
    {
        public GameObject Enemy;
        public EnemyDiedEvent(GameObject enemy) => Enemy = enemy;
    }

    public struct PlayerAttackedEvent
    {
        public float Damage;
        public Vector2 Direction;
        public PlayerAttackedEvent(float damage, Vector2 direction){
            Damage = damage;
            Direction = direction;
        }
    }
}
