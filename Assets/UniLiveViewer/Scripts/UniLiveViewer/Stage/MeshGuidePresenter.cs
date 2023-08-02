using System;
using UniLiveViewer;
using UniRx;
using VContainer;
using VContainer.Unity;

public class MeshGuidePresenter : IStartable, IDisposable
{
    readonly MeshGuide _meshGuide;
    readonly TimelineController _timelineController;

    readonly CompositeDisposable _disposables;

    [Inject]
    public MeshGuidePresenter(MeshGuide meshGuide,
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

        _meshGuide.Initialize(_timelineController);

        UnityEngine.Debug.Log("Trace: MeshGuidePresenter.Start");
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
