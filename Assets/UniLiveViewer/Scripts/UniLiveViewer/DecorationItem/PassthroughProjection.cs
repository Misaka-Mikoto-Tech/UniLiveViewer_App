using UnityEngine;

namespace UniLiveViewer
{   
    //TODO:一括管理に変える
    public class PassthroughProjection : MonoBehaviour
    {
        private OVRPassthroughLayer passthroughLayer;
        [SerializeField] private MeshFilter projectionObject;

        private void Awake()
        {
            GameObject ovrCameraRig = GameObject.Find("PassthroughProjection");
            if (ovrCameraRig == null)
            {
                Debug.LogError("Scene does not contain an OVRCameraRig");
                return;
            }

            passthroughLayer = ovrCameraRig.GetComponent<OVRPassthroughLayer>();
            if (passthroughLayer == null)
            {
                Debug.LogError("OVRCameraRig does not contain an OVRPassthroughLayer component");
            }
        }

        private void OnEnable()
        {
            if (PlayerStateManager.instance.myOVRManager.isInsightPassthroughEnabled)
            {
                passthroughLayer.AddSurfaceGeometry(projectionObject.gameObject, true);
            }
        }

        private void OnDisable()
        {
            passthroughLayer.RemoveSurfaceGeometry(projectionObject.gameObject);
        }

    }
}
