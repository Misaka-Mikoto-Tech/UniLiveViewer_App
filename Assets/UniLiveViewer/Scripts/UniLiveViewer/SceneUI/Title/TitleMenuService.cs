using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Stage.Title;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.SceneUI.Title
{
    public class TitleMenuService
    {
        float _timer;

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

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            _titleMenuSettings.UiRoot.gameObject.SetActive(true);
            OpenMainMenu();
            var group = _titleMenuSettings.MainMenuCanvas.GetComponent<CanvasGroup>();
            SetCanvasGroup(group, true);
            await UniTask.Delay(12000, cancellationToken: cancellation);
            await FadeAsync(group, cancellation);
        }

        async UniTask FadeAsync(CanvasGroup canvasGroup, CancellationToken cancellation)
        {
            // alphaも同時に1だと見た目がおかしくなる
            SetCanvasGroup(canvasGroup, false);
            canvasGroup.alpha = 0;

            while (_timer < 1.0)
            {
                _timer += Time.deltaTime;
                canvasGroup.alpha = _timer;
                await UniTask.Yield(cancellation);
            }
        }

        void SetCanvasGroup(CanvasGroup canvasGroup, bool isTransparent)
        {
            if (isTransparent)
            {
                canvasGroup.alpha = 0;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            else
            {
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public async UniTask LoadScenesAutoAsync(CancellationToken cancellation)
        {
            _titleMenuSettings.UiRoot.gameObject.SetActive(false);
            await UniTask.Delay(2500, cancellationToken: cancellation);//Animation分

            _titleSceneSettings.OvrScreenFade.FadeOut();//デフォ2秒設定
            await UniTask.Delay(2000, cancellationToken: cancellation);

            await _sceneChangeService.ChangePreviousScene(cancellation);
        }

        public void OpenMainMenu()
        {
            _titleMenuSettings.MainMenuCanvas.gameObject.SetActive(true);
            _titleMenuSettings.CustomLiveCanvas.gameObject.SetActive(false);
            _titleMenuSettings.LicenseCanvas.gameObject.SetActive(false);
        }

        public void OpenCustomLive()
        {
            _titleMenuSettings.MainMenuCanvas.gameObject.SetActive(false);
            _titleMenuSettings.CustomLiveCanvas.gameObject.SetActive(true);
            _titleMenuSettings.LicenseCanvas.gameObject.SetActive(false);
        }

        public void OpenLicense()
        {
            _titleMenuSettings.MainMenuCanvas.gameObject.SetActive(false);
            _titleMenuSettings.CustomLiveCanvas.gameObject.SetActive(false);
            _titleMenuSettings.LicenseCanvas.gameObject.SetActive(true);
        }

        public async UniTask QuitAppAsync(CancellationToken cancellation)
        {
            _titleMenuSettings.UiRoot.gameObject.SetActive(false);

            _titleSceneSettings.OvrScreenFade.FadeOut();//デフォ2秒設定
            await UniTask.Delay(3000, cancellationToken: cancellation);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}