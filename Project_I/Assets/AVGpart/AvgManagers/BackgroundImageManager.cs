using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    
    public class BackgroundImageManager : MonoBehaviour
    {
        public static BackgroundImageManager Instance;

        public BackgroundData data;
        
        public Sprite test;

        private void Awake()
        {
            Instance =  this;
        }

        private void Start()
        {
            test = data.FindIllustrationSprite("公馆套房", "月夜");
        }
    }
}
