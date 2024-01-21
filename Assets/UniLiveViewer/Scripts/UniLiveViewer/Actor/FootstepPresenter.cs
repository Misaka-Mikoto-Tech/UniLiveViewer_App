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
        readonly IActorService _actorEntityService;
        readonly FootstepService _footstepService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FootstepPresenter(
            IActorService actorEntityService,
            FootstepService footstepService)
        {
            _actorEntityService = actorEntityService;
            _footstepService = footstepService;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _actorEntityService.ActorEntity()
                .Subscribe(_footstepService.OnChangeActorEntity)
                .AddTo(_disposables);

            await UniTask.CompletedTask;
        }

        void IFixedTickable.FixedTick()
        {
            _footstepService.OnFixedTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
