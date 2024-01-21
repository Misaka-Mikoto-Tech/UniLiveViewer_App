using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Timeline
{
    /// <summary>
    /// TODO: もやはTimelineではない
    /// </summary>
    public class TimelinePresenter : IAsyncStartable, IDisposable
    {
        readonly PlayableMusicService _playableMusicService;
        readonly CompositeDisposable _disposable = new();

        [Inject]
        public TimelinePresenter(
            PlayableMusicService playableMusicService)
        {
            _playableMusicService = playableMusicService;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            await _playableMusicService.OnStartAsync(cancellation);

            OVRManager.InputFocusLost += async () => await HomePause(cancellation);
            OVRManager.InputFocusAcquired += HomeReStart;
            OVRManager.HMDUnmounted += async () => await HomePause(cancellation);//HMDが外された
            OVRManager.HMDMounted += HomeReStart;//HMDが付けられた
        }

        async UniTask HomePause(CancellationToken cancellation)
        {
            await _playableMusicService.ManualModeAsync(cancellation);
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