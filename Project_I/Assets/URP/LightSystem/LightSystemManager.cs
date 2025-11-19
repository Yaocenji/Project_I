using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Project_I.LightSystem
{
    [ExecuteAlways]
    public class LightSystemManager : MonoBehaviour
    {
        [HideInInspector]
        public static LightSystemManager Instance;

        [LabelText("点光源最大数量限制")]
        public const int MAX_SPOTLIGHT_COUNT = 128;

        
        // 有阴影点光源链表
        private LinkedList<SpotLight2D> spotLightsShadowed;
        public int spotLightShadowedCount =>  spotLightsShadowed.Count;
        
        // 无阴影点光源链表
        private LinkedList<SpotLight2D> spotLightsNoShadowed;
        public int spotLightNoShadowedCount =>  spotLightsShadowed.Count;

        // 有阴影点光源Compute Buffer
        private ComputeBuffer spotLights_Shadowed_Data_Buffer;
        // 显存id
        // private int spotLights_Shadowed_Data_Buffer_IDX;
        
        // 无阴影点光源Compute Buffer
        private ComputeBuffer spotLights_NoShadowed_Data_Buffer;
        // 显存id
        // private int spotLights_NoShadowed_Data_Buffer_IDX;
        
        // 阴影caster网格列表
        private Dictionary<ShadowCaster, ShadowCasterData> shadowCasterData;

        
        // init
        private bool initialized = false;

        #if UNITY_EDITOR
        [LabelText("是否自动刷新光照渲染缓存")]
        public bool autoUpdate = false;
        [Button("刷新光照渲染缓存")]
        private void ManualUpdateComputeBuffer()
        {
            UpdateComputeBuffers(true);
        }

        private bool editorEnabled = true;
        #endif
        
        private void Awake()
        {
            Instance = this;

            Init();
        }

        #if UNITY_EDITOR
        private void OnEnable()
        {
            Instance = this;

            Init();
        }
        #endif

        private void OnDisable()
        {
            Uninit();
        }
        
        private void Init()
        {
            if (initialized)
                return;
            
            spotLightsShadowed = new LinkedList<SpotLight2D>();
            spotLightsNoShadowed = new LinkedList<SpotLight2D>();
            
            spotLights_Shadowed_Data_Buffer = new ComputeBuffer(MAX_SPOTLIGHT_COUNT, Marshal.SizeOf(new SpotLight2DData()), ComputeBufferType.Structured);
            spotLights_NoShadowed_Data_Buffer = new ComputeBuffer(MAX_SPOTLIGHT_COUNT, Marshal.SizeOf(new SpotLight2DData()), ComputeBufferType.Structured);
            
            shadowCasterData = new Dictionary<ShadowCaster, ShadowCasterData>();
            
            
            initialized = true;
        }

        private void Uninit()
        {
            if (!initialized)
                return;
            
            if (spotLightsShadowed != null)
                spotLightsShadowed.Clear();
            if (spotLightsNoShadowed != null)
                spotLightsNoShadowed.Clear();
            
            spotLightsShadowed = null;
            spotLightsNoShadowed = null;
            
            spotLights_Shadowed_Data_Buffer.Dispose();
            spotLights_NoShadowed_Data_Buffer.Dispose();
            
            spotLights_Shadowed_Data_Buffer = null;
            spotLights_NoShadowed_Data_Buffer = null;

            shadowCasterData = null;
            
            GC.Collect();
            
            initialized = false;
        }

        public void RegisterSpotLight(SpotLight2D spotLight2D)
        {
            if (!initialized)
                return;
            if (spotLight2D.CastShadows && !spotLightsShadowed.Contains(spotLight2D))
            {
                if (spotLightsShadowed.Count < MAX_SPOTLIGHT_COUNT)
                    spotLightsShadowed.AddLast(spotLight2D);
                else
                    Debug.Log(spotLight2D.gameObject.name + "点光源添加失败：数量达到上限");
            }
            else if (!spotLight2D.CastShadows && !spotLightsNoShadowed.Contains(spotLight2D))
            {
                if (spotLightsNoShadowed.Count < MAX_SPOTLIGHT_COUNT)
                    spotLightsNoShadowed.AddLast(spotLight2D);
                else
                    Debug.Log(spotLight2D.gameObject.name + "点光源添加失败：数量达到上限");
            }
        }

        public void UnregisterSpotLight(SpotLight2D spotLight2D)
        {
            if (!initialized)
                return;
            if (spotLight2D.CastShadows && spotLightsShadowed.Contains(spotLight2D))
            {
                spotLightsShadowed.Remove(spotLight2D);
            }
            else if (!spotLight2D.CastShadows && spotLightsNoShadowed.Contains(spotLight2D))
            {
                spotLightsNoShadowed.Remove(spotLight2D);
            }
        }


        public void RegisterShadowCasterPolygon(ShadowCaster shadowCaster)
        {
            if (shadowCasterData.ContainsKey(shadowCaster))
            {
                
            }
        }
        
        public void UnregisterShadowCasterPolygon(ShadowCaster shadowCaster)
        {
            
        }
        
        // 刷新着色器缓存
        public void UpdateComputeBuffers(bool useDebugData)
        {
            SpotLight2DData[] spotLights_Shadowed_Data = new SpotLight2DData[MAX_SPOTLIGHT_COUNT];
            SpotLight2DData[] spotLights_NoShadowed_Data = new SpotLight2DData[MAX_SPOTLIGHT_COUNT];

            int tmpIdx = 0;
            // 数据全部格式化到 Array
            foreach (var spotLight2D in spotLightsShadowed)
            {
                spotLights_Shadowed_Data[tmpIdx] = spotLight2D.GetStructedData();
                tmpIdx++;
            }

            tmpIdx = 0;
            foreach (var spotLight2D in spotLightsNoShadowed)
            {
                spotLights_NoShadowed_Data[tmpIdx] = spotLight2D.GetStructedData();
                tmpIdx++;
            }
            
            // 传入compute buffer
            spotLights_Shadowed_Data_Buffer.SetData(spotLights_Shadowed_Data);
            spotLights_NoShadowed_Data_Buffer.SetData(spotLights_NoShadowed_Data);
            
            Shader.SetGlobalBuffer("SpotLight2D_Shadowed_Data_Buffer", spotLights_Shadowed_Data_Buffer);
            Shader.SetGlobalBuffer("SpotLight2D_NoShadowed_Data_Buffer", spotLights_NoShadowed_Data_Buffer);
            Shader.SetGlobalInt("_SpotLightShadowedCount", spotLightsShadowed.Count);
            Shader.SetGlobalInt("_SpotLightNoShadowedCount", spotLightsNoShadowed.Count);
            
            
            // Debug
            if (useDebugData)
            {
                Debug.Log("有阴影点光源数据：");
                for (int i = 0; i < spotLightsShadowed.Count; i++)
                {
                    var spotLight2D = spotLights_Shadowed_Data[i];
                    Debug.Log("向量1：" + spotLight2D.Position2DIntensityFallOffStrength +
                              "\n向量2：" + spotLight2D.InnerOutRadiusAndAngle +
                              "\n向量3：" + spotLight2D.ColorAndDirection);
                }

                Debug.Log("无阴影点光源数据：");
                for (int i = 0; i < spotLightsNoShadowed.Count; i++)
                {
                    var spotLight2D = spotLights_NoShadowed_Data[i];
                    Debug.Log("向量1：" + spotLight2D.Position2DIntensityFallOffStrength +
                              "\n向量2：" + spotLight2D.InnerOutRadiusAndAngle +
                              "\n向量3：" + spotLight2D.ColorAndDirection);
                }
            }
        }
        
        #if UNITY_EDITOR
        // 用于editor中的刷新函数
        public void UpdateLightsInEditor() {
            if (!Application.isPlaying && autoUpdate) {
                UpdateComputeBuffers(false);
            }
        }
        #endif


        // Update is called once per frame
        void Update()
        {
            UpdateComputeBuffers(false);
        }
    }
}
