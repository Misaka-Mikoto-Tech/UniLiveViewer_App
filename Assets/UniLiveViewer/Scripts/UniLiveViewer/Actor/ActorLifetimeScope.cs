using UniLiveViewer.Actor.Animation;
using UniLiveViewer.Actor.AttachPoint;
using UniLiveViewer.Actor.Expression;
using UniLiveViewer.Actor.LookAt;
using UniLiveViewer.OVRCustom;
using UniLiveViewer.Timeline;
using UniLiveViewer.ValueObject;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(OVRGrabbable_Custom))]

    public class ActorInstaller : IInstaller
    {
        ActorId _actorId;
        VRMLoadData _data;
        InstanceId _instanceId;
        public ActorInstaller(ActorId actorId, VRMLoadData data, InstanceId instanceId)
        {
            _actorId = actorId;
            _data = data;
            _instanceId = instanceId;
        }

        void IInstaller.Install(IContainerBuilder builder)
        {
            builder.RegisterInstance(_data);
            builder.RegisterInstance(_actorId);
            builder.RegisterInstance(_instanceId);
        }
    }

    public class ActorLifetimeScope : LifetimeScope
    {
        public ActorId ActorId { get; private set; }
        public InstanceId InstanceId { get; private set; }

        [Header("-----ユニーク------")]
        /// <summary>
        /// inspector確認用
        /// </summary>
        [SerializeField] int _actorId;
        [SerializeField] int _instanceId;
        [SerializeField] CharaInfoData _charaInfoData;
        /// <summary>
        /// JumpList用
        /// </summary>
        public string ActorName => _charaInfoData.viewName;

        [Header("-----------")]

        [SerializeField] Rigidbody _rigidbody;
        [SerializeField] CapsuleCollider _collider;
        [SerializeField] OVRGrabbable_Custom _ovrGrabbable;
        [SerializeField] AudioSource _audioSource;

        [Header("-----------")]
        [SerializeField] AttachPoint.AttachPoint _attachPoint;
        [SerializeField] RuntimeAnimatorController _runtimeAnimatorController;

        [SerializeField] LipSync_FBX _lipSyncFBX;
        [SerializeField] LipSync_VRM _lipSyncVRM;
        [SerializeField] FacialSync_FBX _faceSyncFBX;
        [SerializeField] FacialSync_VRM _faceSyncVRM;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_ovrGrabbable);

            PhysicsConfigure(builder);
            FootstepConfigure(builder);

            if (_charaInfoData.ActorType == ActorType.FBX)
            {
                builder.RegisterInstance(_charaInfoData);

                var animator = transform.GetComponentInChildren<Animator>();
                builder.RegisterInstance(animator);

                builder.Register<IActorService, FBXActorService>(Lifetime.Singleton);
                FBXFacialExpressionConfigure(builder);
                builder.RegisterEntryPoint<FBXActorEntityPresenter>();
            }
            else if (_charaInfoData.ActorType == ActorType.VRM)
            {
                var charaInfoData = Instantiate(_charaInfoData);
                builder.RegisterInstance(charaInfoData);

                builder.Register<VRMService>(Lifetime.Singleton);
                builder.Register<IActorService, VRMActorService>(Lifetime.Singleton);
                VRMFacialExpressionConfigure(builder);
                builder.RegisterEntryPoint<VRMActorEntityPresenter>();
                builder.RegisterEntryPoint<SpringBonePresenter>();
            }

            builder.Register<LookatService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<LookatPresenter>();
            AnimationConfigure(builder);
            AttachPointConfigure(builder);
        }

        void Start()
        {
            // 良い解決方法が思いつかない..
            ActorId = Container.Resolve<ActorId>();
            _actorId = ActorId.ID;
            InstanceId = Container.Resolve<InstanceId>();
            _instanceId = InstanceId.Id;
        }

        void PhysicsConfigure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_rigidbody);
            builder.RegisterInstance(_collider);
            builder.RegisterEntryPoint<PhysicsPresenter>(Lifetime.Singleton);
        }

        void FootstepConfigure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_audioSource);
            builder.Register<FootstepService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<FootstepPresenter>(Lifetime.Singleton);
        }

        void FBXFacialExpressionConfigure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_lipSyncFBX).As<ILipSync>();
            builder.RegisterInstance(_faceSyncFBX).As<IFacialSync>();
            builder.Register<ExpressionService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ExpressionPresenter>();
        }

        void VRMFacialExpressionConfigure(IContainerBuilder builder)
        {
            // TODO: Instanceでいいんだっけ
            var lipSync = GameObject.Instantiate(_lipSyncVRM.gameObject).GetComponent<LipSync_VRM>();
            builder.RegisterInstance(lipSync).As<ILipSync>();
            var faceSync = GameObject.Instantiate(_faceSyncVRM.gameObject).GetComponent<FacialSync_VRM>();
            builder.RegisterInstance(faceSync).As<IFacialSync>();
            builder.Register<ExpressionService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<ExpressionPresenter>();
        }

        void AnimationConfigure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_runtimeAnimatorController);
            builder.Register<AnimationService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<AnimationPresenter>();
        }

        void AttachPointConfigure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_attachPoint);
            builder.Register<AttachPointService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<AttachPointPresenter>();
        }
    }
}
