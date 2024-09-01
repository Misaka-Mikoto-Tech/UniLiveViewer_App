using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Stage.Title;
using VContainer;

namespace UniLiveViewer.SceneUI.Title
{
    public class TitleMenuService
    {
        readonly SceneChangeService _sceneChangeService;
        readonly TitleSceneSettings _titleSceneSettings;
        readonly TitleMenuSettings _titleMenuSettings;

        [Inject]
        public TitleMenuService(
            SceneChangeService sceneChangeService,
            TitleSceneSettings titleSceneSettings,
            TitleMenuSettings titleMenuSettings)
        {
            _sceneChangeService = sceneChangeService;
            _titleSceneSettings = titleSceneSettings;
            _titleMenuSettings = titleMenuSettings;
        }

        public void Start()
        {
            _titleMenuSettings.MainMenuCanvas.gameObject.SetActive(true);
            _titleMenuSettings.LicenseCanvas.gameObject.SetActive(false);
            _titleMenuSettings.UiRoot.gameObject.SetActive(true);
        }

        public async UniTask LoadScenesAutoAsync(CancellationToken cancellation)
        {
            _titleMenuSettings.UiRoot.gameObject.SetActive(false);
            _titleSceneSettings.OvrScreenFade.FadeOut();//デフォ2秒設定
            //TODO: 音フェードアウト
            await UniTask.Delay(2000, cancellationToken: cancellation);
            await _sceneChangeService.ChangePreviousScene(cancellation);
        }

        public void OpenLicense(bool isOpen)
        {
            _titleMenuSettings.MainMenuCanvas.gameObject.SetActive(!isOpen);
            _titleMenuSettings.LicenseCanvas.gameObject.SetActive(isOpen);
        }

        public async UniTask QuitAppAsync(CancellationToken cancellation)
        {
            _titleMenuSettings.UiRoot.gameObject.SetActive(false);
            _titleSceneSettings.OvrScreenFade.FadeOut();//デフォ2秒設定
            //TODO: 音フェードアウト
            await UniTask.Delay(2000, cancellationToken: cancellation);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}