using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using UniLiveViewer.Timeline;
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
        readonly PlayableBinderService _playableBinderService;

        readonly CompositeDisposable _disposable = new();

        [Inject]
        public StageLightPresenter(
            ConfigPage configPage,
            StageLightChangeService changeService,
            PlayableBinderService playableBinderService)
        {
            _configPage = configPage;
            _playableBinderService = playableBinderService;
            _changeService = changeService;
        }

        void IStartable.Start()
        {
            _playableBinderService.StageActorCount
                .Subscribe(_changeService.OnChangeSummonedCount)
                .AddTo(_disposable);

            _configPage.StageLightIsWhiteAsObservable
                .Subscribe(_changeService.OnChangeLightColor)
                .AddTo(_disposable);
            _configPage.StageLightIndexAsObservable
                .Subscribe(_changeService.OnChangeStageLight)
                .AddTo(_disposable);
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