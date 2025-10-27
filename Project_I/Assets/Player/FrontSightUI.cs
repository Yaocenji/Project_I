using System;
using System.Collections;
using System.Collections.Generic;
using Project_I;
using UnityEngine;

public class FrontSightUI : MonoBehaviour
{
    private RectTransform rectTransform;
    public FrontSightController playerAircraftController;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
         var scrPos = playerAircraftController.GetScreenPosition();
         rectTransform.position = new Vector3(scrPos.x, scrPos.y, 0);
    }
}
