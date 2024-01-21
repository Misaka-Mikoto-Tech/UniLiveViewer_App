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
        readonly InstanceId _instanceId;
        readonly ISubscriber<ActorAnimationMessage> _subscriber;
        readonly IActorService _actorEntityService;
        readonly LookatService _lookatService;
        readonly CharaInfoData _charaInfoData;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public LookatPresenter(
            InstanceId instanceId,
            ISubscriber<ActorAnimationMessage> subscriber,
            IActorService actorEntityService,
            LookatService lookatService,
            CharaInfoData charaInfoData)
        {
            _instanceId = instanceId;
            _subscriber = subscriber;
            _actorEntityService = actorEntityService;
            _lookatService = lookatService;
            _charaInfoData = charaInfoData;
        }

        void IStartable.Start()
        {
            _actorEntityService.ActorEntity()
                .Subscribe(x =>
                {
                    x?.LookAtBase.Setup(x.GetAnimator, _charaInfoData, Camera.main.transform);//うーむ
                    _lookatService.Setup(x?.HeadLookAt, x?.EyeLookAt);
                }).AddTo(_disposables);

            _subscriber
                .Subscribe(x =>
                {
                    if (_instanceId != x.InstanceId) return;
                    if (x.Mode == Menu.CurrentMode.PRESET)
                    {
                        _lookatService.OnChangeHeadLookAt(true);
                    }
                    else if (x.Mode == Menu.CurrentMode.CUSTOM)
                    {
                        _lookatService.OnChangeHeadLookAt(false);
                    }
                }).AddTo(_disposables);
        }

        void ILateTickable.LateTick()
        {
            _lookatService.OnLateTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
