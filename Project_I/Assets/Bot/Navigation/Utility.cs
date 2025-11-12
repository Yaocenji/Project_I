using UnityEngine;

namespace Project_I.Bot.Navigation
{
    public class Utility
    {
        /// <summary>
        /// 获取一个距离NavMesh边缘足够远的随机可达点。
        /// </summary>
        public static bool TryGetRandomReachablePoint(
            Vector3 origin,
            float minDistance,
            float maxDistance,
            Vector2 forwardDir,
            float directionAngleRange,
            out Vector3[] result)
        {
            for (int i = 0; i < 15; i++) // 尝试多几次
            {
                // 1️⃣ 随机方向
                float angle = Random.Range(-directionAngleRange, directionAngleRange);
                Vector2 dir = Quaternion.Euler(0, 0, angle) * forwardDir.normalized;

                // 2️⃣ 随机距离
                float distance = Random.Range(minDistance, maxDistance);
                Vector3 candidate = origin + (Vector3)(dir * distance);

                // 3️⃣ NavMesh采样
                UnityEngine.AI.NavMeshHit hit;
                if (!UnityEngine.AI.NavMesh.SamplePosition(candidate, out hit, 1.0f, UnityEngine.AI.NavMesh.AllAreas))
                    continue;

                // 5️⃣ 验证路径可达
                UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
                if (UnityEngine.AI.NavMesh.CalculatePath(origin, hit.position, UnityEngine.AI.NavMesh.AllAreas, path) &&
                    path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                {
                    result = path.corners;
                    return true;
                }
            }

            result = new Vector3[1];
            return false;
        }
        
        
        
        /// <summary>
        /// 计算提前量命中点。
        /// 返回 true 表示有解，p 为预测命中位置。
        /// </summary>
        public static bool GetLeadPosition(
            Vector2 Apos, float bulletSpeed,
            Vector2 Bpos, Vector2 Bvel,
            out Vector2 p)
        {
            Vector2 r = Bpos - Apos;
            float a = Vector2.Dot(Bvel, Bvel) - bulletSpeed * bulletSpeed;
            float b = 2f * Vector2.Dot(r, Bvel);
            float c = Vector2.Dot(r, r);

            float discriminant = b * b - 4f * a * c;

            if (discriminant < 0f)
            {
                p = Vector2.zero;
                return false; // 无解，追不上
            }

            float sqrt = Mathf.Sqrt(discriminant);

            // 两个可能的时间
            float t1 = (-b + sqrt) / (2f * a);
            float t2 = (-b - sqrt) / (2f * a);

            // 取最小的正时间
            float t = Mathf.Min(t1, t2);
            if (t < 0f)
                t = Mathf.Max(t1, t2);

            if (t < 0f)
            {
                p = Vector2.zero;
                return false; // 目标在背后，无法命中
            }

            // 命中点
            p = Bpos + Bvel * t;
            return true;
        }

    }
    
}