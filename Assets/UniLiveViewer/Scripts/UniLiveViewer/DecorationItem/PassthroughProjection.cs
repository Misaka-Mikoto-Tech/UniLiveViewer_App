using UnityEngine;

namespace UniLiveViewer
{   
    //TODO:一括管理に変える
    public class PassthroughProjection : MonoBehaviour
    {
        OVRPassthroughLayer _passthroughLayer;
        [SerializeField] MeshFilter _projectionObject;

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
        }

        void OnEnable()
        {
            if (PlayerStateManager.instance.myOVRManager.isInsightPassthroughEnabled)
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
