using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Project_I
{
    // 敌人状态
    public enum BotState
    {
        Idle,
        Patrol,
        Pursue,
        Attack
    }
    
    public class NpcBehaviorController : MonoBehaviour
    {
        private AircraftController aircraftController;
        
        [LabelText("视线长度")]
        public float sight = 100.0f;

        [LabelText("攻击长度")]
        public float attackDistance = 30.0f;
        
        // 当前状态
        public BotState state;

        [NonSerialized]
        // 当前的用于Aircraft的目标点
        public Vector2 targetPos;
        
        [NonSerialized]
        // 默认目标点
        public Vector2 targetPosDefault;
        
        [NonSerialized]
        // 巡逻的目标点
        public Vector2 patrolPos;

        private bool inited = false;
        
        //Debug用：
        private List<Vector2> debugPath;

        private void Awake()
        {
        }

        private void Start()
        {
            // 要获取的组件
            aircraftController = GetComponent<AircraftController>();

            state = BotState.Idle;
            
            debugPath =  new List<Vector2>();
            
            // Debug
            patrolPos = new Vector2(transform.position.x, transform.position.y);
            targetPosDefault = new Vector2(transform.position.x, transform.position.y);
        }

        private void Update()
        {
            // 测试代码：敌人追踪玩家
            // targetPos = GameSceneManager.Instance.player.transform.position;
            
            // 不断地设置目标点
            aircraftController.SetTargetPosition(targetPos);
            
            if (!inited)
            {
                targetPos = targetPosDefault;
                
                aircraftController.SetTargetPosition(targetPosDefault);
                aircraftController.StartStandardThrust();
                
                var ejector = GetComponent<EjectorController>();
                ejector.SwitchEjector(0);
                ejector.BeginEject();
                
                
                inited = true;
            }
            
        }

        public void Die()
        {
            Destroy(gameObject);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
        }
        
        // 动作：追踪玩家
        public void BeginTraceAnother(Bot.TracePlayer btNode, GameObject target)
        {
            state = BotState.Pursue;
            StartCoroutine("TracePlayerOnce", btNode);
        }
        
        // 追踪玩家的携程
        public IEnumerator TracePlayerOnce(Bot.TracePlayer btNode/*, GameObject target*/)
        {
            // 追踪的时长
            float TraceTime = 2.5f;
            // 追踪判定的次数
            int TraceCount = 10;
            
            // 根据上面两个值，计算出携程的一次等待间隔时间
            float Interval = TraceTime / (float)TraceCount;
            
            // 不断遍历，设置新的目标点
            for (int i = 0; i < TraceCount; i++)
            {
                
                debugPath.Clear();
                debugPath.Add(transform.position);
                debugPath.Add(GameSceneManager.Instance.Player.transform.position);
                
                targetPos = GameSceneManager.Instance.Player.transform.position;
                
                yield return new WaitForSeconds(Interval);
            }

            // 结束时：
            state = BotState.Idle;
            targetPosDefault = new Vector2(transform.position.x, transform.position.y);
            btNode.SetTraceEnd();
        }
        
        
        // 返回purchase位置
        public IEnumerator GoBackToPurchasePos(Bot.TracePlayer btNode)
        {
            // 用于存储返回的路径
            List<Vector2> pathPos = new List<Vector2>();
            
            // 根据NavMesh获取路径
            NavMeshHit hit;
            NavMesh.SamplePosition(patrolPos, out hit, 1.0f, NavMesh.AllAreas);
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path))
            {
                // 获取结果
                Vector3[] corners = path.corners;
                // 存储结果
                foreach (Vector2 corner in corners)
                {
                    pathPos.Add(new Vector2(corner.x, corner.y));
                }
            }
            
            //Debug
            debugPath.Clear();
            foreach (Vector2 pos in pathPos)
            {
                debugPath.Add(pos);
            }
            
            // 沿着路径移动
            // 协程更新间隔
            float Interval = 0.1f;
            // 当前的Idx
            int idx = 0;
            // 开始协程的迭代。
            while (idx < pathPos.Count)
            {
                targetPos = pathPos[idx];
                if (Vector3.Distance(transform.position, targetPos) < 3.0f)
                {
                    idx++;
                }
                
                yield return new WaitForSeconds(Interval);
            }
            
            // 迭代结束：该Bot到达了patrol position
        }
        
        void OnDrawGizmos()
        {
            if (debugPath == null) return;
            Gizmos.color = Color.red;
            for (int i = 0; i < debugPath.Count - 1; i++)
                Gizmos.DrawLine(debugPath[i], debugPath[i + 1]);
        }
    }
        
}
