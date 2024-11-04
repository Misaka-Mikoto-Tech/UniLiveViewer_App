using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.AttachPoint
{
    public class AttachPointPresenter : IStartable, IDisposable
    {
        readonly ISubscriber<AllActorOperationMessage> _operationMessageSubscriber;
        readonly ISubscriber<AttachPointMessage> _subscriber;

        readonly AttachPointService _attachPointService;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public AttachPointPresenter(
            ISubscriber<AllActorOperationMessage> operationMessageSubscriber,
            ISubscriber<AttachPointMessage> subscriber,
            AttachPointService attachPointService)
        {
            _operationMessageSubscriber = operationMessageSubscriber;
            _subscriber = subscriber;
            _attachPointService = attachPointService;
        }

        void IStartable.Start()
        {
            _subscriber.Subscribe(x => _attachPointService.SetActive(x.IsActive)).AddTo(_disposables);

            _operationMessageSubscriber
                .Subscribe(x => 
                {
                    if(x.ActorCommand == ActorCommand.TIMELINE_PLAY)
                    {
                        _attachPointService.OnPlayTimeline();
                    }
                }).AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
