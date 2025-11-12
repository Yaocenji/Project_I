using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Project_I.UI
{
    public class ObstacleAvoidanceUIManager : MonoBehaviour
    {
        [Title("射线检测障碍物参数")]
        [LabelText("检测距离")]
        public float detectedDistance = 100;
        [LabelText("检测角度")]
        public int detectedAngle = 90;
        [LabelText("检测角度间隔")]
        public int detectedAngleInterval = 10;
        
        [Title("避障UI显示参数")]
        [LabelText("UI图")]
        public Image uiImage;
        [LabelText("UI最大像素距离")]
        public int maxDistanceUI = 300;
        [LabelText("UI最小像素距离")]
        public int minDistanceUI = 200;
        [LabelText("UI颜色")]
        public Color colorUI = Color.white;

        private Transform playerTransform;
        private Rigidbody2D playerRigidbody2D;
        private RectTransform uiRectTransform;

        private float targetAngle;
        private float thisAngle;
        private Vector2 targetPosition;
        private Vector2 thisPosition;
        
        
        // 记录所有的碰撞
        private List<RaycastHit2D> hits = new List<RaycastHit2D>();
        private List<int> angles = new List<int>();
        
        private void Start()
        {
            playerTransform = GameSceneManager.Instance.Player.transform;
            playerRigidbody2D = playerTransform.GetComponent<Rigidbody2D>();
            
            uiRectTransform = uiImage.gameObject.GetComponent<RectTransform>();
            
            targetAngle = thisAngle = 0;
            targetPosition = thisPosition = playerTransform.position;
        }

        void Update()
        {
            if (LayerDataManager.Instance == null)
                return;
            
            // 初始化准备
            hits.Clear();
            angles.Clear();
            
            // 当前速度方向
            Vector2 forwardDir = playerRigidbody2D.velocity;
            if (forwardDir.magnitude <= 1E-5)
                forwardDir = playerTransform.right;
            else
                forwardDir.Normalize();
            
            // 对n个方向
            for (int angle = -detectedAngle; angle <= detectedAngle; angle += detectedAngleInterval)
            {
                Vector2 dir = Quaternion.Euler(0, 0, angle) * forwardDir;

                RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, dir,
                    detectedDistance, LayerDataManager.Instance.groundLayerMask);
                // 存在碰撞点
                if (hit.collider != null)
                {
                    hits.Add(hit);
                    angles.Add(angle);
                }
            }
            
            // 拿到碰撞点队列，现在先用最近的来显示
            RaycastHit2D currHit = new RaycastHit2D();
            int currAngle = 0;
            float currDistance = float.MaxValue;
            for (int i = 0; i < hits.Count; i++)
            {
                RaycastHit2D hit = hits[i];
                float distance = Vector2.Distance(hit.point, playerTransform.position);
                if (distance < currDistance)
                {
                    currDistance = distance;
                    currAngle = angles[i];
                    currHit = hit;
                }
            }
            
            // 计算alpha
            float tmpD = Mathf.Clamp01(currDistance / detectedDistance);
            float alpha = Mathf.Clamp01(1 - tmpD);
            
            // 计算位置
            float uiScreenDistance = Mathf.Lerp(minDistanceUI, maxDistanceUI, tmpD);
            Vector2 uiScreenPos = (currHit.point - (Vector2)playerTransform.position).normalized * uiScreenDistance;
            
            // 设置
            targetPosition = uiScreenPos;
            targetAngle = Vector2.SignedAngle(Vector2.right, (currHit.point - (Vector2)playerTransform.position).normalized);
            
            thisPosition = Utilities.GenericMath.SpringApproach(thisPosition, targetPosition, 0.05f, Time.deltaTime / Time.fixedDeltaTime);
            thisAngle = Utilities.GenericMath.SpringApproach(thisAngle, targetAngle, 0.5f, Time.deltaTime / Time.fixedDeltaTime);
            
            uiImage.color = new Color(colorUI.r, colorUI.g, colorUI.b, alpha);
            uiRectTransform.anchoredPosition = thisPosition;
            uiRectTransform.rotation = Quaternion.Euler(0, 0, thisAngle);
        }

        private void OnDrawGizmos()
        {
            if (GameSceneManager.Instance == null) return;

            foreach (var hit in hits)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(playerTransform.position, hit.point);
            }
        }
    }
}
