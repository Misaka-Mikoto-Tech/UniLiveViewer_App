using System;
using UniLiveViewer;
using UniRx;
using VContainer;
using VContainer.Unity;

public class QuasiShadowPresenter : IStartable, IDisposable
{
    readonly QuasiShadow _quasiShadow;
    readonly TimelineController _timelineController;

    readonly CompositeDisposable _disposables;

    [Inject]
    public QuasiShadowPresenter(QuasiShadow quasiShadow,
        TimelineController timelineController)
    {
        _quasiShadow = quasiShadow;
        _timelineController = timelineController;

        _disposables = new CompositeDisposable();
    }

    void IStartable.Start()
    {
        UnityEngine.Debug.Log("Trace: QuasiShadowPresenter.Start");

        _timelineController.FieldCharacterCount
            .SkipLatestValueOnSubscribe()
            .Subscribe(_ => _quasiShadow.OnFieldCharacterCount())
            .AddTo(_disposables);

        _quasiShadow.Initialize( _timelineController);

        UnityEngine.Debug.Log("Trace: QuasiShadowPresenter.Start");
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
