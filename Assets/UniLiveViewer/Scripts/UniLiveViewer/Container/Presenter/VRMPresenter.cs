using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniLiveViewer;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class VRMPresenter : IAsyncStartable, IDisposable
{
    readonly IVRMLoaderUI _vrmLoaderUI;
    readonly VRMSwitchController _switchController;
    readonly FileAccessManager _fileAccessManager;
    readonly TextureAssetManager _textureAssetManager;
    readonly ThumbnailController _thumbnailController;
    readonly Transform _thumbnailRoot;

    readonly CompositeDisposable _disposables;

    [Inject]
    public VRMPresenter(
        IVRMLoaderUI vrmLoaderUI,
        VRMSwitchController switchController,
        FileAccessManager fileAccessManager,
        TextureAssetManager textureAssetManager,
        ThumbnailController thumbnailController,
        Transform thumbnailRoot)
    {
        _vrmLoaderUI = vrmLoaderUI;
        _switchController = switchController;
        _fileAccessManager = fileAccessManager;
        _textureAssetManager = textureAssetManager;
        _thumbnailController = thumbnailController;
        _thumbnailRoot = thumbnailRoot;

        _disposables = new CompositeDisposable();
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        Debug.Log("Trace: VRMPresenter.StartAsync");

        await _switchController.InitializeAsync(_vrmLoaderUI, _fileAccessManager, cancellation);

        _switchController.OnOpenPageAsObservable
            .Where(x => x == 0)
            .Subscribe(async _ => {
                _thumbnailRoot.gameObject.SetActive(true);
                await _thumbnailController.SetThumbnail(_textureAssetManager.VrmNames, cancellation);
            })
            .AddTo(_disposables);

        Debug.Log("Trace: VRMPresenter.StartAsync");
    }
    void IDisposable.Dispose()
    {
        _disposables.Dispose();
    }
}
