using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
    namespace AVGpart
    {
        // AVG中的“一步”操作
        public abstract class BasicStep
        {
            public abstract void Execute();
        }
        
        
        
        public class SentencePush: BasicStep
        {
            // 是否有对话
            public bool HasDialogue = true;
            
            // 当前说话的角色索引与角色名字，-1表示无
            public int CharacterIndex;
            public string CharacterName;
            
            public int StandingImageIndex;
            public string StandingImageName;
            
            // 台词内容
            public string Text;

            public override void Execute()
            {
                // TODO realize function Execute()
            }
        }
        
        
        public enum BackgroundType
        {
            Normal,
            Cg
        };

        public class AvgScene
        {
            // 是否是普通场景/CG场景
            public BackgroundType IsNormalOrCgBackground =  BackgroundType.Normal;
            
            // 场景编号/CG编号
            public int BackgroundIndex;
        }
    }
}
