using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

public class TimelinePresenter : IAsyncStartable
{
    readonly PlayableDirector _playableDirector;
    readonly AudioAssetManager _audioAssetManager;
    readonly TimelineController _timelineController;

    [Inject]
    public TimelinePresenter(PlayableDirector playableDirector,
        AudioAssetManager audioAssetManager,
        TimelineController timelineController)
    {
        _playableDirector = playableDirector;
        _audioAssetManager = audioAssetManager;
        _timelineController = timelineController;
    }

    public async UniTask StartAsync(CancellationToken cancellation)
    {
        Debug.Log("Trace: TimelinePresenter.Start");

        _timelineController.OnStart(_playableDirector, _audioAssetManager);

        OVRManager.InputFocusLost += async () => await HomePause(cancellation);
        OVRManager.InputFocusAcquired += HomeReStart;
        OVRManager.HMDUnmounted += async () => await HomePause(cancellation);//HMDが外された
        OVRManager.HMDMounted += HomeReStart;//HMDが付けられた

        Debug.Log("Trace: TimelinePresenter.Start");

        await UniTask.CompletedTask;
    }

    async UniTask HomePause(CancellationToken cancellation)
    {
        await _timelineController.TimelineManualMode();
        Time.timeScale = 0;
    }

    void HomeReStart()
    {
        Time.timeScale = 1;
    }
}
