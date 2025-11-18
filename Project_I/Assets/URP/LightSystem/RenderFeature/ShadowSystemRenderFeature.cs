using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Project_I.LightSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class ShadowSystemRenderFeature : ScriptableRendererFeature
{
    private bool initialized = false;
    
    [LabelText("整个阴影系统使用这个计算着色器")]
    public ComputeShader shadowSystemCompute;
    
    // 场景所有 shadowed polygon edges
    public ComputeBuffer shadowedPoligonBuffer;
    // grid-cell计数buffer
    public ComputeBuffer gridCounterBuffer;
    
    // 中间量GPU前缀和的 block sum 的buffer
    public ComputeBuffer blockSumBuffer;

    private struct GpuPrefixIterativeData
    {
        public int dataOffset;
        public int dataNumber;
        public int sumOffset;
        public int sumNumber;
    }
    
    class CustomRenderPass : ScriptableRenderPass
    {
        // 整个阴影系统使用这个计算着色器
        public ComputeShader shadowSystemCompute;
        
        // 场景所有 shadowed polygon edges
        public ComputeBuffer shadowedPoligonBuffer;
        // grid-cell计数buffer
        public ComputeBuffer gridCounterBuffer;
        
        // 中间量GPU前缀和的 block sum 的buffer
        public ComputeBuffer blockSumBuffer;
        
        // 预先获取资产的id
        private static readonly int CellSizeProperty = Shader.PropertyToID("cellSize");
        private static readonly int GridHorizonalNumberProperty = Shader.PropertyToID("gridHorizonalNumber");
        private static readonly int GridVerticalNumberProperty = Shader.PropertyToID("gridVerticalNumber");
        private static readonly int GridZeroProperty = Shader.PropertyToID("gridZero");
        
        private static readonly int ShadowedPolygonNumberProperty = Shader.PropertyToID("shadowedPolygonNumber");
        private static readonly int ShadowedPolygonProperty = Shader.PropertyToID("shadowedPolygon");
        private static readonly int GridNumberProperty = Shader.PropertyToID("gridNumber");
        private static readonly int GridCounterProperty = Shader.PropertyToID("gridCounter");
        private static readonly int BlockCounterProperty = Shader.PropertyToID("blockCounter");
        
        // 第0步清空用的
        private static readonly int gridCounter_ClearGridCounter_Property = Shader.PropertyToID("gridCounter_ClearGridCounter");
        
        // 第一步计数
        private static readonly int shadowedPolygon_GridCountEdge_Property = Shader.PropertyToID("shadowedPolygon_GridCountEdge");
        private static readonly int gridCounter_GridCountEdge_Property = Shader.PropertyToID("gridCounter_GridCountEdge");
        
        // 第二部GPU前缀和
        // 2.1 上扫
        private static readonly int groupNumber_GpuPrefix_ScanGroup_Property = Shader.PropertyToID("groupNumber_GpuPrefix_ScanGroup");
        private static readonly int data_GpuPrefix_ScanGroup_Property = Shader.PropertyToID("data_GpuPrefix_ScanGroup");
        private static readonly int sum_GpuPrefix_ScanGroup_Property = Shader.PropertyToID("sum_GpuPrefix_ScanGroup");
        // 2.2 上扫
        private static readonly int sum_GpuPrefix_ScanBlockGroup_Property = Shader.PropertyToID("sum_GpuPrefix_ScanBlockGroup");
        private static readonly int data_offset_GpuPrefix_ScanBlockGroup_Property = Shader.PropertyToID("data_offset");
        private static readonly int data_number_GpuPrefix_ScanBlockGroup_Property = Shader.PropertyToID("data_number");
        private static readonly int sum_offset_GpuPrefix_ScanBlockGroup_Property = Shader.PropertyToID("sum_offset");
        private static readonly int sum_number_GpuPrefix_ScanBlockGroup_Property = Shader.PropertyToID("sum_number");
        // 2.3 下扫
        private static readonly int sum_GpuPrefix_DownBlockGroup_Property = Shader.PropertyToID("sum_GpuPrefix_DownBlockGroup");
        // 2.4 下扫
        private static readonly int sum_GpuPrefix_DownGroup_Property = Shader.PropertyToID("sum_GpuPrefix_DownGroup");
        private static readonly int data_GpuPrefix_DownGroup_Property = Shader.PropertyToID("data_GpuPrefix_DownGroup");
        private static readonly int groupNumber_GpuPrefix_DownGroup_Property = Shader.PropertyToID("groupNumber_GpuPrefix_DownGroup");
        
        // pass前
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // 临时的：每帧CPU更新全局polygon信息，这里用SetData传输
            if(ShadowCasterManager.Instance != null && shadowedPoligonBuffer != null)
                shadowedPoligonBuffer.SetData(ShadowCasterManager.Instance.shadowedPoligonData);
            
            // 此处全局设置是为了debug显示用
            Shader.SetGlobalBuffer(GridCounterProperty, gridCounterBuffer);
            Shader.SetGlobalBuffer(BlockCounterProperty, blockSumBuffer);
        }

        // pass中
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var inst = ShadowCasterManager.Instance;
            
            // 添加一个健壮性检查，确保所有需要的资源都已就绪
            if (shadowSystemCompute == null || gridCounterBuffer == null || inst == null)
            {
                Debug.LogError("ShadowSystem: Missing Compute Shader, Buffer, or Manager Instance.");
                return;
            }
            
            CommandBuffer cmd = CommandBufferPool.Get("Grid-Edge Count");
            
            cmd.BeginSample("Grid-Edge Count Sample");
            
            // 网格信息在所有操作之前直接全局地设置
            cmd.SetGlobalFloat(CellSizeProperty, inst.cellSize);
            cmd.SetGlobalInt(GridHorizonalNumberProperty, inst.gridHorizonalNumber);
            cmd.SetGlobalInt(GridVerticalNumberProperty, inst.gridVerticalNumber);
            cmd.SetGlobalVector(GridZeroProperty, inst.gridZero);
            
            // 先清空一道
            int clearKernel = shadowSystemCompute.FindKernel("ClearGridCounter");
            // 健壮性检查
            if (clearKernel == -1)
            {
                Debug.LogError("Kernel 'ClearGridCounter' not found in the compute shader.");
                cmd.EndSample("Grid-Edge Count");
                CommandBufferPool.Release(cmd);
                return;
            }
            // 绑定变量
            cmd.SetComputeIntParam(shadowSystemCompute, GridNumberProperty, inst.GRID_INFO_BUFFER_SIZE);
            // 绑定buffer
            cmd.SetComputeBufferParam(shadowSystemCompute, clearKernel, gridCounter_ClearGridCounter_Property, gridCounterBuffer);
            // 调用
            cmd.DispatchCompute(shadowSystemCompute, clearKernel, inst.GRID_INFO_BUFFER_SIZE / 256 + 1, 1, 1);
            
            // 第一步：计数
            int countKernel = shadowSystemCompute.FindKernel("GridCountEdge");
            // 健壮性检查
            if (countKernel == -1)
            {
                Debug.LogError("Kernel 'GridCountEdge' not found in the compute shader.");
                cmd.EndSample("Grid-Edge Count");
                CommandBufferPool.Release(cmd);
                return;
            }
            // 边数
            int edgeCount = inst.shadowedPoligonData.Count;
            // 绑定变量
            cmd.SetComputeIntParam(shadowSystemCompute, ShadowedPolygonNumberProperty, edgeCount);
            cmd.SetComputeIntParam(shadowSystemCompute, GridNumberProperty, inst.GRID_INFO_BUFFER_SIZE);
            // 绑定buffer
            cmd.SetComputeBufferParam(shadowSystemCompute, countKernel, shadowedPolygon_GridCountEdge_Property, shadowedPoligonBuffer);
            cmd.SetComputeBufferParam(shadowSystemCompute, countKernel, gridCounter_GridCountEdge_Property, gridCounterBuffer);
            // 调用
            cmd.DispatchCompute(shadowSystemCompute, countKernel, edgeCount / 256 + 1, 1, 1);
            
            
            // 第二步：对计数buffer跑一趟GPU Prefix
            // 2.1 上扫
            int gpuPrefix_1_Kernel = shadowSystemCompute.FindKernel("GpuPrefix_ScanGroup");
            // 健壮性检查
            if (gpuPrefix_1_Kernel == -1)
            {
                Debug.LogError("Kernel 'GpuPrefix_ScanGroup' not found in the compute shader.");
                cmd.EndSample("Grid-Edge Count");
                CommandBufferPool.Release(cmd);
                return;
            }
            // 临时计数：当前的组数
            int currGroupNumber = inst.GRID_INFO_BUFFER_SIZE >> 8;
            if (currGroupNumber == 0) currGroupNumber = 1;
            // 绑定变量
            cmd.SetComputeIntParam(shadowSystemCompute, GridNumberProperty, inst.GRID_INFO_BUFFER_SIZE);
            cmd.SetComputeIntParam(shadowSystemCompute, groupNumber_GpuPrefix_ScanGroup_Property, currGroupNumber);
            // 绑定buffer
            cmd.SetComputeBufferParam(shadowSystemCompute, gpuPrefix_1_Kernel, data_GpuPrefix_ScanGroup_Property, gridCounterBuffer);
            cmd.SetComputeBufferParam(shadowSystemCompute, gpuPrefix_1_Kernel, sum_GpuPrefix_ScanGroup_Property, blockSumBuffer );
            // 调用
            cmd.DispatchCompute(shadowSystemCompute, gpuPrefix_1_Kernel, inst.GRID_INFO_BUFFER_SIZE / 256 + 1, 1, 1);
            
            
            /*
            // 2.2 上扫
            // 对block的和，也进行相同的操作
            // TODO 把迭代 dataOffset、dataNumber、GroupOffset、GroupNumber的过程变成查表，节省每次计算
            List<GpuPrefixIterativeData> iterativeData = new List<GpuPrefixIterativeData>();
            
            if (currGroupNumber > 1)
            {
                int currDataNumber = currGroupNumber;
                int currDataOffset = 0;
                int currGroupOffset = currGroupNumber;
                
                currGroupNumber >>= 8;
                if (currGroupNumber == 0) currGroupNumber = 1;
                
                int gpuPrefix_2_Kernel = shadowSystemCompute.FindKernel("GpuPrefix_ScanBlockGroup");
                // 健壮性检查
                if (gpuPrefix_2_Kernel == -1)
                {
                    Debug.LogError("Kernel 'GpuPrefix_ScanBlockGroup' not found in the compute shader.");
                    cmd.EndSample("Grid-Edge Count");
                    CommandBufferPool.Release(cmd);
                    return;
                }
                
                while (true)
                {
                    // 绑定变量
                    cmd.SetComputeIntParam(shadowSystemCompute, data_number_GpuPrefix_ScanBlockGroup_Property, currDataNumber);
                    cmd.SetComputeIntParam(shadowSystemCompute, sum_number_GpuPrefix_ScanBlockGroup_Property, currGroupNumber);
                    cmd.SetComputeIntParam(shadowSystemCompute, data_offset_GpuPrefix_ScanBlockGroup_Property, currDataOffset);
                    cmd.SetComputeIntParam(shadowSystemCompute, sum_offset_GpuPrefix_ScanBlockGroup_Property, currGroupOffset);
                    // 绑定buffer
                    cmd.SetComputeBufferParam(shadowSystemCompute, gpuPrefix_2_Kernel, sum_GpuPrefix_ScanBlockGroup_Property, blockSumBuffer);
                    // 调用
                    cmd.DispatchCompute(shadowSystemCompute, gpuPrefix_2_Kernel, currGroupNumber, 1, 1);
                    
                    GpuPrefixIterativeData newData = new GpuPrefixIterativeData();
                    newData.dataNumber = currDataNumber;
                    newData.dataOffset = currDataOffset;
                    newData.sumNumber = currGroupNumber;
                    newData.sumOffset = currGroupOffset;
                    iterativeData.Add(newData);

                    // 当前循环已经是最后一组了
                    if (currGroupNumber == 1)
                    {
                        // 打印Debug信息
                        //Debug.Log("End Calculate Prefix: " + (currGroupOffset + 1));
                        
                        foreach (GpuPrefixIterativeData currData in iterativeData)
                        {
                            Debug.Log("dataNumber" + currData.dataNumber + "\ndataOffset" + currData.dataOffset + "\nsumNumber" + currData.sumNumber + "\nsumOffset" + currData.sumOffset);
                        }
                        break;
                    }
                    
                    // 不是最后一组，那么继续往后迭代
                    currDataOffset += currDataNumber;
                    currDataNumber = currGroupNumber;
                    
                    currGroupOffset += currGroupNumber;
                    currGroupNumber >>= 8;
                    if (currGroupNumber == 0) currGroupNumber = 1;
                }
            }
            */
            
            
            // 2.3下扫
            /*
            int gpuPrefix_3_Kernel = shadowSystemCompute.FindKernel("GpuPrefix_DownBlockGroup");
            // 健壮性检查
            if (gpuPrefix_3_Kernel == -1)
            {
                Debug.LogError("Kernel 'GpuPrefix_DownBlockGroup' not found in the compute shader.");
                cmd.EndSample("Grid-Edge Count");
                CommandBufferPool.Release(cmd);
                return;
            }
            for (int i = iterativeData.Count - 1; i >= 0; i--)
            {
                cmd.SetComputeIntParam(shadowSystemCompute, data_number_GpuPrefix_ScanBlockGroup_Property, iterativeData[i].dataNumber);
                cmd.SetComputeIntParam(shadowSystemCompute, sum_number_GpuPrefix_ScanBlockGroup_Property, iterativeData[i].sumNumber);
                cmd.SetComputeIntParam(shadowSystemCompute, data_offset_GpuPrefix_ScanBlockGroup_Property, iterativeData[i].dataOffset);
                cmd.SetComputeIntParam(shadowSystemCompute, sum_offset_GpuPrefix_ScanBlockGroup_Property, iterativeData[i].sumOffset);
                // 绑定buffer
                cmd.SetComputeBufferParam(shadowSystemCompute, gpuPrefix_3_Kernel, sum_GpuPrefix_DownBlockGroup_Property, blockSumBuffer);
                // 调用
                cmd.DispatchCompute(shadowSystemCompute, gpuPrefix_3_Kernel, iterativeData[i].sumNumber, 1, 1);
            }
            // 2.4下扫
            int gpuPrefix_4_Kernel = shadowSystemCompute.FindKernel("GpuPrefix_DownGroup");
            // 健壮性检查
            if (gpuPrefix_4_Kernel == -1)
            {
                Debug.LogError("Kernel 'GpuPrefix_DownGroup' not found in the compute shader.");
                cmd.EndSample("Grid-Edge Count");
                CommandBufferPool.Release(cmd);
                return;
            }
            cmd.SetComputeIntParam(shadowSystemCompute, groupNumber_GpuPrefix_DownGroup_Property, inst.GRID_INFO_BUFFER_SIZE >> 8 != 0 ? inst.GRID_INFO_BUFFER_SIZE >> 8 : 1);
            cmd.SetComputeBufferParam(shadowSystemCompute, gpuPrefix_4_Kernel, data_GpuPrefix_DownGroup_Property, gridCounterBuffer);
            cmd.SetComputeBufferParam(shadowSystemCompute, gpuPrefix_4_Kernel, sum_GpuPrefix_DownGroup_Property, blockSumBuffer);
            // 调用
            cmd.DispatchCompute(shadowSystemCompute, gpuPrefix_4_Kernel, inst.GRID_INFO_BUFFER_SIZE / 256 + 1, 1, 1);
            */
            
            cmd.EndSample("Grid-Edge Count Sample");
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // pass后
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();

        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        
        // 不用获取inst的变量在这里赋予
        m_ScriptablePass.shadowSystemCompute = shadowSystemCompute;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        
        // 避免泄漏
        shadowedPoligonBuffer?.Release();
        gridCounterBuffer?.Release();
        blockSumBuffer?.Release();
        
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 1. 尝试获取 Manager 实例
        var inst = ShadowCasterManager.Instance;
        
        // 2. [关键] 如果实例不存在（例如在编辑器非播放模式下，或场景加载初期），则不执行任何操作
        if (inst == null)
        {
            // 你也可以在这里选择释放资源，以防从播放模式切换回编辑模式时资源泄露
            gridCounterBuffer?.Release();
            gridCounterBuffer = null;
            
            shadowedPoligonBuffer?.Release();
            shadowedPoligonBuffer = null;
            return; // 直接返回，不将 Pass 添加到渲染队列
        }
        
        // 3. 检查并按需（重新）创建 ComputeBuffer
        //    - 条件1: Buffer 从未被创建 (gridCounterBuffer == null)
        //    - 条件2: Buffer 存在但大小与当前需求不匹配 (gridCounterBuffer.count != inst.GRID_INFO_BUFFER_SIZE)
        if (gridCounterBuffer == null || !gridCounterBuffer.IsValid() || gridCounterBuffer.count != inst.GRID_INFO_BUFFER_SIZE)
        {
            // 释放旧的（如果存在且有效）
            gridCounterBuffer?.Release();
            // 用新的正确尺寸创建 Buffer
            gridCounterBuffer = new ComputeBuffer(inst.GRID_INFO_BUFFER_SIZE, sizeof(int));
        }
        if (shadowedPoligonBuffer == null || !shadowedPoligonBuffer.IsValid())
        {
            // TODO 动态大小
            
            // 释放旧的（如果存在且有效）
            shadowedPoligonBuffer?.Release();
            
            // 用新的正确尺寸创建 Buffer
            shadowedPoligonBuffer = new ComputeBuffer(inst.shadowedPoligonDataSize, Marshal.SizeOf<Vector4>());
        }
        if (blockSumBuffer == null || !blockSumBuffer.IsValid())
        {
            // 释放旧的（如果存在且有效）
            blockSumBuffer?.Release();
            // 计算buffer大小
            uint bufferSize = 0;
            // 初始大小：GRID_INFO_BUFFER_SIZE / 256
            uint currLayerSize = (uint)inst.GRID_INFO_BUFFER_SIZE >> 8;
            if (currLayerSize == 0)
                currLayerSize = 1;
            while (true)
            {
                bufferSize += currLayerSize;
                
                if (currLayerSize == 1)
                    break;
                
                currLayerSize >>= 8;
                if (currLayerSize == 0)
                    currLayerSize = 1;
            }
            Debug.Log("Block sum buffer: " + bufferSize);
            // 用新的正确尺寸创建 Buffer
            blockSumBuffer = new ComputeBuffer((int)currLayerSize, sizeof(int));
        }
        
        // 4. 确保 Pass 持有最新的资源引用
        m_ScriptablePass.shadowedPoligonBuffer = shadowedPoligonBuffer;
        m_ScriptablePass.gridCounterBuffer = gridCounterBuffer;
        m_ScriptablePass.blockSumBuffer = blockSumBuffer;
        
        renderer.EnqueuePass(m_ScriptablePass);
    }
}
