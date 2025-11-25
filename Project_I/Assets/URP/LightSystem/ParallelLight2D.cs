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
    public struct ParallelLight2DData
    {
        // 起始轴区间
        public Vector4 SlideInterval;
        // 光颜色和强度
        public Vector4 ColorAndIntensity;
        // 光的方向
        public Vector4 Direction;
    }
    
    [ExecuteAlways]
    public class ParallelLight2D : MonoBehaviour
    {
        [LabelText("强度")][ShowInInspector]
        public float intensity;
        
        [LabelText("颜色")][ShowInInspector]
        public Color color = Color.white;
        
        
        
        [LabelText("光源方向")] [ShowInInspector] [ReadOnly]
        public float Direction
        {
            get => transform.rotation.eulerAngles.z;
        }
        [LabelText("衰减")]
        [Range(0.001f, 0.999f)][ShowInInspector]
        public float fallOffStrength;

        [LabelText("锚点")]
        [ShowInInspector]
        [ReadOnly]
        public Vector2 Anchor
        {
            get => transform.position;
        }

        [LabelText("滑动轴方向")]
        [ShowInInspector]
        [ReadOnly]
        public Vector2 SlideDirection
        {
            get => (transform.up).normalized;
        }

        private Vector2 slideInterval;

        [LabelText("滑动轴方向")]
        [ShowInInspector]
        [ReadOnly]
        public Vector2 SlideInterval
        {
            get => slideInterval;
        }
        
        [LabelText("滑动轴端点")]
        [ShowInInspector]
        [ReadOnly]
        public Vector4 SlidePoints
        {
            get
            {
                Vector2 startPos = Anchor + SlideInterval.x * SlideDirection;
                Vector2 endPos = Anchor + SlideInterval.y * SlideDirection;
                return new Vector4(startPos.x, startPos.y, endPos.x, endPos.y);
            }
        }
        
        private Camera mainCamera;
        private bool registered = false;
        
        private void OnEnable()
        {
            Register();
            mainCamera =  Camera.main;
        }

        private void OnDisable()
        {
            Unregister();
        }

        private void Register()
        {
            if (!registered && LightSystemManager.Instance != null)
            {
                LightSystemManager.Instance?.RegisterParallelLight(this);
                registered = true;
            }
        }

        private void Unregister()
        {
            if (registered && LightSystemManager.Instance != null)
            {
                LightSystemManager.Instance?.UnregisterParallelLight(this);
                registered = false;
            }
        }

        // 重整为 FLOAT4 对齐的结构化数据
        public ParallelLight2DData GetStructedData()
        {
            ParallelLight2DData ans;
            ans.ColorAndIntensity = new Vector4(color.r, color.g, color.b, intensity);
            ans.SlideInterval = SlidePoints;
            ans.Direction = new Vector4(Direction, 0, 0, 0);
            return ans;
        }

        private void Update()
        {
            UpdateSlideIntervalByCamera();
        }


        private void UpdateSlideIntervalByCamera()
        {
            // 根据摄像机矩形计算端点位置
            // 左下角点
            Vector2 cameraPos = mainCamera.transform.position;
            float cameraHalfHeight = mainCamera.orthographicSize;
            float cameraWidth =  mainCamera.aspect * cameraHalfHeight;
            float[] tmpT =  new float[4];
            tmpT[0] = CalculateProjectT(cameraPos + new Vector2(+cameraWidth, +cameraHalfHeight));
            tmpT[1] = CalculateProjectT(cameraPos + new Vector2(+cameraWidth, -cameraHalfHeight));
            tmpT[2] = CalculateProjectT(cameraPos + new Vector2(-cameraWidth, +cameraHalfHeight));
            tmpT[3] = CalculateProjectT(cameraPos + new Vector2(-cameraWidth, -cameraHalfHeight));
            
            slideInterval.x = tmpT[0];
            slideInterval.y = tmpT[0];
            foreach (var t in tmpT)
            {
                if (t < slideInterval.x)
                    slideInterval.x = t;
                if (t > slideInterval.y)
                    slideInterval.y = t;
            }

            // 留余量
            slideInterval.x -= 3.0f;
            slideInterval.y += 3.0f;
        }

        /// <summary>
        /// 计算任意一个点，在slider直线上的投影对应的T
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private float CalculateProjectT(Vector2 point)
        {
            Vector2 dir = point - Anchor;
            return Vector2.Dot(dir, SlideDirection);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = color;
            Handles.DrawLine(Anchor + slideInterval.x * SlideDirection, Anchor + slideInterval.y * SlideDirection, 5.0f);
        }
#endif
    }
}
