// --- 文件: BehaviorTreeEditorWindow.cs ---

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Project_I.Bot
{
    public class BehaviorTreeGraphWindow : EditorWindow
    {
        private BehaviorTreeView _graphView;
        private BehaviorTreeConfig _treeConfig;

        public static void ShowWindow(BehaviorTreeConfig treeConfig)
        {
            var window = GetWindow<BehaviorTreeGraphWindow>();
            window.titleContent = new GUIContent($"{treeConfig.name} (行为树编辑器)");
            window.minSize = new Vector2(800, 600);
            window.SetTree(treeConfig);
            
            
        }

        private void SetTree(BehaviorTreeConfig treeConfig)
        {
            _treeConfig = treeConfig;
            if (_graphView != null)
            {
                _graphView.PopulateView(_treeConfig);
            }
        }

        private void OnEnable()
        {
            _graphView = new BehaviorTreeView(this)
            {
                style = { flexGrow = 1 }
            };
            
            var styleSheet = EditorGUIUtility.Load("BehaviorTreeEditor.uss") as StyleSheet;
            if (styleSheet is not null)
            {
                _graphView.styleSheets.Add(styleSheet);
            }

            rootVisualElement.Add(_graphView);

            if (_treeConfig != null)
            {
                _graphView.PopulateView(_treeConfig);
            }
        }

        private void OnDisable()
        {
            if (_graphView != null)
            {
                rootVisualElement.Remove(_graphView);
            }
        }
    }

    public class BehaviorTreeView : GraphView
    {
        private BehaviorTreeConfig _treeConfig;
        private Dictionary<string, string> _nodeNameMap;

        public BehaviorTreeView(EditorWindow editorWindow)
        {
            Insert(0, new GridBackground());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            _nodeNameMap = BehaviorNodeUtils.GetNodeDisplayNameMap();
            graphViewChanged = OnGraphViewChanged;
        }

        // 核心：根据ScriptableObject数据填充视图 (已重写并修复)
        public void PopulateView(BehaviorTreeConfig config)
        {
            _treeConfig = config;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            // BUGFIX 1: 处理使用旧数据结构的资产 (nodes列表为空但rootNode有数据)
            // 如果这是一个旧资产，我们需要遍历它的子节点并填充到新的nodes列表中。
            if (_treeConfig.nodes.Count == 0 && _treeConfig.rootNode != null)
            {
                // 使用递归函数从rootNode开始收集所有节点
                CollectNodesRecursively(_treeConfig.rootNode, _treeConfig.nodes);
                EditorUtility.SetDirty(_treeConfig); // 保存升级后的数据
            }

            // BUGFIX 2: 处理全新的、完全空白的资产
            // 如果在升级后，节点列表仍然为空，为用户创建一个默认的根节点。
            if (_treeConfig.nodes.Count == 0)
            {
                // 创建一个节点，CreateNode方法会自动将其设为rootNode并添加到nodes列表
                _treeConfig.CreateNode("组合|序列器"); 
                EditorUtility.SetDirty(_treeConfig);
            }
            
            // --- 原始逻辑开始 ---

            // 1. 创建所有节点的视图 (现在nodes列表肯定有数据了)
            _treeConfig.nodes.ForEach(CreateNodeView);

            // 2. 创建所有节点之间的连线
            _treeConfig.nodes.ForEach(nodeConfig =>
            {
                var parentView = FindNodeView(nodeConfig);
                nodeConfig.children.ForEach(childConfig =>
                {
                    var childView = FindNodeView(childConfig);
                    if (parentView != null && childView != null)
                    {
                        Edge edge = parentView.OutputPort.ConnectTo(childView.InputPort);
                        AddElement(edge);
                    }
                });
            });
        }
        
        /// <summary>
        /// 递归地将节点及其所有子节点添加到一个列表中
        /// </summary>
        private void CollectNodesRecursively(BehaviorNodeConfig nodeConfig, List<BehaviorNodeConfig> allNodes)
        {
            if (nodeConfig == null || allNodes.Contains(nodeConfig))
            {
                return;
            }
            
            allNodes.Add(nodeConfig);

            foreach (var child in nodeConfig.children)
            {
                CollectNodesRecursively(child, allNodes);
            }
        }
        
        
        
        private BehaviorTreeNodeView FindNodeView(BehaviorNodeConfig nodeConfig)
        {
            return GetNodeByGuid(nodeConfig.guid) as BehaviorTreeNodeView;
        }

        // 处理视图变化，用于同步数据 (已重写)
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    if (element is BehaviorTreeNodeView nodeView)
                    {
                        _treeConfig.DeleteNode(nodeView.NodeConfig);
                    }
                    if (element is Edge edge)
                    {
                        var parentView = edge.output.node as BehaviorTreeNodeView;
                        var childView = edge.input.node as BehaviorTreeNodeView;
                        _treeConfig.RemoveChild(parentView?.NodeConfig, childView?.NodeConfig);
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (var edge in graphViewChange.edgesToCreate)
                {
                    var parentView = edge.output.node as BehaviorTreeNodeView;
                    var childView = edge.input.node as BehaviorTreeNodeView;
                    _treeConfig.AddChild(parentView?.NodeConfig, childView?.NodeConfig);
                }
            }
            
            // 标记资产为已修改状态
            EditorUtility.SetDirty(_treeConfig);
            
            return graphViewChange;
        }
        
        // 右键菜单
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // 在节点上右键
            if (evt.target is BehaviorTreeNodeView nodeView)
            {
                evt.menu.AppendAction("设为根节点", (a) =>
                {
                    _treeConfig.rootNode = nodeView.NodeConfig;
                    // 刷新所有节点的样式
                    nodes.ForEach(n => (n as BehaviorTreeNodeView)?.UpdateRootStyle());
                    EditorUtility.SetDirty(_treeConfig);
                });
                evt.menu.AppendSeparator();
            }

            // 在空白处右键
            var nodeTypes = BehaviorNodeUtils.GetAllBehaviorNodeNames();
            foreach (var nodeType in nodeTypes)
            {
                evt.menu.AppendAction($"新建/{nodeType}", (a) =>
                {
                    CreateNode(nodeType, contentViewContainer.WorldToLocal(a.eventInfo.mousePosition));
                });
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node).ToList();
        }

        private void CreateNode(string nodeName, Vector2 position)
        {
            if (_treeConfig == null) return;
            var nodeConfig = _treeConfig.CreateNode(nodeName); // 使用config中的方法创建
            nodeConfig.position = position;
            CreateNodeView(nodeConfig);
        }

        private void CreateNodeView(BehaviorNodeConfig nodeConfig)
        {
            var nodeView = new BehaviorTreeNodeView(nodeConfig, GetMaxChildrenForNode(nodeConfig.typeName));
            nodeView.SetPosition(new Rect(nodeConfig.position, new Vector2(200, 150)));
            
            // 检查并设置根节点样式
            if (_treeConfig.rootNode == nodeConfig)
            {
                nodeView.AddToClassList("root");
            }

            nodeView.OnNodeConfigMoved = () => EditorUtility.SetDirty(_treeConfig);
            AddElement(nodeView);
        }

        private int GetMaxChildrenForNode(string nodeDisplayName)
        {
            if (_nodeNameMap.TryGetValue(nodeDisplayName, out string assemblyQualifiedName))
            {
                Type type = Type.GetType(assemblyQualifiedName);
                if (type != null)
                {
                    return BehaviorNodeUtils.GetMaxChildCount(type);
                }
            }
            return int.MaxValue;
        }
    }

    public class BehaviorTreeNodeView : Node
    {
        public BehaviorNodeConfig NodeConfig { get; }
        public Port InputPort { get; private set; }
        public Port OutputPort { get; private set; }
        public Action OnNodeConfigMoved;

        public BehaviorTreeNodeView(BehaviorNodeConfig config, int maxChildren) : base()
        {
            this.NodeConfig = config;
            this.title = config.typeName.Split('|').Last();
            this.viewDataKey = config.guid;
            
            string styleClass = GetStyleClass(config.typeName);
            AddToClassList(styleClass);

            if (styleClass != "root")
            {
                InputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                InputPort.portName = "In";
                inputContainer.Add(InputPort);
            }

            if (maxChildren > 0)
            {
                var capacity = maxChildren == 1 ? Port.Capacity.Single : Port.Capacity.Multi;
                OutputPort = InstantiatePort(Orientation.Vertical, Direction.Output, capacity, typeof(bool));
                OutputPort.portName = "Out";
                outputContainer.Add(OutputPort);
            }
        }
        
        public void UpdateRootStyle()
        {
            // 这是一个变通方法，因为直接访问_treeConfig比较麻烦
            // 简单地移除样式，让PopulateView或右键菜单去重新添加
            RemoveFromClassList("root");
        }
        
        private string GetStyleClass(string typeName)
        {
            if (typeName.StartsWith("组合")) return "composite";
            if (typeName.StartsWith("修饰")) return "decorator";
            if (typeName.StartsWith("条件")) return "condition";
            if (typeName.StartsWith("动作")) return "action";
            return "node-base";
        }
        
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            NodeConfig.position.x = newPos.xMin;
            NodeConfig.position.y = newPos.yMin;
            OnNodeConfigMoved?.Invoke(); // 通知GraphView数据已改变
        }
    }

    // 不再需要 BehaviorTreeConfigExtensions
}

#endif