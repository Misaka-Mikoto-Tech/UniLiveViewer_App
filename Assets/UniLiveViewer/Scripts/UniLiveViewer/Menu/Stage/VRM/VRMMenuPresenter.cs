using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.Timeline;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class VRMMenuPresenter : IAsyncStartable, IDisposable
    {
        readonly ISubscriber<VRMMenuShowMessage> _menuShowsubScriber;
        readonly MenuRootService _menuRootService;
        readonly ThumbnailService _thumbnailService;
        readonly CharacterPage _characterPage;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public VRMMenuPresenter(
            ISubscriber<VRMMenuShowMessage> menuShowsubScriber,
            MenuRootService menuRootService,
            ThumbnailService thumbnailService,
            CharacterPage characterPage)
        {
            _menuShowsubScriber = menuShowsubScriber;
            _menuRootService = menuRootService;
            _thumbnailService = thumbnailService;
            _characterPage = characterPage;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _menuShowsubScriber
                .Subscribe(x =>
                {
                    var isEnable = x.PageIndex == -1 ? false : true;
                    _menuRootService.SetEnableRoot(isEnable);
                    if (!isEnable) return;
                    if (x.PageIndex == 0) _thumbnailService.BeginAsync(cancellation).Forget();
                }).AddTo(_disposables);

            _thumbnailService.OnClickAsObservable
                .Subscribe(x =>
                {
                    _characterPage.OnClickThumbnail();
                }).AddTo(_disposables);
            await _thumbnailService.InitializeAsync(cancellation);
            _menuRootService.SetEnableRoot(false);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
