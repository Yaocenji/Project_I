using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project_I
{
    public class BasicFriend : MonoBehaviour
    {
        private void Awake()
        {
            // 注册友方单位
            GameSceneManager.Instance.RegisterFriend(gameObject);
        }
        
        public void Die()
        {
            // 删除友方
            GameSceneManager.Instance.DieFriend(gameObject);
            // 销毁
            Destroy(gameObject);
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
