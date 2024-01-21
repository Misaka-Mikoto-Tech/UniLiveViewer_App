using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.ValueObject;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Expression
{
    public class ExpressionPresenter : IStartable, ILateTickable, IDisposable
    {
        readonly InstanceId _instanceId;
        readonly ExpressionService _expressionService;
        readonly ISubscriber<ActorOperationMessage> _operationSubscriber;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public ExpressionPresenter(
            InstanceId instanceId,
            ExpressionService expressionService,
            ISubscriber<ActorOperationMessage> operationSubscriber)
        {
            _instanceId = instanceId;
            _expressionService = expressionService;
            _operationSubscriber = operationSubscriber;
        }

        void IStartable.Start()
        {
            //表情系はUI側でVRMしか飛んでこないようにしてる
            _operationSubscriber
                .Subscribe(x =>
                {
                    if (_instanceId != x.InstanceId) return;
                    if (x.ActorCommand == ActorCommand.FACILSYNC_ENEBLE)
                    {
                        _expressionService.OnChangeFacialSync(true);
                    }
                    else if (x.ActorCommand == ActorCommand.FACILSYNC_DISABLE)
                    {
                        _expressionService.OnChangeFacialSync(false);
                    }
                    else if (x.ActorCommand == ActorCommand.LIPSYNC_ENEBLE)
                    {
                        _expressionService.OnChangeLipSync(true);
                    }
                    else if (x.ActorCommand == ActorCommand.LIPSYNC_DISABLE)
                    {
                        _expressionService.OnChangeLipSync(false);
                    }
                }).AddTo(_disposables);
        }

        void ILateTickable.LateTick()
        {
            _expressionService.OnLateTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
