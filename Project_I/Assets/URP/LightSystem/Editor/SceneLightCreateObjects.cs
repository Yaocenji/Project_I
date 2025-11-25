using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace Project_I.LightSystem
{
    public class MyAwesomeScriptMenuItems
    {
        // "GameObject/..." 是菜单路径，后面是你的菜单项名称
        [MenuItem("Project_I 自定义光照系统/点光源")]
        private static void CreateSpotLight2DObject()
        {
            // 创建一个新的 GameObject
            GameObject go = new GameObject("Spot Light 2D P_I");

            // 为这个 GameObject 挂载你的 MonoBehaviour 脚本
            go.AddComponent<SpotLight2D>();

            // (可选) 确保新创建的对象被选中
            Selection.activeObject = go;
        }
        
        [MenuItem("Project_I 自定义光照系统/全局光源")]
        private static void CreateGlobalLight2DObject()
        {
            // 创建一个新的 GameObject
            GameObject go = new GameObject("Global Light 2D P_I");

            // 为这个 GameObject 挂载你的 MonoBehaviour 脚本
            go.AddComponent<GlobalLight2D>();

            // (可选) 确保新创建的对象被选中
            Selection.activeObject = go;
        }
    }
}

#endif