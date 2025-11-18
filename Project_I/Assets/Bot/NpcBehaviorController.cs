using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        [NonSerialized] // 目标Transform
        public Transform targetUnitTransform;
        
        [NonSerialized] // 剩余可用 追踪动作时间
        public float traceRemainTime = 16;
        [NonSerialized] // 追踪动作冷却前最长时间
        public const float maxTraceTime = 16;
        
        [NonSerialized] // 剩余可用 开火动作时间
        public float fireRemainTime = 5;
        [NonSerialized] // 开火动作冷却前最长时间
        public const float maxFireTime = 5;
        
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
            
            // 最长追踪的时长
            float traceTime = 25f;
            // 追踪判定的次数
            int traceCount = 200;
            
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
                
                traceRemainTime -= interval;
                if (traceRemainTime <= 0)
                    break;
                
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
                bool hasPreciseFirePos = Bot.Navigation.Utility.GetLeadPosition(transform.position, bulletVelocity.magnitude,
                    targetUnitTransform.position, otherVelocity, out thePreciseFirePos);

                if (hasPreciseFirePos)
                    targetPos = thePreciseFirePos;
                else
                    targetPos = targetUnitTransform.position;
            
                // Debug.Log("已开火" + i);
                // targetPos = targetUnitTransform.position;
                fireRemainTime -= interval;
                if (fireRemainTime <= 0)
                    break;
                
                yield return new WaitForSeconds(interval);
            }
            
            ejectorController.EndEject();
            state = BotState.Patrol;
            btNode.Stop();
        }
        
        // 随机线路协程
        public IEnumerator GoRandomPathOnce(GoRandomPath btNode)
        {
            // 最大协程时间
            float maxTime = 60;
            // 最小采样时间
            float interval = 0.25f;
            // 最长采样次数
            int maxNumber = (int)(60f / 0.65f) + 1;
            
            // 判定到达目标点的距离门限
            float reachDistance = 10.0f;
            
            
            // 计算随机点
            Vector3[] randomPath = null;
            float angleRange = 75.0f;
            float minDistance = 75.0f;
            float maxDistance = 200.0f;

            int maxQueryTime = 15;
            for (int i = 0; i < maxQueryTime; i++)
            {
                var hasAns = Bot.Navigation.Utility.TryGetRandomReachablePoint(
                        transform.position, minDistance, maxDistance, transform.right, angleRange, out randomPath);
                if (hasAns)
                    break;
                else
                {
                    // 放宽查找限制，避免卡死
                    angleRange += 15;
                    angleRange = Mathf.Min(angleRange, 180);

                    minDistance -= 15;
                    minDistance = Mathf.Max(minDistance, 0);
                    
                    maxDistance += 15;
                }
            }
            if (randomPath == null)
            {
                btNode.Stop();
                yield break;
            }
            
            // Debug.Log("随机目标点：" + randomPath[randomPath.Length - 1]);
            
            // 预处理
            LinkedList<Vector2> pathList = new LinkedList<Vector2>();
            foreach (Vector3 v in randomPath)
            {
                pathList.AddLast(v);
            }
            // Debug.Log("生成路径点：" + string.Join(" -> ", pathList.Select(p => p.ToString())));
            
            debugPath.Clear();
            debugPath.Add(transform.position);
            foreach (Vector2 v in pathList)
                debugPath.Add(v);
            
            // 拿到随机路径点，开始按照路径走
            while (pathList.Count > 0)
            {
                targetPos = pathList.First.Value;
                
                // 如果到达当前路径点，那么清除，从下一个路径点继续
                if (Vector2.Distance(transform.position, pathList.First.Value) <= reachDistance)
                {
                    // Debug.Log("到达路径点");
                    pathList.RemoveFirst();
                }
                
                yield return new WaitForSeconds(interval);
            }
            
            btNode.Stop();
            
            // yield return new WaitForSeconds(0.1f);
        }
        
        
        
        // 一定时间后执行某个函数的协程
        public IEnumerator ActionAfterTime(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            
            action();
        }
        
        
        void OnDrawGizmos()
        {
            if (debugPath == null) return;
            if (LayerDataManager.Instance == null) return;
            
            if (gameObject.layer == LayerDataManager.Instance.enemyLayer)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < debugPath.Count - 1; i++)
                    Gizmos.DrawLine(debugPath[i], debugPath[i + 1]);
            }
            else
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < debugPath.Count - 1; i++)
                    Gizmos.DrawLine(debugPath[i], debugPath[i + 1]);
            }
        }
    }
        
}
