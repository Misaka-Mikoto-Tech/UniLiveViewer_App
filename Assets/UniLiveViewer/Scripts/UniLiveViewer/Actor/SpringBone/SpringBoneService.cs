using System;
using System.Collections.Generic;
using System.Linq;
using UniLiveViewer.Stage;
using UnityEngine;
using UniVRM10;
using VContainer;
using VRM;

namespace UniLiveViewer.Actor.SpringBone
{
    public class SpringBoneService : IDisposable
    {
        /// <summary>
        /// Actorサイズ変更時に各ボーンパーティクルをリサイズ計算する為のキャッシュ
        /// VRM10専用
        /// </summary>
        List<float> _defaultBoneJointRadius = new();

        /// <summary>
        /// Playerの手と接触による振動は現状0.xのみ
        /// </summary>
        List<VRMSpringBone> _springBoneList = new();

        ActorEntity _actorEntity;

        readonly CharaInfoData _charaInfoData;
        readonly PlayerHandVRMCollidersService _playerHandVRMColliders;

        [Inject]
        public SpringBoneService(
            CharaInfoData charaInfoData,
            PlayerHandVRMCollidersService playerHandVRMColliders)
        {
            _charaInfoData = charaInfoData;
            _playerHandVRMColliders = playerHandVRMColliders;
        }

        public void OnChangeActorEntity(ActorEntity actorEntity)
        {
            if (_charaInfoData.ActorType != ActorType.VRM) return;

            _actorEntity = actorEntity;
            if (actorEntity == null) return;

            if (actorEntity.GetAnimator.TryGetComponent<Vrm10Instance>(out var instance))
            {
                _defaultBoneJointRadius = instance.SpringBone.Springs
                    .SelectMany(spring => spring.Joints)
                    .Distinct()
                    .Select(joint => joint.m_jointRadius)
                    .ToList();

                SetupSpringBone(_playerHandVRMColliders.UnivrmCollider, instance);
            }
            else
            {
                _springBoneList = actorEntity.GetAnimator.gameObject.GetComponentsInChildren<VRMSpringBone>().ToList();
                SetupSpringBone(_playerHandVRMColliders.UnivrmColliderGroup);

                // 揺れもの接触振動
                foreach (var springBone in _springBoneList)
                {
                    springBone.OnHitLeftHand += OnHitLeftHand;
                    springBone.OnHitRightHand += OnHitRightHand;
                }
            }
        }

        void OnHitLeftHand()
        {
            ControllerVibration.Execute(OVRInput.Controller.LTouch, 1, _charaInfoData.power, _charaInfoData.time);
        }

        void OnHitRightHand()
        {
            ControllerVibration.Execute(OVRInput.Controller.RTouch, 1, _charaInfoData.power, _charaInfoData.time);
        }

        /// <summary>
        /// VRM1.0
        /// </summary>
        /// <param name="fromColliderGroup"></param>
        /// <param name="vrm10Instance"></param>
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
            vrm10Instance.Runtime.ReconstructSpringBone();// jobなので変更反映に必須
        }

        /// <summary>
        /// VRM0.x
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

        public void OnOnChangeActorScale(float localScale)
        {
            if (_charaInfoData.ActorType != ActorType.VRM) return;
            if (_actorEntity == null) return;
            if (!_actorEntity.GetAnimator.TryGetComponent<Vrm10Instance>(out var instance)) return;

            var result = instance.SpringBone.Springs
                    .SelectMany(spring => spring.Joints)
                    .Distinct()
                    .ToList();

            for (int i = 0; i < result.Count; i++)
            {
                result[i].m_jointRadius = _defaultBoneJointRadius[i] * localScale;
            }

            instance.Runtime.ReconstructSpringBone();// jobなので変更反映に必須
        }

        // TODO: 生成時とPrefab時に切り替えないといけないみたい、最終的に使うか微妙
        void SetEnabelSpringBones(bool isEnabel)
        {
            //foreach (var e in _springBoneList)
            //{
            //    e.enabled = isEnabel;
            //}
        }

        void IDisposable.Dispose()
        {
            foreach (var springBone in _springBoneList)
            {
                springBone.OnHitLeftHand -= OnHitLeftHand;
                springBone.OnHitRightHand -= OnHitRightHand;
            }
        }
    }
}
