using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer;
using VContainer;
using VContainer.Unity;

public class VRMPresenter : IAsyncStartable
{
    readonly IVRMLoaderUI _vrmLoaderUI;
    readonly VRMSwitchController _switchController;
    readonly FileAccessManager _fileAccessManager;
    readonly TextureAssetManager _textureAssetManager;
    readonly ThumbnailController _thumbnailController;

    [Inject]
    public VRMPresenter(
        IVRMLoaderUI vrmLoaderUI,
        VRMSwitchController switchController,
        FileAccessManager fileAccessManager,
        TextureAssetManager textureAssetManager,
        ThumbnailController thumbnailController)
    {
        _vrmLoaderUI = vrmLoaderUI;
        _switchController = switchController;
        _fileAccessManager = fileAccessManager;
        _textureAssetManager = textureAssetManager;
        _thumbnailController = thumbnailController;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        UnityEngine.Debug.Log("Trace: VRMPresenter.StartAsync");

        await _switchController.InitializeAsync(_vrmLoaderUI, _fileAccessManager, _textureAssetManager, _thumbnailController, cancellation);

        UnityEngine.Debug.Log("Trace: VRMPresenter.StartAsync");
    }
}
