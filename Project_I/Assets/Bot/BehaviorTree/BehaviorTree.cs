using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace Project_I.Bot
{
    // 行为树
    public class BehaviorTree : MonoBehaviour
    {
        [LabelText("行为树配置文件")]
        [Tooltip("指定此敌人使用的行为树 ScriptableObject 配置。")]
        public BehaviorTreeConfig treeConfig;
        
        [LabelText("当前NPC控制脚本")]
        public NpcBehaviorController npcBehaviorController;
        [LabelText("决策间隔（频率）")]
        public float interval = 0.3f;
        
        [HideInInspector]
        public ActionNode actingNode = null;   // 当前正在运行的节点
        
        
        // 有些动作节点结束的时候，会改用一次临时interval
        private bool useTemporalInterval = false;
        private float temporalInterval = 0.3f;

        private float lastTime;
        
        private BehaviorTreeNode root;
        
        // 用于记录所有节点
        private List<BehaviorTreeNode> nodesList = new List<BehaviorTreeNode>();
        
        
        // 用于缓存节点显示名称到具体Type的映射，避免运行时重复反射，提高性能
        private Dictionary<string, Type> nodeTypeCache;

        void Start()
        {
            lastTime = Time.time;
            //currDecideCount = 0;
            root = SetupTree(); 
        }

        void Update()
        {
            // 计算当前已经过时间除以决策间隔，并于已决策次数比较，若不等说明少一次决策
            if ( Time.time - lastTime > (useTemporalInterval ? temporalInterval : interval) )
            {
                NodeStatus ans = NodeStatus.FAILURE;
                // 进行一次决策
                if (root != null)
                {
                    ans = Tick();
                }
                
                // 如果ans为正
                if (ans == NodeStatus.SUCCESS)
                {
                    // 那么全部刷成Running（默认状态）
                    SetAllStatusRunning();
                }
                
                // 更新上一Tick发生时间
                lastTime = Time.time;
                
                // 如果当前是使用了临时interval
                // 那么改回不使用
                if (useTemporalInterval)
                    useTemporalInterval = false;
            }
        }

        private NodeStatus Tick()
        {
            /*// 当前正在运行动作节点
            if (actingNode != null)
            {
                if (actingNode.CanBeInterrupted){
                    // TODO 打断他然后调用
                    actingNode.Interrupt();
                    actingNode = null;
                }
                else
                {
                    // 否则就啥也不干
                    return NodeStatus.FAILURE;
                }
            }*/
            // 没有运行中的节点
            return root.Tick();
        }

        // 动作节点 申请运行权
        public bool ApplyActionNodeExecute(ActionNode acNode)
        {
            // 特判：如果是自身，那么不予管理
            if (actingNode == acNode)
                return false;
            
            // 当前没有正在执行的动作节点
            if (actingNode == null)
            {
                // 给予运行权
                actingNode = acNode;
                return true;
            }
            // 当前有正在运行的动作节点
            else
            {
                // 如果可以打断
                if (actingNode.CanBeInterrupted)
                {
                    Debug.Log("申请成功：打断了当前正在运行的节点");
                    actingNode.Interrupt();
                    actingNode = acNode;
                    return true;
                }
                // 若不可打断
                else
                {
                    Debug.Log("申请失败：当前正在运行的节点无法被打断");
                    return false;
                }
            }
        }
        // 动作节点：释放运行权
        public bool ReleaseActionNodeExecute(ActionNode acNode)
        {
            if (actingNode == acNode)
            {
                actingNode = null;
                return true;
            }

            return false;
        }

        private void SetAllStatusRunning()
        {
            foreach (BehaviorTreeNode node in nodesList)
                node.status =  NodeStatus.RUNNING;
        }

        
        // --- 重写 SetupTree 方法 ---
        protected virtual BehaviorTreeNode SetupTree()
        {
            if (treeConfig is null || treeConfig.rootNode is null)
            {
                Debug.LogError("行为树配置为空!", this);
                return null;
            }

            var nodeMap = BehaviorNodeUtils.GetNodeDisplayNameMap();
            HashSet<BehaviorTreeNode> createdNodes = new HashSet<BehaviorTreeNode>();
            
            // 递归创建所有节点
            var ans = CreateNodeRecursive(treeConfig.rootNode, ref nodeMap, ref createdNodes);
            
            Debug.Log("创建行为树完成");

            return ans;
        }

        private BehaviorTreeNode CreateNodeRecursive(BehaviorNodeConfig config, ref Dictionary<string, string> nameMap, 
            ref HashSet<BehaviorTreeNode> createdNodes)
        {
            if (config == null)
                return null;
            
            if (!nameMap.TryGetValue(config.typeName, out string fullTypeName))
            {
                Debug.LogError($"找不到节点类型: {config.typeName}");
                return null;
            }

            Type type = Type.GetType(fullTypeName);
            if (type == null)
            {
                Debug.LogError($"无法从类型名创建类型: {fullTypeName}");
                return null;
            }

            // 使用 Activator.CreateInstance 创建节点实例
            // 我们将使用一个无参构造函数，然后通过一个方法来初始化参数
            BehaviorTreeNode node = Activator.CreateInstance(type) as BehaviorTreeNode;
            node.tree = this;

            // --- 传递参数和上下文 ---
            //var realNode = node as type;
            node.Initialize(npcBehaviorController, transform, config.parameters);
            createdNodes.Add(node);
            
            // --- 递归创建并连接子节点 ---
            if (config.children != null && config.children.Any())
            {
                List<BehaviorTreeNode> childNodes = new List<BehaviorTreeNode>();
                foreach (var childConfig in config.children)
                {
                    childNodes.Add(CreateNodeRecursive(childConfig, ref nameMap, ref createdNodes));
                }
                node.SetChildren(childNodes);
            }

            // 记录一下所有的node
            if (node is not null)
            {
                nodesList.Add(node);
            }

            return node;
        }
        

        public void SetTemporalInterval(float interval)
        {
            useTemporalInterval = true;
            temporalInterval = interval;
        }
    }
}