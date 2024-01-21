using System;
using UniLiveViewer.Actor;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Timeline
{
    public class FakeShadowPresenter : IStartable, ITickable, IDisposable
    {
        readonly IActorService _actorService;
        readonly FakeShadowService _fakeShadowService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FakeShadowPresenter(
            IActorService actorService,
            FakeShadowService fakeShadowService)
        {
            _actorService = actorService;
            _fakeShadowService = fakeShadowService;
        }

        void IStartable.Start()
        {
            _actorService.ActorEntity()
                .Subscribe(_fakeShadowService.OnChangeActorEntity)
                .AddTo(_disposables);
            _actorService.ActorState()
                .Select(x => x == ActorState.FIELD)
                .Subscribe(_fakeShadowService.SetEnable)
                .AddTo(_disposables);
            _actorService.RootScalar()
                .Subscribe(_fakeShadowService.OnChangeRootScalar)
                .AddTo(_disposables);

            _fakeShadowService.Setup();
        }

        void ITickable.Tick()
        {
            _fakeShadowService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _fakeShadowService.Dispose();
            _disposables.Dispose();
        }
    }
}
