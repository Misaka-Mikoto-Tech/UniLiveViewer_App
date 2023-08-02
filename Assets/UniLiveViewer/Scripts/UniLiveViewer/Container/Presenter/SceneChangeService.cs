using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UniLiveViewer;
using UnityEngine.SceneManagement;
using VContainer;

public class SceneChangeService
{
    public IReadOnlyDictionary<SceneMode, string> MAP => _map;
    Dictionary<SceneMode, string> _map;

    readonly OVRScreenFade _screenFade;
    const int DEFAULT_TIME = 2000;

    [Inject]
    public SceneChangeService(OVRScreenFade screenFade)
    {
        _screenFade = screenFade;

        _map = new Dictionary<SceneMode, string>()
        {
            { SceneMode.CANDY_LIVE ,"LiveScene" },
            { SceneMode.KAGURA_LIVE ,"KAGURAScene" },
            { SceneMode.VIEWER ,"ViewerScene" },
            { SceneMode.GYMNASIUM ,"GymnasiumScene" },
        };
    }

    public async UniTask Change(string nextSceneName, int bufferTime, CancellationToken token)
    {
        await InternalChange(nextSceneName, bufferTime, token);
    }

    public async UniTask Change(SceneMode nextScene, int bufferTime, CancellationToken token)
    {
        var nextSceneName = _map[nextScene];
        await InternalChange(nextSceneName, bufferTime, token);
    }

    async UniTask InternalChange(string nextSceneName, int bufferTime,CancellationToken token)
    {
        _screenFade.FadeOut();

        //完全非同期は無理
        var async = SceneManager.LoadSceneAsync(nextSceneName);
        async.allowSceneActivation = false;
        await UniTask.Delay(bufferTime, cancellationToken: token);
        async.allowSceneActivation = true;
    }
}
