﻿using System;
using System.Collections.Generic;
using System.Linq;
using UniLiveViewer.Actor.LookAt;
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

        // TODO: 出来れば参照消したい
        public NormalizedBoneGenerator NormalizedBoneGenerator => _normalizedBoneGenerator;
        readonly NormalizedBoneGenerator _normalizedBoneGenerator;

        public LookAtService LookAtService => _lookAtService;
        readonly LookAtService _lookAtService;

        public IReadOnlyDictionary<HumanBodyBones, Transform> BoneMap => _boneMap;
        readonly Dictionary<HumanBodyBones, Transform> _boneMap;

        /// <summary>
        /// 身長
        /// </summary>
        float _height;

        public ActorEntity(Animator animator, CharaInfoData charaInfoData,
            VMDPlayer_Custom vmdPlayer, LookAtService lookAtAllocator,
            NormalizedBoneGenerator normalizedBoneGenerator)
        {
            _animator = animator;
            _charaInfoData = charaInfoData;
            _vmdPlayer = vmdPlayer;
            _normalizedBoneGenerator = normalizedBoneGenerator;
            _lookAtService = lookAtAllocator;

            _animator.applyRootMotion = true;// MEMO: trueの方がペアダンス若干いい気がする

            _boneMap = Enum.GetValues(typeof(HumanBodyBones))
                .Cast<HumanBodyBones>()
                .Where(b => b != HumanBodyBones.LastBone)
                .ToDictionary(bone => bone, bone => animator.GetBoneTransform(bone));
            _height = _boneMap[HumanBodyBones.Head].position.y - _boneMap[HumanBodyBones.Spine].position.y;

            _normalizedBoneGenerator.Setup(_boneMap);

            var target = Camera.main.transform;
            if (charaInfoData.ActorType == ActorType.FBX)
            {
                _lookAtService.FBXSetup(animator, target);
            }
            else if (charaInfoData.ActorType == ActorType.VRM)
            {
                var go = animator.gameObject;
                if (go.TryGetComponent<Vrm10Instance>(out var vrm10Instance))
                {
                    _lookAtService.VRM10Setup(animator, target, vrm10Instance);
                }
                //0.x系
                else
                {
                    if (go.TryGetComponent<VRMLookAtBoneApplyer>(out var boneApplyer))
                    {
                        _lookAtService.VRMSetup(animator, target, boneApplyer);
                    }
                    else if (go.TryGetComponent<VRMLookAtBlendShapeApplyer>(out var blendShapeApplyer))
                    {
                        _lookAtService.VRMSetup(animator, target, blendShapeApplyer);
                    }
                    else
                    {
                        //UV？
                    }
                }
            }
        }
    }
}
