using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    private float _currTimeScale;

    public float currTimeScale
    {
        get => _currTimeScale;
    }

    private EjectorController playerEjectorController;

    private bool lastFrameAimingState = false;
    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1;
        playerEjectorController = GameSceneManager.Instance.Player.GetComponent<EjectorController>();
    }

    private void Update()
    {
        if (playerEjectorController.getAiming != lastFrameAimingState)
        {
            if (playerEjectorController.getAiming) BeginAiming();
            else EndAiming();
            lastFrameAimingState = playerEjectorController.getAiming;
        }
    }

    public void BeginAiming()
    {
        Time.timeScale = playerEjectorController.AimingTimeScale;
    }
    public void EndAiming()
    {
        Time.timeScale = 1;
    }
}
    
}
