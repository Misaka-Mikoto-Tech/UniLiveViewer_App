using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Option
{
    public class GuideAnchorPresenter : IStartable, ITickable, IDisposable
    {
        readonly ISubscriber<AllActorOptionMessage> _subscriber;
        readonly IActorEntity _actorEntity;
        readonly GuideAnchorService _guideAnchorService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public GuideAnchorPresenter(
            ISubscriber<AllActorOptionMessage> subscriber,
            IActorEntity actorEntity,
            GuideAnchorService guideAnchorService)
        {
            _subscriber = subscriber;
            _actorEntity = actorEntity;
            _guideAnchorService = guideAnchorService;
        }

        void IStartable.Start()
        {
            _actorEntity.ActorEntity()
                .Subscribe(_guideAnchorService.OnChangeActorEntity)
                .AddTo(_disposables);

            _subscriber
                .Subscribe(x =>
                {
                    if (x.ActorState == ActorState.NULL) return;
                    if (x.ActorState != _actorEntity.ActorState().Value) return;
                    _guideAnchorService.SetEnable(x.ActorCommand == ActorOptionCommand.GUIDE_ANCHOR_ENEBLE);
                }).AddTo(_disposables);

            _guideAnchorService.Setup();
        }

        void ITickable.Tick()
        {
            if (!_actorEntity.Active().Value) return;
            _guideAnchorService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
