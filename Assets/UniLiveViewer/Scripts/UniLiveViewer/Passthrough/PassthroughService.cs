using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace UniLiveViewer
{
    [RequireComponent(typeof(OVRManager))]
    public class PassthroughService : MonoBehaviour
    {
        OVRManager _ovrManager;
        Camera _camera;

        public void Initialize(OVRManager ovrManager, Camera camera)
        {
            _camera = camera;
            _ovrManager = ovrManager;

            Switching(false);
        }

        public void Switching(bool isEnable)
        {
            if (isEnable)
            {
                _camera.clearFlags = CameraClearFlags.Color;
                _ovrManager.isInsightPassthroughEnabled = true;
            }
            else
            {
                var e = GameObject.FindGameObjectsWithTag("Passthrough");
                int max = e.Length;
                for (int i = 0; i < max; i++)
                {
                    Destroy(e[max - i - 1]);
                }

                _camera.clearFlags = CameraClearFlags.Skybox;
                _ovrManager.isInsightPassthroughEnabled = false;
            }
        }

        public bool IsInsightPassthroughEnabled()
        {
            return _ovrManager.isInsightPassthroughEnabled;
        }
    }

}