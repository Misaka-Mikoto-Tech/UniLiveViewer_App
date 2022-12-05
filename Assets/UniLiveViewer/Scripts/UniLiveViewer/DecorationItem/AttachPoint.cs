using UnityEngine;

namespace UniLiveViewer
{ 
    public class AttachPoint : MonoBehaviour
    {
        public CharaController myCharaCon;
        MeshRenderer _meshRenderer;
        SphereCollider _sphereCollider;

        // Start is called before the first frame update
        void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _sphereCollider = GetComponent<SphereCollider>();
        }

        public void SetActive(bool isActive)
        {
            _meshRenderer.enabled = isActive;
            _sphereCollider.enabled = isActive;
        }
    }
}