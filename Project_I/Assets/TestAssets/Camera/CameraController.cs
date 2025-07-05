using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Test
{
public class CameraController : MonoBehaviour
{
    public Transform playerTransform;

    private Vector2 _position;

    // Update is called once per frame
    void FixedUpdate()
    {
        _position = (19.0f * transform.position + playerTransform.position) / 20.0f;
        transform.position = new Vector3(_position.x, _position.y, -10);
    }
}
    
}
