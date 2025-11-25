using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Sirenix.OdinInspector;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Project_I.LightSystem
{
    /* 维护一个全局的 Vector4 Array：
     * 一个 Vector4 = Vector2 start + Vector2 end 表示一条有向边
     * 单层页表 页式管理系统
     * 总大小：
     * 页面大小：16 个 Vector4，16 * 16 = 256 Byte
     * 一个 Vector4：4*float = 4*4 = 16 Byte
     *
     *
     * 技术方案：
     *
     * 首先是预处理内容
     * 对于场景中的所有参与阴影生成的Sprite，都预处理得到其外轮廓的polygon（有向边的集合，一条有向边包含两个Vector2表示起终点，因此一条有向边直接用一个Vector4表示）。
     * 划定阴影计算范围的矩形（可以是自动生成，也可以是手动划定。基本等于游玩区域，这个区域大于等于所有参与阴影生成的sprite组成的AABB包围盒）。
     * 
     * 准备两个显存空间：
     * 一个包含布尔值的四叉树（这个可以做到定长，因为四叉树容易用线性数据存储）；
     * 一个二维数据，每个元素对应下文所述的每个单元正方形，用来存储单元四边形包含哪些边的。
     *
     * 依照范围矩形，设定一个单位正方形边长d，构建逻辑上的四叉树（每个节点代表一块正方形，每个节点有四个子节点分别代表这个正方形十字划分为四个更小正方形），
     * 这个四叉树理应是完全四叉树，而且显然每个节点代表的正方形的边长是d的2的幂数倍。
     *
     * 接下来是每帧内容
     * 对每个polygon的边，用类似于”光栅化“的方法，计算出它涉及到哪些单位正方形，然后在这些单位正方形对应的叶子节点中，记录这条边的索引。
     * 记录索引仅仅在最底层的单位正方形进行，而对于其他节点（包括非叶子节点，也包括叶子结点），则仅仅记录一个布尔值，表明这个节点代表的正方形是否存在边。构建这棵布尔树的方法：自底向上，先标记叶子结点的布尔值，然后用类似于mipmap的方法，跑多次computeshader即可。只不过不是计算方法平均而是求并。
     *
     * 接下来也是每帧内容，有点像屏幕空间反射的优化查找过程。
     * 对于场景中的每个需要投影的点光源，都构建一个由32为int（实则为0-1定点小数）组成的1Dshadowmap。像素的一维坐标表示一个方向。比如shadowmap是1*2048，那么相当于把这个点光源的360°分割为2048份，每个位置就表示一个方向，记录的是0-1相对深度（转化为定点小数存储到int中，不仅提升精度还可以原子操作。）
     * 对于每个光源的每个方向，都发一条射线（只需算出射线的起点和方向）。然后用同上文的类似于”光栅化“的方法，只不过上文的”光栅化“是直接在最小的单元正方形的二维数据空间进行的，这一次先从第一层节点代表的最大的正方形的空间进行，然后沿着方向找到第一个布尔值为真（表示对应最大正方形包含了polygon）的节点，依次同理地沿着树向下找，直到找到和当前射线相交的最小单元正方形，然后查找第一段所说的记录包含边的数据，然后判定。直到判定到第一个相交的边，算是找到了最近的交点，把交点距离01化写入到1Dshadowmap对应坐标位置。
     *
     * 渲染的时候，就只需要在片元着色器里遍历光源，计算方向代表的shadowmap坐标，然后采样判断是否被阴影遮挡即可。
     * 
     */
    
    public struct ShadowMapInfo
    {
        public float Depth;
        public uint Id;
    };
    
    [ExecuteAlways]
    public class ShadowCasterManager : MonoBehaviour
    {
        public static ShadowCasterManager Instance;

        // 初始的边buffer大小
        // [ReadOnly]
        public int shadowedPoligonDataSize = 16384;
        
        // 核心：边数据
        [HideInInspector]
        public List<PolygonEdge> shadowedPoligonData;
        
        // 场景中的 shadowed poligon 列表
        private HashSet<ShadowCaster> shadowCasters;
        
        // 核心数据：cell边长
        [LabelText("单元Cell 尺寸")]
        public float cellSize = 4.0f;
        
        // 核心数据：grid数量（指数）
        [LabelText("Grid的宽数量级（2的幂指数）")]
        [Range(1, 13)]
        public int gridHorizonalNumberOM = 10;
        public int gridHorizonalNumber
        {
            get => (1 << gridHorizonalNumberOM);
        }
        [LabelText("Grid的高数量级（2的幂指数）")]
        [Range(1, 13)]
        public int gridVerticalNumberOM = 10;
        public int gridVerticalNumber
        {
            get => (1 << gridVerticalNumberOM);
        }
        
        
        
        [LabelText("平行光源的阴影精度（2的幂指数）")]
        public int parallelShadowMapPrecision = 11;
        public int parallelShadowMapHorizonalRes
        {
            get => (1 << parallelShadowMapPrecision);
        }
        
        [LabelText("点光源的阴影精度（2的幂指数）")]
        public int spotShadowMapPrecision = 11;
        public int spotShadowMapHorizonalRes
        {
            get => (1 << spotShadowMapPrecision);
        }
        
        [LabelText("渲染网格")]
        public bool drawGrid = false;
        [LabelText("渲染内部网格线")]
        public bool drawInnerGrid = false;
        
        // 获取二维网格区域的宽
        public float gridHorizonalSize
        {
            get => gridHorizonalNumber * cellSize;
        }
        // 获取二维网格区域的高
        public float gridVerticalSize
        {
            get => gridVerticalNumber * cellSize;
        }
        
        // 获取二维网格区域的中心点
        public Vector2 gridCenter => 
            new Vector2(transform.position.x, transform.position.y);
        
        // 获取二维网格区域的起始点
        public Vector2 gridZero =>
            gridCenter - new Vector2(gridHorizonalSize / 2.0f, gridVerticalSize / 2.0f);
        
        // 获取二维网格区域的Rect
        public Vector4 gridRect
        {
            get
            {
                return new Vector4(gridZero.x, gridZero.y, gridHorizonalSize, gridVerticalSize);
            }
        }
        
        // grid-info buffer大小
        public int GRID_INFO_BUFFER_SIZE => gridHorizonalNumber * gridVerticalNumber;
        
        // grid-edge pool buffer的大小 预估值
        public const int GRID_EDGE_POOL_BUFFER_MAXSIZE = 1920000;
        
        
        // spotShadowMap分辨率
        public Vector2Int spotShadowMapResolution
        {
            get => new Vector2Int(spotShadowMapHorizonalRes, LightSystemManager.MAX_SPOTLIGHT_COUNT);
        }
        // spotShadowmap总大小
        public int spotShadowMapPixelNumber
        {
            get => spotShadowMapHorizonalRes * LightSystemManager.MAX_SPOTLIGHT_COUNT;
        }
        
        
        // parallelShadowMap分辨率
        public Vector2Int parallelShadowMapResolution
        {
            get => new Vector2Int(parallelShadowMapHorizonalRes, LightSystemManager.MAX_PARALLELLIGHT_COUNT);
        }
        // parallelShadowmap总大小
        public int parallelShadowMapPixelNumber
        {
            get => parallelShadowMapHorizonalRes * LightSystemManager.MAX_PARALLELLIGHT_COUNT;
        }
        
        // 点光阴影纹理
        public ComputeBuffer spotLight_ShadowMap_Buffer;
        // 平行光阴影纹理
        public ComputeBuffer parallelLight_ShadowMap_Buffer;
        
        // 当前分配的ID
        public uint nextID = 1;
        
        private bool initialized = false;
        
        private void Init()
        {
            if (initialized)
                return;
            
            Instance = this;
            
            shadowedPoligonData = new  List<PolygonEdge>();
            
            shadowCasters = new HashSet<ShadowCaster>();
            
            
            // 初始化两张纹理所用数据
            int initDataSize = Mathf.Max(spotShadowMapPixelNumber, parallelShadowMapPixelNumber);
            ShadowMapInfo[] iniData = new ShadowMapInfo[initDataSize];
            for (int i = 0; i < initDataSize; i++)
            {
                iniData[i].Id = 0;
                iniData[i].Depth = 1.0f;
            }
            
            // 创建点光的阴影纹理
            spotLight_ShadowMap_Buffer = new ComputeBuffer(spotShadowMapPixelNumber, Marshal.SizeOf<ShadowMapInfo>() /* 用32位浮动点表示 */);
            spotLight_ShadowMap_Buffer.SetData(iniData, 0, 0, spotShadowMapPixelNumber);
            
            // 创建平行光阴影纹理
            parallelLight_ShadowMap_Buffer =
                new ComputeBuffer(parallelShadowMapPixelNumber, Marshal.SizeOf<ShadowMapInfo>());
            parallelLight_ShadowMap_Buffer.SetData(iniData, 0, 0, parallelShadowMapPixelNumber);
            
            // shadowed polygon ID管理器
            nextID = 1;
            
            initialized = true;
        }

        private void Uninit()
        {
            if (!initialized)
                return;
            
            shadowCasters.Clear();
            shadowCasters  = null;
            
            spotLight_ShadowMap_Buffer.Dispose();
            spotLight_ShadowMap_Buffer =  null;
            
            GC.Collect();
            
            initialized = false;
        }

        private void Awake()
        {
            Init();
        }
        
        #if UNITY_EDITOR
        private void OnEnable()
        {
            Init();
        }
        #endif

        private void OnDisable()
        {
            Uninit();
        }

        public void RegisterShadowedPoligon(ShadowCaster shadowCaster)
        {
            if (shadowCasters != null && !shadowCasters.Contains(shadowCaster))
            {
                shadowCasters.Add(shadowCaster);
                shadowCaster.scID = nextID;
                nextID++;
                // Debug.Log("阴影多边形添加成功：" +  shadowCaster.gameObject.name + " id:" + nextID);
            }
        }
        public void UnregisterShadowedPoligon(ShadowCaster shadowCaster)
        {
            if (shadowCasters != null && shadowCasters.Contains(shadowCaster))
            {
                shadowCasters.Remove(shadowCaster);
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            UpdateShadowedPolygonData();
        }

        public void UpdateShadowedPolygonData()
        {
            foreach (ShadowCaster caster in shadowCasters)
            {
                caster.GenerateOutline(true);
            }
            shadowedPoligonData.Clear();
            foreach (var caster in shadowCasters)
            {
                foreach (var edge in caster.outline)
                {
                    // 根据edge长度来：
                    PolygonEdge polygonEdge = new PolygonEdge();
                    polygonEdge.Edge =  edge;
                    polygonEdge.Id = caster.scID;
                    shadowedPoligonData.Add(polygonEdge);
                }
            }
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (drawGrid)
            {
                var rect = gridRect;
                Gizmos.color = Color.yellow;

                for (int i = 0; i <= gridHorizonalNumber; i++)
                {
                    if (i == 0 || i ==  gridHorizonalNumber)
                        Gizmos.color = Color.yellow;
                    else
                        if (drawInnerGrid)
                            Gizmos.color = new Color(1, 0, 0, 0.1f);
                    
                    if (!drawInnerGrid && i != 0 && i != gridHorizonalNumber)
                        continue;
                    
                    Gizmos.DrawLine(new Vector3(rect.x + i * cellSize, rect.y, 0),
                        new Vector3(rect.x + i * cellSize, rect.y + rect.w, 0));

                }
                for (int i = 0; i <= gridVerticalNumber; i++)
                {
                    if (i == 0 || i ==  gridVerticalNumber)
                        Gizmos.color = Color.yellow;
                    else
                        if (drawInnerGrid)
                            Gizmos.color = new Color(1, 0, 0, 0.1f);
                    
                    if (!drawInnerGrid && i != 0 && i != gridHorizonalNumber)
                        continue;
                    
                    Gizmos.DrawLine(new Vector3(rect.x, rect.y + i * cellSize, 0),
                        new Vector3(rect.x + rect.z, rect.y + i * cellSize, 0));
                }
            }
        }
        #endif
    }
}
