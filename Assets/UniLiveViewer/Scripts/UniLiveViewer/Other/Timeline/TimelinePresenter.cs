using UniLiveViewer;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

public class TimelinePresenter : IStartable
{
    readonly PlayableDirector _playableDirector;
    readonly TimelineController _timelineController;
    readonly AudioAssetManager _audioAssetManager;

    [Inject]
    public TimelinePresenter(PlayableDirector playableDirector,
        TimelineController timelineController,
        AudioAssetManager audioAssetManager)
    {
        _playableDirector = playableDirector;
        _timelineController = timelineController;
        _audioAssetManager = audioAssetManager;
    }

    void IStartable.Start()
    {
        UnityEngine.Debug.Log("Trace: TimelinePresenter.Start");

        _timelineController.Initialize(_playableDirector, _audioAssetManager);

        UnityEngine.Debug.Log("Trace: TimelinePresenter.Start");
    }
}
