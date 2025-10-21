using UnityEngine;

namespace Project_I.AVGpart
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;
        
        public SoundData data;
        
        private void Awake()
        {
            Instance =  this;
        }
    }
}