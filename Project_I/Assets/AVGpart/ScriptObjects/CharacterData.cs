using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I.AVGpart
{
    [Serializable]
    public class SingleIllustrationInfo         // 一张立绘（差分）
    {
        public string ExpressionName;
        public Sprite Illustration;
    }
    
    [Serializable]
    public class SamePostureIllustrationsInfo   // 相同服装和姿势的立绘列表，按照表情排
    {
        public string PostureName;
        public List<SingleIllustrationInfo> IlluList;
    }
    
    [Serializable]
    public class SameClothingIllustrationsInfo  // 一套衣服的所有立绘列表，按照姿势排
    {
        public string ClothingName;
        public List<SamePostureIllustrationsInfo> IlluList;
    }
    
    [Serializable]
    public class CharacterIllustrationsInfo  // 一个角色的所有立绘列表，按照服装造型排
    {
        public List<SameClothingIllustrationsInfo> IlluList;
    }

    // [CustomEditor(typeof(CharacterIllustrationsInfo))]
    // public class CharacterIllustrationsInfoEditor : Editor
    // {
    //     public override VisualElement CreateInspectorGUI()
    //     {
    //         // Create a new VisualElement to be the root of our Inspector UI.
    //         VisualElement myInspector = new VisualElement();
    //
    //         // Add a simple label.
    //         myInspector.Add(new Label("This is a custom Inspector"));
    //
    //         // Return the finished Inspector UI.
    //         return myInspector;
    //     }
    // }
    
    
    [Serializable]
    public class CharacterInfo
    {
        // 角色名（唯一识别代号）
        public string Name;
        // 别名（可能有多个）
        public List<string> Aliases;
        
        // 立绘三维数组，且每个元素带一个string作为标记/表意
        // 第一层：按照服装的类型分，一套衣服一个element；
        // 第二层：同一套衣服下，按照姿势/gesture分；
        // 第三层：同一套衣服的同一个姿势下，按照表情分；
        public CharacterIllustrationsInfo  IlluList;
    }
    
    [CreateAssetMenu(fileName = "CharacterData", menuName = "AvgSO/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [SerializeField]
        public List<CharacterInfo> charactorInfos;
        
        // 按照角色、服装、姿势、表情查找对应立绘
        public Sprite FindIllustrationSprite
        (string characterName, string clothingName, string postureName, string expressionName)
        {
            Sprite targetIllustration = null;
            foreach (CharacterInfo characterInfo in charactorInfos) 
            {
                // 查找是否是这个角色
                bool isThisCharacter = false;
                if (characterName is null)
                {
                    isThisCharacter = false;
                }
                else if (characterName.Equals(characterInfo.Name))
                {
                    isThisCharacter = true;
                }
                else
                {
                    foreach (var alias in characterInfo.Aliases)
                    {
                        if (characterName.Equals(alias))
                        {
                            isThisCharacter = true;
                            break;
                        }
                    }
                }
                if (!isThisCharacter) continue;
                
                // 确定当前角色就是要找的角色，根据服装查找立绘
                foreach (var clothingIllustrations in characterInfo.IlluList.IlluList)
                {
                    if (!clothingName.Equals(clothingIllustrations.ClothingName))
                        continue;
                    // 确定当前服装正确
                    foreach (var postureIllustrations in clothingIllustrations.IlluList)
                    {
                        if (!postureName.Equals(postureIllustrations.PostureName))
                            continue;
                        // 确定当前姿势正确
                        foreach (var expressionIllustration in postureIllustrations.IlluList)
                        {
                            if  (!expressionName.Equals(expressionIllustration.ExpressionName))
                                continue;
                            // 找到当前立绘差分
                            targetIllustration = expressionIllustration.Illustration;
                            
                            break;
                        }
                        break;
                    }
                    break;
                }
                break;
            }
            return targetIllustration;
        }
    }

}