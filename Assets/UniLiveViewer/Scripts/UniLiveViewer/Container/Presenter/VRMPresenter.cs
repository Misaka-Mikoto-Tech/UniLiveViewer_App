using UniLiveViewer;
using VContainer;
using VContainer.Unity;

public class VRMPresenter : IStartable
{
    readonly IVRMLoaderUI _vrmLoaderUI;
    readonly VRMSwitchController _switchController;
    readonly TextureAssetManager _textureAssetManager;

    [Inject]
    public VRMPresenter(
        IVRMLoaderUI vrmLoaderUI,
        VRMSwitchController switchController,
        TextureAssetManager textureAssetManager)
    {
        _vrmLoaderUI = vrmLoaderUI;
        _switchController = switchController;
        _textureAssetManager = textureAssetManager;
    }

    void IStartable.Start()
    {
        _switchController.Initialize(_vrmLoaderUI);
        _textureAssetManager.Initialize(_vrmLoaderUI);
    }
}
