using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Project_I.UI
{
    [AddComponentMenu("UI/UILineRenderer")]
    public class UILineRenderer : Graphic
    {
        [Header("基础折线参数")]
        public List<Vector2> points = new List<Vector2>();
        public float thickness = 2f;

        [Header("顶点属性（可选）")]
        public List<float> perPointThickness = new List<float>();
        public List<Color> perPointColor = new List<Color>();

        [Header("拐角平滑（圆弧）")]
        [Tooltip("拐角圆弧细分数（凸角），0 为不绘制圆弧。内角时会自动收缩以避免内侧重叠。")]
        [Range(0, 32)]
        public int cornerSmoothness = 6;

        // 可选：限制外侧延展的最大倍数（防止非常尖锐角产生极长的miter）
        public float maxMiterLimit = 8f;

        public void SetPoints(List<Vector2> pts)
        {
            points = pts;
            SetVerticesDirty();
        }

        public void SetThickness(float t)
        {
            thickness = t;
            SetVerticesDirty();
        }

        public void SetColor(Color c)
        {
            color = c;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (points == null || points.Count < 2) return;

            int n = points.Count;

            // 先为每个点计算“调整后的位置”和“miter info”
            // adjustedPoints 用来绘制段的端点，避免内侧重叠（对内角做收缩）
            Vector2[] adjusted = new Vector2[n];

            // 存储每点的 miter 信息，用于后续绘制圆弧半径
            Vector2[] miters = new Vector2[n];
            float[] miterLens = new float[n];

            // 先初始化为原始点
            for (int i = 0; i < n; i++)
            {
                adjusted[i] = points[i];
                miters[i] = Vector2.zero;
                miterLens[i] = 1f;
            }

            // 计算每个内部点的 miter（基于相邻段的法线）
            for (int i = 1; i < n - 1; i++)
            {
                Vector2 a = points[i - 1];
                Vector2 b = points[i];
                Vector2 c = points[i + 1];

                Vector2 d1 = (b - a);
                Vector2 d2 = (c - b);
                if (d1.sqrMagnitude < Mathf.Epsilon || d2.sqrMagnitude < Mathf.Epsilon)
                {
                    miters[i] = Vector2.zero;
                    miterLens[i] = 1f;
                    continue;
                }
                d1.Normalize();
                d2.Normalize();

                Vector2 n1 = new Vector2(-d1.y, d1.x); // left normal of segment (a->b)
                Vector2 n2 = new Vector2(-d2.y, d2.x); // left normal of segment (b->c)

                Vector2 m = n1 + n2;
                float mLenSq = m.sqrMagnitude;
                if (mLenSq < 1e-6f)
                {
                    // 近 180° 的情况：使用单段法线
                    miters[i] = n1;
                    miterLens[i] = 1f;
                    continue;
                }

                m.Normalize();
                // dot = m · n1
                float dot = Vector2.Dot(m, n1);
                // miter length factor: how much to scale m to reach the true offset
                float miterLen = 1f / Mathf.Max(1e-6f, dot);

                // 限制极端 miter（防止尖角产生极长伸出）
                float limited = Mathf.Clamp(miterLen, 0.0f, maxMiterLimit);

                miters[i] = m; // m 是 bisector-like 的方向
                miterLens[i] = limited;

                // 判断凹角/凸角：通过 dot(d1, d2) 或 cross 来判断
                // cross < 0 => 右转 (clockwise)，cross > 0 => 左转 (ccw)
                float cross = d1.x * d2.y - d1.y * d2.x;

                // 当 cross 与 (Vector2.Dot(n1, m) 的符号一致时为凸角，否则为凹角
                // 简化：在凹角（concave）时，miterLen 的实际效果会把点推向内侧，需要把点收缩回来
                // 当 miterLen < 1 时（通常为凹角），我们把 adjusted 点沿 m 向内移动以避免内侧矩形重叠
                float halfT = GetThicknessAt(i) * 0.5f;
                if (miterLen < 1f + 1e-4f)
                {
                    // concave-ish：计算一个收缩量，使得矩形端点不会重叠
                    // 收缩量 = halfT * (1 - miterLen) 投影到 bisector 方向
                    float shrink = halfT * (1f - miterLen);
                    // 为避免 direction flip，使用 (d1 + d2) as bisector direction for inward movement
                    Vector2 bis = (d1 + d2);
                    if (bis.sqrMagnitude > 1e-6f)
                    {
                        bis.Normalize();
                        // bis 指向两个段之间中间方向（面向内部或外部）
                        // 我们需要沿着 bisector 朝“内部”方向收缩。
                        // 进一步判断：如果 Vector2.Dot(bis, m) > 0，m 与 bis 同向，收缩沿 bis 方向
                        float sign = Vector2.Dot(bis, m) >= 0f ? -1f : 1f;
                        adjusted[i] = b + bis * (shrink * sign);
                    }
                    else
                    {
                        // 极端情况：退回到原点不变
                        adjusted[i] = b;
                    }
                }
                else
                {
                    // convex-ish：为了让圆弧更好填补间隙，我们可以不移动 adjusted 点（保持原位）
                    adjusted[i] = b;
                }
            }

            // 端点保持原位
            adjusted[0] = points[0];
            adjusted[n - 1] = points[n - 1];

            // —— 使用 adjusted 点绘制每段矩形（每段端点使用 adjusted） —— 
            for (int i = 0; i < n - 1; i++)
            {
                Vector2 p1 = adjusted[i];
                Vector2 p2 = adjusted[i + 1];

                float halfT1 = GetThicknessAt(i) * 0.5f;
                float halfT2 = GetThicknessAt(i + 1) * 0.5f;

                Color c1 = GetColorAt(i);
                Color c2 = GetColorAt(i + 1);

                AddSegmentQuad(vh, p1, p2, halfT1, halfT2, c1, c2);
            }

            // —— 在每个内部点绘制外侧圆弧（round join） —— 
            if (cornerSmoothness > 0 && n >= 3)
            {
                for (int i = 1; i < n - 1; i++)
                {
                    DrawRoundJoinAt(vh, i, adjusted);
                }
            }
        }

        private float GetThicknessAt(int i)
        {
            if (perPointThickness != null && i < perPointThickness.Count && perPointThickness[i] > 0f)
                return perPointThickness[i];
            return thickness;
        }

        private Color GetColorAt(int i)
        {
            if (perPointColor != null && i < perPointColor.Count)
                return perPointColor[i];
            return color;
        }

        // 添加每段的矩形（两端使用已调整的位置）
        private void AddSegmentQuad(VertexHelper vh, Vector2 p1, Vector2 p2, float halfT1, float halfT2, Color c1, Color c2)
        {
            Vector2 dir = (p2 - p1);
            float len = dir.magnitude;
            if (len <= Mathf.Epsilon) return;
            dir /= len;
            Vector2 normal = new Vector2(-dir.y, dir.x);

            Vector2 v1 = p1 - normal * halfT1;
            Vector2 v2 = p1 + normal * halfT1;
            Vector2 v3 = p2 + normal * halfT2;
            Vector2 v4 = p2 - normal * halfT2;

            int idx = vh.currentVertCount;

            vh.AddVert(v1, c1, Vector2.zero);
            vh.AddVert(v2, c1, Vector2.zero);
            vh.AddVert(v3, c2, Vector2.zero);
            vh.AddVert(v4, c2, Vector2.zero);

            vh.AddTriangle(idx + 0, idx + 1, idx + 2);
            vh.AddTriangle(idx + 2, idx + 3, idx + 0);
        }

        // 在索引 i 处绘制外侧圆弧（基于原始点 points[i]，但使用 half-thickness）
        // adjusted 参数用于避免过渡时与已绘制矩形冲突（圆弧位置中心仍用原点）
        private void DrawRoundJoinAt(VertexHelper vh, int i, Vector2[] adjusted)
        {
            // 使用原始位置作为弧的中心（这样弧与矩形能自然衔接）；但矩形端点已经被 adjusted 修正以避免内侧重叠
            Vector2 prev = points[i - 1];
            Vector2 curr = points[i];
            Vector2 next = points[i + 1];

            Vector2 dPrev = (curr - prev);
            Vector2 dNext = (next - curr);
            if (dPrev.sqrMagnitude < Mathf.Epsilon || dNext.sqrMagnitude < Mathf.Epsilon) return;

            dPrev.Normalize();
            dNext.Normalize();

            Vector2 nPrev = new Vector2(-dPrev.y, dPrev.x);
            Vector2 nNext = new Vector2(-dNext.y, dNext.x);

            // 计算 bisector-like miter 与 miterLen（用于判断凹凸）
            Vector2 m = nPrev + nNext;
            float mLenSq = m.sqrMagnitude;
            if (mLenSq < 1e-6f)
            {
                // 近 180° ，两法线几乎相反 -> 不需要绘制圆弧
                return;
            }
            m.Normalize();
            float dot = Vector2.Dot(m, nPrev);
            float miterLen = 1f / Mathf.Max(1e-6f, dot);

            // 判定是否为凹角（concave）
            bool isConcave = miterLen < 1f;

            // 我们只绘制外侧（凸角）的圆弧；凹角我们已通过 adjusted 把内侧收缩，故不用绘制内弧
            if (isConcave) return;

            float halfT = GetThicknessAt(i) * 0.5f;
            Color col = GetColorAt(i);

            // 角度起点与终点（基于法线方向）
            float a0 = Mathf.Atan2(nPrev.y, nPrev.x);
            float a1 = Mathf.Atan2(nNext.y, nNext.x);
            float delta = a1 - a0;
            while (delta <= -Mathf.PI) delta += 2 * Mathf.PI;
            while (delta > Mathf.PI) delta -= 2 * Mathf.PI;

            // 为了保证走外侧的短弧（视觉更自然），如果 delta 为负则表示方向需要修正
            // 但上面 isConcave 已过滤凹角，delta 应该代表外侧弧的角度范围（通常小）
            int steps = Mathf.Max(1, cornerSmoothness);
            float step = delta / steps;

            // 中心顶点（使用原始 curr 作为圆弧中心）
            int centerIndex = vh.currentVertCount;
            vh.AddVert(curr, col, Vector2.zero);

            List<int> arcIndices = new List<int>(steps + 1);
            for (int s = 0; s <= steps; s++)
            {
                float a = a0 + step * s;
                Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                Vector2 pos = curr + dir * halfT;

                int vi = vh.currentVertCount;
                vh.AddVert(pos, col, Vector2.zero);
                arcIndices.Add(vi);

                if (s > 0)
                {
                    vh.AddTriangle(centerIndex, arcIndices[s - 1], arcIndices[s]);
                }
            }
        }

        public void Refresh()
        {
            SetVerticesDirty();
        }
    }
}
