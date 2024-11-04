using MessagePipe;
using System;
using UniLiveViewer.Actor;
using UniLiveViewer.MessagePipe;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Timeline
{
    public class FakeShadowPresenter : IStartable, ITickable, IDisposable
    {
        readonly IActorEntity _actorEntity;
        readonly FakeShadowService _fakeShadowService;
        readonly ISubscriber<AllActorOperationMessage> _allSubscriber;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FakeShadowPresenter(
            IActorEntity actorService,
            FakeShadowService fakeShadowService,
            ISubscriber<AllActorOperationMessage> allSubscriber)
        {
            _actorEntity = actorService;
            _fakeShadowService = fakeShadowService;
            _allSubscriber = allSubscriber;
        }

        void IStartable.Start()
        {
            _allSubscriber
                .Subscribe(x =>
                {
                    if (x.ActorState != ActorState.FIELD) return;
                    if (x.ActorCommand != ActorCommand.UPDATE_SHADOW ) return;
                    _fakeShadowService.OnUpdateShadowType();
                }).AddTo(_disposables);

            _actorEntity.ActorEntity()
                .Subscribe(_fakeShadowService.OnChangeActorEntity)
                .AddTo(_disposables);
            _actorEntity.ActorState()
                .Select(x => x == ActorState.FIELD)
                .Subscribe(_fakeShadowService.SetEnable)
                .AddTo(_disposables);
            _actorEntity.RootScalar()
                .Subscribe(_fakeShadowService.OnChangeRootScalar)
                .AddTo(_disposables);

            _fakeShadowService.Setup();
        }

        void ITickable.Tick()
        {
            if (!_actorEntity.Active().Value) return;
            _fakeShadowService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _fakeShadowService.Dispose();
            _disposables.Dispose();
        }
    }
}
