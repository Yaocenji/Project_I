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
        
        // 文件读取器
        public StreamReader scriptReader;
        
        // 用户输入
        public PlayerInput _playerInput;
        
        // 播放过程中的变量
        public Sprite currBackground;
        public Sprite currCg;

        private void Start()
        {
            scriptReader = new StreamReader(scriptPath);
            
            _playerInput =  new PlayerInput();
            _playerInput.Enable();
            _playerInput.Player.PlayNext.canceled += PlayNext;
        }

        // 核心方法：下一步！
        private void PlayNext(InputAction.CallbackContext obj)
        {
            //throw new NotImplementedException();
        }
    }
}
