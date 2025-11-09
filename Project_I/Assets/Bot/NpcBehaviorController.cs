using System;
using System.Collections;
using System.Collections.Generic;
using Project_I.Bot;
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
        private bool inited = false;
        
        [LabelText("视线长度")]
        public float sight = 100.0f;

        [LabelText("攻击长度")]
        public float attackDistance = 30.0f;
        
        [LabelText("当前状态")]
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
        
        // 以下是行为树BlackBoard
        [NonSerialized]
        public Transform targetUnitTransform;
        [NonSerialized]
        public bool fireCooledDown = true;
        
        //Debug用：
        private List<Vector2> debugPath;


        private void Start()
        {
            // 要获取的组件
            aircraftController = GetComponent<AircraftController>();

            state = BotState.Idle;
            
            debugPath =  new List<Vector2>();
            
            // Debug
            patrolPos = new Vector2(transform.position.x, transform.position.y);
            targetPosDefault = new Vector2(transform.position.x, transform.position.y);
            
            // 要是有敌人或者友军似了
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
                // ejector.BeginEject();
                
                
                inited = true;
            }
            
        }
        
        // 追踪目标的携程
        public IEnumerator TraceTargetOnce(TraceTarget btNode)
        {
            state = BotState.Pursue;
            
            // 追踪的时长
            float traceTime = 2.5f;
            // 追踪判定的次数
            int traceCount = 20;
            
            // 根据上面两个值，计算出携程的一次等待间隔时间
            float interval = traceTime / traceCount;
            
            // 不断遍历，设置新的目标点
            for (int i = 0; i < traceCount; i++)
            {
                
                debugPath.Clear();
                debugPath.Add(transform.position);
                
                if (targetUnitTransform != null)
                {
                    
                    debugPath.Add(targetUnitTransform.position);

                    targetPos = targetUnitTransform.position;
                }
                
                yield return new WaitForSeconds(interval);
            }

            // 结束时：
            state = BotState.Idle;
            targetPosDefault = new Vector2(transform.position.x, transform.position.y);
            btNode.Stop();
        }
        
        
        // 返回patrol位置
        public IEnumerator GoBackTPatrolPos(TraceTarget btNode)
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
            float interval = 0.1f;
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
                
                yield return new WaitForSeconds(interval);
            }
            
            // 迭代结束：该Bot到达了patrol position
        }
        
        
        // 开火协程
        public IEnumerator FireToTargetOnce(FireToTarget btNode)
        {
            float time = btNode.fireTime;
            int number = 20;
            float interval = time / number;
            
            Debug.Log("time: " + time);
            Debug.Log("interval: " + interval);
            
            state = BotState.Attack;
            
            // 获取ejector
            var ejectorController = GetComponent<EjectorController>();
            // 选择0号武器槽：枪炮
            ejectorController.SwitchEjector(0);
            // 发射
            ejectorController.BeginEject();
            
            // 自己的rg
            Rigidbody2D thisRg = GetComponent<Rigidbody2D>();
            // 目标的rg
            Rigidbody2D otherRg = targetUnitTransform.GetComponent<Rigidbody2D>();

            for (int i = 0; i < number; i++)
            {
                if (otherRg == null)
                {
                    // 如果当前的目标已经死了，那么直接break，提前结束当前的扫射轮
                    break;
                }
                
                // 获得自己的速度
                Vector2 thisVelocity = thisRg.velocity;
                // 获取目标的速度
                Vector2 otherVelocity = otherRg.velocity;
                // 火控启动！
                // 发射的子弹的弹速
                Vector2 bulletVelocity = thisVelocity + btNode.bulletSpeed * thisVelocity.normalized;
                // 淘宝火控
                Vector2 thePreciseFirePos;
                bool hasPreciseFirePos = GetLeadPosition(transform.position, bulletVelocity.magnitude,
                    targetUnitTransform.position, otherVelocity, out thePreciseFirePos);

                if (hasPreciseFirePos)
                    targetPos = thePreciseFirePos;
                else
                    targetPos = targetUnitTransform.position;
            
                Debug.Log("已开火" + i);
                // targetPos = targetUnitTransform.position;

                yield return new WaitForSeconds(interval);
            }
            
            ejectorController.EndEject();
            state = BotState.Patrol;
            btNode.Stop();
        }
        
        // 随机线路
        public IEnumerator GoRandomPathOnce(GoRandomPath btNode)
        {
            // 计算随机点
        }
        
        /// <summary>
        /// 计算提前量命中点。
        /// 返回 true 表示有解，p 为预测命中位置。
        /// </summary>
        public static bool GetLeadPosition(
            Vector2 Apos, float bulletSpeed,
            Vector2 Bpos, Vector2 Bvel,
            out Vector2 p)
        {
            Vector2 r = Bpos - Apos;
            float a = Vector2.Dot(Bvel, Bvel) - bulletSpeed * bulletSpeed;
            float b = 2f * Vector2.Dot(r, Bvel);
            float c = Vector2.Dot(r, r);

            float discriminant = b * b - 4f * a * c;

            if (discriminant < 0f)
            {
                p = Vector2.zero;
                return false; // 无解，追不上
            }

            float sqrt = Mathf.Sqrt(discriminant);

            // 两个可能的时间
            float t1 = (-b + sqrt) / (2f * a);
            float t2 = (-b - sqrt) / (2f * a);

            // 取最小的正时间
            float t = Mathf.Min(t1, t2);
            if (t < 0f)
                t = Mathf.Max(t1, t2);

            if (t < 0f)
            {
                p = Vector2.zero;
                return false; // 目标在背后，无法命中
            }

            // 命中点
            p = Bpos + Bvel * t;
            return true;
        }
        
        
        // 一定时间后执行某个函数的协程
        public IEnumerator ActionAfterTime(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            
            action();
        }
        
        
        void OnDrawGizmos()
        {
            /*if (debugPath == null) return;
            
            if (gameObject.layer == LayerDataManager.Instance.enemyLayer)
            {
                /*Gizmos.color = Color.red;
                for (int i = 0; i < debugPath.Count - 1; i++)
                    Gizmos.DrawLine(debugPath[i], debugPath[i + 1]);#1#
            }
            else
            {
                /*Gizmos.color = Color.green;
                for (int i = 0; i < debugPath.Count - 1; i++)
                    Gizmos.DrawLine(debugPath[i], debugPath[i + 1]);#1#
            }*/
        }
    }
        
}
