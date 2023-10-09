using System;
using UniLiveViewer.Timeline;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    public class QuasiShadowPresenter : IStartable, IDisposable
    {
        readonly QuasiShadowService _quasiShadow;
        readonly TimelineController _timelineController;

        readonly CompositeDisposable _disposables;

        [Inject]
        public QuasiShadowPresenter(
            QuasiShadowService quasiShadow,
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

            _quasiShadow.OnStart();

            UnityEngine.Debug.Log("Trace: QuasiShadowPresenter.Start");
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }

}