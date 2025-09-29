using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
    namespace AVGpart
    {
        public class SentencePush
        {
            // 是否是普通场景/CG场景
            public bool IsNormalOrCgBackground = true;
            
            // 场景编号/CG编号
            public int BackgroundIndex;
            
            // 是否有对话
            public bool HasDialogue = true;
            
            // -1表示无立绘（心理活动或主角台词）
            public int CharacterIndex;
            public string CharacterName;
            
            public int StandingImageIndex;
            public string StandingImageName;
            
            // 台词内容
            public string Text;
        }
    }
}
