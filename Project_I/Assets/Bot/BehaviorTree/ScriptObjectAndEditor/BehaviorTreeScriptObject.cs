using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

/*#if UNITY_EDITOR
using UnityEditor;
#endif*/

namespace Project_I.Bot
{
    [CreateAssetMenu(fileName = "BehaviorTreeSO", menuName = "BotBehaviorSO/BehaviorTree", order = 0)]
    public class BehaviorTreeConfig : ScriptableObject
    {
        [Tooltip("行为树的根节点。运行时从此节点开始执行。")]
        public BehaviorNodeConfig rootNode; 
        
        [Tooltip("编辑器中存在的所有节点列表。")]
        [HideInInspector]
        public List<BehaviorNodeConfig> nodes = new List<BehaviorNodeConfig>();
        
        
        #if UNITY_EDITOR
        [Button("打开行为树编辑器", ButtonSizes.Large)]
        private void OpenEditorWindow()
        {
            // TODO
            BehaviorTreeGraphWindow.ShowWindow(this);
        }
        
        // === 以下为编辑器使用的数据操作方法 ===

        public BehaviorNodeConfig CreateNode(string nodeTypeName)
        {
            var nodeConfig = new BehaviorNodeConfig
            {
                guid = GUID.Generate().ToString(),
                typeName = nodeTypeName
            };
            nodes.Add(nodeConfig);

            if (rootNode == null)
            {
                rootNode = nodeConfig;
            }
            
            EditorUtility.SetDirty(this);
            return nodeConfig;
        }
        
        public void DeleteNode(BehaviorNodeConfig nodeToDelete)
        {
            if (nodeToDelete == null) return;

            // 从所有节点的子列表中移除它
            foreach (var node in nodes)
            {
                node.children.Remove(nodeToDelete);
            }

            // 从主列表中移除它
            nodes.Remove(nodeToDelete);
            
            // 如果删除的是根节点，将根节点设为null（或列表中的第一个）
            if (rootNode == nodeToDelete)
            {
                rootNode = nodes.FirstOrDefault();
            }

            EditorUtility.SetDirty(this);
        }
        
        public void AddChild(BehaviorNodeConfig parent, BehaviorNodeConfig child)
        {
            if (parent == null || child == null) return;
            
            // 确保它们都在列表中
            if (nodes.Contains(parent) && nodes.Contains(child))
            {
                // --- 防御性修复 ---
                // 在添加子节点前，确保父节点的 children 列表不是 null。
                // 这可以修复从旧资产加载的数据。
                if (parent.children == null)
                {
                    parent.children = new List<BehaviorNodeConfig>();
                }
                // ---------------------
                
                parent.children.Add(child);
                EditorUtility.SetDirty(this);
            }
        }

        public void RemoveChild(BehaviorNodeConfig parent, BehaviorNodeConfig child)
        {
            if (parent == null || child == null) return;

            if (nodes.Contains(parent) && nodes.Contains(child))
            {
                parent.children.Remove(child);
                EditorUtility.SetDirty(this);
            }
        }
        
        #endif
    }

    [Serializable]
    public class BehaviorNodeConfig
    {
        [ValueDropdown("AllNodesTypes")]
        public string typeName; // 自动通过下拉选择，不再手输
        
        public List<BehaviorNodeConfig> children = new List<BehaviorNodeConfig>();
        
        [HideInInspector] public string guid; // 添加GUID
        [HideInInspector] public Vector2 position; // 记录编辑器中位置
        
        public Dictionary<string, string> parameters = new Dictionary<string, string>(); // 节点参数(可选)
        
        // 所有类的中文别名
        private static string[] AllNodesTypes
        {
            get
            {
                return BehaviorNodeUtils.GetAllBehaviorNodeNames();
            }
        }
    }
    
}