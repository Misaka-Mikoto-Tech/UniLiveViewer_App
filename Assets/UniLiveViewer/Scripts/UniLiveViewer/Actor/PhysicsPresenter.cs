using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor
{
    public class PhysicsPresenter : IStartable
    {
        readonly CharaInfoData _charaInfoData;
        readonly Rigidbody _rigidbody;
        readonly CapsuleCollider _capsuleCollider;

        [Inject]
        public PhysicsPresenter(
            CharaInfoData charaInfoData,
            Rigidbody rigidbody,
            CapsuleCollider capsuleCollider)
        {
            _charaInfoData = charaInfoData;
            _rigidbody = rigidbody;
            _capsuleCollider = capsuleCollider;
        }

        void IStartable.Start()
        {
            _capsuleCollider.center = _charaInfoData.ColliderCenter;
            _capsuleCollider.radius = _charaInfoData.ColliderRadius;
            _capsuleCollider.height = _charaInfoData.ColliderHeight;

            _rigidbody.collisionDetectionMode = _charaInfoData.RigidbodyMode;
            _rigidbody.isKinematic = _charaInfoData.RigidbodyKinematic;
            _rigidbody.useGravity = _charaInfoData.RigidbodyGravity;
        }
    }
}
