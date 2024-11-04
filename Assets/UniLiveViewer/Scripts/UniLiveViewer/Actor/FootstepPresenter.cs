using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor
{
    public class FootstepPresenter : IAsyncStartable, IFixedTickable, IDisposable
    {
        readonly IActorEntity _actorEntity;
        readonly FootStepService _footStepService;
        readonly RootAudioSourceService _rootAudioSourceService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FootstepPresenter(
            IActorEntity actorEntity,
            FootStepService footStepService,
            RootAudioSourceService rootAudioSourceService)
        {
            _actorEntity = actorEntity;
            _footStepService = footStepService;
            _rootAudioSourceService = rootAudioSourceService;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _actorEntity.ActorEntity()
                .Subscribe(_footStepService.OnChangeActorEntity)
                .AddTo(_disposables);

            _rootAudioSourceService.FootStepsVolumeRate
                .Subscribe(_footStepService.SetVolume)
                .AddTo(_disposables);

            await UniTask.CompletedTask;
        }

        void IFixedTickable.FixedTick()
        {
            if (!_actorEntity.Active().Value) return;
            _footStepService.OnFixedTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
