using UnityEngine;
using VContainer;

namespace UniLiveViewer
{   
    //TODO:一括管理に変える
    public class PassthroughProjection : MonoBehaviour
    {
        OVRPassthroughLayer _passthroughLayer;
        MeshFilter _projectionObject;
        PassthroughService _passthroughService;

        void Awake()
        {
            GameObject ovrCameraRig = GameObject.Find("PassthroughProjection");
            if (ovrCameraRig == null)
            {
                Debug.LogError("Scene does not contain an OVRCameraRig");
                return;
            }

            _passthroughLayer = ovrCameraRig.GetComponent<OVRPassthroughLayer>();
            if (_passthroughLayer == null)
            {
                Debug.LogError("OVRCameraRig does not contain an OVRPassthroughLayer component");
            }

            var player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLifetimeScope>();
            _passthroughService = player.Container.Resolve<PassthroughService>();
            // TODO: 強引すぎ見直す
            _projectionObject = transform.parent.GetComponent<MeshFilter>();
        }

        void OnEnable()
        {
            if (_passthroughService.IsInsightPassthroughEnabled())
            {
                _passthroughLayer.AddSurfaceGeometry(_projectionObject.gameObject, true);
            }
        }

        void OnDisable()
        {
            _passthroughLayer.RemoveSurfaceGeometry(_projectionObject.gameObject);
        }

    }
}
