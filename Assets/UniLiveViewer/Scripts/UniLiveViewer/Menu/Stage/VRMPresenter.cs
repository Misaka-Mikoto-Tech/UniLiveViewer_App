using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class VRMPresenter : IAsyncStartable, IDisposable
    {
        readonly IVRMLoaderUI _vrmLoaderUI;
        readonly VRMSwitchController _switchController;
        readonly FileAccessManager _fileAccessManager;
        readonly ThumbnailController _thumbnailController;
        readonly Transform _thumbnailRoot;

        readonly CompositeDisposable _disposables;

        [Inject]
        public VRMPresenter(
            IVRMLoaderUI vrmLoaderUI,
            FileAccessManager fileAccessManager,
            VRMSwitchController switchController,
            ThumbnailController thumbnailController,
            Transform thumbnailRoot)
        {
            _vrmLoaderUI = vrmLoaderUI;
            _fileAccessManager = fileAccessManager;
            _switchController = switchController;
            _thumbnailController = thumbnailController;
            _thumbnailRoot = thumbnailRoot;

            _disposables = new CompositeDisposable();
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            Debug.Log("Trace: VRMPresenter.StartAsync");

            await _switchController.OnStartAsync(_vrmLoaderUI, _fileAccessManager, cancellation);

            _switchController.OnOpenPageAsObservable
                .Where(x => x == 0)
                .Subscribe(async _ =>
                {
                    _thumbnailRoot.gameObject.SetActive(true);
                    await _thumbnailController.SetThumbnail(cancellation);
                })
                .AddTo(_disposables);

            Debug.Log("Trace: VRMPresenter.StartAsync");
        }
        void IDisposable.Dispose()
        {
            _disposables.Dispose();
        }
    }

}