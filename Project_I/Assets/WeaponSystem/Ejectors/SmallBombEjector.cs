using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project_I
{
public class SmallBombEjector : BasicEjector
{
    public GameObject smallBomb;
    // 准星预制体
    public GameObject aimingSightPrefab;
    private GameObject aimingSight;
    
    [Header("炸弹数据")]
    [Header("存放数量")]
    public int bombNumber = 4;
    [Header("冷却")]
    public float bombColdTime = 8.0f;
    [Header("出膛速度")]
    public float speed = 2.5f;
    [Header("散布")]
    public float sigma = 3.0f;
    
    // 当前飞机的参数
    private AircraftController _aircraftController;
    
    // 计时器
    private float[] timerBomb;
    
    // 用于模拟的一些数据
    [Header("落点计算模拟时间步长")]
    public float stepTime = 0.05f;
    private int _simulateTimes;
    private float _bombMass;
    // 落点
    private Vector4 _landPositionData;
    
    // 是否在瞄准
    private bool isAiming;
    
    // Start is called before the first frame update
    void Start()
    {
        timerBomb = new float[bombNumber];
        for (int i = 0; i < bombNumber; i++)
        {
            timerBomb[i] = bombColdTime + 0.001f;
        }
        
        _aircraftController = GetComponent<AircraftController>();

        _simulateTimes = (int)(smallBomb.GetComponent<SmallBomb>().lifeTime / stepTime);
        _bombMass = smallBomb.GetComponent<Rigidbody2D>().mass;
        _landPositionData = Vector4.zero;
        // 预制体实例化
        aimingSight = Instantiate(aimingSightPrefab);
        aimingSight.SetActive(false);

        isAiming = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 每个冷却都++
        for (int i = 0; i < bombNumber; i++)
        {
            if (timerBomb[i] <= bombColdTime)
                timerBomb[i] += Time.deltaTime;
        }
        
        // 计算落点
        if (isAiming)
        {
            _landPositionData = TrajectoryLandPosition();
            // 没有着弹点，不显示
            if (_landPositionData == Vector4.zero)
            {
                aimingSight.transform.position = transform.position + Vector3.one * 3000;
            }
            // 有着弹点
            else
            {
                aimingSight.transform.position = new Vector3(_landPositionData.x, _landPositionData.y, 0);
                aimingSight.transform.right = new Vector3(-_landPositionData.z, -_landPositionData.w, 0);
            }
        }
    }

    public override void BeginEject()
    {
        // 找一个合适的丢了
        for (int i = 0; i < bombNumber; i++)
        {
            if (timerBomb[i] >= bombColdTime)
            {
                // 丢这个
                timerBomb[i] = 0;
                
                GameObject newBulletObject = Instantiate(smallBomb);
                BasicBullet newBullet = newBulletObject.GetComponent<BasicBullet>();
                
                if (gameObject.layer == LayerDataManager.Instance.playerLayer || gameObject.layer == LayerDataManager.Instance.friendlyLayer)
                    newBulletObject.layer = LayerDataManager.Instance.friendlyBulletLayer;
                else if (gameObject.layer == LayerDataManager.Instance.enemyLayer)
                    newBulletObject.layer = LayerDataManager.Instance.enemyBulletLayer;
                
                newBulletObject.transform.position = transform.position;
                newBulletObject.transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + Random.Range(-sigma, sigma));


                Rigidbody2D newRB = newBulletObject.GetComponent<Rigidbody2D>();
        
                newRB.AddForce(_aircraftController.getVelocity * newRB.mass, ForceMode2D.Impulse);
                newRB.AddForce(newRB.mass * speed * newBulletObject.transform.right, ForceMode2D.Impulse);
                
                break;
            }
        }
    }
    
    public override Vector2 AimingCameraPos(Vector2 aircraftPos, Vector2 mouseTargetPos, Vector2 targetAircraftPos)
    {
        Vector2 aimPos = aircraftPos + (mouseTargetPos - aircraftPos).normalized * aimingCameraDistance;
        
        if (_landPositionData == Vector4.zero)
            return (aircraftPos + aimPos) / 2.0f;
        else
        {
            // 计算三角形最长边
            aimingCameraSize = Mathf.Max((aircraftPos - aimPos).magnitude,
                Mathf.Max(((Vector2)_landPositionData - aimPos).magnitude,
                    ((Vector2)_landPositionData - aircraftPos).magnitude));
            return (aircraftPos + aimPos + (Vector2)_landPositionData) / 3.0f;
        }
    }

    public override void BeginAiming()
    {
        aimingSight.SetActive(true);
        isAiming = true;
    }
    public override void EndAiming()
    {
        aimingSight.SetActive(false);
        isAiming = false;
    }

    // 弹道计算器
    private Vector4 TrajectoryLandPosition()
    {
        // 无解析解，通过步进计算方式求解
        // 初始速度等于机体速度+发射速度
        Vector2 v0 = _aircraftController.getVelocity + speed * (Vector2)transform.right;
        // 初始位置
        Vector2 p0 = transform.position;
        // 当前速度
        Vector2 v = v0;
        Vector2 p = p0;

        bool flag = false;
        Vector4 ans = Vector4.zero;
        for (int i = 0; i < _simulateTimes; i++)
        {
            // 先进行射线检测
            var recastResult = Physics2D.Raycast(p, v, v.magnitude * stepTime, LayerDataManager.Instance.groundLayerMask);
            // 如果碰到东西了
            if (recastResult.collider is not null)
            {
                flag = true;
                ans = new Vector4(recastResult.point.x, recastResult.point.y, recastResult.normal.x,
                    recastResult.normal.y);
                break;
            }
            // 下一步迭代
            p += stepTime * v;
            float bSpeed = v.magnitude;
            Vector2 fricF = -v.normalized * (bSpeed * bSpeed) / 45.0f;
            v += (fricF / _bombMass + Physics2D.gravity) * stepTime;
            
        }
        if (!flag) return Vector4.zero;
        else return ans;
    }
}
    
}
