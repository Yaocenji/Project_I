using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Project_I.LightSystem
{
    public struct ShadowCasterData
    {
        public Vector4[] Polygon;
        public Vector4 Rotate_Scele_2x2_Matrix;
        public Vector2 PositionVector;
    }
    
    public struct PolygonEdge
    {
        public Vector4 Edge;
        public uint Id;
    }
    
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ShadowCaster : MonoBehaviour
    {
        [LabelText("是否显示外轮廓")] public bool drawOutline = false;
        
        [HideInInspector]
        public Vector4[] outline;
        
        private bool registered = false;

        [HideInInspector]
        public uint scID = 0;
        
        private SpriteRenderer spriteRenderer;
        private MaterialPropertyBlock mpb;
        private MaterialPropertyBlock GetMpb
        {
            get
            {
                if (mpb == null)
                    mpb = new MaterialPropertyBlock();
                return mpb;
            }
        }
        
        void OnEnable()
        {
            GenerateOutline(true);
            Register();
            
            SetId2Mpb();
        }

        private void OnDisable()
        {
            Unregister();
        }

        private void Register()
        {
            if (!registered && ShadowCasterManager.Instance != null)
            {
                ShadowCasterManager.Instance?.RegisterShadowedPoligon(this);
                registered = true;
            }
        }

        private void Unregister()
        {
            if (registered && ShadowCasterManager.Instance != null)
            {
                ShadowCasterManager.Instance?.UnregisterShadowedPoligon(this);
                registered = false;
            }
        }

        private void GenerateOutline(bool useTransform)
        {
            var sprite = GetComponent<SpriteRenderer>().sprite;
            outline = SpriteOutlineExtractor.ExtractOutline(sprite);
            Debug.Log(gameObject.name + " 轮廓线数： " + outline.Length);
            if (useTransform)
            {
                // 获取二维矩阵
                Vector4 matrix2x2 = Vector4.zero;    // 矩阵行存储
                Vector2 right = transform.right.normalized * transform.localScale.x;
                Vector2 up = transform.up.normalized * transform.localScale.y;
                matrix2x2 = new Vector4(right.x, up.x, right.y, up.y);
                // 获取平移向量
                Vector2 posMove = transform.position;

                for (int i = 0; i < outline.Length; i++)
                {
                    var edge = outline[i];
                    // 将2x2矩阵左乘到每个顶点的二维列向量
                    Vector4 newEdge = Vector4.zero;
                    newEdge.x = matrix2x2.x * edge.x + matrix2x2.y * edge.y;
                    newEdge.y = matrix2x2.z * edge.x + matrix2x2.w * edge.y;
                    newEdge.z = matrix2x2.x * edge.z + matrix2x2.y * edge.w;
                    newEdge.w = matrix2x2.z * edge.z + matrix2x2.w * edge.w;
                    // 添加平移
                    newEdge.x += posMove.x;
                    newEdge.y += posMove.y;
                    newEdge.z += posMove.x;
                    newEdge.w += posMove.y;
                    // 写回数据
                    outline[i] = newEdge;
                }
            }
        }

        private void SetId2Mpb()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.GetPropertyBlock(GetMpb);
            
            // 用SetInt传进去会被重新按值转换，所以强转成float传进去
            GetMpb.SetInt("objId", (int)scID);
            
            // Debug.Log("bitFloatID: " + bitFloatID + "\nobjId: " + scID);
            
            spriteRenderer.SetPropertyBlock(GetMpb);
        }

        // Update is called once per frame
        void Update()
        {
            GenerateOutline(true);
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (outline == null)
                return;
            
            if (drawOutline)
            {
                foreach (var edge in outline)
                {
                    Gizmos.color = Color.blue;
                    Vector3 start = new Vector3(edge.x, edge.y, 0);
                    Vector3 end = new Vector3(edge.z, edge.w, 0);
                    Gizmos.DrawLine(start, end);

                    Vector3 close2End = (start + end * 4) / 5.0f;
                    Gizmos.DrawCube(close2End, new Vector3(0.03f, 0.03f, 0.03f));
                }
            }
        }
        #endif
    }
}
