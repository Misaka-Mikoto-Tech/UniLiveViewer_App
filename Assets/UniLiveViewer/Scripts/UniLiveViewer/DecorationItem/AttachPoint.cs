using UnityEngine;

namespace UniLiveViewer
{ 
    public class AttachPoint : MonoBehaviour
    {
        public CharaController myCharaCon;
        private MeshRenderer meshRenderer;
        private SphereCollider sphereCollider;

        // Start is called before the first frame update
        void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            sphereCollider = GetComponent<SphereCollider>();
        }

        public void SetActive(bool isActive)
        {
            meshRenderer.enabled = isActive;
            sphereCollider.enabled = isActive;
        }
    }
}