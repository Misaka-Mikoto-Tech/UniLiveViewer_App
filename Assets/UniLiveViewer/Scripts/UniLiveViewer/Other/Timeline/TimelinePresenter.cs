using UniLiveViewer;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

public class TimelinePresenter : IStartable
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

    void IStartable.Start()
    {
        UnityEngine.Debug.Log("Trace: TimelinePresenter.Start");

        _timelineController.OnStart(_playableDirector, _audioAssetManager);

        UnityEngine.Debug.Log("Trace: TimelinePresenter.Start");
    }
}
