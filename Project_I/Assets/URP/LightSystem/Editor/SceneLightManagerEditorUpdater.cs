#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project_I.LightSystem
{
    [InitializeOnLoad]
    public static  class SceneLightManagerEditorUpdater
    {
        /// <summary>
        /// 用于在编辑器中实时更新 SceneLightManager 的 Compute Buffer 数据
        /// </summary>
        static SceneLightManagerEditorUpdater() {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView view) {
            if (LightSystemManager.Instance != null) {
                LightSystemManager.Instance.UpdateLightsInEditor();
            }
        }
    }
}

#endif
