using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniLiveViewer.Actor.LookAt;
using UniLiveViewer.Player;
using UniLiveViewer.Stage;
using UnityEngine;
using UniVRM10;
using VRM;

namespace UniLiveViewer.Actor
{
    public class ActorEntity
    {
        public Animator GetAnimator => _animator;
        readonly Animator _animator;

        public VMDPlayer_Custom GetVMDPlayer => _vmdPlayer;
        readonly VMDPlayer_Custom _vmdPlayer;

        public CharaInfoData CharaInfoData => _charaInfoData;
        readonly CharaInfoData _charaInfoData;

        public LookAtBase LookAtBase => _lookAtBase;
        readonly LookAtBase _lookAtBase;
        public IHeadLookAt HeadLookAt => _headLookAt;
        readonly IHeadLookAt _headLookAt;
        public IEyeLookAt EyeLookAt => _eyeLookAt;
        readonly IEyeLookAt _eyeLookAt;

        /// <summary>
        /// 振動用に公開、1.0は現状なし
        /// </summary>
        public IReadOnlyList<VRMSpringBone> SpringBoneList => _springBoneList;
        readonly List<VRMSpringBone> _springBoneList = new();

        public IReadOnlyDictionary<HumanBodyBones, Transform> BoneMap => _boneMap;
        readonly Dictionary<HumanBodyBones, Transform> _boneMap;

        /// <summary>
        /// 身長
        /// </summary>
        float _height;

        public ActorEntity(Animator animator, CharaInfoData charaInfoData, VMDPlayer_Custom vmdPlayer, PlayerHandVRMCollidersService playerHandVRMColliders = null)
        {
            _animator = animator;
            _charaInfoData = charaInfoData;
            _vmdPlayer = vmdPlayer;

            _boneMap = Enum.GetValues(typeof(HumanBodyBones))
                .Cast<HumanBodyBones>()
                .Where(b => b != HumanBodyBones.LastBone)
                .ToDictionary(bone => bone, bone => animator.GetBoneTransform(bone));
            _height = _boneMap[HumanBodyBones.Head].position.y - _boneMap[HumanBodyBones.Spine].position.y;

            if (charaInfoData.ActorType == ActorType.FBX)
            {
                _lookAtBase = animator.GetComponent<LookAtBase>();
                _headLookAt = animator.GetComponent<IHeadLookAt>();
                _eyeLookAt = animator.GetComponent<IEyeLookAt>();
            }
            else if (charaInfoData.ActorType == ActorType.VRM)
            {
                var go = animator.gameObject;
                if (go.TryGetComponent<Vrm10Instance>(out var vrm10Instance))
                {
                    var eyeLookAt = go.AddComponent<LookAt_VRM10>();
                    _lookAtBase = eyeLookAt.GetComponent<LookAtBase>();
                    _headLookAt = go.GetComponent<IHeadLookAt>();
                    _eyeLookAt = go.GetComponent<IEyeLookAt>();

                    charaInfoData.ExpressionType = ExpressionType.VRM10;
                    eyeLookAt.Setup(vrm10Instance);

                    SetupSpringBone(playerHandVRMColliders.UnivrmCollider, vrm10Instance);
                }
                //0.x系
                else
                {
                    if (go.TryGetComponent<VRMLookAtBoneApplyer>(out var boneApplyer))
                    {
                        var eyeLookAt = go.AddComponent<LookAt_VRMBone>();
                        _lookAtBase = eyeLookAt.GetComponent<LookAtBase>();
                        _headLookAt = go.GetComponent<IHeadLookAt>();
                        _eyeLookAt = go.GetComponent<IEyeLookAt>();

                        charaInfoData.ExpressionType = ExpressionType.VRM_Bone;
                        eyeLookAt.Setup(boneApplyer);
                    }
                    else if (go.TryGetComponent<VRMLookAtBlendShapeApplyer>(out var blendShapeApplyer))
                    {
                        var eyeLookAt = go.AddComponent<LookAt_VRMBlendShape>();
                        _lookAtBase = eyeLookAt.GetComponent<LookAtBase>();
                        _headLookAt = go.GetComponent<IHeadLookAt>();
                        _eyeLookAt = go.GetComponent<IEyeLookAt>();

                        charaInfoData.ExpressionType = ExpressionType.VRM_BlendShape;
                        eyeLookAt.Setup(blendShapeApplyer);
                    }
                    else
                    {
                        //UV？
                    }

                    var dummy = new CancellationToken();
                    SetupHeadLookAt(go, dummy).Forget();
                    _springBoneList = go.GetComponentsInChildren<VRMSpringBone>().ToList();
                    SetupSpringBone(playerHandVRMColliders.UnivrmColliderGroup);
                }
            }
        }

        /// <summary>
        /// VRM専用
        /// </summary>
        /// <param name="go"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        async UniTask SetupHeadLookAt(GameObject go, CancellationToken cancellation)
        {
            //lookAtBaseのメソッドInject待ち
            await UniTask.Yield(cancellation);

            var headLookAt = go.GetComponent<VRMLookAtHead>();
            headLookAt.Target = _lookAtBase.LookTarget;
            headLookAt.UpdateType = UpdateType.LateUpdate;
        }

        /// <summary>
        /// VRM専用
        /// </summary>
        void SetupSpringBone(VRMSpringBoneColliderGroup[] fromColliderGroup)
        {

            foreach (var dest in _springBoneList)
            {
                dest.ColliderGroups = dest.ColliderGroups?.Length > 0
                    ? dest.ColliderGroups.Concat(fromColliderGroup).ToArray() // 既存ColliderGroupsと新fromColliderGroupを結合
                    : fromColliderGroup;
            }
        }

        void SetupSpringBone(VRM10SpringBoneCollider[] fromColliderGroup, Vrm10Instance vrm10Instance)
        {
            var destColliderGroup = vrm10Instance.GetComponentsInChildren<VRM10SpringBoneColliderGroup>().ToArray();

            foreach (var dest in destColliderGroup)
            {
                if (dest.Colliders != null && 0 < dest.Colliders.Count)
                {
                    dest.Colliders.AddRange(fromColliderGroup);
                }
                else
                {
                    dest.Colliders = new List<VRM10SpringBoneCollider>(fromColliderGroup);
                }
            }
            vrm10Instance.Runtime.ReconstructSpringBone();//MEMO: jobなので変更反映に必須
        }
    }
}
