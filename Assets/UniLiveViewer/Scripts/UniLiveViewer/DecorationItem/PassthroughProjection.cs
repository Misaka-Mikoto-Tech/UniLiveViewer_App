using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class PassthroughProjection : MonoBehaviour
    {
        private OVRPassthroughLayer passthroughLayer;
        [SerializeField] private MeshFilter projectionObject;

        // Start is called before the first frame update
        void Start()
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

            passthroughLayer.AddSurfaceGeometry(projectionObject.gameObject, true);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {
            passthroughLayer.RemoveSurfaceGeometry(projectionObject.gameObject);
        }
    }
}
