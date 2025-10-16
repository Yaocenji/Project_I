using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance;
        
        public CharacterData data;
        
        public Sprite test;
        
        private void Awake()
        {
            Instance =  this;
            
        }

        private void Start()
        {
            test = data.FindIllustrationSprite("珂赛特", "校服", "抱书", "严肃");
        }
    }
}
