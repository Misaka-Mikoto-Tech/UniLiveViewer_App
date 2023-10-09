using Cysharp.Threading.Tasks;
using System;
using UniLiveViewer.Player;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class MenuGripperPresenter : IStartable, IDisposable
    {
        readonly FileAccessManager _fileAccessManager;
        readonly MenuGripperService _menuGripperService;
        readonly PlayerStateManager _playerStateManager;
        readonly CompositeDisposable _disposables;

        [Inject]
        public MenuGripperPresenter(
            FileAccessManager fileAccessManager,
            MenuGripperService menuGripperService,
            PlayerStateManager playerStateManager)
        {
            _fileAccessManager = fileAccessManager;
            _menuGripperService = menuGripperService;
            _playerStateManager = playerStateManager;

            _disposables = new CompositeDisposable();
        }

        void IStartable.Start()
        {
            _fileAccessManager.LoadEndAsObservable
                .Subscribe(_ => _menuGripperService.Initialize())
                .AddTo(_disposables);
            _playerStateManager.MainUISwitchingAsObservable
                .Subscribe(_menuGripperService.OnSwitchEnable)
                .AddTo(_disposables);

            _menuGripperService.OnStart();
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
