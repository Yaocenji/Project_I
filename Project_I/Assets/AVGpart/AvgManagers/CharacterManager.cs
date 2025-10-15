using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance;
        
        public readonly CharacterData data;
        
        private void Awake()
        {
            Instance =  this;
            
        }
    }
}
