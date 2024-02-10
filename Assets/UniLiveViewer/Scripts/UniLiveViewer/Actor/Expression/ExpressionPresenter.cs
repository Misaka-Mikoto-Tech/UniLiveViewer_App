﻿using Cysharp.Threading.Tasks;
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
        bool _isTick;

        readonly InstanceId _instanceId;
        readonly IActorEntity _actorEntity;
        readonly ExpressionService _expressionService;
        readonly ISubscriber<ActorOperationMessage> _operationSubscriber;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public ExpressionPresenter(
            InstanceId instanceId,
            IActorEntity actorEntity,
            ExpressionService expressionService,
            ISubscriber<ActorOperationMessage> operationSubscriber)
        {
            _instanceId = instanceId;
            _actorEntity = actorEntity;
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

            _actorEntity.Active()
                .Subscribe(x => _isTick = x)
                .AddTo(_disposables);
        }

        void ILateTickable.LateTick()
        {
            if (!_isTick) return;
            _expressionService.OnLateTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
