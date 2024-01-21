using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.Stage;
using UniLiveViewer.ValueObject;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor
{
    public class FBXActorEntityPresenter : IAsyncStartable, ITickable, IDisposable
    {
        readonly ISubscriber<AllActorOperationMessage> _allSubscriber;
        readonly ISubscriber<ActorOperationMessage> _subscriber;
        readonly ISubscriber<ActorResizeMessage> _resizeSubscriber;
        readonly IActorService _actorEntityService;
        readonly InstanceId _instanceId;
        readonly GeneratorPortalAnchor _firstParent;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public FBXActorEntityPresenter(
            ISubscriber<AllActorOperationMessage> allSubscriber,
            ISubscriber<ActorOperationMessage> subscriber,
            ISubscriber<ActorResizeMessage> resizeSubscriber,
            IActorService actorEntityService,
            InstanceId instanceId,
            GeneratorPortalAnchor firstParent)
        {
            _allSubscriber = allSubscriber;
            _subscriber = subscriber;
            _resizeSubscriber = resizeSubscriber;
            _actorEntityService = actorEntityService;
            _instanceId = instanceId;
            _firstParent = firstParent;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _resizeSubscriber
                .Subscribe(x =>
                {
                    if (x.InstanceId != _instanceId) return;
                    _actorEntityService.AddRootScalar(x.AddScale);
                }).AddTo(_disposables);

            _allSubscriber
                .Subscribe(x =>
                {
                    if (x.ActorState != _actorEntityService.ActorState().Value) return;
                    OnCommand(x.ActorCommand);
                }).AddTo(_disposables);
            _subscriber
                .Subscribe(x =>
                {
                    if (x.InstanceId != _instanceId) return;
                    OnCommand(x.ActorCommand);
                }).AddTo(_disposables);

            await _actorEntityService.SetupAsync(_firstParent.transform, cancellation);
        }

        void OnCommand(ActorCommand command)
        {
            if (command == ActorCommand.ACTIVE)
            {
                _actorEntityService.Activate(true);
            }
            if (command == ActorCommand.INACTIVE)
            {
                _actorEntityService.Activate(false);
            }
            if (command == ActorCommand.DELETE)
            {
                _actorEntityService.Delete();
            }
        }

        void ITickable.Tick()
        {
            _actorEntityService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
