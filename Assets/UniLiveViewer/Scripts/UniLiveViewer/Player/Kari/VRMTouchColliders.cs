using UnityEngine;
using VRM;

namespace UniLiveViewer.Player
{
    /// <summary>
    /// VRM接触用のPlayerHandコライダー
    /// </summary>
    public class VRMTouchColliders : MonoBehaviour
    {
        public VRMSpringBoneColliderGroup[] colliders;
        [SerializeField] float _scaleSize = 1;

        void Start()
        {
            foreach (var colGroup in colliders)
            {
                for (int i = 0; i < colGroup.Colliders.Length; i++)
                {
                    colGroup.Colliders[i].Radius *= _scaleSize;
                }
            }
        }
    }
}