using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Kari;
using UniLiveViewer.Menu;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Gymnasium
{
    public class StageLightPresenter : IStartable, ITickable, IDisposable
    {
        readonly ConfigPage _configPage;
        readonly StageLightChangeService _changeService;
        readonly ISubscriber<SummonedCount> _subscriber;

        readonly CompositeDisposable _disposable;

        [Inject]
        public StageLightPresenter(
            ConfigPage configPage,
            ISubscriber<SummonedCount> subscriber,
            StageLightChangeService changeService)
        {
            _configPage = configPage;
            _subscriber = subscriber;
            _changeService = changeService;

            _disposable = new CompositeDisposable();
        }

        void IStartable.Start()
        {
            Debug.Log("Trace: StageLightPresenter.Start");

            _subscriber
                .Subscribe(x => _changeService.OnChangeSummonedCount(x.Value))
                .AddTo(_disposable);

            _configPage.StageLightIsWhiteAsObservable
                .Subscribe(_changeService.OnChangeLightColor)
                .AddTo(_disposable);
            _configPage.StageLightIndexAsObservable
                .Subscribe(_changeService.OnChangeStageLight)
                .AddTo(_disposable);

            Debug.Log("Trace: StageLightPresenter.Start");
        }

        void ITickable.Tick()
        {
            _changeService.OnTick();
        }

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }
}