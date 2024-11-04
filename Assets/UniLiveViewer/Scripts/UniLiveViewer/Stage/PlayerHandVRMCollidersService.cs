using System.Linq;
using UnityEngine;
using UniVRM10;
using VRM;
using NanaCiel;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// 0.Xと1.0系のSpringBoneColliderを統括用
    /// </summary>
    public class PlayerHandVRMCollidersService : MonoBehaviour
    {
        [SerializeField] PlayerHandVRMCollidersAnchor[] _colliderAnchors;

        public VRMSpringBoneColliderGroup[] UnivrmColliderGroup => _univrmColliderGroup;
        VRMSpringBoneColliderGroup[] _univrmColliderGroup;

        public VRM10SpringBoneCollider[] UnivrmCollider => _univrmCollider;
        VRM10SpringBoneCollider[] _univrmCollider;

        [SerializeField] float _scale = 1;

        void Awake()
        {
            _univrmColliderGroup = _colliderAnchors
            .SelectMany(anchor => anchor.TryGetComponent<VRMSpringBoneColliderGroup>(out var colliderGroup) ? new[] { colliderGroup } : Enumerable.Empty<VRMSpringBoneColliderGroup>())
            .Select(colliderGroup =>
            {
                foreach (var collider in colliderGroup.Colliders)
                {
                    collider.Radius *= _scale;
                }
                return colliderGroup;
            }).ToArray();

            _univrmCollider = _colliderAnchors
                .SelectMany(anchor => ExtensionMethods.TryGetComponents<VRM10SpringBoneCollider>(anchor.transform))
                .Select(collider =>
                {
                    collider.Radius *= _scale;
                    return collider;
                }).ToArray();
        }
    }
}