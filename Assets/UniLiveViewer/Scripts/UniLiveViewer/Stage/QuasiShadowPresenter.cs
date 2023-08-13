using System;
using UniLiveViewer;
using UniRx;
using VContainer;
using VContainer.Unity;

public class QuasiShadowPresenter : IStartable, IDisposable
{
    readonly QuasiShadowSetting _setting;
    readonly QuasiShadowService _quasiShadow;
    readonly TimelineController _timelineController;

    readonly CompositeDisposable _disposables;

    [Inject]
    public QuasiShadowPresenter(
        QuasiShadowSetting quasiShadowSetting,
        QuasiShadowService quasiShadow,
        TimelineController timelineController)
    {
        _setting = quasiShadowSetting;
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

        _quasiShadow.OnStart(_timelineController, _setting);

        UnityEngine.Debug.Log("Trace: QuasiShadowPresenter.Start");
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
