using Cysharp.Threading.Tasks;
using MessagePipe;
using NanaCiel;
using System;
using System.Threading;
using UniGLTF;
using UniLiveViewer.Actor.AttachPoint;
using UniLiveViewer.Actor.Expression;
using UniLiveViewer.Stage;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using UniVRM10;
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
        readonly ILipSync _lipSync;
        readonly IFacialSync _faceSync;
        readonly PlayerHandVRMCollidersService _playerHandVRMColliders;

        //VRMUI非表示専用
        readonly IPublisher<VRMLoadResultData> _publisher;

        [Inject]
        public VRMActorService(
            LifetimeScope lifetimeScope,
            VRMService vrmService,
            CharaInfoData charaInfoData,
            VRMLoadData data,
            ILipSync lipSync,
            IFacialSync facialSync,
            AttachPointService attachPointService,
            IPublisher<VRMLoadResultData> publisher,
            PlayerHandVRMCollidersService playerHandVRMColliders)
        {
            _lifetimeScope = lifetimeScope;
            _vrmService = vrmService;
            _charaInfoData = charaInfoData;
            _attachPointService = attachPointService;
            _publisher = publisher;

            _data = data;
            _lipSync = lipSync;
            _faceSync = facialSync;
            _playerHandVRMColliders = playerHandVRMColliders;
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
                if (FileReadAndWriteUtility.UserProfile.IsVRM10)
                {
                    var instance = await _vrmService.Load10Async(_data.FullPath, cancellation);//1.0
                    await SetupInternalAsync(instance, cancellation);
                }
                else
                {
                    var instance = await _vrmService.LoadAsync(_data.FullPath, cancellation);
                    await SetupInternalAsync(instance, cancellation);
                }
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

        async UniTask SetupInternalAsync(Vrm10Instance instance, CancellationToken cancellation)
        {
            var go = instance.gameObject;
            go.transform.SetParent(_lifetimeScope.transform, false);
            go.name = instance.Vrm.Meta.Name;
            go.layer = Constants.LayerNoGrabObject;//オートカメラ識別にも利用

            // 表情系
            var runtimeExpression = instance.Runtime.Expression;
            _lipSync.Setup(instance.transform, expression: runtimeExpression);
            _faceSync.Setup(instance.transform, expression: runtimeExpression);

            await UniTask.Delay(100);// TODO: 最後に調整

            _charaInfoData.viewName = go.name;
            var vmdPlayer = go.AddComponent<VMDPlayer_Custom>();
            var charaInfoData = GameObject.Instantiate(_charaInfoData);
            vmdPlayer.Initialize(charaInfoData, _faceSync, _lipSync);

            _actorEntity.Value = new ActorEntity(instance.GetComponent<Animator>(), _charaInfoData, vmdPlayer, _playerHandVRMColliders);

            await _attachPointService.SetupAsync(_actorEntity.Value.BoneMap, cancellation);

            var runtimeGltfInstance = instance.GetComponent<RuntimeGltfInstance>();
            runtimeGltfInstance.EnableUpdateWhenOffscreen(); // Mesh消え対策
            runtimeGltfInstance.ShowMeshes();
        }

        async UniTask SetupInternalAsync(RuntimeGltfInstance instance, CancellationToken cancellation)
        {
            var go = instance.gameObject;
            go.transform.SetParent(_lifetimeScope.transform, false);
            go.name = go.GetComponent<VRMMeta>().Meta.Title;
            go.layer = Constants.LayerNoGrabObject;//オートカメラ識別にも利用

            // 表情系
            var vrmBlendShape = go.GetComponent<VRMBlendShapeProxy>();
            _lipSync.Setup(instance.transform, vrmBlendShape);
            _faceSync.Setup(instance.transform, vrmBlendShape);

            await UniTask.Delay(100);// TODO: 最後に調整

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
            var charaInfoData = GameObject.Instantiate(_charaInfoData);
            vmdPlayer.Initialize(charaInfoData, _faceSync, _lipSync);

            _actorEntity.Value = new ActorEntity(instance.GetComponent<Animator>(), _charaInfoData, vmdPlayer, _playerHandVRMColliders);

            await _attachPointService.SetupAsync(_actorEntity.Value.BoneMap, cancellation);

            instance.EnableUpdateWhenOffscreen(); // Mesh消え対策
            instance.ShowMeshes();
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

            //TODO: これもどうにかしたい、小さい場合にまだ変対応しきれていない？
            if (_actorEntity.Value.GetAnimator.TryGetComponent<Vrm10Instance>(out var instance))
            {
                instance.Runtime.ReconstructSpringBone();
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
