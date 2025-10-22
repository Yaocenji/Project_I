using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project_I.AVGpart
{
    // 动作基类
    public abstract class PerformMove
    {
        public bool isCompleted;
        
        public Action Callback;

        public PerformMove()
        {
            isCompleted = false;
        }
        
        public abstract void Execute(ref float deltaTime);
        public abstract void ExecuteToEnd();
        
        public virtual void Finish()
        {
            isCompleted = true;
            FinishCallback();
        }

        public virtual void FinishCallback()
        {
            if (Callback != null)
            {
                Callback();
            }
        }
    }
    
    // 动作：切换sprite
    public class PerformMove_SwitchIllustration: PerformMove
    {
        
        // 当前要操作的对象
        public Image img;
        // 要切换的目标图片
        public Sprite targetSprite;

        public PerformMove_SwitchIllustration(Image img, Sprite targetSprite)
        {
            this.img = img;
            this.targetSprite = targetSprite;
        }
        public override void Execute(ref float deltaTime)
        {
            ExecuteToEnd();
            deltaTime -= 0;
        }

        public override void ExecuteToEnd()
        {
            img.sprite = targetSprite;
            Finish();
        }
    }
    
    // 空动作：什么也不干
    public class PerformMove_DoNothing : PerformMove
    {
        // 什么也不干的总时长：
        public float PROCESS_TIME = 1.0f;
        // 已经的进度
        private float t;

        public PerformMove_DoNothing(float time = 1.0f)
        {
            PROCESS_TIME = time;
        }
        public override void Execute(ref float deltaTime)
        {
            // 尝试判定是否结束了？
            if (t + deltaTime >= PROCESS_TIME)  // 已结束
            {
                Finish();
                deltaTime -= PROCESS_TIME - t;
            }
            else
            {
                // 未结束
                t += deltaTime;
                deltaTime = 0;
            }
        }
        public override void ExecuteToEnd()
        {
            Finish();
        }
    }
    
    // 基动作：某个浮点值直接变化到某个目标值
    public abstract class PerformMove_FloatChange : PerformMove
    {
        // 目标该值
        public float targetValue = 1.0f;

        public override void Execute(ref float deltaTime)
        {
            ExecuteToEnd();
            deltaTime -= 0;
        }

        public override void ExecuteToEnd()
        {
            SetValue(targetValue);
            Finish();
        }

        public abstract float GetValue();
        public abstract void SetValue(float newValue);
    }
    
    // 基动作：某个浮点值平滑变化到目标值
    public abstract class PerformMove_FloatSmoothProcess: PerformMove
    {
        // 完成平滑变化的总时长：
        public float PROCESS_TIME = 1.0f;
        // 目标该值
        public float targetValue = 1.0f;
        
        // 一个用来标记是不是第一次execute的flag
        private bool neverExecuted;
        // 最初的该值
        private float oriValue;
        // 已经运行的进度
        private float t;
        public PerformMove_FloatSmoothProcess():base()
        {
            neverExecuted = true;
        }

        public override void Execute(ref float deltaTime)
        {
            // 第一次调用，记录当前的alpha值
            if (neverExecuted)
            {
                oriValue = GetValue();
                t = 0;
                neverExecuted = false;
            }
            // 正常的过程
            float newValue = Mathf.SmoothStep(oriValue, targetValue, (t + deltaTime) / PROCESS_TIME);
            SetValue(newValue);
            
            // 尝试判定是否结束了？
            if (t + deltaTime >= PROCESS_TIME)  // 已结束
            {
                Finish();
                deltaTime -= PROCESS_TIME - t;
            }
            else
            {
                // 未结束
                t += deltaTime;
                deltaTime = 0;
            }
        }

        public override void ExecuteToEnd()
        {
            SetValue(targetValue);
            Finish();
        }

        public abstract float GetValue();
        public abstract void SetValue(float newValue);
    }
    
    // 动作：图片alpha淡入淡出
    public class PerformMove_ImageFadeTo : PerformMove_FloatSmoothProcess
    {
        public Image targetImg;

        public PerformMove_ImageFadeTo(Image img, float value = 1.0f, float process_time = 1.0f) : base()
        {
            targetImg = img;
            PROCESS_TIME = process_time;
            targetValue = value;
        }

        public override float GetValue()
        {
            return targetImg.color.a;
        }

        public override void SetValue(float newValue)
        {
            targetImg.color = new Color(targetImg.color.r, targetImg.color.g, targetImg.color.b, newValue);
        }
    }
    
    // 动作：文字tmp淡入淡出
    public class PerformMove_TextMeshProFadeTo : PerformMove_FloatSmoothProcess
    {
        public TextMeshProUGUI targetText;

        public PerformMove_TextMeshProFadeTo(TextMeshProUGUI text, float value = 1.0f, float process_time = 1.0f) : base()
        {
            targetText = text;
            PROCESS_TIME = process_time;
            targetValue = value;
        }

        public override float GetValue()
        {
            return targetText.color.a;
        }

        public override void SetValue(float newValue)
        {
            targetText.color = new Color(targetText.color.r, targetText.color.g, targetText.color.b, newValue);
        }
    }
    
    // 动作：canvasgroup淡入淡出
    public class PerformMove_CanvasGroupFadeTo : PerformMove_FloatSmoothProcess
    {
        public CanvasGroup canvasGroup;
        public PerformMove_CanvasGroupFadeTo(CanvasGroup cvg, float value = 1.0f, float process_time = 1.0f) : base()
        {
            canvasGroup = cvg;
            PROCESS_TIME = process_time;
            targetValue = value;
        }
        public override float GetValue()
        {
            return canvasGroup.alpha;
        }

        public override void SetValue(float newValue)
        {
            canvasGroup.alpha = newValue;
        }
    }
    
    
    // 动作：声音音量淡入淡出
    public class PerformMove_AudioVolumeFadeTo : PerformMove_FloatSmoothProcess
    {
        public AudioSource audioSource;

        public PerformMove_AudioVolumeFadeTo(AudioSource audioSource, float value = 1.0f, float process_time = 1.0f) :
            base()
        {
            targetValue = value;
            this.audioSource = audioSource;
            PROCESS_TIME = process_time;
        }

        public override float GetValue()
        {
            return audioSource.volume;
        }

        public override void SetValue(float newValue)
        {
            audioSource.volume = newValue;
        }
    }
    
    // 动作：文本逐个打出
    public class PerformMove_TextTypewritter : PerformMove
    {
        // 目标文本
        public string targetText;
        // 目标文本显示框
        public TextMeshProUGUI textUI;
        // 出字间隔
        public float singleWordTime = 0.15f;
        
        // 已经运行的时间进度
        private float t;
        // 已经打出的字数
        private int n;

        public PerformMove_TextTypewritter(string targetText, TextMeshProUGUI textUI, float singleWordTime = 0.15f):base()
        {
            t = 0;
            n = 0;
            this.targetText = targetText;
            this.textUI = textUI;
            this.singleWordTime = singleWordTime;
        }
        
        public override void Execute(ref float deltaTime)
        {
            t += deltaTime;
            n = (int)(t / singleWordTime);
            textUI.text = targetText.Substring(0, Mathf.Min(n, targetText.Length));
            if (n >= targetText.Length)
            {
                Finish();
            }
        }

        public override void ExecuteToEnd()
        {
            textUI.text = targetText;
            Finish();
        }
    }
    
    
    // 动作序列
    public class PerformMoveSequence
    {
        // 是否执行完毕
        public bool isCompleted;
        
        // 动作列表
        public List<PerformMove> moves;
        // 当前运行的动作
        public int currMoveIndex = 0;

        public PerformMoveSequence()
        {
            moves = new List<PerformMove>();
            isCompleted =  false;
        }
        
        // 执行
        public void Execute(ref float deltaTime)
        {
            while (true)
            {
                // 进行一次执行
                moves[currMoveIndex].Execute(ref deltaTime);
                // 如果没有执行完毕，直接退出
                if (!moves[currMoveIndex].isCompleted)
                    break;
                // 如果恰好执行完毕该过程
                // 下一步
                currMoveIndex++;
                // 如果整个序列都执行完了，直接退出
                if (currMoveIndex >= moves.Count)
                    break;
                // 否则就直接推进
                continue;
            }
            // 如果整个序列都执行完了
            if (currMoveIndex >= moves.Count)
            {
                Finish();
            }
        }
        
        // 序列执行完毕
        public void Finish()
        {
            isCompleted =  true;
        }
    }
}