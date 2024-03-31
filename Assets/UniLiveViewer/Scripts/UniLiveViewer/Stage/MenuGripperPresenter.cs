using Cysharp.Threading.Tasks;
using System;
using UniLiveViewer.Player;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class MenuGripperPresenter : IStartable, ILateTickable, IDisposable
    {
        readonly FileAccessManager _fileAccessManager;
        readonly MenuGripperService _menuGripperService;
        readonly PlayerStateManager _playerStateManager;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public MenuGripperPresenter(
            FileAccessManager fileAccessManager,
            MenuGripperService menuGripperService,
            PlayerStateManager playerStateManager)
        {
            _fileAccessManager = fileAccessManager;
            _menuGripperService = menuGripperService;
            _playerStateManager = playerStateManager;
        }

        void IStartable.Start()
        {
            _fileAccessManager.LoadEndAsObservable
                .Subscribe(_ => _menuGripperService.Initialize())
                .AddTo(_disposables);
            _playerStateManager.MainMenuSwitchingAsObservable
                .Subscribe(_menuGripperService.OnMenuSwitching)
                .AddTo(_disposables);

            _menuGripperService.OnMenuSwitching(false);
        }

        void ILateTickable.LateTick()
        {
            _menuGripperService.OnLateTick();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
