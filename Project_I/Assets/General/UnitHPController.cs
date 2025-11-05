using System;
using System.Collections;
using System.Collections.Generic;
using Project_I;
using Sirenix.OdinInspector;
using UnityEngine;

public class UnitHPController : MonoBehaviour
{
    [LabelText("血量")] public float hp = 20.0f;

    /*[LabelText("玩家单位")]*/ private PlayerController player;
    // TODO 友方单位
    /*[LabelText("敌方单位")]*/ private NpcBehaviorController npcBehaviorController;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        npcBehaviorController = GetComponent<NpcBehaviorController>();
    }

    public void Hit(float damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            hp = 0;
            Die();
        }
    }

    public void Die()
    {
        if (player is not null)
        {
            player.Die();
        }

        // TODO 友方单位
        
        if (npcBehaviorController is not null)
            npcBehaviorController.Die();
    }
}
