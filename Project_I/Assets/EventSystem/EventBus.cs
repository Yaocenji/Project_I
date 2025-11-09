using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Project_I.EventSystem
{
    public static class EventBus
    {
        // 存储每种事件类型及其对应的监听列表
        private static readonly Dictionary<Type, List<Delegate>> listeners = new();

        // 注册事件监听
        public static void Subscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (!listeners.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                listeners[type] = list;
            }

            list.Add(callback);
        }

        // 取消事件监听
        public static void Unsubscribe<T>(Action<T> callback)
        {
            var type = typeof(T);
            if (listeners.TryGetValue(type, out var list))
            {
                list.Remove(callback);
                if (list.Count == 0)
                    listeners.Remove(type);
            }
        }

        // 广播事件
        public static void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (listeners.TryGetValue(type, out var list))
            {
                // 复制一份，防止广播期间修改
                var tmpList = list.ToArray();
                foreach (var callback in tmpList)
                    (callback as Action<T>)?.Invoke(eventData);
            }
        }

        // 清空所有监听（可选）
        public static void Clear() => listeners.Clear();
    }
}

