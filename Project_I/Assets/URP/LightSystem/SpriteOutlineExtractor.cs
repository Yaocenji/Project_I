using UnityEngine;
using System.Collections.Generic;

namespace Project_I.LightSystem
{
    public static class SpriteOutlineExtractor
    {
        struct EdgeKey
        {
            public ushort a, b;
            public EdgeKey(ushort i0, ushort i1)
            {
                // 无向边键：保证小的在前，便于查找
                if (i0 < i1) { a = i0; b = i1; }
                else         { a = i1; b = i0; }
            }

            public override int GetHashCode() => (a << 16) ^ b;
            public override bool Equals(object obj)
            {
                var o = (EdgeKey)obj;
                return o.a == a && o.b == b;
            }
        }

        struct EdgeValue
        {
            public int count;      // 出现次数
            public ushort oriA;    // 首次出现时的有向起点
            public ushort oriB;    // 首次出现时的有向终点
        }

        /// <summary>
        /// 返回所有轮廓边，每条边由两个顶点坐标组成，且保留了它们在原三角中的方向（oriA -> oriB）。
        /// </summary>
        public static void ExtractOutline(Sprite sprite, ref List<Vector2> outPointsA, ref List<Vector2> outPointsB)
        {
            outPointsA.Clear();
            outPointsB.Clear();

            var verts = sprite.vertices;      // Vector2[]
            var tris  = sprite.triangles;     // ushort[]

            int triCount = tris.Length / 3;

            // 估算字典容量，避免扩容
            Dictionary<EdgeKey, EdgeValue> edgeMap = new Dictionary<EdgeKey, EdgeValue>(triCount * 2);

            // 1. 统计所有边出现次数，并记录首次出现时的方向
            for (int i = 0; i < triCount; i++)
            {
                ushort i0 = tris[i * 3 + 0];
                ushort i1 = tris[i * 3 + 1];
                ushort i2 = tris[i * 3 + 2];

                CountEdge(edgeMap, i0, i1);
                CountEdge(edgeMap, i1, i2);
                CountEdge(edgeMap, i2, i0);
            }

            // 2. 出现一次的边就是轮廓，使用存下来的有向信息输出
            foreach (var kv in edgeMap)
            {
                if (kv.Value.count == 1)
                {
                    var a = verts[kv.Value.oriA];
                    var b = verts[kv.Value.oriB];
                    outPointsA.Add(a);
                    outPointsB.Add(b);
                }
            }
        }

        static void CountEdge(Dictionary<EdgeKey, EdgeValue> map, ushort a, ushort b)
        {
            var key = new EdgeKey(a, b);
            if (map.TryGetValue(key, out EdgeValue val))
            {
                val.count += 1;
                map[key] = val;
            }
            else
            {
                EdgeValue nv = new EdgeValue { count = 1, oriA = a, oriB = b };
                map.Add(key, nv);
            }
        }
        
        
        // 获取逆时针外轮廓网格
        public static Vector4[] ExtractOutline(Sprite sprite)
        {
            List<Vector2> outPointsA = new List<Vector2>();
            List<Vector2> outPointsB = new List<Vector2>();
            ExtractOutline(sprite, ref outPointsA, ref outPointsB);
            Vector4[] ans = new Vector4[outPointsA.Count];
            for (int i = 0; i < ans.Length; i++)
            {
                ans[i] = new Vector4(outPointsB[i].x, outPointsB[i].y, outPointsA[i].x, outPointsA[i].y);
            }

            return ans;
        }
    }
}