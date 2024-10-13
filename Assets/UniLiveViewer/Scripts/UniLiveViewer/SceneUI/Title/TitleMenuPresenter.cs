using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Stage.Title;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.SceneUI.Title
{
    public class TitleMenuPresenter : IInitializable, IAsyncStartable, IDisposable
    {
        readonly TitleMenuService _titleMenuService;
        readonly SceneChangeService _sceneChangeService;
        readonly TitleMenuSettings _titleMenuSettings;
        readonly CompositeDisposable _disposable = new();
        readonly IPublisher<SceneTransitionMessage> _sceneTransitionPublisher;

        [Inject]
        public TitleMenuPresenter(
            TitleMenuService titleMenuService,
            TitleMenuSettings titleMenuSettings,
            SceneChangeService sceneChangeService,
            IPublisher<SceneTransitionMessage> sceneTransitionPublisher)
        {
            _titleMenuService = titleMenuService;
            _titleMenuSettings = titleMenuSettings;
            _sceneChangeService = sceneChangeService;
            _sceneTransitionPublisher = sceneTransitionPublisher;
        }

        void IInitializable.Initialize()
        {
            _sceneChangeService.Initialize();
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _titleMenuSettings.MainMenuButton[0].onClick.AsObservable()
                .Subscribe(async _ =>
                {
                    _sceneTransitionPublisher.Publish(new SceneTransitionMessage());
                    await _titleMenuService.LoadScenesAutoAsync(cancellation);
                })
                .AddTo(_disposable);
            _titleMenuSettings.MainMenuButton[1].onClick.AsObservable()
                .Subscribe(_ => _titleMenuService.OpenLicense(true))
                .AddTo(_disposable);
            _titleMenuSettings.MainMenuButton[2].onClick.AsObservable()
                .Subscribe(async _ => await _titleMenuService.QuitAppAsync(cancellation))
                .AddTo(_disposable);
            _titleMenuSettings.MainMenuButton[3].onClick.AsObservable()
                .Subscribe(_ => _titleMenuService.OpenLicense(false))
                .AddTo(_disposable);

            _titleMenuService.StartAsync(cancellation).Forget();
            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}