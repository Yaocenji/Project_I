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

    /*public float MapLayerRect.z = 1200;
    public float MapLayerRect.w = 900;*/

    [Header("实际地图尺寸（中心位置+宽高）")]
    public Vector4 MapLayerRect =  new Vector4(0, 0, 2500, 1440);

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
        //MapLayerRect = new Vector4(MapLayerRect.x - MapLayerRect.z * 0.5f, MapLayerRect.y - MapLayerRect.w * 0.5f, MapLayerRect.z, MapLayerRect.w);
        
        // 初始化layer的scale
        foreach (var layer in Enumerable.Concat(foreLayerData, backLayerData))
        {
            layer.layerGameObject.transform.localScale *= layer.scale;
        }
    }

    void Update()
    {
        // 玩家世界空间位置
        Vector2 playerPos = GameSceneManager.Instance.Player.transform.position;
        // 玩家相对游戏地图AABB的相对位置
        Vector2 playerPosRelative = playerPos - (Vector2)MapLayerRect;
        // 玩家在游戏地图AABB中的比例位置
        //Vector2 playerPosProportion = playerPosRelative / new Vector2(MapLayerRect.z, MapLayerRect.w);
        // 设置对应位置
        foreach (var layer in Enumerable.Concat(foreLayerData, backLayerData))
        {
            Vector2 currLayerPosRelative = playerPosRelative * layer.scale;
            layer.layerGameObject.transform.position = playerPos - currLayerPosRelative;
        }
        
        //Debug.Log(playerPosProportion);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        
        // draw top line
        Gizmos.DrawLine(new Vector3(MapLayerRect.x - MapLayerRect.z * 0.5f, MapLayerRect.y - MapLayerRect.w * 0.5f), 
                        new Vector3(MapLayerRect.x - MapLayerRect.z * 0.5f + MapLayerRect.z, 
                                    MapLayerRect.y - MapLayerRect.w * 0.5f));
        
        // draw bottom line
        Gizmos.DrawLine(new Vector3(MapLayerRect.x - MapLayerRect.z * 0.5f, 
                                    MapLayerRect.y - MapLayerRect.w * 0.5f + MapLayerRect.w), 
                        new Vector3(MapLayerRect.x - MapLayerRect.z * 0.5f + MapLayerRect.z, 
                                    MapLayerRect.y - MapLayerRect.w * 0.5f + MapLayerRect.w));
        
        // draw right line
        Gizmos.DrawLine(new Vector3(MapLayerRect.x - MapLayerRect.z * 0.5f + MapLayerRect.z, 
                                    MapLayerRect.y - MapLayerRect.w * 0.5f),
                        new Vector3(MapLayerRect.x - MapLayerRect.z * 0.5f + MapLayerRect.z, 
                                    MapLayerRect.y - MapLayerRect.w * 0.5f + MapLayerRect.w));
        
        // draw left line
        Gizmos.DrawLine(new Vector3(MapLayerRect.x - MapLayerRect.z * 0.5f, MapLayerRect.y - MapLayerRect.w * 0.5f),
                        new Vector3(MapLayerRect.x - MapLayerRect.z * 0.5f, 
                            MapLayerRect.y - MapLayerRect.w * 0.5f + MapLayerRect.w));
        
    }
}
    
}
