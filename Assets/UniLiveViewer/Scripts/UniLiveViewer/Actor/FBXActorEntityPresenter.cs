﻿using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.Stage;
using UniLiveViewer.ValueObject;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor
{
    public class FBXActorEntityPresenter : IAsyncStartable, ITickable, IDisposable
    {
        bool _isTick;

        readonly ISubscriber<AllActorOperationMessage> _allSubscriber;
        readonly ISubscriber<ActorOperationMessage> _subscriber;
        readonly ISubscriber<ActorResizeMessage> _resizeSubscriber;
        readonly IActorEntity _actorEntity;
        readonly InstanceId _instanceId;
        readonly GeneratorPortalAnchor _firstParent;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FBXActorEntityPresenter(
            ISubscriber<AllActorOperationMessage> allSubscriber,
            ISubscriber<ActorOperationMessage> subscriber,
            ISubscriber<ActorResizeMessage> resizeSubscriber,
            IActorEntity actorEntity,
            InstanceId instanceId,
            GeneratorPortalAnchor firstParent)
        {
            _allSubscriber = allSubscriber;
            _subscriber = subscriber;
            _resizeSubscriber = resizeSubscriber;
            _actorEntity = actorEntity;
            _instanceId = instanceId;
            _firstParent = firstParent;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _resizeSubscriber
                .Subscribe(x =>
                {
                    if (x.InstanceId != _instanceId) return;
                    _actorEntity.AddRootScalar(x.AddScale);
                }).AddTo(_disposables);

            _allSubscriber
                .Subscribe(x =>
                {
                    if (x.ActorState != _actorEntity.ActorState().Value) return;
                    OnCommand(x.ActorCommand);
                }).AddTo(_disposables);
            _subscriber
                .Subscribe(x =>
                {
                    if (x.InstanceId != _instanceId) return;
                    OnCommand(x.ActorCommand);
                }).AddTo(_disposables);

            _actorEntity.Active()
                .Subscribe(x => _isTick = x)
                .AddTo(_disposables);

            await _actorEntity.SetupAsync(_firstParent.transform, cancellation);
        }

        void OnCommand(ActorCommand command)
        {
            if (command == ActorCommand.ACTIVE)
            {
                _actorEntity.Activate(true);
            }
            if (command == ActorCommand.INACTIVE)
            {
                _actorEntity.Activate(false);
            }
            if (command == ActorCommand.DELETE)
            {
                _actorEntity.Delete();
            }
        }

        void ITickable.Tick()
        {
            if (!_isTick) return;
            _actorEntity.OnTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
