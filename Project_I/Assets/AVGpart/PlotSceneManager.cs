using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

// 场景（一段叙事）
/*
 * Project_I场景数据结构层级规范：
 * 最大的单位：幕。这个作为游戏的设计但是不在数据结构里面体现（可能仅仅在UI中体现）
 * 一幕中有N段剧情，每一段单独从时间线菜单里面点进去玩，玩完了自动退回时间线菜单，这个叫做scene
 */
namespace Project_I.AVGpart
{
    public class PlotSceneManager: MonoBehaviour
    {
        static public PlotSceneManager Instance;
        
        // 当前游玩的剧情脚本路径
        public string scriptPath;
        public TextAsset testTextAsset;
        
        // 文件读取器
        public StreamReader scriptReader;
        
        // 用户输入
        public PlayerInput _playerInput;
        
        // 自动播放
        public bool autoPlay = false;
        // 单句自动播放
        public bool autoPlayOnce = false;
        
        // 分割后的每一行文本
        private string[] plotTextLines;
        
        // 读取位置
        private int readIndex;
        
        // 播放状态
        private bool oldIsPlaying;
        private bool isPlaying;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            //scriptReader = new StreamReader(scriptPath);
            testTextAsset = Resources.Load<TextAsset>(scriptPath) as TextAsset;

            plotTextLines = testTextAsset.ToString().Split("\n");
            //Debug.Log(plotTextLines[0]);
            
            _playerInput =  new PlayerInput();
            _playerInput.Enable();
            _playerInput.Player.PlayNext.canceled += PlayNext;

            readIndex = 0;

            PlayNext();
        }

        private void OnDestroy()
        {
            _playerInput.Player.PlayNext.canceled -= PlayNext;
            _playerInput.Disable();
        }

        private void Update()
        {
            isPlaying = PlotScenePlayer.Instance.IsPlaying;

            /*if (oldIsPlaying && !isPlaying)
            {
                PlayEnd();
            }*/
            
            oldIsPlaying = isPlaying;
        }

        public void PlayNext(InputAction.CallbackContext obj)
        {
            if (!PlotScenePlayer.Instance.IsPlaying)
                PlayNext();
            else
                SkipOneStep();
        }

        // 核心方法：下一步！
        private void PlayNext()
        {
            while (true)
            {
                // 如果播放完毕
                if (readIndex >= plotTextLines.Length)
                {
                    SceneEnd();
                }
                
                // 当前脚本行
                string currLine = plotTextLines[readIndex];
                if (currLine.EndsWith("\r"))
                {
                    currLine = currLine.Substring(0, currLine.Length - 1);
                }
                
                // 开始分析
                if (currLine == "" || currLine == " " || currLine == "\r")  // 空白行直接跳过
                {
                    // do nothing.
                    Debug.Log(readIndex);
                }
                else // 有内容的行
                {
                    if (currLine.StartsWith("<初始化>") || currLine.StartsWith("<Initialization>"))
                    {
                        PlotScenePlayer.Instance.InitPlay();
                    }
                    else if (currLine.StartsWith("## ")) // 这表示一个标题，即切换场景
                    {
                        string chapterName = currLine.Substring(3, currLine.Length - 3);
                        PlotScenePlayer.Instance.DisplayChapter(chapterName);
                        // 不应当退出
                        // break;
                    }
                    else if (currLine.StartsWith("END")) // 这表示一个标题，即切换场景
                    {
                        PlotScenePlayer.Instance.EndChapter();
                        break;
                    }
                    else if (currLine.StartsWith("<设置背景>") || currLine.StartsWith("<Background>"))
                    {
                        string[] backgroundData = currLine.Split(' ');
                        PlotScenePlayer.Instance.SetBackground(backgroundData[1], backgroundData[2]);
                    }
                    else if (currLine.StartsWith("<设置CG>") || currLine.StartsWith("<CG>"))
                    {
                        string[] cgData = currLine.Split(' ');
                        PlotScenePlayer.Instance.SetCG(cgData[1], cgData[2]);
                    }
                    else if (currLine.StartsWith("<出场人物>") || currLine.StartsWith("<Character>"))
                    {
                        string[] characterStr = currLine.Split(' ');
                        for (int i = 1; i < characterStr.Length; i++)
                        {
                            PlotScenePlayer.Instance.AddCharacter(characterStr[i]);
                        }
                    }
                    else if (currLine.StartsWith("<人物别名>") || currLine.StartsWith("<Alias>"))
                    {
                        string[] characterStr = currLine.Split(' ');
                        PlotScenePlayer.Instance.SetCharacterAlias(characterStr[1], characterStr[2]);
                    }
                    else if (currLine.StartsWith("<循环播放>") || currLine.StartsWith("<LoopPlay>"))
                    {
                        string[] str = currLine.Split(' ');
                        string soundName = str[1];
                        float soundVolume = str.Length >= 3 ? float.Parse(str[2]) : 1.0f;
                        float soundStereo = str.Length >= 4 ? float.Parse(str[3]) : 0.0f;

                        PlotScenePlayer.Instance.PlaySound(soundName, true, soundVolume, soundStereo);
                    }
                    else if (currLine.StartsWith("</循环播放>") || currLine.StartsWith("</LoopPlay>"))
                    {
                        string[] str = currLine.Split(' ');
                        string soundName = str[1];

                        PlotScenePlayer.Instance.EndSound(soundName);
                    }
                    else if (currLine.StartsWith("<单次播放>") || currLine.StartsWith("<OncePlay>"))
                    {
                        string[] str = currLine.Split(' ');
                        string soundName = str[1];
                        float soundVolume = str.Length >= 3 ? float.Parse(str[2]) : 1.0f;
                        float soundStereo = str.Length >= 4 ? float.Parse(str[3]) : 0.0f;

                        PlotScenePlayer.Instance.PlaySound(soundName, false, soundVolume, soundStereo);
                    }

                    else if (currLine.StartsWith("--手动断点--") || currLine.StartsWith("--ManualBreakPoint--"))
                    {
                        break;
                    }
                    else if (currLine.StartsWith("--单次自动播放断点--") || currLine.StartsWith("--AutoBreakPoint--"))
                    {
                        autoPlayOnce = true;
                        break;
                    }
                    else if (currLine.StartsWith("<设置立绘>") || currLine.StartsWith("<Illustration>"))
                    {
                        string[] characterStr = currLine.Split(' ');
                        PlotScenePlayer.Instance.SetIllustration(characterStr[1], characterStr[2], characterStr[3], characterStr[4]);
                    }
                    else if (currLine.StartsWith("<立绘位置>") || currLine.StartsWith("<IllustrationPos>"))
                    {
                        string[] characterStr = currLine.Split(' ');
                        int p = 0;
                        switch (characterStr[2])
                        {
                            case "左":
                            {
                                p = 0;
                                break;
                            }
                            case "中":
                            {
                                p = 1;
                                break;
                            }
                            case "右":
                            {
                                p = 2;
                                break;
                            }
                            case "闲":
                            {
                                p = -1;
                                break;
                            }
                        }
                        PlotScenePlayer.Instance.SetIllustrationPosition(characterStr[1], p);
                    }
                    else if (currLine.StartsWith("<对话框>") || currLine.StartsWith("<DialogBox>"))
                    {
                        string[] str = currLine.Split(' ');
                        if (str[1] == "开")
                            PlotScenePlayer.Instance.SetDialogBoxDisplay(true);
                        else if (str[1] == "关")
                            PlotScenePlayer.Instance.SetDialogBoxDisplay(false);
                        else
                        {
                            // TODO: 报错
                        }
                    }
                    else if (currLine.StartsWith("> ")) // 这一行是旁白/主角的内心活动
                    {
                        string text = currLine.Substring(2, currLine.Length - 2);
                        PlotScenePlayer.Instance.DisplayNarrator(text);
                        break;
                    }
                    else if (currLine.StartsWith("**")) // 对话
                    {
                        string[] str = currLine.Split('：');
                        // 人物名字
                        string characterName = str[0].Substring(2, str[0].Length - 4);
                        // 对话内容
                        string dialogText = str[1];

                        PlotScenePlayer.Instance.DisplayDialog(characterName, dialogText);

                        break;
                    }
                    else
                    {
                        // TODO: Temporally Do Nothing.
                        Debug.Log("未识别行：" + currLine);
                    }
                }

                readIndex++;
            }
            readIndex++;
        }
        
        // 一句话播放结束
        public void PlayEnd()
        {
            if (autoPlay)
                PlayNext();
            else if (autoPlayOnce)
            {
                PlayNext();
                autoPlayOnce = false;
                Debug.Log("触发单次自动播放");
            }
        }
        
        // 播放到一半时，点击快进
        public void SkipOneStep()
        {
            PlotScenePlayer.Instance.SkipOneStep();
        }
        
        // 整个播放结束后的
        private void SceneEnd()
        {
            // TODO
        }
    }
}
