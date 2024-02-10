using Cysharp.Threading.Tasks;
using MessagePipe;
using NanaCiel;
using System;
using System.Threading;
using UniGLTF;
using UniLiveViewer.Actor.AttachPoint;
using UniLiveViewer.Actor.Expression;
using UniLiveViewer.Player;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using VRM;

namespace UniLiveViewer.Actor
{
    /// <summary>
    /// 識別子がまだないので整理して作る
    /// </summary>
    public class VRMActorService : IActorEntity
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

        readonly LifetimeScope _lifetimeScope;
        readonly VRMService _vrmService;
        readonly CharaInfoData _charaInfoData;
        readonly AttachPointService _attachPointService;
        readonly VRMLoadData _data;
        readonly LipSync_VRM _lipSync;
        readonly FacialSync_VRM _faceSync;

        //VRMUI非表示専用
        readonly IPublisher<VRMLoadResultData> _publisher;

        [Inject]
        public VRMActorService(
            LifetimeScope lifetimeScope,
            VRMService vrmService,
            CharaInfoData charaInfoData,
            VRMLoadData data,
            LipSync_VRM lipSync,
            FacialSync_VRM facialSync,
            AttachPointService attachPointService,
            IPublisher<VRMLoadResultData> publisher)
        {
            _lifetimeScope = lifetimeScope;
            _vrmService = vrmService;
            _charaInfoData = charaInfoData;
            _attachPointService = attachPointService;
            _publisher = publisher;

            _data = data;
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
            return;
#if UNITY_EDITOR
            var fullPath = UnityEditor.EditorUtility.OpenFilePanel("Open VRM", "", "vrm");
            await SetupAsync(firstParent, cancellation);
#endif
        }

        public async UniTask SetupAsync(Transform firstParent, CancellationToken cancellation)
        {
            try
            {
                var instance = await _vrmService.LoadAsync(_data.FullPath, cancellation);
                await SetupInternalAsync(instance, cancellation);
                SetState(ActorState.MINIATURE, firstParent);
                _lifetimeScope.transform.localPosition = Vector3.zero;
                _lifetimeScope.transform.localRotation = Quaternion.identity;

                _publisher.Publish(new VRMLoadResultData(this));
            }
            catch (Exception ex)
            {
                _publisher.Publish(new VRMLoadResultData(null));
            }
        }

        /// <summary>
        /// 各種設定
        /// </summary>
        /// <param name="instance"></param>
        async UniTask SetupInternalAsync(RuntimeGltfInstance instance, CancellationToken cancellation)
        {
            var go = instance.gameObject;
            go.transform.SetParent(_lifetimeScope.transform, false);
            go.name = go.GetComponent<VRMMeta>().Meta.Title;
            go.layer = Constants.LayerNoGrabObject;//オートカメラ識別にも利用

            var animator = instance.GetComponent<Animator>();
            {
                // 表情系
                var vrmBlendShape = go.GetComponent<VRMBlendShapeProxy>();
                _lipSync.Setup(instance.transform, vrmBlendShape);
                _faceSync.Setup(instance.transform, vrmBlendShape);
            }

            // TODO: 最後に調整、デバッグ用
            await UniTask.Delay(100);

            // マテリアルURP化
            var materialConverter = (IMaterialConverter)new MaterialConverter(go.layer);
            await materialConverter.Convert(instance.SkinnedMeshRenderers, cancellation);

            //使わないようにする
            go.AddComponent<MaterialManager>();

            //AttachPointとか追加される前にmeshrenderのみマテリアル調整
            var meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers?.Length > 0)
            {
                await materialConverter.Conversion_Item(meshRenderers, cancellation).OnError();
            }

            _charaInfoData.viewName = go.name;

            var vmdPlayer = go.AddComponent<VMDPlayer_Custom>();
            vmdPlayer.Initialize(_charaInfoData, _faceSync, _lipSync);

            var lifetimeScope = LifetimeScope.FindObjectOfType<PlayerLifetimeScope>();//ﾕﾙｼﾃ
            var colliders = lifetimeScope.Container.Resolve<VRMTouchColliders>();
            _actorEntity.Value = new ActorEntity(animator, _charaInfoData, vmdPlayer, colliders);

            await _attachPointService.SetupAsync(_actorEntity.Value.BoneMap, cancellation);

            instance.EnableUpdateWhenOffscreen(); // Mesh消え対策、パフォーマンスとトレードオフ
            instance.ShowMeshes(); // 使うときに表示する
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
