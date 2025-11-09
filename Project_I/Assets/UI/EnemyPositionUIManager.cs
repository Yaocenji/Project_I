using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.UI
{
    public class EnemyPositionUIManager : MonoBehaviour
    {
        void Awake()
        {
            EventSystem.EventBus.Subscribe<EventSystem.EnemyRegisteredEvent>(OnEnemyRegistered);
            EventSystem.EventBus.Subscribe<EventSystem.EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnDestroy()
        {
            EventSystem.EventBus.Unsubscribe<EventSystem.EnemyRegisteredEvent>(OnEnemyRegistered);
            EventSystem.EventBus.Unsubscribe<EventSystem.EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnEnemyRegistered(EventSystem.EnemyRegisteredEvent e)
        {
            Debug.Log($"新敌人：{e.Enemy.name}");
            // 添加 UI 元素
        }

        private void OnEnemyDied(EventSystem.EnemyDiedEvent e)
        {
            Debug.Log($"敌人死亡：{e.Enemy.name}");
            // 移除 UI 元素
        }
    }
}
