using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Project_I.Bot
{
    // 检测当前敌人是否能看到具备潜在目标的视野
    [NodeName("条件|能否看见潜在目标")]
    [NodeChildLimit(0)]
    public class IfSeeLatentTarget : ConditionNode
    {
        // 潜在目标序列，这是一个引用
        private HashSet<Transform> latentTargetTransforms;
        
        // 需要连续成功几次才成功？
        private int number;
        
        // 最近的几次射线检测
        private bool[] recentCheck;

        // --- 无参构造函数 ---
        public IfSeeLatentTarget() : base() { }

        // --- 重写 Initialize 方法来解析参数 ---
        public override void Initialize(NpcBehaviorController ownerNpcBehaviorController, Transform ownerTransform, List<BehaviorNodeParameter> parameters)
        {
            base.Initialize(ownerNpcBehaviorController, ownerTransform, parameters);
            
            // 设置潜在的transform目标
            if (ownerNpcBehaviorController.gameObject.layer == LayerDataManager.Instance.enemyLayer)
                latentTargetTransforms = GameSceneManager.Instance.PartyATransforms;
            else
                latentTargetTransforms = GameSceneManager.Instance.PartyBTransforms;

            // 从字典中安全地读取 "number" 参数
            if (FindParameterAsString("number", out string numStr, ref parameters))
            {
                int.TryParse(numStr, out number);
                // Debug.Log("行为树config已设置number：" + number);
            }
            else
            {
                // 给到默认值
                number = 5;
            }

            recentCheck = new bool[this.number];
            for (int i = 0; i < number; i++)
            {
                recentCheck[i] = false;
            }
        }
        
        protected override bool Check()
        {
            bool thisANS = false;
            List<Transform> canBeSeenTransforms = new List<Transform>();
            
            // 每个都射线检测一次，取最近者作为该次的目标
            foreach (Transform latentTargetTransform in latentTargetTransforms)
            {
                // 通过发射射线检测
                // 目标方向
                Vector2 targetDirection = (latentTargetTransform.position - this.transform.position).normalized;
                // 二维射线检测
                RaycastHit2D hit2D = Physics2D.Raycast(this.transform.position, targetDirection,
                    NpcBehaviorController.sight,
                    LayerDataManager.Instance.groundLayerMask |
                    ( NpcBehaviorController.gameObject.layer == LayerDataManager.Instance.enemyLayer ? 
                        (LayerDataManager.Instance.playerLayerMask | 
                        LayerDataManager.Instance.friendlyLayerMask ) : 
                        LayerDataManager.Instance.enemyLayerMask));

                // 分析检测结果
                if (hit2D.collider is null)
                {
                    continue;
                }
                else
                {
                    // 被地形阻隔
                    if (hit2D.collider.gameObject.layer == LayerDataManager.Instance.groundLayer)
                    {
                        continue;
                    }
                    // 能直接看到该目标
                    else
                    {
                        canBeSeenTransforms.Add(hit2D.collider.transform);
                    }
                }
            }

            if (canBeSeenTransforms.Count <= 0)
            {
                // Debug.Log("没有看见任何目标");
                return false;
            }
            
            // 获取了能看到的所有目标
            // 筛选最优先的
            thisANS = true;
            float distMin = float.MaxValue;
            Transform theTarget = null;
            foreach (Transform latentTargetTransform in canBeSeenTransforms)
            {
                float thisDistance = (latentTargetTransform.position - this.transform.position).magnitude;
                if (thisDistance < distMin)
                {
                    distMin = thisDistance;
                    theTarget = latentTargetTransform;
                }
            }
            
            // 最优先的目标：theTarget
            // 在黑板上记录theTarget
            NpcBehaviorController.targetUnitTransform = theTarget;
            // Debug.Log("更新目标：" + theTarget.gameObject.name);
            
            // 近期数据整体前移
            for (int i = 0; i < number - 1; i++)
            {
                recentCheck[i] = recentCheck[i + 1];
            }
            // 塞入新数据
            recentCheck[number - 1] = thisANS;
            
            // 必须近期好几次检测都为成功，才算看见了
            for (int i = 0; i < number; i++)
            {
                if (!recentCheck[i])
                {
                    return false;
                }
            }
            
            // Debug.Log("成功看见目标");
            return true;
        }
    }
    
    // 检测当前敌人是否能看到具备玩家视野
    [NodeName("条件|当前目标是否在一定范围内")]
    [NodeChildLimit(0)]
    public class IfPursueTargetInAttackRadius : ConditionNode
    {
        private float radius;

        // --- 移除构造函数参数 ---
        public IfPursueTargetInAttackRadius() : base() { }
        
        public IfPursueTargetInAttackRadius(NpcBehaviorController npcBehaviorController, float radius):base()
        {
            this.NpcBehaviorController = npcBehaviorController;
            this.radius = radius;
        }

        // --- 重写 Initialize 方法来解析参数 ---
        public override void Initialize(NpcBehaviorController ownerNpcBehaviorController, Transform ownerTransform, List<BehaviorNodeParameter> parameters)
        {
            base.Initialize(ownerNpcBehaviorController, ownerTransform, parameters);

            // 从字典中安全地读取 "radius" 参数
            if (FindParameterAsString("radius", out string radiusStr, ref parameters)){
                    float.TryParse(radiusStr, out radius);
                    // Debug.Log("行为树config已设置radius：" +  radius);
            }
            else
            {
                // 如果参数不存在或解析失败，给一个默认值
                radius = NpcBehaviorController.sight;
            }

        }
        
        protected override bool Check()
        {
            if (NpcBehaviorController.targetUnitTransform is null)
                return false;
            
            float dist = Vector2.Distance(NpcBehaviorController.targetUnitTransform.position, this.transform.position);
            return dist <= radius;
        }
    }
    
    
    
    // 检测当前敌人是否能看到具备玩家视野
    [NodeName("条件|当前目标是否在合适枪线上")]
    [NodeChildLimit(0)]
    public class IfPursueTargetInGunLine : ConditionNode
    {
        private float angle;

        // --- 无参构造函数 ---
        public IfPursueTargetInGunLine() : base() { }
        
        public IfPursueTargetInGunLine(NpcBehaviorController npcBehaviorController, float angle):base()
        {
            this.NpcBehaviorController = npcBehaviorController;
            this.angle = angle;
        }

        // --- 重写 Initialize 方法来解析参数 ---
        public override void Initialize(NpcBehaviorController ownerNpcBehaviorController, Transform ownerTransform, List<BehaviorNodeParameter> parameters)
        {
            base.Initialize(ownerNpcBehaviorController, ownerTransform, parameters);

            // 从字典中安全地读取 "radius" 参数
            if (FindParameterAsString("angle", out string angleStr, ref parameters)){
                float.TryParse(angleStr, out angle);
                // Debug.Log("行为树config已设置radius：" +  radius);
            }
            else
            {
                // 如果参数不存在或解析失败，给一个默认值
                angle = 10.0f;
            }

        }
        
        protected override bool Check()
        {
            if (NpcBehaviorController.targetUnitTransform is null)
                return false;
            
            Vector2 targetDirection = (NpcBehaviorController.targetUnitTransform.position - this.transform.position).normalized;
            Vector2 thisDirection = (NpcBehaviorController.GetComponent<Rigidbody2D>().velocity).normalized;
            float realAngle = Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(thisDirection, targetDirection));
            
            // Debug.Log(realAngle);
            
            return realAngle <= angle;
        }
    }
    
    
    // 追踪
    [NodeName("动作|追踪目标")]
    [NodeChildLimit(0)]
    public class TraceTarget : ActionNode
    {
        private bool traceFinished = false;
        protected override void Start()
        {
            traceFinished = false;
            UseTemporalTickInterval = false;

            NpcBehaviorController.StartCoroutine(NpcBehaviorController.TraceTargetOnce(this));
            
            // Debug.Log("开始追踪目标");
        }

        protected override bool Check()
        {
            return traceFinished;
        }

        public void End()
        {
            traceFinished = true;
            // Debug.Log("结束追踪目标");
        }
    }
    
    // 向目标开火
    [NodeName("动作|向目标开火")]
    [NodeChildLimit(0)]
    public class FireToTarget : ActionNode
    {
        [HideInInspector]
        // 开火时间
        public float fireTime;
        [HideInInspector]
        // 假设的弹速（计算）提前量
        public float bulletSpeed;
        
        private bool fireToTargetFinished;
        

        public override void Initialize(NpcBehaviorController ownerNpcBehaviorController, Transform ownerTransform, List<BehaviorNodeParameter> parameters)
        {
            base.Initialize(ownerNpcBehaviorController, ownerTransform, parameters);

            if (FindParameterAsString("fireTime", out string fireTimeStr, ref parameters))
            {
                float.TryParse(fireTimeStr, out fireTime);
            }
            else
            {
                fireTime = 1.0f;
            }
            
            if (FindParameterAsString("bulletSpeed", out string bulletSpeed, ref parameters))
            {
                float.TryParse(bulletSpeed, out fireTime);
            }
            else
            {
                fireTime = 1.0f;
            }
        }

        protected override void Start()
        {
            fireToTargetFinished = false;
            
            UseTemporalTickInterval = false;
            
            NpcBehaviorController.StartCoroutine(NpcBehaviorController.FireToTargetOnce(this));

            // Debug.Log("开始向目标开火");
        }

        protected override bool Check()
        {
            return fireToTargetFinished;
        }

        public void End()
        {
            fireToTargetFinished = true;
            // Debug.Log("结束向目标开火");
        }
    }
}
    
    
