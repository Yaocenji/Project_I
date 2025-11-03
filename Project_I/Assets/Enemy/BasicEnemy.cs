using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace Project_I
{
    // 敌人状态
    public enum EnemyState
    {
        Idle,
        Patrol,
        Pursue,
        Attack
    }
    
    public class BasicEnemy : MonoBehaviour
    {
        private AircraftController aircraftController;
        
        [LabelText("血量")]
        public float hp = 30f;
        [LabelText("视线长度")]
        public float sight = 20.0f;
        
        // 当前状态
        public EnemyState state;

        [NonSerialized]
        public Vector2 targetPos;

        private bool inited = false;

        private void Awake()
        {
            // 注册敌人
            GameSceneManager.Instance.RegisterEnemy(gameObject);
        }

        private void Start()
        {
            // 要获取的组件
            aircraftController = GetComponent<AircraftController>();
            targetPos = Vector2.zero;
        }

        private void Update()
        {
            // 测试代码：敌人追踪玩家
            // targetPos = GameSceneManager.Instance.player.transform.position;
            
            
            aircraftController.SetTargetPosition(targetPos);
            
            if (!inited)
            {
                aircraftController.SetTargetPosition(targetPos);
                aircraftController.StartStandardThrust();
                inited = true;
            }
            
            // 可能的死亡
            if (hp <= 0)
            {
                Die();
            }
        }

        public void Hit(float dmg)
        {
            hp -= dmg;
        }

        private void Die()
        {
            Destroy(gameObject);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
        }
        
        // 动作：追踪玩家
        public void BeginTracePlayer(Bot.TracePlayer btNode)
        {
            StartCoroutine("TracePlayerOnce", btNode);
        }
        public IEnumerator TracePlayerOnce(Bot.TracePlayer btNode)
        {
            float TraceTime = 1.0f;
            int TraceCount = 5;
            float Interval = TraceTime / (float)TraceCount;
            
            for (int i = 0; i < TraceCount; i++)
            {
                targetPos = GameSceneManager.Instance.player.transform.position;
                yield return new WaitForSeconds(Interval);
            }

            btNode.SetTraceEnd();
        }
    }
        
}
