﻿using Cysharp.Threading.Tasks;
using System.Threading;
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
        protected override void Awake()
        {
            autoRun = false;
            base.Awake();
        }

        public async UniTask BuildAsync(CancellationToken cancellation)
        {
            await UniTask.RunOnThreadPool(Build, cancellationToken: cancellation);

            // 良い解決方法が思いつかない..
            ActorId = Container.Resolve<ActorId>();
            _actorId = ActorId.ID;
            InstanceId = Container.Resolve<InstanceId>();
            _instanceId = InstanceId.Id;
        }

        public ActorId ActorId { get; private set; }
        public InstanceId InstanceId { get; private set; }

        [Header("-----ユニーク------")]
        /// <summary>
        /// inspector確認用
        /// </summary>
        [SerializeField] int _actorId;
        [SerializeField] int _instanceId;
        [SerializeField] CharaInfoData _charaInfoData;
        [SerializeField] Animator _animator;//FBXのみ

        /// <summary>
        /// JumpList用
        /// </summary>
        public string ActorName => _charaInfoData.viewName;

        [Header("-----------")]

        [SerializeField] Rigidbody _rigidbody;
        [SerializeField] CapsuleCollider _collider;
        [SerializeField] OVRGrabbable_Custom _ovrGrabbable;
        [SerializeField] AudioSourceService _audioSourceService;

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
                builder.RegisterInstance(_animator);

                builder.Register<IActorEntity, FBXActorService>(Lifetime.Singleton);
                FBXFacialExpressionConfigure(builder);
            }
            else if (_charaInfoData.ActorType == ActorType.VRM)
            {
                builder.RegisterInstance(_charaInfoData);

                builder.Register<VRMService>(Lifetime.Singleton);
                builder.Register<IActorEntity, VRMActorService>(Lifetime.Singleton);
                VRMFacialExpressionConfigure(builder);
                builder.RegisterEntryPoint<SpringBonePresenter>();
            }

            builder.RegisterEntryPoint<ActorEntityPresenter>();

            builder.Register<LookatService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<LookatPresenter>();
            AnimationConfigure(builder);
            AttachPointConfigure(builder);
        }

        void PhysicsConfigure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_rigidbody);
            builder.RegisterInstance(_collider);
            builder.RegisterEntryPoint<PhysicsPresenter>(Lifetime.Singleton);
        }

        void FootstepConfigure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_audioSourceService);
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
            builder.RegisterInstance(_lipSyncVRM).As<ILipSync>();
            builder.RegisterInstance(_faceSyncVRM).As<IFacialSync>();
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
