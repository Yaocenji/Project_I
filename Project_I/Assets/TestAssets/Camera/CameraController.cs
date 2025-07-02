using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerTransform;

    private Vector2 _position;
    // Start is called before the first frame update
    void Awake()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _position = (transform.position + 3.0f * playerTransform.position) / 4.0f;
        transform.position = new Vector3(_position.x, _position.y, -10);
    }
}
