using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class SceneEffectManager : MonoBehaviour
{
    public static SceneEffectManager Instance;

    [Header("实际地图尺寸（左下角坐标+宽高）")]
    public Vector4 orignalMapLayerAABB = new Vector4(-128, -128, 256, 256);

    [Header("第0层前景缩放倍数")]
    public float foreGroundLayer0Scale = 2.0f;
    
    [Header("第1层前景缩放倍数")]
    public float foreGroundLayer1Scale = 4.0f;

    public GameObject foreGroundLayer0;
    public GameObject foreGroundLayer1;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // 从玩家在origin地图的所在位置比例
        Vector2 playerPos = GameSceneManager.Instance.player.transform.position;
        
        Vector2 playerPosRelative = playerPos - (Vector2)orignalMapLayerAABB;
        
        Vector2 playerPosProportion = playerPosRelative / new Vector2(orignalMapLayerAABB.z, orignalMapLayerAABB.w);

        Vector2 foreGroundLayer0PosRelative = playerPosRelative * foreGroundLayer0Scale;
        
        Vector2 foreGroundLayer1PosRelative = playerPosRelative * foreGroundLayer1Scale;

        foreGroundLayer0.transform.position = playerPos - foreGroundLayer0PosRelative;
        
        foreGroundLayer1.transform.position = playerPos - foreGroundLayer1PosRelative;
        
        Debug.Log(playerPosProportion);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        
        // draw top line
        Gizmos.DrawLine(new Vector3(orignalMapLayerAABB.x, orignalMapLayerAABB.y), 
                        new Vector3(orignalMapLayerAABB.x + orignalMapLayerAABB.z, 
                                    orignalMapLayerAABB.y));
        
        // draw bottom line
        Gizmos.DrawLine(new Vector3(orignalMapLayerAABB.x, 
                                    orignalMapLayerAABB.y + orignalMapLayerAABB.w), 
                        new Vector3(orignalMapLayerAABB.x + orignalMapLayerAABB.z, 
                                    orignalMapLayerAABB.y + orignalMapLayerAABB.w));
        
        // draw right line
        Gizmos.DrawLine(new Vector3(orignalMapLayerAABB.x + orignalMapLayerAABB.z, 
                                    orignalMapLayerAABB.y),
                        new Vector3(orignalMapLayerAABB.x + orignalMapLayerAABB.z, 
                                    orignalMapLayerAABB.y + orignalMapLayerAABB.w));
        
        // draw left line
        Gizmos.DrawLine(new Vector3(orignalMapLayerAABB.x, orignalMapLayerAABB.y),
                        new Vector3(orignalMapLayerAABB.x, 
                            orignalMapLayerAABB.y + orignalMapLayerAABB.w));
        
    }
}
    
}
