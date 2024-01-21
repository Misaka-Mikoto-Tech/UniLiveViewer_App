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
        readonly IActorService _actorService;
        readonly GuideAnchorService _guideAnchorService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public GuideAnchorPresenter(
            ISubscriber<AllActorOptionMessage> subscriber,
            IActorService actorService,
            GuideAnchorService guideAnchorService)
        {
            _subscriber = subscriber;
            _actorService = actorService;
            _guideAnchorService = guideAnchorService;
        }

        void IStartable.Start()
        {
            _actorService.ActorEntity()
                .Subscribe(_guideAnchorService.OnChangeActorEntity)
                .AddTo(_disposables);

            _subscriber
                .Subscribe(x =>
                {
                    if (x.ActorState == ActorState.NULL) return;
                    if (x.ActorState != _actorService.ActorState().Value) return;
                    _guideAnchorService.SetEnable(x.ActorCommand == ActorOptionCommand.GUID_ANCHOR_ENEBLE);
                }).AddTo(_disposables);

            _guideAnchorService.Setup();
        }

        void ITickable.Tick()
        {
            _guideAnchorService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
