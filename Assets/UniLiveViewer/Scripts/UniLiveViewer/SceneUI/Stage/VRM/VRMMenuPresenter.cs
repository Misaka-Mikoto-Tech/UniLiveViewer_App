using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniRx;
using VContainer;
using VContainer.Unity;
using NanaCiel;

namespace UniLiveViewer.Menu
{
    public class VRMMenuPresenter : IAsyncStartable, IDisposable
    {
        /// <summary>
        /// サムネページ処理停止用
        /// </summary>
        CancellationTokenSource _cts;

        readonly ISubscriber<VRMMenuShowMessage> _menuShowSubscriber;
        readonly VRMMenuRootService _vrmMenuRootService;
        readonly ThumbnailService _thumbnailService;
        readonly CharacterPage _characterPage;
        readonly CompositeDisposable _disposables = new();

        [Inject]
        public VRMMenuPresenter(
            ISubscriber<VRMMenuShowMessage> menuShowSubscriber,
            VRMMenuRootService vrmMenuRootService,
            ThumbnailService thumbnailService,
            CharacterPage characterPage)
        {
            _menuShowSubscriber = menuShowSubscriber;
            _vrmMenuRootService = vrmMenuRootService;
            _thumbnailService = thumbnailService;
            _characterPage = characterPage;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _menuShowSubscriber
                .Subscribe(async x =>
                {
                    _cts?.Cancel();//ページ状態更新と見なす
                    var isEnable = x.PageIndex == -1 ? false : true;

                    _vrmMenuRootService.SetEnableRoot(isEnable);
                    if (!isEnable) return;
                    if (x.PageIndex == 0)
                    {
                        _cts = new CancellationTokenSource();
                        await _thumbnailService.BeginAsync(_cts.Token).IgnoreCancellationException();
                    }
                }).AddTo(_disposables);

            _thumbnailService.OnClickAsObservable
                .Subscribe(x =>
                {
                    _characterPage.OnClickThumbnail();
                }).AddTo(_disposables);
            await _thumbnailService.InitializeAsync(cancellation);
            _vrmMenuRootService.SetEnableRoot(false);
        }

        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }
}
