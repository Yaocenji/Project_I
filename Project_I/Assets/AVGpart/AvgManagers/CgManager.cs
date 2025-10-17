using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    public class CgManager : MonoBehaviour
    {
        public static CgManager Instance;
        
        public CgData data;

        public Sprite test;
        
        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            test = data.FindIllustrationSprite("序章_珂赛特废墟救出卡洛斯", "无差分");
        }
    }
    
}
