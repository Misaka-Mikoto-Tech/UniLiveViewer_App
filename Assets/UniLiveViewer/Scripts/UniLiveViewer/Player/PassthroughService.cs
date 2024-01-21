using UnityEngine;
using UnityEngine.Rendering.Universal;
using VContainer;

namespace UniLiveViewer.Player
{
    [RequireComponent(typeof(OVRManager))]
    public class PassthroughService : MonoBehaviour
    {
        OVRManager _ovrManager;
        Camera _camera;
        /// <summary>
        /// パススルーとポスプロ共存できないので無効化しておく
        /// </summary>
        UniversalAdditionalCameraData _cameraData;
        bool _cachePostProcessing;

        [Inject]
        public void Construct(OVRManager ovrManager, Camera camera)
        {
            _camera = camera;
            _ovrManager = ovrManager;
        }

        public void OnStart()
        {
            _cameraData = _camera.GetComponent<UniversalAdditionalCameraData>();
            _cachePostProcessing = _cameraData.renderPostProcessing;
            Switching(false);
        }

        public void Switching(bool isEnable)
        {
            if (isEnable)
            {
                _camera.clearFlags = CameraClearFlags.Color;
                _cameraData.renderPostProcessing = false;
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
                _cameraData.renderPostProcessing = _cachePostProcessing;
                _ovrManager.isInsightPassthroughEnabled = false;
            }
        }

        public bool IsInsightPassthroughEnabled()
        {
            return _ovrManager.isInsightPassthroughEnabled;
        }
    }

}