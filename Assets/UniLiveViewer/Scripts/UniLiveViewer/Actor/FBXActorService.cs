﻿using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.Actor.AttachPoint;
using UniLiveViewer.Actor.Expression;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor
{
    /// <summary>
    /// 識別子がまだないので整理して作る
    /// </summary>
    public class FBXActorService : IActorEntity
    {
        IReactiveProperty<ActorEntity> IActorEntity.ActorEntity() => _actorEntity;
        readonly ReactiveProperty<ActorEntity> _actorEntity = new();

        IReactiveProperty<bool> IActorEntity.Active() => _active;
        readonly ReactiveProperty<bool> _active = new(true);

        IReactiveProperty<float> IActorEntity.RootScalar() => _rootScalar;
        readonly ReactiveProperty<float> _rootScalar = new(FileReadAndWriteUtility.UserProfile.InitCharaSize);

        IReactiveProperty<ActorState> IActorEntity.ActorState() => _actorState;
        readonly ReactiveProperty<ActorState> _actorState = new(ActorState.NULL);

        /// <summary>
        /// 主に親指定時のスケール問題を解決する目的で
        /// ミニチュア時とLineSelector（こっちは座標上書き目的もある）
        /// </summary>
        Transform _overrideAnchor;

        //FBXのみLSで捜査してるけどどうしようか
        readonly Animator _animatorCache;
        readonly LifetimeScope _lifetimeScope;
        readonly CharaInfoData _charaInfoData;
        readonly AttachPointService _attachPointService;
        readonly LipSync_FBX _lipSync;
        readonly FacialSync_FBX _faceSync;

        [Inject]
        public FBXActorService(
            Animator animator,
            LifetimeScope lifetimeScope,
            CharaInfoData charaInfoData,
            AttachPointService attachPointService,
            LipSync_FBX lipSync,
            FacialSync_FBX facialSync)
        {
            _animatorCache = animator;
            _lifetimeScope = lifetimeScope;
            _charaInfoData = charaInfoData;
            _attachPointService = attachPointService;
            _lipSync = lipSync;
            _faceSync = facialSync;
        }

        /// <summary>
        /// デバッグ用
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        async UniTask IActorEntity.EditorOnlySetupAsync(Transform firstParent, CancellationToken cancellation)
        {
#if UNITY_EDITOR
            //何もしない   
#endif
        }

        public async UniTask SetupAsync(Transform firstParent, CancellationToken cancellation)
        {
            await SetupInternalAsync(firstParent, cancellation);
        }

        /// <summary>
        /// 各種設定
        /// </summary>
        /// <param name="instance"></param>
        async UniTask SetupInternalAsync(Transform firstParent, CancellationToken cancellation)
        {
            var go = _animatorCache.gameObject;
            {
                // 表情系
                _lipSync.Setup(go.transform);
                _faceSync.Setup(go.transform);
            }

            // TODO: 最後に調整、デバッグ用
            await UniTask.Delay(100);

            var vmdPlayer = go.AddComponent<VMDPlayer_Custom>();
            var charaInfoData = GameObject.Instantiate(_charaInfoData);
            vmdPlayer.Initialize(charaInfoData, _faceSync, _lipSync);
            _actorEntity.Value = new ActorEntity(_animatorCache, _charaInfoData, vmdPlayer);

            await _attachPointService.SetupAsync(_actorEntity.Value.BoneMap, cancellation);

            SetState(ActorState.MINIATURE, firstParent);
            _lifetimeScope.transform.localPosition = Vector3.zero;
            _lifetimeScope.transform.localRotation = Quaternion.identity;
        }

        void IActorEntity.Activate(bool isActive)
        {
            _active.Value = isActive;

            if (_lifetimeScope.gameObject.activeSelf == isActive) return;
            _lifetimeScope.gameObject.SetActive(isActive);
        }

        void IActorEntity.AddRootScalar(float add)
        {
            _rootScalar.Value = Mathf.Clamp(_rootScalar.Value + add, 0.25f, 20.0f);
            _lifetimeScope.transform.localScale = Vector3.one * _rootScalar.Value;
            //_actorEntity.Value.GetAnimator.transform.localScale = Vector3.one * _customScalar;
        }

        /// <summary>
        /// 状態設定
        /// </summary>
        /// <param name="setState"></param>
        /// <param name="overrideTarget">対象位置に座標を合わせる</param>
        public void SetState(ActorState setState, Transform overrideTarget)
        {
            if (_actorEntity.Value == null) return;
            var rootGameObject = _lifetimeScope.gameObject;
            var globalScale = Vector3.zero;

            _overrideAnchor = overrideTarget;
            _actorState.Value = setState;
            switch (_actorState.Value)
            {
                case ActorState.NULL:
                    //VRMとPrefab用
                    globalScale = Vector3.one;
                    rootGameObject.layer = Constants.LayerNoDefault;

                    // TODO: 主にVRMPrefab化の時用なので後でどうにかする
                    //リセットして無効化しておく
                    //LookAtVRM.EyeReset();
                    //LookAtVRM.SetEnable(false);
                    //SetLookAt(false);

                    break;
                case ActorState.MINIATURE:
                    globalScale = new Vector3(0.26f, 0.26f, 0.26f);
                    rootGameObject.layer = Constants.LayerNoGrabObject;
                    break;
                case ActorState.HOLD:
                    globalScale = new Vector3(0.26f, 0.26f, 0.26f);
                    break;
                case ActorState.ON_CIRCLE:
                    globalScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                case ActorState.FIELD:
                    globalScale = new Vector3(1.0f, 1.0f, 1.0f);
                    rootGameObject.layer = Constants.LayerNoFieldObject;
                    break;
            }

            //座標の上書き設定
            var rootTransform = _lifetimeScope.transform;
            if (_overrideAnchor)
            {
                rootTransform.parent = _overrideAnchor;

                //親Scaleの影響を無視する為に算出
                globalScale.x *= 1 / _overrideAnchor.lossyScale.x;
                globalScale.y *= 1 / _overrideAnchor.lossyScale.y;
                globalScale.z *= 1 / _overrideAnchor.lossyScale.z;

                rootTransform.position = _overrideAnchor.position;
                rootTransform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            else rootTransform.parent = null;

            if (_actorState.Value == ActorState.MINIATURE || _actorState.Value == ActorState.HOLD)
            {
                rootTransform.localScale = globalScale;
            }
            else
            {
                rootTransform.localScale = globalScale * _rootScalar.Value;
            }
        }

        void IActorEntity.OnTick()
        {
            //OVRgrab側で掴まれている為、常時上書き
            if (_actorState.Value == ActorState.ON_CIRCLE)
            {
                _lifetimeScope.transform.position = _overrideAnchor.position;
                _lifetimeScope.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
        }

        void IActorEntity.Delete()
        {
            GameObject.Destroy(_lifetimeScope.gameObject);
        }
    }
}
