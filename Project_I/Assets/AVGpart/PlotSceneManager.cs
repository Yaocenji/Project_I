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
    public class PlotScenePlayer: MonoBehaviour
    {
        // 当前游玩的剧情脚本路径
        public string scriptPath;
        
        public TextAsset testTextAsset;
        
        // 文件读取器
        public StreamReader scriptReader;
        
        // 用户输入
        public PlayerInput _playerInput;
        
        // 播放过程中的变量
        public Sprite currBackground;
        public Sprite currCg;
        
        // 分割后的每一行文本
        private string[] plotTextLines;
        
        // 读取位置
        private int readIndex;

        private void Start()
        {
            scriptReader = new StreamReader(scriptPath);

            plotTextLines = testTextAsset.ToString().Split("\n");
            
            _playerInput =  new PlayerInput();
            _playerInput.Enable();
            _playerInput.Player.PlayNext.canceled += PlayNext;

            readIndex = 0;
        }

        private void OnDestroy()
        {
            _playerInput.Player.PlayNext.canceled -= PlayNext;
            _playerInput.Disable();
        }

        // 核心方法：下一步！
        private void PlayNext(InputAction.CallbackContext obj)
        {
            while (true)
            {
                // 如果播放完毕
                if (readIndex >= plotTextLines.Length)
                {
                    PlayEnd();
                }
                
                // 当前脚本行
                string currLine = plotTextLines[readIndex];
                // 开始分析
                if (currLine == "" || currLine == " ")  // 空白行直接跳过
                {
                    // do nothing.
                }
                else // 有内容的行
                {
                    if (currLine.StartsWith("## ")) // 用开头的标志识别：这是小章节标题行
                    {
                        string chapterName = currLine.Substring(3,  currLine.Length - 3);
                        
                    }
                }

                readIndex++;
            }
        }
        
        private 
        
        // 播放结束后的
        private void PlayEnd()
        {
            // TODO
        }
    }
}
