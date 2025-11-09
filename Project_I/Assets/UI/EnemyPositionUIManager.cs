using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project_I.UI
{
    public class EnemyPositionUIManager : MonoBehaviour
    {
        [LabelText("敌方UI显示")] public GameObject enemyPointUI;
        
        // 存储UI和enemy的映射
        private Dictionary<GameObject, GameObject> enemy2UI;
        
        void Awake()
        {
            EventSystem.EventBus.Subscribe<EventSystem.EnemyRegisteredEvent>(OnEnemyRegistered);
            EventSystem.EventBus.Subscribe<EventSystem.EnemyDiedEvent>(OnEnemyDied);
            enemy2UI = new Dictionary<GameObject, GameObject>();
        }

        private void Start()
        {
        }

        private void OnDestroy()
        {
            EventSystem.EventBus.Unsubscribe<EventSystem.EnemyRegisteredEvent>(OnEnemyRegistered);
            EventSystem.EventBus.Unsubscribe<EventSystem.EnemyDiedEvent>(OnEnemyDied);
        }

        private void OnEnemyRegistered(EventSystem.EnemyRegisteredEvent e)
        {
            Debug.Log($"新敌人：{e.Enemy.name}");
            
            // 添加 UI 元素，加入为子物体
            GameObject newEnemyPointerUI = Instantiate(enemyPointUI, transform);
            TargetPositionUIController targetPositionUIController = newEnemyPointerUI.gameObject.GetComponent<TargetPositionUIController>();
            targetPositionUIController.target = e.Enemy.transform;

            enemy2UI.Add(e.Enemy, newEnemyPointerUI);
        }

        private void OnEnemyDied(EventSystem.EnemyDiedEvent e)
        {
            Debug.Log($"敌人死亡：{e.Enemy.name}");
            
            // 移除 UI 元素
            if (enemy2UI.ContainsKey(e.Enemy))
            {
                Destroy(enemy2UI[e.Enemy]);
                enemy2UI.Remove(e.Enemy);
            }
        }
    }
}
