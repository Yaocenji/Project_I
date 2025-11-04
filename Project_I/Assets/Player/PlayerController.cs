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
    private AircraftController aircraftController;
    private EjectorController ejectorController;
    
    private PlayerInput _playerInput;
    private Vector2 _mousePositionWs;

    public PlayerInput getInputActions
    {
        get => _playerInput;
    }

    public Vector2 mousePositionWs
    {
        get => _mousePositionWs;
    }
    
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

        _playerInput.Player.SwitchToWeaponUp.started += SwithToEjectorUp;
        _playerInput.Player.SwitchToWeaponDown.started += SwithToEjectorDown;
        _playerInput.Player.SwitchToWeaponLeft.started += SwithToEjectorLeft;
        _playerInput.Player.SwitchToWeaponRight.started += SwithToEjectorRight;

        _mousePositionWs = Vector2.zero;
        
        // 其他组件
        aircraftController = GetComponent<AircraftController>();
        ejectorController = GetComponent<EjectorController>();
    }

    public void Die()
    {
        // TODO
        Debug.Log("玩家死了");
    }

    void Update()
    {
        // _mousePositionWs = mainCamera.ScreenToWorldPoint(_playerInput.Player.MousePosition.ReadValue<Vector2>());
        // aircraftController.SetTargetPosition(_mousePositionWs);
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
    
    private void SwithToEjectorUp(InputAction.CallbackContext obj)
    {
        ejectorController.SwitchEjector(0);
    }
    private void SwithToEjectorDown(InputAction.CallbackContext obj)
    {
        ejectorController.SwitchEjector(1);
    }
    private void SwithToEjectorLeft(InputAction.CallbackContext obj)
    {
        ejectorController.SwitchEjector(2);
    }
    private void SwithToEjectorRight(InputAction.CallbackContext obj)
    {
        ejectorController.SwitchEjector(3);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Log");
    }
}
    
}
