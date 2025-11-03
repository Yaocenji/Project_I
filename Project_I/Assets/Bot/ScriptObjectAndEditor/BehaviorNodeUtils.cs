using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project_I.Bot
{
    // 为节点添加一些属性：
    // 节点名
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NodeNameAttribute : Attribute
    {
        public string DisplayName { get; }

        public NodeNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
    // 节点的最大子节点数量
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class NodeChildLimitAttribute : Attribute
    {
        public int MaxChildren { get; }
        public int MinChildren { get; }

        public NodeChildLimitAttribute(int maxChildren, int minChildren = 0)
        {
            MaxChildren = maxChildren;
            MinChildren = minChildren;
        }
    }
    
    
    public static class BehaviorNodeUtils
    {
        public static int GetMaxChildCount(Type nodeType)
        {
            var attr = nodeType.GetCustomAttributes(typeof(NodeChildLimitAttribute), false)
                .FirstOrDefault() as NodeChildLimitAttribute;

            return attr?.MaxChildren ?? int.MaxValue;
        }
        
        
        
        /// <summary>
        /// 获取所有行为树节点基类的派生类 NodeName属性 中文别名
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllBehaviorNodeNames()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(BehaviorTreeNode).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t =>
                {
                    var attr = t.GetCustomAttributes(typeof(NodeNameAttribute), false)
                        .FirstOrDefault() as NodeNameAttribute;
                    return attr != null ? attr.DisplayName : t.Name;
                })
                .ToArray();
        }
        
        /// <summary>
        /// 获取所有行为树节点基类的派生类类名
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllBehaviorNodeTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(BehaviorTreeNode).IsAssignableFrom(t) && !t.IsAbstract)
                .Select(t =>
                {
                    var attr = t.GetCustomAttributes(typeof(NodeNameAttribute), false)
                        .FirstOrDefault() as NodeNameAttribute;
                    return attr != null ? attr.DisplayName : t.Name;
                })
                .ToArray();
        }
        
        /// <summary>
        /// 获取所有行为树节点基类的派生类的 中文别名 到 类名 的映射
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GetNodeDisplayNameMap()
        {
            var map = new Dictionary<string, string>();

            var nodeTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(BehaviorTreeNode).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var type in nodeTypes)
            {
                // 获取别名特性
                var attr = type.GetCustomAttributes(typeof(NodeNameAttribute), false)
                    .FirstOrDefault() as NodeNameAttribute;

                string displayName = attr != null ? attr.DisplayName : type.Name;
                string className = type.AssemblyQualifiedName; // 或 t.FullName

                // 防止重名（不同类别名相同）
                if (!map.ContainsKey(displayName))
                    map.Add(displayName, className);
                else
                    UnityEngine.Debug.LogWarning($"重复的节点别名：{displayName}");
            }

            return map;
        }
    }
}