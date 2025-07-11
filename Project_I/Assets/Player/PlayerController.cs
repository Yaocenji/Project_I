using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Project_I
{
public class PlayerController : MonoBehaviour
{
    public Camera mainCamera;
    
    private PlayerInput _playerInput;
    private AircraftController _aircraftController;

    private Vector2 _mousePositionWs;
    
    private void Awake()
    {
        _playerInput = new PlayerInput();
        _playerInput.Enable();
        _playerInput.Player.StandardThrust.started += StartStandardThrust;
        _playerInput.Player.StandardThrust.canceled += EndStandardThrust;
        _playerInput.Player.AugmentationThrust.started += StartAugmentationThrust;
        _playerInput.Player.AugmentationThrust.canceled += EndAugmentationThrust;

        _aircraftController = GetComponent<AircraftController>();
        
        _mousePositionWs = Vector2.zero;
        
    }

    void Update()
    {
        _mousePositionWs = mainCamera.ScreenToWorldPoint(_playerInput.Player.MousePosition.ReadValue<Vector2>());
        _aircraftController.SetTargetPosition(_mousePositionWs);
    }

    private void StartStandardThrust(InputAction.CallbackContext obj)
    {
        _aircraftController.StartStandardThrust();
    }
    private void EndStandardThrust(InputAction.CallbackContext obj)
    {
        _aircraftController.EndStandardThrust();
    }
    
    private void StartAugmentationThrust(InputAction.CallbackContext obj)
    {
        _aircraftController.StartAugmentationThrust();
    }
    private void EndAugmentationThrust(InputAction.CallbackContext obj)
    {
        _aircraftController.EndAugmentationThrust();
    }
}
    
}
