using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer;
using UniLiveViewer.SceneLoader;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// シーン遷移・ファイル準備のみ
/// </summary>
public class TitleScenePresenter : IAsyncStartable
{
    readonly SceneChangeService _sceneChangeService;
    readonly FileAccessManager _fileAccessManager;
    readonly OVRScreenFade _ovrScreenFade;

    [Inject]
    public TitleScenePresenter(
        SceneChangeService sceneChangeService,
        FileAccessManager fileAccessManager,
        OVRScreenFade ovrScreenFade)
    {
        _sceneChangeService = sceneChangeService;
        _fileAccessManager = fileAccessManager;
        _ovrScreenFade = ovrScreenFade;
    }

    async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
    {
        _sceneChangeService.Setup(_ovrScreenFade);
        _sceneChangeService.SetupFirstScene();
        // TODO: この段階でフォルダ必要？（stage入ってからNGは面倒なのはある）
        //await _fileAccessManager.OnStartAsync(null, null, cancellation);
    }
}
