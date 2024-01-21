using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Actor;
using UniLiveViewer.MessagePipe;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Timeline
{
    public class PlayableBinderPresenter : IStartable, IDisposable
    {
        readonly ISubscriber<AllActorOperationMessage> _allSubscriber;
        readonly ISubscriber<ActorOperationMessage> _subscriber;
        readonly PlayableBinderService _playableBinderService;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public PlayableBinderPresenter(
            ISubscriber<AllActorOperationMessage> allSubscriber,
            ISubscriber<ActorOperationMessage> subscriber,
            PlayableBinderService playableBinderService)
        {
            _allSubscriber = allSubscriber;
            _subscriber = subscriber;
            _playableBinderService = playableBinderService;
        }

        void IStartable.Start()
        {
            _allSubscriber
                .Subscribe(x =>
                {
                    if (x.ActorCommand == ActorCommand.DELETE)
                    {
                        _playableBinderService.OnDeleteAllActor();
                    }
                }).AddTo(_disposables);
            _subscriber
                .Subscribe(x =>
                {
                    if (x.ActorCommand == ActorCommand.DELETE)
                    {
                        // 魔法陣カーソル削除を想定
                        _playableBinderService.OnDeleteActor(x.InstanceId);
                    }
                }).AddTo(_disposables);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }

}