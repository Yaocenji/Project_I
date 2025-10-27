using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerCameraContraller : MonoBehaviour
{
    private Camera mainCamera;
    private Camera thisCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        thisCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        thisCamera.transform.position = mainCamera.transform.position;
        thisCamera.orthographicSize = mainCamera.orthographicSize;
    }
}
