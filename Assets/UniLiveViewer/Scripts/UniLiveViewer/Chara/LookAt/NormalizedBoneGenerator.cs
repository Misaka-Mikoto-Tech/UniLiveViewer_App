using UnityEngine;

namespace UniLiveViewer 
{
    /// <summary>
    /// 仮実装
    /// </summary>
    public class NormalizedBoneGenerator : MonoBehaviour
    {
        [Header("＜ボーンAnchor＞")]
        public Transform headAnchor;
        public Transform chestAnchor;
        public Transform lEyeAnchor;
        public Transform rEyeAnchor;

        [Header("＜仮想Anchor(正面用)＞")]
        public Transform virtualEye;
        public Transform virtualHead;
        public Transform virtualChest;

        public Transform lookTarget_limit;
        private Animator animator;

        void Awake()
        {
            animator = GetComponent<Animator>();

            //各種ボーンからアンカーを取得
            headAnchor = animator.GetBoneTransform(HumanBodyBones.Head);
            chestAnchor = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if (!chestAnchor) chestAnchor = animator.GetBoneTransform(HumanBodyBones.Chest);
            lEyeAnchor = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            rEyeAnchor = animator.GetBoneTransform(HumanBodyBones.RightEye);
        }

        // Start is called before the first frame update
        void Start()
        {
            //仮想ルートを生成(頭の正面用)
            virtualChest = new GameObject("[Normalized] VirtualChest").transform;
            virtualChest.forward = transform.forward;
            virtualChest.parent = chestAnchor;
            virtualChest.localPosition = Vector3.zero;
            virtualChest.gameObject.layer = SystemInfo.layerNo_VirtualHead;

            //仮想ヘッドを生成(目の正面用)
            virtualHead = new GameObject("[Normalized] VirtualHead").transform;
            virtualHead.forward = transform.forward;
            virtualHead.parent = headAnchor;
            virtualHead.localPosition = Vector3.zero;
            virtualHead.gameObject.layer = SystemInfo.layerNo_VirtualHead;
            virtualHead.gameObject.AddComponent(typeof(SphereCollider));
            var col = virtualHead.GetComponent<SphereCollider>();
            col.radius = 0.06f;
            col.isTrigger = true;

            //仮想アイを生成
            virtualEye = new GameObject("[Normalized] VirtualEye").transform;
            virtualEye.parent = headAnchor;
            virtualEye.gameObject.layer = SystemInfo.layerNo_VirtualHead;
            virtualEye.localPosition = Vector3.zero;
            virtualEye.rotation = transform.rotation;

            lookTarget_limit = new GameObject("[Normalized] LookTarget_limit").transform;
            lookTarget_limit.parent = transform;
            lookTarget_limit.position = virtualHead.position + virtualHead.forward;
        }
    }
}