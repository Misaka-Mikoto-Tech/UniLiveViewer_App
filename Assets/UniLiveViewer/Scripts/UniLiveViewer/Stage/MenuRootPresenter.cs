using Cysharp.Threading.Tasks;
using System;
using UniLiveViewer.Player;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class MenuRootPresenter : IStartable, IDisposable
    {
        readonly FileAccessManager _fileAccessManager;
        readonly MenuRootService _menuRootService;
        readonly PlayerInputService _playerInputService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public MenuRootPresenter(
            FileAccessManager fileAccessManager,
            MenuRootService menuRootService,
            PlayerInputService playerInputService)
        {
            _fileAccessManager = fileAccessManager;
            _menuRootService = menuRootService;
            _playerInputService = playerInputService;
        }

        void IStartable.Start()
        {
            _fileAccessManager.LoadEndAsObservable
                .Subscribe(_ => _menuRootService.OnLoadEnd())
                .AddTo(_disposables);
            _playerInputService.ClickMenuAsObservable()
                .Where(x => x == PlayerHandType.RHand)
                .Subscribe(_ => _menuRootService.OnMenuSwitching())
                .AddTo(_disposables);

            _menuRootService.Initialize();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
