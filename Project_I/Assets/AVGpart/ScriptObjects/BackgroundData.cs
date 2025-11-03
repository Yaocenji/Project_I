using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    [Serializable]
    public class SingleBackgroundIllustrationInfo
    {
        public string DiffName;
        public Sprite Illustration;
    }
    
    [Serializable]
    public class BackgroundIllustrationsInfo
    {
        public List<SingleBackgroundIllustrationInfo> IlluList;
    }

    [Serializable]
    public class BackgroundInfo
    {
        // 背景名
        public string Name;
        
        // 背景图一层数组，一个场景的时间/状态差分
        public BackgroundIllustrationsInfo  IlluList;
    }
    
    [CreateAssetMenu(fileName = "BackgroundData", menuName = "AvgSO/BackgroundData")]
    public class BackgroundData : ScriptableObject
    {
        [SerializeField]
        public List<BackgroundInfo> backgroundInfos;
        
        // 通过场景名和差分名查找背景图
        public Sprite FindIllustrationSprite(string backgroundName, string diffName)
        {
            Sprite targetIllustration = null;
            foreach (BackgroundInfo backgroundInfo in backgroundInfos) 
            {
                if (!backgroundName.Equals(backgroundInfo.Name))
                {
                    continue;
                }
                foreach (var singleBackgroundIllustrationInfo in backgroundInfo.IlluList.IlluList)
                {
                    if (!diffName.Equals(singleBackgroundIllustrationInfo.DiffName))
                    {
                        continue;
                    }
                    targetIllustration = singleBackgroundIllustrationInfo.Illustration;
                    break;
                }
                break;
            }
            return targetIllustration;
        }
    }
}
