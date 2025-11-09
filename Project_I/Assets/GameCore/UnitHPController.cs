using System;
using System.Collections;
using System.Collections.Generic;
using Project_I;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project_I
{

    public class UnitHPController : MonoBehaviour
    {
        [LabelText("血量")] public float hp = 20.0f;

        private PlayerController player;
        private NpcBehaviorController npcBehaviorController;

        private void Start()
        {
            player = GetComponent<PlayerController>();
            npcBehaviorController = GetComponent<NpcBehaviorController>();
        }

        public void Hit(float damage)
        {
            hp -= damage;
            if (hp <= 0)
            {
                hp = 0;
                Die();
            }
        }

        public void Die()
        {
            if (player is not null)
            {
                player.Die();
            }

            if (gameObject.layer == LayerDataManager.Instance.enemyLayer)
            {
                GameSceneManager.Instance.DieEnemy(gameObject);
                // GetComponent<BasicEnemy>().Die();
            }
            else if (gameObject.layer == LayerDataManager.Instance.friendlyLayer)
            {
                GameSceneManager.Instance.DieFriend(gameObject);
                // GetComponent<BasicFriend>().Die();
            }
        }
    }
}
