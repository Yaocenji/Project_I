using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using Unity.Mathematics;
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
        public List<Vector4> outline;
        
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
            outline = new List<Vector4>();
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

        public void GenerateOutline(bool useTransform)
        {
            var sprite = GetComponent<SpriteRenderer>().sprite;
            outline.Clear();
            var tmpOutlineData = SpriteOutlineExtractor.ExtractOutline(sprite);
            // Debug.Log(gameObject.name + " 轮廓线数： " + tmpOutlineData.Length);
            if (useTransform)
            {
                // 获取二维矩阵
                Vector4 matrix2x2 = Vector4.zero;    // 矩阵行存储
                Vector2 right = transform.right.normalized * transform.lossyScale.x;
                Vector2 up = transform.up.normalized * transform.lossyScale.y;
                matrix2x2 = new Vector4(right.x, up.x, right.y, up.y);
                // 获取平移向量
                Vector2 posMove = transform.position;

                for (int i = 0; i < tmpOutlineData.Length; i++)
                {
                    var edge = tmpOutlineData[i];

                    // 边的起终点
                    Vector2 start = new Vector2(edge.x, edge.y);
                    Vector2 end = new Vector2(edge.z, edge.w);
                    // 边的长度
                    float edgeLen = Vector2.Distance(start, end);
                    // 分段的长度
                    float splitLen = ShadowCasterManager.Instance.cellSize * 2.0f;
                    // 分出的段数
                    int split = (int)Mathf.Ceil(edgeLen / splitLen);
                    // 步数
                    Vector2 step = (end - start).normalized * splitLen;

                    for (int j = 0; j < split; j++)
                    {
                        Vector2 splitStart = start + j * step;
                        Vector2 splitEnd = splitStart + step * 1.02f;
                        if (j == split - 1)
                            splitEnd = end;
                        
                        // 将2x2矩阵左乘到每个顶点的二维列向量
                        Vector4 newEdge = Vector4.zero;
                        newEdge.x = matrix2x2.x * splitStart.x + matrix2x2.y * splitStart.y;
                        newEdge.y = matrix2x2.z * splitStart.x + matrix2x2.w * splitStart.y;
                        newEdge.z = matrix2x2.x * splitEnd.x + matrix2x2.y * splitEnd.y;
                        newEdge.w = matrix2x2.z * splitEnd.x + matrix2x2.w * splitEnd.y;
                        // 添加平移
                        newEdge.x += posMove.x;
                        newEdge.y += posMove.y;
                        newEdge.z += posMove.x;
                        newEdge.w += posMove.y;
                        // 写回数据
                        outline.Add(newEdge);
                    }
                    
                }
            }
            // Debug.Log(gameObject.name + " 分段后的轮廓线数： " + outline.Count);
        }

        private void SetId2Mpb()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.GetPropertyBlock(GetMpb);
            
            // 用SetInt传进去会被重新按值转换，所以强转成float传进去
            GetMpb.SetInt("shadowPolygonID", (int)scID);
            
            // Debug.Log("bitFloatID: " + bitFloatID + "\nobjId: " + scID);
            
            spriteRenderer.SetPropertyBlock(GetMpb);
        }

        // Update is called once per frame
        void Update()
        {
            // GenerateOutline(true);
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
