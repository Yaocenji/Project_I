
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Project_I.UI
{
    public class TargetPositionUIController : MonoBehaviour
    {
        [LabelText("图标")]
        public Image icon;
        [LabelText("线条")]
        public UILineRenderer line;
        
        [LabelText("线条内层半径")]
        public float innerRadius = 350;
        [LabelText("外层半径")]
        public float outerRadius = 500;
        [LabelText("目标单位")]
        public Transform target;
        [LabelText("颜色")]
        public Color color =  Color.white;

        private RectTransform iconRectTransform;
        private Camera mainCamera;
        private CameraController mainCameraController;
        private Rigidbody2D targetRigidbody2D;
        
        // 用于实际的innerRadius、outerRadius
        private float dynamicInnerRadius;
        private float dynamicOuterRadius;

        private float rawThickness;
        
        private void Start()
        {
            line.points.SetLength(2);
            mainCamera =  Camera.main;
            mainCameraController = mainCamera.gameObject.GetComponent<CameraController>();
            iconRectTransform = icon.GetComponent<RectTransform>();
            targetRigidbody2D =  target.GetComponent<Rigidbody2D>();
            rawThickness = line.thickness;
        }

        // Update is called once per frame
        void Update()
        {
            if (target is null)
            {
                Debug.LogError("target is null");
                return;
            }
            
            // 计算当前的方向
            Vector2 thisPos = mainCamera.transform.position;
            Vector2 targetPos = target.position;
            Vector2 direction = (targetPos - thisPos).normalized;
            Vector2 targetScreenPos = mainCamera.WorldToScreenPoint(target.position);
            
            // 速度系数
            float sp = targetRigidbody2D.velocity.magnitude;
            float tmpT = 1.0f - Mathf.Exp(-0.05f * sp);
            // 设置动态地inner、outer半径
            float maxRadiusParam = 1.5f;
            dynamicInnerRadius = innerRadius + innerRadius * (maxRadiusParam - 1) * tmpT;
            dynamicOuterRadius = outerRadius + outerRadius * (maxRadiusParam - 1) * tmpT;
            
            // 设置图标位置
            iconRectTransform.anchoredPosition = direction * dynamicOuterRadius;
            // 设置图标的旋转s
            // iconRectTransform.rotation = target.rotation;
            
            // 设置指示线位置、姿态
            line.points[0] = direction * dynamicInnerRadius;
            line.points[1] = direction * (dynamicOuterRadius - 30);
            
            // 设置透明度：随距离变化
            float alpha;
            float d = (targetPos - thisPos).magnitude / (outerRadius * (mainCameraController.RawCameraSize / (Screen.height * 0.5f)));
            
            if (d <= 1)
                alpha = 0;
            else if (d <= 1.5)
                alpha = (d - 1) * 2;
            else if (d <= 6)
                alpha = 1;
            else
                alpha = Mathf.Exp(-0.1f * (d - 6));
            
            icon.color = new Color(color.r, color.g, color.b, alpha);
            line.color = new Color(color.r, color.g, color.b, alpha);
            
            // 线的粗细：随距离变化
            float maxThicknessParam = 1.75f;
            line.thickness = rawThickness + rawThickness * (maxThicknessParam - 1) * Mathf.Exp(-2 * (d - 1));
            line.Refresh();
            
            /*if (targetScreenPos.x > Screen.width || targetScreenPos.y > Screen.height || targetScreenPos.x < 0 || targetScreenPos.y < 0)
            {
                icon.color = new Color(color.r, color.g, color.b, alpha);
                line.color = new Color(color.r, color.g, color.b, alpha);
            }
            else
            {
                icon.color = new Color(color.r, color.g, color.b, 0.0f);
                line.color = new Color(color.r, color.g, color.b, 0.0f);
            }*/
        }
    }
}
