using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Project_I.Bot
{
    // 检测当前敌人是否能看到具备玩家视野
    [NodeName("条件|能否看见玩家")]
    [NodeChildLimit(0)]
    public class IfSeePlayer : ConditionNode
    {
        private Transform playerTransform;
        // 需要连续成功几次才成功？
        private int number;
        
        // 最近的几次射线检测
        private bool[] recentCheck;

        // --- 无参构造函数 ---
        public IfSeePlayer() : base() { }

        // --- 重写 Initialize 方法来解析参数 ---
        public override void Initialize(BasicEnemy ownerEnemy, Transform ownerTransform, Dictionary<string, string> parameters)
        {
            base.Initialize(ownerEnemy, ownerTransform, parameters);
            
            playerTransform = GameSceneManager.Instance.player.transform;

            // 从字典中安全地读取 "number" 参数
            if (parameters.TryGetValue("number", out string numberValue))
            {
                int.TryParse(numberValue, out this.number);
            }
            
            // 如果参数不存在或解析失败，给一个默认值
            if (this.number <= 0) this.number = 5;

            recentCheck = new bool[this.number];
            for (int i = 0; i < number; i++)
            {
                recentCheck[i] = false;
            }
        }
        
        protected override bool Check()
        {
            // 通过发射射线检测；
            //Transform playerTransform = GameSceneManager.Instance.player.transform;
            // 玩家方向
            Vector2 playerDirection = (playerTransform.position - this.transform.position).normalized;
            // 二维射线检测
            RaycastHit2D hit2D = Physics2D.Raycast(this.transform.position, playerDirection, 
                enemy.sight, LayerDataManager.Instance.groundLayerMask | LayerDataManager.Instance.playerLayerMask);
            
            bool thisANS = false;
            
            //Debug.DrawLine(this.playerTransform.position, this.playerTransform.position + (Vector3)playerDirection * enemy.sight, Color.red);
            
            // 分析检测结果
            if (hit2D.collider is null)
            {
                thisANS = false;
            }
            else
            {
                // 能直接看到玩家。
                if (hit2D.collider.gameObject == GameSceneManager.Instance.player.gameObject)
                {
                    thisANS = true;
                }
                // 被地形阻隔
                else
                {
                    thisANS = false;
                }
            }
            
            /*if (thisANS)
                Debug.Log("射线检测：成功");
            else
                Debug.Log("射线检测：失败");*/
            
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
            // ("成功看到玩家");
            return true;
        }
    }
    
    // 检测当前敌人是否能看到具备玩家视野
    [NodeName("条件|玩家是否在一定范围内")]
    [NodeChildLimit(0)]
    public class IfPlayerInAttackRadius : ConditionNode
    {
        private Transform playerTransform;
        private float radius;

        // --- 移除构造函数参数 ---
        public IfPlayerInAttackRadius() : base() { }

        // --- 重写 Initialize 方法来解析参数 ---
        public override void Initialize(BasicEnemy ownerEnemy, Transform ownerTransform, Dictionary<string, string> parameters)
        {
            base.Initialize(ownerEnemy, ownerTransform, parameters);
            
            playerTransform = GameSceneManager.Instance.player.transform;

            // 从字典中安全地读取 "number" 参数
            if (parameters.TryGetValue("radius", out string radiusValue))
            {
                float.TryParse(radiusValue, out this.radius);
            }
            
            // 如果参数不存在或解析失败，给一个默认值
            if (this.radius <= 0) this.radius = 7.5f;

        }
        
        public IfPlayerInAttackRadius(Transform playerTransform, BasicEnemy enemy, float radius):base()
        {
            this.playerTransform = playerTransform;
            this.enemy = enemy;
            this.radius = radius;
        }
        
        protected override bool Check()
        {
            float dist = Vector2.Distance(playerTransform.position, this.playerTransform.position);
            
            /*if (dist <= this.radius)
                Debug.Log("在玩家附近的一定范围内");*/
            
            return dist <= radius;
        }
    }
    
    // 敌人追踪玩家
    [NodeName("动作|追踪玩家")]
    [NodeChildLimit(0)]
    public class TracePlayer : ActionNode
    {
        private bool traceFinished = false;
        protected override void Start()
        {
            traceFinished = false;
            UseTemporalTickInterval = false;
            
            tree.enemy.BeginTracePlayer(this);
            
            // Debug.Log("开始追踪玩家");
        }

        protected override bool Check()
        {
            return traceFinished;
        }

        public void SetTraceEnd()
        {
            traceFinished = true;
            // Debug.Log("结束追踪玩家");
        }
    }
}
    
    
