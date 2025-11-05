using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
    public class BasicEnemy : MonoBehaviour
    {
        private void Awake()
        {
            // 注册敌人
            GameSceneManager.Instance.RegisterEnemy(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
