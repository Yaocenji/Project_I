using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project_I
{
public class SceneEffectManager : MonoBehaviour
{
    public static SceneEffectManager Instance;

    [Header("实际地图尺寸（左下角坐标+宽高）")]
    public Vector4 orignalMapLayerAABB = new Vector4(-128, -128, 256, 256);

    [Serializable]
    public class LayerData
    {
        public float scale = 1.0f;
        public GameObject layerGameObject = null;
    }
    
    [Header("前景信息")]
    public LayerData[]  foreLayerData = new LayerData[2];
    [Header("背景信息")]
    public LayerData[]  backLayerData = new LayerData[4];

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // 初始化layer的scale
        foreach (var layer in Enumerable.Concat(foreLayerData, backLayerData))
        {
            layer.layerGameObject.transform.localScale *= layer.scale;
        }
    }

    void Update()
    {
        // 玩家世界空间位置
        Vector2 playerPos = GameSceneManager.Instance.player.transform.position;
        // 玩家相对游戏地图AABB的相对位置
        Vector2 playerPosRelative = playerPos - (Vector2)orignalMapLayerAABB;
        // 玩家在游戏地图AABB中的比例位置
        Vector2 playerPosProportion = playerPosRelative / new Vector2(orignalMapLayerAABB.z, orignalMapLayerAABB.w);
        // 设置对应位置
        foreach (var layer in Enumerable.Concat(foreLayerData, backLayerData))
        {
            Vector2 currForeLayerPosRelative = playerPosRelative * layer.scale;
            layer.layerGameObject.transform.position = playerPos - currForeLayerPosRelative
                                    + layer.scale * new Vector2(orignalMapLayerAABB.z,  orignalMapLayerAABB.w) / 2.0f;
        }
        
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
