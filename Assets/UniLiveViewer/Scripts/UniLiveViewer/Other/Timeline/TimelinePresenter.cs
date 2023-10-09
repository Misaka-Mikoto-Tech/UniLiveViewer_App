using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading;
using UniLiveViewer.Kari;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Timeline
{
    public class TimelinePresenter : IAsyncStartable, IDisposable
    {
        readonly IPublisher<SummonedCount> _publisher;
        readonly PlayableDirector _playableDirector;
        readonly AudioAssetManager _audioAssetManager;
        readonly TimelineController _timelineController;

        readonly CompositeDisposable _disposable;

        [Inject]
        public TimelinePresenter(
            IPublisher<SummonedCount> publisher,
            PlayableDirector playableDirector,
            AudioAssetManager audioAssetManager,
            TimelineController timelineController)
        {
            _publisher = publisher;
            _playableDirector = playableDirector;
            _audioAssetManager = audioAssetManager;
            _timelineController = timelineController;

            _disposable = new CompositeDisposable();
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            Debug.Log("Trace: TimelinePresenter.Start");

            _timelineController.OnStart(_playableDirector, _audioAssetManager);

            OVRManager.InputFocusLost += async () => await HomePause(cancellation);
            OVRManager.InputFocusAcquired += HomeReStart;
            OVRManager.HMDUnmounted += async () => await HomePause(cancellation);//HMDが外された
            OVRManager.HMDMounted += HomeReStart;//HMDが付けられた

            _timelineController.FieldCharacterCount
                .Subscribe(x => _publisher.Publish(new SummonedCount(x)))
                .AddTo(_disposable);

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

        void IDisposable.Dispose()
        {
            _disposable.Dispose();
        }
    }

}