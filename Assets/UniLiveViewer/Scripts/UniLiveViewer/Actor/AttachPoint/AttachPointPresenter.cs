using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.AttachPoint
{
    public class AttachPointPresenter : IStartable, ITickable, IDisposable
    {
        readonly ISubscriber<AttachPointMessage> _subscriber;
        readonly IActorEntity _actorEntity;
        readonly AttachPointService _attachPointService;
        readonly PlayableDirector _playableDirector;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public AttachPointPresenter(
            ISubscriber<AttachPointMessage> subscriber,
            IActorEntity actorEntity,
            AttachPointService attachPointService,
            PlayableDirector playableDirector)
        {
            _subscriber = subscriber;
            _actorEntity = actorEntity;
            _attachPointService = attachPointService;
            _playableDirector = playableDirector;
        }

        void IStartable.Start()
        {
            _subscriber
                .Subscribe(x =>
                {
                    if (_playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) return;
                    _attachPointService.SetActive(x.IsActive);
                }).AddTo(_disposables);
        }

        void ITickable.Tick()
        {
            if (!_actorEntity.Active().Value) return;
            _attachPointService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
