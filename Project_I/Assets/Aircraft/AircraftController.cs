using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Project_I
{
public class AircraftController : MonoBehaviour
{
    // 当前目标点的世界空间坐标
    private Vector2 _targetPositionWs;
    
    // 重力常数
    private const float g = 9.81f;
    // 物理帧帧周期
    private const float fixedFrameTime = 0.02f;
    
    // 飞机参数
    
    [Header("慢车出力")]
    public float engineF_0 = 5.0f;
    [Header("满车出力")]
    public float engineF_1 = 15.5f;
    [Header("加力出力")]
    public float engineF_2 = 30.2f;
    
    [Header("飞机质量")]
    public float mass = 1.0f;
    
    [Header("最小回转角速度")]
    public float minRotateRatio = 60f;
    [Header("最大回转角速度")]
    public float maxRotateRatio = 150f;
    
    [Header("最佳回转速度起点")]
    public float minBestRotateSpeed = 5.0f;
    [Header("最佳回转速度终点")]
    public float maxBestRotateSpeed = 20.0f;
    [Header("锁舵速度")]
    public float rudderLockSpeed = 40.0f;

    [Header("舵从中立位到最大位的所需的时间")]
    public float rudderAgility = 0.3f;
    
    // 是否使用推进
    private bool useStandardThrust;
    // 是否使用加力
    private bool useAugmentationThrust;
        
    // 几个关键力
    private Vector2 engineF;
    private Vector2 liftF;
    private Vector2 fricF;
    private Vector2 gravityF;
    
    // 一个物理帧中的合力
    private Vector2 sumF;

    // 运动状态
    private Vector2 velocity;
    private float speed;
    private float angularVelocity;
    
    void Awake()
    {
        _targetPositionWs = transform.position + transform.right;
        speed = 0;
        useStandardThrust = false;
        useAugmentationThrust = false;
        velocity = Vector2.zero;
        speed = 0;
        angularVelocity = 0;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // 初始化
        {
            sumF = Vector2.zero;
        }

        // 飞行动力
        {
            // 重力
            gravityF = mass * g * Vector2.down;
            // 引擎
            if (!useStandardThrust)
                engineF = engineF_0 * transform.right;
            else if (useStandardThrust && !useAugmentationThrust)
                engineF = engineF_1 * transform.right;
            else
                engineF = engineF_2 * transform.right;
            // 升力
            if (!useStandardThrust)
                liftF = -gravityF * 0.5f;
            else
                liftF = -gravityF;
            // 阻力
            fricF = -velocity.normalized * (speed * speed) / 15.0f;

            // 合力
            sumF = gravityF + engineF + liftF + fricF;
        }

        // 旋转
        {
            Vector2 targetDir = (_targetPositionWs - (Vector2)transform.position).normalized;
            float targetAngle = targetDir.y >= 0 ? Mathf.Acos(targetDir.x) : -Mathf.Acos(targetDir.x);
            Vector3 cross = Vector3.Cross(transform.right, targetDir);
            float dot = Vector2.Dot(transform.right, targetDir);
            // 计算要变化的弧度（逆正顺负）
            float deltaAngle = dot >= 0 ? Mathf.Asin(cross.z) : ( cross.z >= 0 ? Mathf.PI - Mathf.Asin(cross.z) : - Mathf.PI - Mathf.Asin(cross.z) );
            // 计算当前的最大角速度
            float currMaxAngularVelocity;
            if (speed >= minBestRotateSpeed && speed <= maxBestRotateSpeed)
            {
                currMaxAngularVelocity = maxRotateRatio;
            }else if (speed <= minBestRotateSpeed)
            {
                currMaxAngularVelocity = Mathf.Lerp(minRotateRatio, maxRotateRatio, speed / minBestRotateSpeed);
            }else if (speed >= maxBestRotateSpeed && speed <= rudderLockSpeed)
            {
                currMaxAngularVelocity = Mathf.Lerp(maxRotateRatio, 0,
                    (speed - maxBestRotateSpeed) / (rudderLockSpeed - maxBestRotateSpeed));
            }else
            {
                currMaxAngularVelocity = 0;
            }

            //float ans = (deltaAngle + Mathf.Sign(deltaAngle) * currMaxAngularVelocity * fixedFrameTime) / 2;
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (deltaAngle / Mathf.PI * 180.0f) * 0.05f + 0.02f);
            //transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + Mathf.Sign(deltaAngle) * currMaxAngularVelocity * fixedFrameTime);
        }
        
        // 水平安定与存能
        {
            // 正交分解
            Vector2 parallelVelocity = Vector2.Dot(velocity, transform.right) / Vector2.Dot(transform.right, transform.right) 
                                       * (Vector2)transform.right;
            Vector2 verticalVelocity = velocity - parallelVelocity;

            velocity = (parallelVelocity + verticalVelocity.magnitude * 0.05f * parallelVelocity.normalized) + verticalVelocity * 0.95f;
        }

        // 物理计算启动
        {
            // 开始作用 （动量定理）
            velocity += sumF * fixedFrameTime / mass;

            // 开始作用 （产生位移）
            transform.position += (Vector3)(velocity * fixedFrameTime);

            // 计算物理量
            speed = velocity.magnitude;
        }
    }

    public void SetTargetPosition(Vector2 tar)
    {
        _targetPositionWs = tar;
    }
    
    public void StartStandardThrust()
    {
        useStandardThrust = true;
    }
    public void EndStandardThrust()
    {
        useStandardThrust = false;
    }

    public void StartAugmentationThrust()
    {
        useAugmentationThrust = true;
    }
    public void EndAugmentationThrust()
    {
        useAugmentationThrust = false;
    }
}
    
}
