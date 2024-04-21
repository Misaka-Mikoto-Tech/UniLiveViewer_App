using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.ValueObject;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.LookAt
{
    public class LookatPresenter : IStartable, ILateTickable, IDisposable
    {
        bool _isActorActive;

        readonly InstanceId _instanceId;
        readonly ISubscriber<ActorAnimationMessage> _subscriber;
        readonly IActorEntity _actorEntity;
        readonly LookAtService _lookAtService;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public LookatPresenter(
            InstanceId instanceId,
            ISubscriber<ActorAnimationMessage> subscriber,
            IActorEntity actorEntity,
            LookAtService lookAtService)
        {
            _instanceId = instanceId;
            _subscriber = subscriber;
            _actorEntity = actorEntity;
            _lookAtService = lookAtService;
        }

        void IStartable.Start()
        {
            _subscriber
                .Subscribe(x =>
                {
                    if (_instanceId != x.InstanceId) return;
                    if (!_isActorActive) return;
                    if (x.Mode == Menu.CurrentMode.PRESET)
                    {
                        _lookAtService.SetHeadEnable(true);
                    }
                    else if (x.Mode == Menu.CurrentMode.CUSTOM)
                    {
                        _lookAtService.SetHeadEnable(false);
                    }
                }).AddTo(_disposables);

            _actorEntity.Active()
                .Subscribe(x => _isActorActive = x)
                .AddTo(_disposables);
        }

        void ILateTickable.LateTick()
        {
            if (!_isActorActive) return;
            if (Time.timeScale == 0) return;

            _lookAtService.OnLateTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
