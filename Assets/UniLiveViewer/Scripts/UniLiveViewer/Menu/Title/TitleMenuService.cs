using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.SceneLoader;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    public class TitleMenuService : MonoBehaviour
    {
        [SerializeField] Transform _uiRoot;
        [SerializeField] AudioSourceService _audioSourceService;

        SceneChangeService _sceneChangeService;
        OVRScreenFade _ovrScreenFade;
        CancellationToken _cancellationToken;

        void Awake()
        {
            _cancellationToken = this.GetCancellationTokenOnDestroy();
        }

        [Inject]
        public void Construct(SceneChangeService sceneChangeService, OVRScreenFade ovrScreenFade)
        {
            _sceneChangeService = sceneChangeService;
            _ovrScreenFade = ovrScreenFade;
        }

        void Start()
        {
            _uiRoot.gameObject.SetActive(false);
            LoadScenesAutoAsync(_cancellationToken).Forget();
        }

        async UniTask LoadScenesAutoAsync(CancellationToken cancellation)
        {
            _ovrScreenFade.FadeOut();
            await _sceneChangeService.ChangePreviousScene(cancellation);
        }
    }
}