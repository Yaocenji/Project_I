using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using EventBus = Project_I.EventSystem.EventBus;

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
    
    
    // A队伍的transform合集
    public HashSet<Transform> PartyATransforms;
    // B队伍的transform合集
    public HashSet<Transform> PartyBTransforms;

    private void Awake()
    {
        Instance = this;

        PartyATransforms = new HashSet<Transform>();
        PartyBTransforms = new HashSet<Transform>();
        
        _fiend = new HashSet<GameObject>();
        _enemy = new HashSet<GameObject>();
        
        // 注册事件：
        EventBus.Subscribe<EventSystem.PlayerAttackedEvent>(PlayerHit);
    }

    private void OnDestroy()
    {
        EventBus.Unsubscribe<EventSystem.PlayerAttackedEvent>(PlayerHit);
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
        
        if (pl.layer == LayerDataManager.Instance.playerLayer)
        {
            _player = pl;
            PartyATransforms.Add(pl.transform);
            return true;
        }

        return false;
    }
    
    // 注册友军
    public void RegisterFriend(GameObject ff)
    {
        if (ff.layer == LayerDataManager.Instance.friendlyLayer)
        {
            Debug.Log("注册友方单位");
            _fiend.Add(ff);
            PartyATransforms.Add(ff.transform);
        }
    }
    
    // 注册敌人
    public void RegisterEnemy(GameObject en)
    {
        if (en.layer == LayerDataManager.Instance.enemyLayer)
        {
            Debug.Log("注册敌方单位");
            _enemy.Add(en);
            PartyBTransforms.Add(en.transform);
            EventSystem.EventBus.Publish(new EventSystem.EnemyRegisteredEvent(en));
        }
    }
    
    // 注册准星
    public bool RegisterFrontSight(GameObject fs)
    {
        if (_frontSight is not null)
            return false;
        _frontSight = fs;
        return true;
    }
    
    // 友方单位死亡
    public void DieFriend(GameObject ff)
    {
        if (_fiend.Contains(ff))
        {
            _fiend.Remove(ff);
            PartyATransforms.Remove(ff.transform);
        }
    }
    // 敌方单位死亡
    public void DieEnemy(GameObject en)
    {
        if (_enemy.Contains(en))
        {
            _enemy.Remove(en);
            PartyBTransforms.Remove(en.transform);
            EventSystem.EventBus.Publish(new EventSystem.EnemyDiedEvent(en));
        }
        Destroy(en, 0.05f);
    }
    
    
    // 玩家受击
    public void PlayerHit(EventSystem.PlayerAttackedEvent ev)
    {
        CameraController.Instance.AddSingleShake(-0.5f * ev.Direction);
    }
}
}