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
            /*if (_graphView != null)
            {
                rootVisualElement.Remove(_graphView);
            }*/
            
            // --- THE CRITICAL FIX IS HERE ---
            // When the window is closed, we must clean up everything.
            if (_graphView != null)
            {
                // 1. Remove the graph view from the window's visual tree.
                rootVisualElement.Remove(_graphView);

                // 2. IMPORTANT: Null out our reference to the graph view.
                // This ensures that OnEnable will start with a completely
                // fresh state and prevents any old event listeners from lingering.
                _graphView = null;
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

            // --- BUGFIX: 采用更健壮的事件处理方式 ---
            // 1. 在进行任何操作之前，先彻底清空所有旧的事件处理函数。
            //    这可以防止任何从上一个窗口会话中残留的"僵尸"订阅。
            graphViewChanged = null; 

            // 2. 删除所有旧的UI元素。因为我们已经清空了事件，所以这个操作不会触发任何回调。
            DeleteElements(graphElements);
            // ---------------------------------------------

            // (处理旧资产和新资产的逻辑保持不变)
            if (_treeConfig.nodes.Count == 0 && _treeConfig.rootNode != null)
            {
                CollectNodesRecursively(_treeConfig.rootNode, _treeConfig.nodes);
                EditorUtility.SetDirty(_treeConfig);
            }
            if (_treeConfig.nodes.Count == 0)
            {
                _treeConfig.CreateNode("组合|序列器");
                EditorUtility.SetDirty(_treeConfig);
            }
            
            _treeConfig.nodes.ForEach(CreateNodeView);

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
            
            // --- BUGFIX: 在所有UI都构建完毕后，再重新订阅事件处理函数 ---
            // 这样可以确保只有用户之后的操作才会触发它。
            graphViewChanged += OnGraphViewChanged;
            // ---------------------------------------------
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
            // REMOVED: EditorUtility.SetDirty(_treeConfig);
            // This call is redundant because the DeleteNode, AddChild, etc.
            // methods inside BehaviorTreeConfig already call SetDirty.
            // EditorUtility.SetDirty(_treeConfig);
            
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
            if (_treeConfig is null) return;
            var nodeConfig = _treeConfig.CreateNode(nodeName); // 使用config中的方法创建
            nodeConfig.position = position;
            CreateNodeView(nodeConfig);
        }

        private void CreateNodeView(BehaviorNodeConfig nodeConfig)
        {
            var nodeView = new BehaviorTreeNodeView(nodeConfig, GetMaxChildrenForNode(nodeConfig.typeName));
            nodeView.SetPosition(new Rect(nodeConfig.position, new Vector2(200, 150)));
            
            nodeView.userData = _treeConfig;
            
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

        // 用于存放参数UI的容器
        private VisualElement parametersContainer;
        
        public BehaviorTreeNodeView(BehaviorNodeConfig config, int maxChildren) : base()
        {
            this.NodeConfig = config;
            this.title = config.typeName.Split('|').Last();
            this.viewDataKey = config.guid;
            
            style.width = 250; // 让节点宽一点以容纳参数
            
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
            
            
            // --- 新增: 初始化并绘制参数UI ---
            parametersContainer = new VisualElement();
            mainContainer.Add(parametersContainer);
            
            var addButton = new Button(() => {
                string newKey = $"Param{NodeConfig.parameters.Count + 1}";
                foreach (var param in NodeConfig.parameters)
                {
                    if (param.Key == newKey)
                        return;
                }
                NodeConfig.parameters.Add(new BehaviorNodeParameter(newKey, "default_value"));
                DrawParametersUI(); // 重新绘制
            }) { text = "添加参数" };
            mainContainer.Add(addButton);

            DrawParametersUI();
        }
        
        
        // --- 新增: 绘制参数UI的方法 ---
        private void DrawParametersUI()
        {
            parametersContainer.Clear();

            var toRemove = new List<string>();

            foreach (var param in NodeConfig.parameters)
            {
                var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center } };

                var keyField = new TextField { value = param.Key, style = { width = 100 } };
                var valueField = new TextField { value = param.Value, style = { flexGrow = 1 } };
                var removeButton = new Button(() => {
                    toRemove.Add(param.Key);
                }) { text = "X", style = { width = 20, height = 20 } };

                // 当值改变时更新字典
                // 注意: 直接修改字典key很复杂，通常不推荐。这里我们只允许修改value。
                valueField.RegisterValueChangedCallback(evt => {
                    for (int i = 0; i < NodeConfig.parameters.Count; i++)
                    {
                        if (NodeConfig.parameters[i].Key == param.Key)
                        {
                            NodeConfig.parameters[i] = new BehaviorNodeParameter(NodeConfig.parameters[i].Key, evt.newValue);
                        }
                    }
                    // NodeConfig.parameters[param.Key] = evt.newValue;
                    EditorUtility.SetDirty((UnityEngine.Object)this.userData); // 标记资产已修改
                });

                row.Add(keyField);
                row.Add(valueField);
                row.Add(removeButton);
                parametersContainer.Add(row);
            }

            // 延迟移除，避免在遍历时修改集合
            if (toRemove.Any())
            {
                List<int> toRemoveIdxs = new List<int>();
                foreach (var key in toRemove)
                {
                    toRemoveIdxs.Add(NodeConfig.parameters.FindIndex(param => param.Key == key));
                }

                foreach (var idx in toRemoveIdxs)
                {
                    NodeConfig.parameters.RemoveAt(idx);
                }
                DrawParametersUI(); // 再次重绘以反映删除
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