using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


namespace Project_I
{
public class PlayerController : MonoBehaviour
{
    // 需要的组件
    private Camera mainCamera;
    private AircraftController aircraftController;
    private EjectorController ejectorController;
    
    private PlayerInput _playerInput;
    private Vector2 _mousePositionWs;
    
    private void Awake()
    {
        // 注册成为玩家
        GameSceneManager.Instance.RegisterPlayer(gameObject);
    }

    private void Start()
    {
        _playerInput = new PlayerInput();
        _playerInput.Enable();
        _playerInput.Player.StandardThrust.started += StartStandardThrust;
        _playerInput.Player.StandardThrust.canceled += EndStandardThrust;
        _playerInput.Player.AugmentationThrust.started += StartAugmentationThrust;
        _playerInput.Player.AugmentationThrust.canceled += EndAugmentationThrust;
        
        _playerInput.Player.MainAttack.started += StartMainAttack;
        _playerInput.Player.MainAttack.canceled += EndMainAttack;
        _playerInput.Player.Aim.started += StartAim;
        _playerInput.Player.Aim.canceled += EndAim;

        _mousePositionWs = Vector2.zero;
        
        // 其他组件
        mainCamera = GameSceneManager.Instance.mainCamera;
        aircraftController = GetComponent<AircraftController>();
        ejectorController = GetComponent<EjectorController>();
    }

    void Update()
    {
        _mousePositionWs = mainCamera.ScreenToWorldPoint(_playerInput.Player.MousePosition.ReadValue<Vector2>());
        aircraftController.SetTargetPosition(_mousePositionWs);
    }

    private void StartStandardThrust(InputAction.CallbackContext obj)
    {
        aircraftController.StartStandardThrust();
    }
    private void EndStandardThrust(InputAction.CallbackContext obj)
    {
        aircraftController.EndStandardThrust();
    }
    
    private void StartAugmentationThrust(InputAction.CallbackContext obj)
    {
        aircraftController.StartAugmentationThrust();
    }
    private void EndAugmentationThrust(InputAction.CallbackContext obj)
    {
        aircraftController.EndAugmentationThrust();
    }
    
    private void StartMainAttack(InputAction.CallbackContext obj)
    {
        ejectorController.BeginEject();
    }
    private void EndMainAttack(InputAction.CallbackContext obj)
    {
        ejectorController.EndEject();
    }
    
    private void StartAim(InputAction.CallbackContext obj)
    {
        ejectorController.BeginAiming();
    }
    private void EndAim(InputAction.CallbackContext obj)
    {
        ejectorController.EndAiming();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Log");
    }
}
    
}
