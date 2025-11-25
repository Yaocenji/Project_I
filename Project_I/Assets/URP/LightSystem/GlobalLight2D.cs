using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Project_I.LightSystem
{
    public struct GlobalLight2DData
    {
        // 光颜色和强度
        public Vector4 ColorAndIntensity;
    }
    
    [ExecuteAlways]
    public class GlobalLight2D : MonoBehaviour
    {
        [LabelText("强度")][ShowInInspector]
        public float intensity;
        
        [LabelText("颜色")][ShowInInspector]
        public Color color = Color.white;
        
        private bool registered = false;
        
        private void OnEnable()
        {
            Register();
        }

        private void OnDisable()
        {
            Unregister();
        }

        private void Register()
        {
            if (!registered && LightSystemManager.Instance != null)
            {
                LightSystemManager.Instance?.RegisterGlobalLight(this);
                registered = true;
            }
        }

        private void Unregister()
        {
            if (registered && LightSystemManager.Instance != null)
            {
                LightSystemManager.Instance?.UnregisterGlobalLight(this);
                registered = false;
            }
        }

        // 重整为 FLOAT4 对齐的结构化数据
        public GlobalLight2DData GetStructedData()
        {
            GlobalLight2DData ans;
            ans.ColorAndIntensity = new Vector4(color.r, color.g, color.b, intensity);

            return ans;
        }
        
        
    }
}
