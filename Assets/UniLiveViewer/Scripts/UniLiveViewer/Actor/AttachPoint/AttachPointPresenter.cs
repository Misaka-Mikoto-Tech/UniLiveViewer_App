using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniRx;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.AttachPoint
{
    public class AttachPointPresenter : IStartable, ITickable, IDisposable
    {
        readonly ISubscriber<AttachPointMessage> _subscriber;
        readonly AttachPointService _attachPointService;
        readonly PlayableDirector _playableDirector;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public AttachPointPresenter(
            ISubscriber<AttachPointMessage> subscriber,
            AttachPointService attachPointService,
            PlayableDirector playableDirector)
        {
            _subscriber = subscriber;
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
            _attachPointService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
