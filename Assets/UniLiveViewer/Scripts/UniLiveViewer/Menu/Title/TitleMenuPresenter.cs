using Cysharp.Threading.Tasks;
using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class TitleMenuPresenter : IStartable, IDisposable
    {
        readonly TitleBackGroundService _backGroundService;
        readonly TitleMenuService _menuService;

        readonly CompositeDisposable _disposable;

        [Inject]
        public TitleMenuPresenter(
            TitleBackGroundService titleBackGroundService,
            TitleMenuService titleMenuService)
        {
            _backGroundService = titleBackGroundService;
            _menuService = titleMenuService;

            _disposable = new CompositeDisposable();
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