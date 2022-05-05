using Cysharp.Threading.Tasks;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniLiveViewer
{
    public class OverrideUIController : MonoBehaviour
    {
        public static OverrideUIController instance;

        [SerializeField] private PlayerStateManager player;
        [SerializeField] private FileAccessManager fileManager;
        private BackGroundController backGroundCon;

        [Header("UI")]
        [SerializeField] private LoadAnimation anime_Loading;
        [SerializeField] private Camera overlayCamera;
        [SerializeField] private Renderer _rendererFade;
        [SerializeField] private Renderer _rendererClosing;
        [SerializeField] private TextMesh[] vmdError = new TextMesh[2];

        private MaterialPropertyBlock materialPropertyBlock;
        private Color baseColor;
        
        private CancellationToken cancellation_Token;

        private void Awake()
        {
            GlobalConfig.CheckNowScene();

            cancellation_Token = this.GetCancellationTokenOnDestroy();

            if (SystemInfo.sceneMode == SceneMode.VIEWER)
            {
                backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();
            }

            _rendererClosing.sharedMaterial.SetFloat("_Scala", 0);
            _rendererClosing.enabled = false;

            materialPropertyBlock = new MaterialPropertyBlock();
            _rendererFade.GetPropertyBlock(materialPropertyBlock);

            baseColor = _rendererFade.material.GetColor("_BaseColor");

            //暗転する
            baseColor.a = 1;//不透明に
            materialPropertyBlock.SetColor("_BaseColor", baseColor);
            _rendererFade.SetPropertyBlock(materialPropertyBlock);

            fileManager.onLoadStart += () => { anime_Loading.gameObject.SetActive(true); };
            fileManager.onVMDLoadError += VMDLoadError;
            fileManager.onThumbnailCompleted += () => { FinishLoading().Forget(); };

            foreach (var e in vmdError)
            {
                if (e.gameObject.activeSelf) e.gameObject.SetActive(false);
            }

            instance = this;
        }

        private void Start()
        {
            
        }

        public static void LoadNextScene(string sceneName)
        {
            instance.LoadScene(sceneName).Forget();
        }

        private void VMDLoadError()
        {
            Debug.Log("読み込み失敗発生==" + SystemInfo.userProfile.data.LanguageCode);

            if (anime_Loading.gameObject.activeSelf) anime_Loading.gameObject.SetActive(false);

            if (SystemInfo.userProfile.data.LanguageCode == (int)USE_LANGUAGE.JP)
            {
                vmdError[0].gameObject.SetActive(true);
            }
            else if(SystemInfo.userProfile.data.LanguageCode == (int)USE_LANGUAGE.EN)
            {
                vmdError[1].gameObject.SetActive(true);
            }
        }

        private async  UniTaskVoid FinishLoading()
        {
            await UniTask.Yield(cancellation_Token);

            //まずloadingアニメーションを消す
            anime_Loading.gameObject.SetActive(false);
            await UniTask.Yield(cancellation_Token);

            //暗転する
            baseColor = materialPropertyBlock.GetColor("_BaseColor");
            baseColor.a = 1;//不透明に

            while (baseColor.a >= 0.0f)
            {
                baseColor.a -= Time.deltaTime;

                materialPropertyBlock.SetColor("_BaseColor", baseColor);
                _rendererFade.SetPropertyBlock(materialPropertyBlock);
                await UniTask.Yield(cancellation_Token);
            }

            overlayCamera.enabled = false;
            player.enabled = true;//操作可能に
        }

        private async UniTask LoadScene(string sceneName)
        {
            overlayCamera.enabled = true;
            player.enabled = false;//操作不可に
            await UniTask.Yield(cancellation_Token);

            //閉幕演出
            _rendererClosing.enabled = true;
            float t = 0;

            for (int i = 0; i < 50; i++)
            {
                t += Time.deltaTime * (50 - i) * 0.1f;
                _rendererClosing.sharedMaterial.SetFloat("_Scala", t);
                await UniTask.Yield(cancellation_Token);
            }

            while (t < 150)
            {
                t += Time.deltaTime * 150;
                _rendererClosing.sharedMaterial.SetFloat("_Scala", t);
                await UniTask.Yield(cancellation_Token);
            }

            //skyboxの初期化
            if (backGroundCon) backGroundCon.SetInit();

            anime_Loading.gameObject.SetActive(true);

            //ロードが早すぎても最低演出分は待機する
            var async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;
            await UniTask.Delay(1000,cancellationToken: cancellation_Token);
            async.allowSceneActivation = true;
        }
    }

}