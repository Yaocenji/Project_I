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
        private BasicEnemy enemy;
        private Transform playerTransform;
        // 需要连续成功几次才成功？
        private int number;
        
        // 最近的几次射线检测
        private bool[] recentCheck;

        public IfSeePlayer(Transform playerTransform, BasicEnemy enemy, int number):base()
        {
            this.playerTransform = playerTransform;
            this.enemy = enemy;
            this.number = number;
            
            recentCheck = new bool[number];
            for (int i = 0; i < number; i++)
            {
                recentCheck[i] = false;
            }
        }
        
        protected override bool Check()
        {
            // 通过发射射线检测；
            Transform playerTransform = GameSceneManager.Instance.player.transform;
            // 玩家方向
            Vector2 playerDirection = (playerTransform.position - this.playerTransform.position).normalized;
            // 二维射线检测
            RaycastHit2D hit2D = Physics2D.Raycast(this.playerTransform.position, playerDirection, 
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
            Debug.Log("成功");
            return true;
        }
    }
    
    // 检测当前敌人是否能看到具备玩家视野
    [NodeName("条件|玩家是否在一定范围内")]
    [NodeChildLimit(0)]
    public class IfPlayerInAttackRadius : ConditionNode
    {
        private BasicEnemy enemy;
        private Transform playerTransform;
        private float radius;

        public IfPlayerInAttackRadius(Transform playerTransform, BasicEnemy enemy, float radius):base()
        {
            this.playerTransform = playerTransform;
            this.enemy = enemy;
            this.radius = radius;
        }
        
        protected override bool Check()
        {
            float dist = Vector2.Distance(playerTransform.position, this.playerTransform.position);
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
        }

        protected override bool Check()
        {
            return traceFinished;
        }

        public void SetTraceEnd()
        {
            traceFinished = true;
        }
    }
}
    
    
