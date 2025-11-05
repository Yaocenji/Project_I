using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;

    private Camera _mainCamera;
    public Camera mainCamera
    {
        get => _mainCamera;
    }

    private GameObject _player;
    public GameObject Player { get => _player; }


    private HashSet<GameObject> _fiend;
    public HashSet<GameObject> Friend { get => _fiend; }

    private HashSet<GameObject> _enemy;
    public HashSet<GameObject> Enemy { get => _enemy; }

    private GameObject _frontSight;
    public GameObject FrontSight { get => _frontSight; }

    private void Awake()
    {
        Instance = this;

        _fiend = new HashSet<GameObject>();
        _enemy = new HashSet<GameObject>();
    }
    
    // 注册主摄像机
    public bool RegisterMainCamera(Camera mc)
    {
        if (_mainCamera is not null)
            return false;
        _mainCamera = mc;
        return true;
    }

    // 注册玩家
    public bool RegisterPlayer(GameObject pl)
    {
        if (_player is not null)
            return false;
        _player = pl;
        return true;
    }
    
    // 注册友军
    public void RegisterFriend(GameObject ff)
    {
        ff.AddComponent<GameSceneManager>();
    }
    
    // 注册敌人
    public void RegisterEnemy(GameObject en)
    {
        _enemy.Add(en);
    }
    
    // 注册准星
    public bool RegisterFrontSight(GameObject fs)
    {
        if (_frontSight is not null)
            return false;
        _frontSight = fs;
        return true;
    }
    
}
}