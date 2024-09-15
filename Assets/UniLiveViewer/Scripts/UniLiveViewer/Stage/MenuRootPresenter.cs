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
        readonly MenuRootService _menuGripperService;
        readonly PlayerInputService _playerInputService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public MenuRootPresenter(
            FileAccessManager fileAccessManager,
            MenuRootService menuGripperService,
            PlayerInputService playerInputService)
        {
            _fileAccessManager = fileAccessManager;
            _menuGripperService = menuGripperService;
            _playerInputService = playerInputService;
        }

        void IStartable.Start()
        {
            _fileAccessManager.LoadEndAsObservable
                .Subscribe(_ => _menuGripperService.OnLoadEnd())
                .AddTo(_disposables);
            _playerInputService.ClickMenuAsObservable()
                .Where(x => x == PlayerHandType.RHand)
                .Subscribe(_ => _menuGripperService.OnMenuSwitching())
                .AddTo(_disposables);

            _menuGripperService.Initialize();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
