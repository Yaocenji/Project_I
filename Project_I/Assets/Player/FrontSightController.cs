using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
// 将原始鼠标输入的方法转化为鼠标不动而准星移动的方法
public class FrontSightController : MonoBehaviour
{
    
    private Camera mainCamera;
    private PlayerController playerController;
    private AircraftController playerAircraftController;
    private EjectorController playerEjectorController;

    // 计算各种位置
    private Vector2 screenSize;
    private Vector2 _thisMouseDeltaPosition;
    private Vector2 _screenPosition;

    private void Awake()
    {
        GameSceneManager.Instance.RegisterFrontSight(gameObject);
    }

    void Start()
    {
        mainCamera = GameSceneManager.Instance.mainCamera;
        playerController = GameSceneManager.Instance.player.GetComponent<PlayerController>();
        playerEjectorController = GameSceneManager.Instance.player.GetComponent<EjectorController>();
        playerAircraftController = GameSceneManager.Instance.player.GetComponent<AircraftController>();
        
        screenSize = new Vector2(Screen.width,  Screen.height); 
        
        var tempP = mainCamera.WorldToScreenPoint(new Vector2(transform.position.x, transform.position.y));
        _screenPosition = new Vector2(tempP.x, tempP.y);

        // 鼠标归位
        Cursor.lockState = CursorLockMode.Locked;
        //Debug.Log(playerController.getInputActions.Player.MousePosition.ReadValue<Vector2>());
    }

    // Update is called once per frame
    void Update()
    {
        // 原方法
        /*_thisMousePosition = mainCamera.ScreenToWorldPoint(playerController.getInputActions.Player.MousePosition.ReadValue<Vector2>());
        playerAircraftController.SetTargetPosition(_thisMousePosition);*/
        
        //Debug.Log(playerController.getInputActions.Player.MouseDeltaPosition.ReadValue<Vector2>());
        
        // 新方法
        // 获取delta
        _thisMouseDeltaPosition = playerController.getInputActions.Player.MouseDeltaPosition.ReadValue<Vector2>();
        
        // 如果是瞄准模式，则会修改鼠标灵敏度
        if (playerEjectorController.getAiming)
        {
            _thisMouseDeltaPosition *= playerEjectorController.AimingMouseSensitivity;
        }
        
        // 计算实际的屏幕空间位置
        _screenPosition += _thisMouseDeltaPosition;
        // 不要超出屏幕
        _screenPosition.x = Mathf.Clamp(_screenPosition.x, 0, screenSize.x);
        _screenPosition.y = Mathf.Clamp(_screenPosition.y, 0, screenSize.y);
        
        
        // 转换到世界空间位置
        var tempP = mainCamera.ScreenToWorldPoint(_screenPosition);
        transform.position = new Vector3(tempP.x, tempP.y, 0);
        
        playerAircraftController.SetTargetPosition(transform.position);
    }

    public Vector2 GetScreenPosition()
    {
        return _screenPosition;
    }
}
    
}
