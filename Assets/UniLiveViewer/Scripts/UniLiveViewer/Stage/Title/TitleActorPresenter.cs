using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Threading;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Title.Actor
{
    public class TitleActorPresenter : IAsyncStartable
    {
        readonly TitleActorAnimatorService _animatorService;
        readonly ISubscriber<SceneTransitionMessage> _sceneTransitionSubscriber;
        readonly CompositeDisposable _disposable = new();

        [Inject]
        public TitleActorPresenter(
            TitleActorAnimatorService animatorService,
            ISubscriber<SceneTransitionMessage> sceneTransitionSubscriber)
        {
            _animatorService = animatorService;
            _sceneTransitionSubscriber = sceneTransitionSubscriber;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _sceneTransitionSubscriber
                .Subscribe(x =>
                {
                    _animatorService.OnSceneTransitionAsync(cancellation).Forget();
                }).AddTo(_disposable);

            await UniTask.CompletedTask;
        }
    }
}