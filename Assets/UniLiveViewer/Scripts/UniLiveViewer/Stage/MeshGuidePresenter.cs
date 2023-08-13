using System;
using UniLiveViewer;
using UniRx;
using VContainer;
using VContainer.Unity;

public class MeshGuidePresenter : IStartable, ITickable , IDisposable
{
    readonly MeshGuideService _meshGuide;
    readonly TimelineController _timelineController;

    readonly CompositeDisposable _disposables;

    [Inject]
    public MeshGuidePresenter(MeshGuideService meshGuide,
        TimelineController timelineController)
    {
        _meshGuide = meshGuide;
        _timelineController = timelineController;

        _disposables = new CompositeDisposable();
    }

    void IStartable.Start()
    {
        UnityEngine.Debug.Log("Trace: MeshGuidePresenter.Start");

        _timelineController.FieldCharacterCount
            .SkipLatestValueOnSubscribe()
            .Subscribe(_ => _meshGuide.OnFieldCharacterCount())
            .AddTo(_disposables);

        _meshGuide.OnStart(_timelineController);

        UnityEngine.Debug.Log("Trace: MeshGuidePresenter.Start");
    }

    void ITickable.Tick()
    {
        _meshGuide.OnTick();
    }

    void IDisposable.Dispose()
    {
        _disposables.Dispose();
    }
}
