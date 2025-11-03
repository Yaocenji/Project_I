using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    [Serializable]
    public class SingleCgIllustrationInfo
    {
        public string DiffName;
        public Sprite Illustration;
    }
    
    [Serializable]
    public class CgIllustrationsInfo
    {
        public List<SingleCgIllustrationInfo> IlluList;
    }

    [Serializable]
    public class CgInfo
    {
        // 背景名
        public string Name;
        
        // 背景图一层数组，一个场景的时间/状态差分
        public CgIllustrationsInfo  IlluList;
    }
    
    
    [CreateAssetMenu(fileName = "CgData", menuName = "AvgSO/CgData")]
    public class CgData : ScriptableObject
    {
        [SerializeField]
        public List<CgInfo> cgInfos;
        
        // 通过场景名和差分名查找CG图
        public Sprite FindIllustrationSprite(string cgName, string diffName)
        {
            Sprite targetIllustration = null;
            foreach (var cgInfo in cgInfos) 
            {
                if (!cgName.Equals(cgInfo.Name))
                {
                    continue;
                }
                foreach (var singleCgIllustrationInfo in cgInfo.IlluList.IlluList)
                {
                    if (!diffName.Equals(singleCgIllustrationInfo.DiffName))
                    {
                        continue;
                    }
                    targetIllustration = singleCgIllustrationInfo.Illustration;
                    break;
                }
                break;
            }
            return targetIllustration;
        }
    }
}