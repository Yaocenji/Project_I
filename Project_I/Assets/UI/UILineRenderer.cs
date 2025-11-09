using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Project_I.UI
{

    [AddComponentMenu("UI/UILineRenderer")]
    public class UILineRenderer : Graphic
    {
        public List<Vector2> points = new List<Vector2>();
        public float thickness = 2f;

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

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (points == null || points.Count < 2) return;

            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];
                DrawLine(vh, p1, p2);
            }
        }

        private void DrawLine(VertexHelper vh, Vector2 p1, Vector2 p2)
        {
            var direction = (p2 - p1).normalized;
            var normal = new Vector2(-direction.y, direction.x) * thickness * 0.5f;

            var v1 = p1 - normal;
            var v2 = p1 + normal;
            var v3 = p2 + normal;
            var v4 = p2 - normal;

            int idx = vh.currentVertCount;

            vh.AddVert(v1, color, Vector2.zero);
            vh.AddVert(v2, color, Vector2.zero);
            vh.AddVert(v3, color, Vector2.zero);
            vh.AddVert(v4, color, Vector2.zero);

            vh.AddTriangle(idx + 0, idx + 1, idx + 2);
            vh.AddTriangle(idx + 2, idx + 3, idx + 0);
        }
        
        public void Refresh()
        {
            SetVerticesDirty();
        }
        
    }
    
    
}

