﻿using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.Actor.Expression;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.ValueObject;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Actor.Animation
{
    public class AnimationPresenter : IAsyncStartable, ILateTickable, IDisposable
    {
        bool _isTick;

        readonly InstanceId _instanceId;
        readonly ISubscriber<AllActorOperationMessage> _allSubscriber;
        readonly AnimationService _animationService;
        readonly ExpressionService _expressionService;
        readonly IActorEntity _actorEntity;
        readonly ISubscriber<ActorAnimationMessage> _subscriber;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public AnimationPresenter(
            InstanceId instanceId,
            ISubscriber<AllActorOperationMessage> allSubscriber,
            AnimationService animationService,
            ExpressionService expressionService,
            IActorEntity actorEntity,
            ISubscriber<ActorAnimationMessage> subscriber)
        {
            _instanceId = instanceId;
            _allSubscriber = allSubscriber;
            _animationService = animationService;
            _expressionService = expressionService;
            _actorEntity = actorEntity;
            _subscriber = subscriber;
        }

        UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _actorEntity.ActorEntity()
                .Subscribe(_animationService.OnChangeAnimator)
                .AddTo(_disposables);

            _actorEntity.ActorState()
                .Subscribe();

            _subscriber
                .Subscribe(async x =>
                {
                    if (_instanceId != x.InstanceId) return;
                    await _animationService.SetAnimationAsync(x.Mode, x.AnimationIndex, x.IsReverse, cancellation);
                    _expressionService.OnChangeMode(x.Mode);
                }).AddTo(_disposables);

            _allSubscriber
                .Subscribe(x =>
                {
                    if (x.ActorState != ActorState.NULL) return;
                    if (x.ActorCommand == ActorCommand.TIMELINE_PLAY)
                    {
                        _animationService.ReturnRuntimeAnimatorController();
                    }
                    else if (x.ActorCommand == ActorCommand.TIMELINE_NONPLAY)
                    {
                        _animationService.RemoveRuntimeAnimatorController();
                    }
                }).AddTo(_disposables);

            _actorEntity.Active()
                .Subscribe(x => _isTick = x)
                .AddTo(_disposables);

            return UniTask.CompletedTask;
        }

        void ILateTickable.LateTick()
        {
            if (!_isTick) return;
            //掴まれている時以外は常時
            if (_actorEntity.ActorState().Value == ActorState.HOLD) return;
            _animationService.OnLateTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
