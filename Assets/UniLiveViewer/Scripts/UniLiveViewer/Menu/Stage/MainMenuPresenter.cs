using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.Player;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    /// <summary>
    /// 全体とactor以外用（未整理）
    /// </summary>
    public class MainMenuPresenter : IAsyncStartable, IDisposable
    {
        readonly PlayerStateManager _playerStateManager;
        readonly MeneRoot _meneRoot;
        readonly AudioPlaybackPage _audioPlaybackPage;
        readonly ItemPage _itemPage;
        readonly ConfigPage _configPage;
        readonly JumpList _jumpList;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public MainMenuPresenter(
            PlayerStateManager playerStateManager,
            MeneRoot meneRoot,
            AudioPlaybackPage audioPlaybackPage,
            ItemPage itemPage,
            ConfigPage configPage,
            JumpList jumpList)
        {
            _playerStateManager = playerStateManager;
            _meneRoot = meneRoot;
            _audioPlaybackPage = audioPlaybackPage;
            _itemPage = itemPage;
            _configPage = configPage;
            _jumpList = jumpList;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _playerStateManager.MainMenuSwitchingAsObservable
                .Subscribe(SwitchEnable)
                .AddTo(_disposables);

            _jumpList.OnSelectAsObservable
                .Subscribe(_audioPlaybackPage.OnJumpSelect).AddTo(_disposables);

            _audioPlaybackPage.StartAsync(cancellation).Forget();
            _itemPage.OnStart();
            _configPage.OnStart();

            await UniTask.CompletedTask;
        }

        void SwitchEnable(bool isEnable)
        {
            if (_meneRoot.gameObject.activeSelf != isEnable) _meneRoot.gameObject.SetActive(isEnable);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
