using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    [Serializable]
    public class SoundInfo
    {
        // 角色名（唯一识别代号）
        public string Name;
        // 声音
        public AudioClip  sound;
    }
    
    [CreateAssetMenu(fileName = "SoundData", menuName = "AvgSO/SoundData")]
    public class SoundData : ScriptableObject
    {
        [SerializeField]
        public List<SoundInfo> soundInfos;
        
        // 查找声音
        public AudioClip FindSound(string soundName)
        {
            AudioClip targetSound = null;
            foreach (SoundInfo soundInfo in soundInfos)
            {
                if (soundName.Equals(soundInfo.Name))
                {
                    targetSound = soundInfo.sound;
                    break;
                }
            }
            return targetSound;
        }
    }
}