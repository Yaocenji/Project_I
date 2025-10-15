using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 场景（一段叙事）
/*
 * Project_I场景数据结构层级规范：
 * 最大的单位：幕。这个作为游戏的设计但是不在数据结构里面体现（可能仅仅在UI中体现）
 * 一幕中有N段剧情，每一段单独从时间线菜单里面点进去玩，玩完了自动退回时间线菜单，这个叫做“序列”即sequence
 * 一个sequence里面包含多个scene，每一个scene就在一个地方发生，所以它的背景图也就一张，CG图可能不止一张，CG图和背景图分开算的
 */
namespace Project_I.AVGpart
{
    public class PlotScene: MonoBehaviour
    {
        public Sprite backgroundTexture;
        
    }
}
