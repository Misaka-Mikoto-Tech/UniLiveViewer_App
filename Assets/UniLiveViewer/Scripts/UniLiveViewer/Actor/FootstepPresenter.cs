using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor
{
    public class FootstepPresenter : IAsyncStartable, IFixedTickable, IDisposable
    {
        readonly IActorEntity _actorEntity;
        readonly FootstepService _footstepService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FootstepPresenter(
            IActorEntity actorEntity,
            FootstepService footstepService)
        {
            _actorEntity = actorEntity;
            _footstepService = footstepService;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _actorEntity.ActorEntity()
                .Subscribe(_footstepService.OnChangeActorEntity)
                .AddTo(_disposables);

            await UniTask.CompletedTask;
        }

        void IFixedTickable.FixedTick()
        {
            if (!_actorEntity.Active().Value) return;
            _footstepService.OnFixedTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
