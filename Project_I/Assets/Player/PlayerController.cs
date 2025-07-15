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
    public Camera mainCamera;
    public AircraftController aircraftController;
    public EjectorManager ejectorManager;
    
    private PlayerInput _playerInput;
    private Vector2 _mousePositionWs;
    
    private void Awake()
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

        //aircraftController = GetComponent<AircraftController>();
        
        _mousePositionWs = Vector2.zero;
        
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
        ejectorManager.BeginEject();
    }
    private void EndMainAttack(InputAction.CallbackContext obj)
    {
        ejectorManager.EndEject();
    }
    
    private void StartAim(InputAction.CallbackContext obj)
    {
        ejectorManager.BeginAiming();
    }
    private void EndAim(InputAction.CallbackContext obj)
    {
        ejectorManager.EndAiming();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Log");
    }
}
    
}
