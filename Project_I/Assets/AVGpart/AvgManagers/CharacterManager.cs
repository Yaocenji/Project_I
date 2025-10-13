using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    [Serializable]
    public class CharactorInfo
    {
        // 角色名（唯一识别代号）
        public string Name;
        // 别名（可能有多个）
        public List<string> Aliases = new List<string>();
        // 立绘（必然有多个差分）
        public List<Sprite> Illustrations = new List<Sprite>();
    }
    
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager Instance;
        
        // public bool ifReadDataFromFile = false;
        //public string characterInfoFile;
        
        [SerializeField]
        public List<CharactorInfo> charactorInfos;
        private void Awake()
        {
            Instance =  this;
            // 构造角色数据集
            //charactorInfos = new List<CharactorInfo>();
            /*
            if (ifReadDataFromFile)
            {
                //读取文件
                string[] rawText = File.ReadAllLines(Application.dataPath + characterInfoFile);
                // 逐行解析并写入角色数据
                // 当前的角色数据
                CharactorInfo current = new CharactorInfo();
                for (int i = 0; i < rawText.Length; i++)
                {
                    // 空行无意义
                    if (rawText[i] == "")
                    {
                        continue;
                    }
                    // 一个新角色，以character独占一行表示。
                    else if (rawText[i] == "character")
                    {
                        // 提交上一个角色
                        charactorInfos.Add(current);
                        current = new CharactorInfo();
                    }
                    // 指定角色的唯一名字
                    else
                    {
                        string[] words = rawText[i].Split(' ');
                        if (words[0] == "name")
                        {
                            current.Name = words[1];
                        }
                        else if (words[0] == "alias")
                        {
                            for (int j = 1; j < words.Length; j++)
                            {
                                current.Aliases.Add(words[j]);
                            }
                        }
                        else if (words[0] == "illustration")
                        {
                            for (int j = 1; j < words.Length; j++)
                            {
                                current.Illustrations.Add(words[j]);
                            }
                        }
                    }
                }
            }
            */
        }
    }
}
