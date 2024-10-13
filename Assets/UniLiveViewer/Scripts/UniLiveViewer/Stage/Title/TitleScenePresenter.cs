using Cysharp.Threading.Tasks;
using MessagePipe;
using System.Threading;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage.Title
{
    public class TitleScenePresenter : IAsyncStartable
    {
        float _timer;

        readonly AudioSource _mainAudioSource;
        readonly TitleSceneSettings _titleSceneSettings;
        readonly ISubscriber<SceneTransitionMessage> _sceneTransitionSubscriber;
        readonly CompositeDisposable _disposable = new();

        [Inject]
        public TitleScenePresenter(
            AudioSource mainAudioSource,
            TitleSceneSettings titleSceneSettings,
            ISubscriber<SceneTransitionMessage> sceneTransitionSubscriber)
        {
            _mainAudioSource = mainAudioSource;
            _titleSceneSettings = titleSceneSettings;
            _sceneTransitionSubscriber = sceneTransitionSubscriber;
        }

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            _titleSceneSettings.AppVersionText.text = "ver " + Application.version;

            _sceneTransitionSubscriber
                .Subscribe(x =>
                {
                    EndFadeAsync(cancellation).Forget();
                }).AddTo(_disposable);

            StartFadeAsync(cancellation).Forget();

            // 保留
            //_mainAudioSource.playOnAwake = false;
            //_mainAudioSource.loop = true;
            //await UniTask.Delay(1000);
            //_mainAudioSource.time = 3.5f;
            //_mainAudioSource.Play();

            await UniTask.CompletedTask;
        }

        async UniTask StartFadeAsync(CancellationToken cancellation)
        {
            _titleSceneSettings.SpriteRenderer.color = new Color(1, 1, 1, 0);
            _titleSceneSettings.AppVersionText.color = new Color(1, 1, 1, 0);
            await UniTask.Delay(11000, cancellationToken: cancellation);
            while (_timer < 1.0)
            {
                _timer += Time.deltaTime;
                _titleSceneSettings.SpriteRenderer.color = new Color(1, 1, 1, _timer);
                _titleSceneSettings.AppVersionText.color = new Color(1, 1, 1, _timer);
                await UniTask.Yield(cancellation);
            }
        }

        async UniTask EndFadeAsync(CancellationToken cancellation)
        {
            _timer = _mainAudioSource.volume;
            await UniTask.Delay(2000, cancellationToken: cancellation);
            while (0.0f < _timer)
            {
                _timer -= Time.deltaTime * 0.12f;
                _mainAudioSource.volume = _timer;
                await UniTask.Yield(cancellation);
            }
        }
    }
}