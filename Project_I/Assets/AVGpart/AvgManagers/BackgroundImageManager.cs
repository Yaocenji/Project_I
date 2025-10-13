using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    [Serializable]
    public class BackgroundImageInfo
    {
        // 唯一识别名称
        public string name;
        // 对应精灵对象
        public Sprite sprite;
    }
    
    public class BackgroundImageManager : MonoBehaviour
    {
        public static BackgroundImageManager Instance;
        
        [SerializeField]
        public  List<BackgroundImageInfo> backgroundImageInfos;

        private void Awake()
        {
            Instance =  this;
        }
    }
}
