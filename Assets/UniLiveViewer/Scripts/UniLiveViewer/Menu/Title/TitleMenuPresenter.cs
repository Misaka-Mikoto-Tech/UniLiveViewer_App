using Cysharp.Threading.Tasks;
using System;
using UniLiveViewer.SceneLoader;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class TitleMenuPresenter : IInitializable, IStartable, IDisposable
    {
        readonly SceneChangeService _sceneChangeService;
        readonly TitleBackGroundService _backGroundService;
        readonly TitleMenuService _menuService;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public TitleMenuPresenter(
            TitleBackGroundService titleBackGroundService,
            TitleMenuService titleMenuService,
            SceneChangeService sceneChangeService)
        {
            _sceneChangeService = sceneChangeService;
            _backGroundService = titleBackGroundService;
            _menuService = titleMenuService;
        }

        void IInitializable.Initialize()
        {
            _sceneChangeService.Initialize();
        }

        void IStartable.Start()
        {
            _menuService.ChangeSceneAsObservable
                .Subscribe(_backGroundService.OnChangeLanguage)
                .AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

    }
}