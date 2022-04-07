using UnityEngine;
using VRM;

namespace UniLiveViewer
{
    public class VRMTouchColliders : MonoBehaviour
    {
        public VRMSpringBoneColliderGroup[] colliders = null;
        [SerializeField] private float ScaleSize = 1;

        private void Start()
        {
            foreach (var colGroup in colliders)
            {
                for (int i = 0; i < colGroup.Colliders.Length; i++)
                {
                    colGroup.Colliders[i].Radius *= ScaleSize;
                }
            }
        }
    }
}