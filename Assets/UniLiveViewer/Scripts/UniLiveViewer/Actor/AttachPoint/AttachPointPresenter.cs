using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.AttachPoint
{
    public class AttachPointPresenter : IStartable, ITickable, IDisposable
    {
        readonly ISubscriber<AttachPointMessage> _subscriber;
        readonly IActorEntity _actorEntity;
        readonly AttachPointService _attachPointService;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public AttachPointPresenter(
            ISubscriber<AttachPointMessage> subscriber,
            IActorEntity actorEntity,
            AttachPointService attachPointService)
        {
            _subscriber = subscriber;
            _actorEntity = actorEntity;
            _attachPointService = attachPointService;
        }

        void IStartable.Start()
        {
            _subscriber.Subscribe(x => _attachPointService.SetActive(x.IsActive)).AddTo(_disposables);
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
