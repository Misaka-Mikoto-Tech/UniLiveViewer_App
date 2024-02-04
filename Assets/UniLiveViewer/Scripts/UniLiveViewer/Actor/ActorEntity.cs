using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniLiveViewer.Actor.LookAt;
using UniLiveViewer.Player;
using UnityEngine;
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

        public IReadOnlyList<VRMSpringBone> SpringBoneList => _springBoneList;
        readonly List<VRMSpringBone> _springBoneList = new();

        public IReadOnlyDictionary<HumanBodyBones, Transform> BoneMap => _boneMap;
        readonly Dictionary<HumanBodyBones, Transform> _boneMap;

        /// <summary>
        /// 身長
        /// </summary>
        float _height;

        public ActorEntity(Animator animator, CharaInfoData charaInfoData, VMDPlayer_Custom vmdPlayer, VRMTouchColliders touchCollider = null)
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
                    //UV？知らない子ですね...
                }
                
                var dummy = new CancellationToken();
                SetupHeadLookAt(go, dummy).Forget();
                _springBoneList = go.GetComponentsInChildren<VRMSpringBone>().ToList();
                SetupSpringBone(touchCollider);
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
        void SetupSpringBone(VRMTouchColliders touchCollider)
        {
            var colliderList = new List<VRMSpringBoneColliderGroup>();//統合用
            for (int i = 0; i < _springBoneList.Count; i++)
            {
                //各配列をリストに統合
                if (_springBoneList[i].ColliderGroups is VRMSpringBoneColliderGroup[] && _springBoneList[i].ColliderGroups.Length > 0)
                {
                    colliderList.AddRange(_springBoneList[i].ColliderGroups);//既存コライダー
                    colliderList.AddRange(touchCollider.colliders);//追加コライダー(PlayerHand)                                                                                                                                                                                                                        
                    //リストから配列に戻す
                    _springBoneList[i].ColliderGroups = colliderList.ToArray();
                    colliderList.Clear();
                }
                else
                {
                    _springBoneList[i].ColliderGroups = touchCollider.colliders;
                }
            }
        }
    }
}
