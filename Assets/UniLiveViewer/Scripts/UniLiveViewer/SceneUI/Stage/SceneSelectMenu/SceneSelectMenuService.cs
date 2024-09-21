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
        readonly MenuRootService _menuRootService;
        readonly SceneChangeService _sceneChangeService;
        readonly PlayableMusicService _playableMusicService;
        readonly RootAudioSourceService _rootAudioSourceService;

        [Inject]
        public SceneSelectMenuService(
            MenuRootService menuRootService,
            SceneChangeService sceneChangeService,
            PlayableMusicService playableMusicService,
            RootAudioSourceService rootAudioSourceService)
        {
            _menuRootService = menuRootService;
            _sceneChangeService = sceneChangeService;
            _playableMusicService = playableMusicService;
            _rootAudioSourceService = rootAudioSourceService;
        }

        public async UniTask OnChangeSceneAsync(SceneType sceneType)
        {
            _rootAudioSourceService.PlayOneShot(AudioSE.ButtonClick);

            var dummy = new CancellationToken();
            await _playableMusicService.ManualModeAsync(dummy);// 音が割れるので止める

            await UniTask.Delay(100, cancellationToken: dummy);
            _menuRootService.OnMenuSwitching();//開いてる想定なので閉じる

            _rootAudioSourceService.PlayOneShot(AudioSE.SceneTransition);
            await BlackoutCurtain.instance.FadeoutAsync(dummy);
            await _sceneChangeService.ChangeAsync(sceneType, dummy);
        }
    }
}