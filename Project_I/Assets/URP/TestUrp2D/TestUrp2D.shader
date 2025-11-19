Shader "Test/TestUrp2D"
{
    Properties
    {
        [MainTexture] _MainTex ("MainTexture", 2D) = "white" {}
        [MainColor]_MainColor ("Main Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        
        //[ToggleOff] _Enable_ShadowCast("_Enable_ShadowCast", Int) = 1

        // //Option Enum
        // [Header(Option)]
        // [Enum(UnityEngine.Rendering.BlendOp)]_BlendOp("BlendOp", Float) = 0.0
        // [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Float) = 1.0
        // [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Float) = 0.0
        // [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlendAlpha("SrcBlendAlpha", Range(0, 1)) = 1.0
        // [Enum(UnityEngine.Rendering.BlendMode)]_DstBlendAlpha("DstBlendAlpha", Range(0, 1)) = 0.0
        // [Header(ZTest)]
        // [ToggleUI]_ZWrite("ZWrite", Float) = 1.0
        // [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4.0
        // [ToggleUI]_ZClip("ZClip", Float) = 1.0
        // [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull", Float) = 2.0
        // [Header(Mask)]
        // //[Enum(UnityEngine.Rendering.ColorWriteMask)]_ColorMask("ColorMask", Float) = 15.0
        // //[ToggleUI]_AlphaToMask("AlphaToMask", Float) = 0.0

        // //Stencil enum
        // [Header(Stencil)]
        // [IntRange]_Stencil ("Stencil ID", Range(0,255)) = 0
        // [IntRange]_StencilWriteMask ("Stencil Write Mask", Range(0,255)) = 255
        // [IntRange]_StencilReadMask ("Stencil Read Mask", Range(0,255)) = 255
        // [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp("StencilComp", Float) = 0.0
        // [Enum(UnityEngine.Rendering.StencilOp)]_StencilOp("StencilOp", Float) = 0.0
        // [Enum(UnityEngine.Rendering.StencilOp)]_StencilOpFail("StencilOpFail", Float) = 0.0
        // [Enum(UnityEngine.Rendering.StencilOp)]_StencilOpZFail("StencilOpZFail", Float) = 0.0
        // [Enum(UnityEngine.Rendering.StencilOp)]_StencilOpZFailFront("StencilOpZFailFront", Float) = 0.0
        // [Enum(UnityEngine.Rendering.CompareFunction)]_StencilCompFront("StencilCompFront", Float) = 0.0
        // [Enum(UnityEngine.Rendering.StencilOp)]_StencilOpFront("StencilOpFront", Float) = 0.0
        // [Enum(UnityEngine.Rendering.CompareFunction)]_StencilCompBack("StencilCompBack", Float) = 0.0
        // [Enum(UnityEngine.Rendering.StencilOp)]_StencilOpBack("StencilOpBack", Float) = 0.0
        // [Enum(UnityEngine.Rendering.StencilOp)]_StencilOpZFailBack("StencilOpZFailBack", Float) = 0.0
        //[HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline"="UniversalPipeline"
            // "RenderType"="Background"
            "RenderType"="Opaque"
            // "RenderType"="Transparent"
            // "RenderType"="TransparentCutout"
            // "RenderType"="Overlay"

            //"Queue" = "Background"
            "Queue"="Geometry"
            //"Queue" = "AlphaTest"
            //"Queue" = "Transparent"
            //"Queue" = "TransparentCutout"
            //"Queue" = "Overlay"
            //"IgnoreProjector" = "True"
        }
        //LOD 100
        Pass
        {
            Tags
			{
				"LightMode"="Universal2D"
			}

            // BlendOp [_BlendOp]
            // Blend [_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
            // ZWrite [_ZWrite]
            // ZTest [_ZTest]
            // ZClip [_ZClip]
            // Cull [_Cull]
            // ColorMask [_ColorMask]
            // AlphaToMask [_AlphaToMask]

            // Stencil
            // {
            //     Ref [_Stencil]
            //     Comp [_StencilComp]
            //     ReadMask [_StencilReadMask]
            //     WriteMask [_StencilWriteMask]
            //     Pass [_StencilOp]
            //     Fail [_StencilOpFail]
            //     ZFail [_StencilOpZFail]
            //     ZFailFront [_StencilOpZFailFront]
            //     CompFront [_StencilCompFront]
            //     PassFront [_StencilOpFront]
            //     CompBack [_StencilCompBack]
            //     PassBack [_StencilOpBack]
            //     ZFailBack [_StencilOpZFailBack]
            // }

            //Geometry
            ZWrite On
            ZTest LEqual
            Cull Back

            ////Transparent
            //ZWrite Off
            //Blend SrcAlpha OneMinusSrcAlpha // 传统透明度
            //Blend One OneMinusSrcAlpha // 预乘透明度
            //Blend OneMinusDstColor One // 软加法
            //Blend DstColor Zero // 正片叠底（相乘）
            //Blend OneMinusDstColor One // 滤色 //柔和叠加（soft Additive）
            //Blend DstColor SrcColor // 2x相乘 (2X Multiply)
            //Blend One One // 线性减淡
            //BlendOp Min Blend One One //变暗
            //BlendOp Max Blend One One //变亮




            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #include "Assets/URP/LightSystem/LightSystemInclude.hlsl"

            

            // 调试画板
            TEXTURE2D(debug_Canvas);
            SAMPLER(samplerdebug_Canvas);
            
            //尽量对齐到float4,否则unity底层会自己填padding来对齐,会有空间浪费
            //Align to float4 as much as possible, otherwise the underlying Unity will fill in padding to align, which will waste space
            CBUFFER_START(UnityPerMaterial)
            int _SpotLightShadowedCount;
            int _SpotLightNoShadowedCount;
            
            half4 _MainColor;
            float _Metallic;
            float _Smoothness;
            float4 _MainTex_ST;

            float cellSize;
            int gridHorizonalNumber;
            int gridVerticalNumber;
            float2 gridZero;

            
            int shadowMapResolution_X;
            
            float4 debug_Canvas_ST;
            CBUFFER_END

            CBUFFER_START(UnityPerObject)
            uint objId;
            CBUFFER_END

            StructuredBuffer<uint> gridCounter;
            StructuredBuffer<uint> blockCounter;

            struct GridEdgeInfo
            {
                uint offset;
                uint count;
                uint writePointer;
            };
            StructuredBuffer<GridEdgeInfo> gridEdgeInfo;
            StructuredBuffer<uint> gridEdgePool;

            ////GPU Instancing 和SRP Batcher冲突 根据需要确定是否开启
            ////GPU Installing and SRP Batcher conflict, determine whether to enable as needed
            // #pragma multi_compile_instancing
            // #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            
            // UNITY_INSTANCING_BUFFER_START(PerInstance)
            // //UNITY_DEFINE_INSTANCED_PROP(float4, _MainColor)
            // UNITY_INSTANCING_BUFFER_END(PerInstance)

            ////接收阴影关键字
            ////Receive shadow keywords
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            // #pragma multi_compile _ _SHADOWS_SOFT

            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                uint objectID : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varings vert (Attributes IN)
            {
                Varings OUT;
                ////GPU Instancing
                // UNITY_SETUP_INSTANCE_ID(IN);
                // UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS.xyz);
                //OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.positionCS = positionInputs.positionCS;
                OUT.positionWS = positionInputs.positionWS;
                OUT.viewDirWS = GetCameraPositionWS() - positionInputs.positionWS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.objectID = objId;
                return OUT;
            }

            half4 frag (Varings IN) : SV_Target
            {
                ////GPU Instancing
                //UNITY_SETUP_INSTANCE_ID(IN);
                //half4 mainColor = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _MainColor);

                //采样纹理
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                float3  lightColorSum = 0;
                UNITY_LOOP for (int i = 0; i < _SpotLightShadowedCount; ++i)
                {
                    SpotLight2DData lightData = SpotLight2D_Shadowed_Data_Buffer[i];
                    float2 lightPos = GetSpotLightPosition(lightData);
                    float2 light_2_Frag = IN.positionWS.xy - lightPos;

                    float distWS = length(light_2_Frag);
                    // 距离外的，直接不管
                    if (distWS >= lightData.inoutRadius_inoutAngles.y)
                        continue;

                    // 这是方位角（弧度制）
                    float2 dir = normalize(light_2_Frag);
                    float radi = dir.y >= 0 ? acos(dir.x) : MY_TWO_PI - acos(dir.x);
                    radi = clamp(radi, 0, MY_TWO_PI);
                    float2 directionRange = float2(lightData.color_direction.w - lightData.inoutRadius_inoutAngles.w,
                    lightData.color_direction.w + lightData.inoutRadius_inoutAngles.w);
                    directionRange = radians(directionRange);
                    if (radi + MY_TWO_PI >= directionRange.x && radi + MY_TWO_PI <= directionRange.y )
                    {
                        radi += MY_TWO_PI;
                    }
                    if (radi - MY_TWO_PI >= directionRange.x && radi - MY_TWO_PI <= directionRange.y )
                    {
                        radi -= MY_TWO_PI;
                    }

                    float shadow;
                    if (radi < directionRange.x && radi > directionRange.y)
                        shadow = 1.0f;
                    else
                    {
                        float dist = length(light_2_Frag);

                        float samplePosX = (radi - directionRange.x) / (radians(lightData.inoutRadius_inoutAngles.w) * 2) * shadowMapResolution_X;

                        float samplePosX_deci = frac(samplePosX);
                        
                        int shadowIdx_Left = int(samplePosX);
                        int shadowIdx_Right = ceil(samplePosX);
                        
                        uint shadowIdxFlatten_Left = i * shadowMapResolution_X + shadowIdx_Left;
                        uint shadowIdxFlatten_Right = i * shadowMapResolution_X + shadowIdx_Right;

                        
                        float sampledDepth = SpotLight2D_ShadowMap_Buffer[shadowIdxFlatten_Left].Depth * (1 - samplePosX_deci)
                                            + SpotLight2D_ShadowMap_Buffer[shadowIdxFlatten_Right].Depth * samplePosX_deci;

                        /*float debugID = SpotLight2D_ShadowMap_Buffer[shadowIdxFlatten_Left].Id * (1 - samplePosX_deci)
                                            + SpotLight2D_ShadowMap_Buffer[shadowIdxFlatten_Right].Id * samplePosX_deci;*/
                        
                        float curr01DepthFloat = clamp(dist / lightData.inoutRadius_inoutAngles.y, 0, 1);
                        shadow = sampledDepth < curr01DepthFloat ? 0.01 : 1;
                        // 如果被遮挡了
                        if (sampledDepth < curr01DepthFloat)
                        {
                            if (SpotLight2D_ShadowMap_Buffer[shadowIdxFlatten_Left].Id == IN.objectID &&
                                SpotLight2D_ShadowMap_Buffer[shadowIdxFlatten_Right].Id == IN.objectID)
                            {
                                shadow = 1;
                            }
                        }

                        // shadow = debugID / 10.0f;
                    }
                    
                    lightColorSum += shadow * GetSpotLightColor(lightData) * GetSpotLightIntensity(lightData) * GetSpotLightAttenuationWS(lightData, light_2_Frag);
                    
                }
                UNITY_LOOP for (int i = 0; i < _SpotLightNoShadowedCount; ++i)
                {
                    SpotLight2DData lightData = SpotLight2D_NoShadowed_Data_Buffer[i];
                    float2 lightPos = GetSpotLightPosition(lightData);
                    float2 light_2_frag = IN.positionWS.xy - lightPos;
                    lightColorSum += GetSpotLightColor(lightData) * GetSpotLightIntensity(lightData) * GetSpotLightAttenuationWS(lightData, light_2_frag);
                }

                float3 ans = lightColorSum * texColor;

                
                // debug
                int2 currCell = int2((IN.positionWS - gridZero) / cellSize);
                int idx = currCell.y * gridHorizonalNumber + currCell.x;
                int idx1 = idx / 256.0f;
                float cnt0 = gridCounter[idx] / 10000.0f;
                float cnt1 = blockCounter[idx1] / 10.0f;
                float cnt2 = gridEdgeInfo[idx].offset / 100.0f;
                float cnt3 = gridEdgePool[idx] / 100.0f;
                float cnt4 = SpotLight2D_ShadowMap_Buffer[idx].Depth;
                //float3 cnt5 = [currCell].xyz;
                float3 cnt5 = SAMPLE_TEXTURE2D(debug_Canvas, samplerdebug_Canvas, float2(currCell) / 1024.0f);
                
                float3 tmpAns = cnt0;
                if (currCell.x < 0 || currCell.x > gridHorizonalNumber ||
                    currCell.y < 0 || currCell.y > gridVerticalNumber)
                    tmpAns = 0;
                
                return float4(ans, 1);
            }
            ENDHLSL
        }
    }
    //使用官方的Diffuse作为FallBack会增加大量变体，可以考虑自定义
    //FallBack "Diffuse"
}