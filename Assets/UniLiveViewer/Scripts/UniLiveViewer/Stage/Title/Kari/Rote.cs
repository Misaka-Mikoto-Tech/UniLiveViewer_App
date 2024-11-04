using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer.SceneUI.Title.Kari
{
    public class Rote : MonoBehaviour
    {
        public float speed = 10;

        void Start()
        {
        }

        void Update()
        {
            transform.rotation *= Quaternion.Euler(0, Time.deltaTime * speed, 0);
        }
    }
}
