using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer.Actor.LookAt
{
    /// <summary>
    /// 仮実装 一旦可視化する目的にLS階層でMonoBehaviour
    /// TODO: 存在意義から見直し
    /// </summary>
    public class NormalizedBoneGenerator : MonoBehaviour
    {
        [Header("＜ボーンAnchor＞")]
        [SerializeField] Transform _headAnchor;
        [SerializeField] Transform _chestAnchor;
        public Transform LEyeAnchor => _lEyeAnchor;
        [SerializeField] Transform _lEyeAnchor;
        public Transform REyeAnchor => _rEyeAnchor;
        [SerializeField] Transform _rEyeAnchor;

        public Transform VirtualEye => _virtualEye;
        [Header("＜仮想Anchor(正面用)＞")]
        [SerializeField] Transform _virtualEye;
        public Transform VirtualHead => _virtualHead;
        [SerializeField] Transform _virtualHead;
        public Transform VirtualChest => _virtualChest;
        [SerializeField] Transform _virtualChest;
        public Transform LookTargetLimit => _lookTargetLimit;
        [SerializeField] Transform _lookTargetLimit;


        public void Setup(IReadOnlyDictionary<HumanBodyBones, Transform> boneMap)
        {
            //各種ボーンからアンカーを取得
            _headAnchor = boneMap[HumanBodyBones.Head];
            _chestAnchor = boneMap[HumanBodyBones.UpperChest];
            if (!_chestAnchor) _chestAnchor = boneMap[HumanBodyBones.Chest];
            _lEyeAnchor = boneMap[HumanBodyBones.LeftEye];
            _rEyeAnchor = boneMap[HumanBodyBones.RightEye];

            //仮想ルートを生成(頭の正面用)
            _virtualChest = new GameObject("[Normalized] VirtualChest").transform;
            _virtualChest.forward = transform.forward;
            _virtualChest.parent = _chestAnchor;
            _virtualChest.localPosition = Vector3.zero;
            _virtualChest.gameObject.layer = Constants.LayerNoVirtualHead;

            //仮想ヘッドを生成(目の正面用)
            _virtualHead = new GameObject("[Normalized] VirtualHead").transform;
            _virtualHead.forward = transform.forward;
            _virtualHead.parent = _headAnchor;
            _virtualHead.localPosition = Vector3.zero;
            _virtualHead.gameObject.layer = Constants.LayerNoVirtualHead;
            _virtualHead.gameObject.AddComponent(typeof(SphereCollider));
            var col = _virtualHead.GetComponent<SphereCollider>();
            col.radius = 0.06f;
            col.isTrigger = true;

            //仮想アイを生成
            _virtualEye = new GameObject("[Normalized] VirtualEye").transform;
            _virtualEye.parent = _headAnchor;
            _virtualEye.gameObject.layer = Constants.LayerNoVirtualHead;
            _virtualEye.localPosition = Vector3.zero;
            _virtualEye.rotation = transform.rotation;

            _lookTargetLimit = new GameObject("[Normalized] LookTarget_limit").transform;
            _lookTargetLimit.parent = transform;
            _lookTargetLimit.position = _virtualHead.position + _virtualHead.forward;
        }
    }
}