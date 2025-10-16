using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    [Serializable]
    public class SingleBackgroundIllustrationInfo
    {
        public string ConditionName;
        public Sprite Illustration;
    }
    
    [Serializable]
    public class BackgroundIllustrationsInfo
    {
        List<SingleBackgroundIllustrationInfo> IlluList;
    }

    [Serializable]
    public class BackgroundInfo
    {
        // 背景名
        public string Name;
        
        // 背景图一层数组，一个场景的时间/状态差分
        public BackgroundIllustrationsInfo  IlluList;
    }
    
    [CreateAssetMenu(fileName = "BackgroundData", menuName = "GameData_ScriptableObjects/BackgroundData")]
    public class BackgroundData : ScriptableObject
    {
        [SerializeField]
        public List<BackgroundInfo> backgroundInfos;
    }
}
