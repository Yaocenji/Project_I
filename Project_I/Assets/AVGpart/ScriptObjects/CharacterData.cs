using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Project_I.AVGpart
{
    
    [Serializable]
    public class CharactorInfo
    {
        // 角色名（唯一识别代号）
        public string Name;
        // 别名（可能有多个）
        public List<string> Aliases;
        // 立绘三维数组，且每个元素带一个string作为标记/表意
        // 第一层：按照服装的类型分，一套衣服一个element；
        // 第二层：同一套衣服下，按照姿势/gesture分；
        // 第三层：同一套衣服的同一个姿势下，按照表情分；
        public List<List<List<Sprite>>>  Illustrations;
    }
    
    [CreateAssetMenu(fileName = "CharacterData", menuName = "GameData_ScriptableObjects/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField]
        public List<CharactorInfo> charactorInfos;
    }

}