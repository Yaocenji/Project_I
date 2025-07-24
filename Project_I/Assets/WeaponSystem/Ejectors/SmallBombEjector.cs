using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class SmallBombEjector : BasicEjector
{
    public GameObject smallBomb;
    
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
    
    // Start is called before the first frame update
    void Start()
    {
        timerBomb = new float[bombNumber];
        for (int i = 0; i < bombNumber; i++)
        {
            timerBomb[i] = bombColdTime + 0.001f;
        }
        
        _aircraftController = GetComponent<AircraftController>();
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
                newBulletObject.transform.rotation = UnityEngine.Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + Random.Range(-sigma, sigma));


                Rigidbody2D newRB = newBulletObject.GetComponent<Rigidbody2D>();
        
                newRB.AddForce(_aircraftController.getVelocity * newRB.mass, ForceMode2D.Impulse);
                newRB.AddForce(newRB.mass * speed * newBulletObject.transform.right, ForceMode2D.Impulse);
                
                break;
            }
        }
    }
    
    public override Vector2 AimingCameraPos(Vector2 aircraftPos, Vector2 mouseTargetPos, Vector2 targetAircraftPos)
    {
        return aircraftPos + (mouseTargetPos - aircraftPos).normalized * aimingCameraDistance / 2.0f;
    }
}
    
}
