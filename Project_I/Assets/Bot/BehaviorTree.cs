using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Project_I.Bot
{
    // 行为树
    public class BehaviorTree : MonoBehaviour
    {
        [Header("行为树配置文件")]
        [Tooltip("指定此敌人使用的行为树 ScriptableObject 配置。")]
        public BehaviorTreeConfig config;
        
        [Header("当前敌人脚本")]
        public BasicEnemy enemy;
        [Header("决策间隔（频率）")]
        public float interval = 0.3f;
        
        // 有些动作节点结束的时候，会改用一次临时interval
        private bool useTemporalInterval = false;
        private float temporalInterval = 0.3f;

        private float lastTime;
        
        private BehaviorTreeNode root;
        
        
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
                // 进行一次决策
                if (root != null)
                {
                    root.Tick();
                }
                // 更新上一Tick发生时间
                lastTime = Time.time;
                
                // 如果当前是使用了临时interval
                // 那么改回不使用
                if (useTemporalInterval)
                    useTemporalInterval = false;
            }
        }

        protected virtual BehaviorTreeNode SetupTree()
        {
            /*BehaviorTreeNode rootNode = new InvertNode();
            rootNode.tree = this;
            
            // cast
            InvertNode realRoot = rootNode as InvertNode;
            realRoot.tree = this;
            realRoot.child = new IfSeePlayer(transform, enemy, 5);

            
            // Debug
            var currType = Type.GetType("Project_I.Bot.InvertNode");
            if (currType != null)
            {
                var thisNode = Activator.CreateInstance(currType);
                Debug.Log(thisNode.GetType().ToString());
            }
            
            return rootNode;*/
            
            // 检查配置文件是否已分配
            if (config is null)
            {
                Debug.LogError("行为树配置文件 (BehaviorTreeConfig) 未在 Inspector 中指定！", this);
                return null;
            }

            // 检查根节点是否存在
            if (config.rootNode is null)
            {
                Debug.LogError("指定的行为树配置文件中没有根节点 (rootNode)！", this);
                return null;
            }

            // 初始化节点类型缓存
            InitializeNodeTypeCache();

            // 从配置文件的根节点开始，递归地创建整个行为树
            return CreateNodeRecursive(config.rootNode);
        }
        
        
        /// <summary>
        /// 初始化节点名称到Type的映射缓存
        /// </summary>
        private void InitializeNodeTypeCache()
        {
            nodeTypeCache = new Dictionary<string, Type>();
            // 从工具类获取所有 "显示名称" -> "完整类名" 的映射
            var displayNameMap = BehaviorNodeUtils.GetNodeDisplayNameMap();
            foreach (var pair in displayNameMap)
            {
                // 使用 Type.GetType 获取对应的类定义
                Type type = Type.GetType(pair.Value);
                if (type != null)
                {
                    nodeTypeCache[pair.Key] = type;
                }
                else
                {
                    Debug.LogWarning($"无法找到节点类型: {pair.Value}");
                }
            }
        }

        /// <summary>
        /// 递归地创建行为树节点实例
        /// </summary>
        /// <param name="nodeConfig">节点的配置数据</param>
        /// <returns>创建好的节点运行时实例</returns>
        private BehaviorTreeNode CreateNodeRecursive(BehaviorNodeConfig nodeConfig)
        {
            if (nodeConfig == null) return null;

            // 从缓存中查找节点类型
            if (!nodeTypeCache.TryGetValue(nodeConfig.typeName, out Type nodeType))
            {
                Debug.LogError($"未知的节点类型: '{nodeConfig.typeName}'");
                return null;
            }

            BehaviorTreeNode instance;

            // --- 特殊节点处理 ---
            // 对于构造函数需要特定参数(如transform, enemy)的节点，需要在此处特殊处理。
            // 这是一个可以扩展的地方，例如可以将参数也序列化到 BehaviorNodeConfig 中。
            // 目前，我们根据你原始代码的例子来硬编码处理 IfSeePlayer。
            if (nodeType == typeof(IfSeePlayer))
            {
                // 假设 'number' 参数为5，如同你原始的硬编码。
                // 更好的做法是将 '5' 这个值也保存在 BehaviorNodeConfig 中。
                instance = new IfSeePlayer(transform, enemy, 5);
            }
            else
            {
                // 对于其他所有节点，我们尝试使用无参构造函数创建实例
                instance = Activator.CreateInstance(nodeType) as BehaviorTreeNode;
            }

            if (instance == null)
            {
                Debug.LogError($"无法创建节点实例: '{nodeConfig.typeName}'");
                return null;
            }

            // 为创建的节点设置其所属的行为树
            instance.tree = this;

            // --- 递归处理子节点 ---
            if (nodeConfig.children != null && nodeConfig.children.Count > 0)
            {
                // 组合节点 (有 children 列表)
                if (instance is SequenceNode seq)
                {
                    nodeConfig.children.ForEach(childConfig => seq.children.Add(CreateNodeRecursive(childConfig)));
                }
                else if (instance is SelectNode sel)
                {
                    nodeConfig.children.ForEach(childConfig => sel.children.Add(CreateNodeRecursive(childConfig)));
                }
                else if (instance is ParallelNode par)
                {
                    nodeConfig.children.ForEach(childConfig => par.children.Add(CreateNodeRecursive(childConfig)));
                }
                // 修饰节点 (只有单个 child)
                else if (instance is InvertNode inv)
                {
                    inv.child = CreateNodeRecursive(nodeConfig.children.FirstOrDefault());
                }
                else if (instance is LoopNode loop)
                {
                    // 注意：LoopNode 的循环次数等参数也未在Config中序列化，此处会使用默认值。
                    loop.child = CreateNodeRecursive(nodeConfig.children.FirstOrDefault());
                }
            }

            return instance;
        }


        public void SetTemporalInterval(float interval)
        {
            useTemporalInterval = true;
            temporalInterval = interval;
        }
    }
}