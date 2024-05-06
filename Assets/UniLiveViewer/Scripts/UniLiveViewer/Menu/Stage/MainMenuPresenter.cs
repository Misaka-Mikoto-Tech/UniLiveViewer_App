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
        bool _isRootActive = true;

        readonly PlayerInputService _playerInputService;
        readonly MeneRoot _meneRoot;
        readonly AudioPlaybackPage _audioPlaybackPage;
        readonly ItemPage _itemPage;
        readonly ConfigPage _configPage;
        readonly JumpList _jumpList;
        readonly AudioSourceService _audioSourceService;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public MainMenuPresenter(
            PlayerInputService playerInputService,
            MeneRoot meneRoot,
            AudioPlaybackPage audioPlaybackPage,
            ItemPage itemPage,
            ConfigPage configPage,
            JumpList jumpList,
            AudioSourceService audioSourceService)
        {
            _playerInputService = playerInputService;
            _meneRoot = meneRoot;
            _audioPlaybackPage = audioPlaybackPage;
            _itemPage = itemPage;
            _configPage = configPage;
            _jumpList = jumpList;
            _audioSourceService = audioSourceService;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _playerInputService.ClickMenuAsObservable()
                .Where(x => x == PlayerHandType.RHand)
                .Subscribe(_ => SwitchEnable()).AddTo(_disposables);

            _jumpList.OnSelectAsObservable
                .Subscribe(_audioPlaybackPage.OnJumpSelect).AddTo(_disposables);

            _audioPlaybackPage.StartAsync(cancellation).Forget();
            _itemPage.OnStart();
            _configPage.OnStart();

            await UniTask.CompletedTask;
        }

        void SwitchEnable()
        {
            _isRootActive = !_isRootActive;
            _meneRoot.gameObject.SetActive(_isRootActive);

            if (_isRootActive) _audioSourceService.PlayOneShot(2);//表示音
            else _audioSourceService.PlayOneShot(3);//非表示音
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
