using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;
using VContainer;

public class SceneChangeService
{
    readonly OVRScreenFade _screenFade;

    [Inject]
    public SceneChangeService(OVRScreenFade screenFade)
    {
        _screenFade = screenFade;
    }

    public async UniTask Change(string nextSceneName, int bufferTime, CancellationToken token)
    {
        await InternalChange(nextSceneName, bufferTime, token);
    }

    async UniTask InternalChange(string nextSceneName, int bufferTime, CancellationToken token)
    {
        _screenFade.FadeOut();

        //完全非同期は無理
        var async = SceneManager.LoadSceneAsync(nextSceneName);
        async.allowSceneActivation = false;
        await UniTask.Delay(bufferTime, cancellationToken: token);
        async.allowSceneActivation = true;
    }
}
