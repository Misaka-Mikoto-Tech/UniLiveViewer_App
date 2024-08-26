using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Stage;
using UniLiveViewer.Timeline;
using VContainer;

namespace UniLiveViewer.Menu.SceneSelect
{
    public class SceneSelectMenuService
    {
        readonly SceneChangeService _sceneChangeService;
        readonly PlayableMusicService _playableMusicService;
        readonly RootAudioSourceService _rootAudioSourceService;
        readonly MeneRoot _meneRoot;

        [Inject]
        public SceneSelectMenuService(
            SceneChangeService sceneChangeService,
            PlayableMusicService playableMusicService,
            RootAudioSourceService rootAudioSourceService,
            MeneRoot meneRoot)
        {
            _sceneChangeService = sceneChangeService;
            _playableMusicService = playableMusicService;
            _rootAudioSourceService = rootAudioSourceService;
            _meneRoot = meneRoot;
        }

        public async UniTask OnChangeSceneAsync(SceneType sceneType)
        {
            _rootAudioSourceService.PlayOneShot(AudioSE.ButtonClick);

            var dummy = new CancellationToken();
            await _playableMusicService.ManualModeAsync(dummy);// 音が割れるので止める
            await UniTask.Delay(100, cancellationToken: dummy);
            _rootAudioSourceService.PlayOneShot(AudioSE.SceneTransition);
            await BlackoutCurtain.instance.FadeoutAsync(dummy);

            _meneRoot.gameObject.SetActive(false);//UIが透けて見えるので隠す

            await _sceneChangeService.ChangeAsync(sceneType, dummy);
        }
    }
}