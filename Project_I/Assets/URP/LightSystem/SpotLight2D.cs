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
    public struct SpotLight2DData
    {
        // 2D位置、光强、衰减强度
        public Vector4 Position2DIntensityFallOffStrength;
        // 内半径、外半径、内角度、外角度
        public Vector4 InnerOutRadiusAndAngle;
        // 光颜色、方向角
        public Vector4 ColorAndDirection;
    }
    
    [ExecuteAlways]
    public class SpotLight2D : MonoBehaviour
    {
        [HideInInspector]
        public bool castShadows = false;
        [LabelText("阴影")][ShowInInspector]
        public bool CastShadows
        {
            set
            {
                if (castShadows != value)
                {
                    Unregister();
                    castShadows = value;
                    Register();
                }
            }
            get => castShadows;
        }
        
        [LabelText("光源位置")][ShowInInspector][ReadOnly]
        public Vector2 Position2D
        {
            get => new (transform.position.x, transform.position.y);
        }
        
        [LabelText("强度")][ShowInInspector]
        public float intensity;
        
        [LabelText("衰减")]
        [Range(0.001f, 0.999f)][ShowInInspector]
        public float fallOffStrength;
        
        [HideInInspector]
        public float innerRadius = 0;
        [HideInInspector]
        public float outerRadius = 1;
        [LabelText("内半径")][ShowInInspector]
        public float InnerRadius
        {
            set
            {
                if (value <= outerRadius && value >= 0)
                    innerRadius = value;
            }
            get => innerRadius;
        }
        [LabelText("外半径")][ShowInInspector]
        public float OuterRadius{
            set
            {
                if (value >= innerRadius)
                    outerRadius = value;
            }
            get => outerRadius;
        }
        
        [LabelText("内外角度")][ShowInInspector]
        [MinMaxSlider(0f, 360f, true)]
        public Vector2 angleRange = new Vector2(360f, 360f);
        public float InnerAngle => angleRange.x;
        public float OuterAngle => angleRange.y;

        [LabelText("颜色")][ShowInInspector]
        public Color color = Color.white;

        [LabelText("光源方向")] [ShowInInspector] [ReadOnly]
        public float Direction
        {
            get => transform.rotation.eulerAngles.z;
        }
        
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
                LightSystemManager.Instance?.RegisterSpotLight(this);
                registered = true;
            }
        }

        private void Unregister()
        {
            if (registered && LightSystemManager.Instance != null)
            {
                LightSystemManager.Instance?.UnregisterSpotLight(this);
                registered = false;
            }
        }

        // 重整为 FLOAT4 对齐的结构化数据
        public SpotLight2DData GetStructedData()
        {
            SpotLight2DData ans;
            ans.Position2DIntensityFallOffStrength = new Vector4(
                Position2D.x, Position2D.y, intensity, fallOffStrength);
            ans.InnerOutRadiusAndAngle = new Vector4(innerRadius, outerRadius, InnerAngle, OuterAngle);
            ans.ColorAndDirection = new Vector4(color.r, color.g, color.b, Direction);

            return ans;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        }
        
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // 设置颜色
            Handles.color = color;
            Gizmos.color = color;

            // 获取中心位置
            Vector3 position = transform.position;

            // 计算扇形的起始角度。我们的朝向是扇形的中心线，所以需要各减去一半的扇形角度。
            float startAngle = Direction - OuterAngle;
        
            // 将角度转换为起始方向向量。在2D中，我们通常绕Z轴旋转。
            // Vector3.up or Vector3.right 都可以作为初始方向，这里使用 Vector3.right。
            Vector3 fromDirection = Quaternion.Euler(0, 0, startAngle) * Vector3.right;

            // 对于2D视图，法线方向应指向Z轴
            Vector3 normal = Vector3.forward;

            // 绘制线框扇形

            // 1. 绘制弧形
            Handles.DrawWireArc(position, normal, fromDirection, OuterAngle * 2, OuterRadius);

            // 2. 绘制两条从圆心出发的边
            Vector3 startEdge = fromDirection * OuterRadius;
            // 通过将起始方向向量旋转扇形角度，得到结束方向向量
            Vector3 endEdge = (Quaternion.Euler(0, 0, OuterAngle * 2) * fromDirection) * OuterRadius;

            Gizmos.DrawLine(position, position + startEdge);
            Gizmos.DrawLine(position, position + endEdge);
            
        }
        #endif
    }
}
