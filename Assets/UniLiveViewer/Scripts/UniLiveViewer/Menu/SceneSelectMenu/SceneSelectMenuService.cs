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
        readonly AudioSourceService _audioSourceService;
        readonly MeneRoot _meneRoot;

        [Inject]
        public SceneSelectMenuService(
            SceneChangeService sceneChangeService,
            PlayableMusicService playableMusicService,
            AudioSourceService audioSourceService,
            MeneRoot meneRoot)
        {
            _sceneChangeService = sceneChangeService;
            _playableMusicService = playableMusicService;
            _audioSourceService = audioSourceService;
            _meneRoot = meneRoot;
        }

        public async UniTask OnChangeSceneAsync(SceneType sceneType)
        {
            _audioSourceService.PlayOneShot(0);

            var dummy = new CancellationToken();
            await _playableMusicService.ManualModeAsync(dummy);// 音が割れるので止める
            await UniTask.Delay(100, cancellationToken: dummy);
            await BlackoutCurtain.instance.FadeoutAsync(dummy);

            _meneRoot.gameObject.SetActive(false);//UIが透けて見えるので隠す

            await _sceneChangeService.ChangeAsync(sceneType, dummy);
        }
    }
}