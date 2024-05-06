using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Player
{
    public class PlayerInputPresenter : IStartable, ITickable, IDisposable
    {
        /// <summary>
        /// ロード完了まで操作不可
        /// </summary>
        bool _isTick = false;

        readonly FileAccessManager _fileAccessManager;
        readonly PlayerInputService _playerInputService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public PlayerInputPresenter(
            FileAccessManager fileAccessManager,
            PlayerInputService playerInputService)
        {
            _fileAccessManager = fileAccessManager;
            _playerInputService = playerInputService;
        }

        void IStartable.Start()
        {
            _fileAccessManager.LoadEndAsObservable
                .Subscribe(_ => _isTick = true)
                .AddTo(_disposables);
        }

        void ITickable.Tick()
        {
            if (!_isTick) return;
            _playerInputService.OnTick();
        }
        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}