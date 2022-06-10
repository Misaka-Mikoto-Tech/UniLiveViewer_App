using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniLiveViewer
{
    public class BlackoutCurtain : MonoBehaviour
    {
        [SerializeField] private LoadAnimation loadAnimation;
        [SerializeField] private Renderer renderer_Cutoff;
        [SerializeField] private Renderer renderer_Brack;
        [SerializeField] private TextMesh[] vmdError = new TextMesh[2];

        [SerializeField] private AnimationCurve curve;
        public static BlackoutCurtain instance;

        private PlayerStateManager player;
        private FileAccessManager fileManager;
        private MaterialPropertyBlock materialPropertyBlock;
        private Color color;
        private CancellationToken cancellation_Token;

        private void Awake()
        {
            cancellation_Token = this.GetCancellationTokenOnDestroy();

            //不透明黒
            color = new Color(0,0,0,1);
            materialPropertyBlock = new MaterialPropertyBlock();
            renderer_Brack.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetColor("_BaseColor", color);
            renderer_Brack.SetPropertyBlock(materialPropertyBlock);

            renderer_Brack.enabled = true;
            renderer_Cutoff.enabled = false;
            if (loadAnimation.gameObject.activeSelf) loadAnimation.gameObject.SetActive(false);

            foreach (var e in vmdError)
            {
                if (e.gameObject.activeSelf) e.gameObject.SetActive(false);
            }

            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            player = PlayerStateManager.instance;
            transform.parent = player.myCamera.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            fileManager = GameObject.FindGameObjectWithTag("AppConfig").gameObject.GetComponent<FileAccessManager>();

            fileManager.onLoadStart += () => {
                loadAnimation.gameObject.SetActive(true);
            };
            fileManager.onVMDLoadError += VMDLoadError;
            fileManager.onLoadSuccess += () => {
                FinishLoading().Forget();
            };
        }

        /// <summary>
        /// 暗転しシーンを遷移する
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public async UniTask StartBlackout(string sceneName)
        {
            player.enabled = false;//操作不可に
            renderer_Brack.enabled = false;
            renderer_Cutoff.enabled = true;
            if (loadAnimation.gameObject.activeSelf) loadAnimation.gameObject.SetActive(false);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation_Token);

            //閉幕演出
            float t = 0;
            while (t < 2.5f)
            {
                renderer_Cutoff.sharedMaterial.SetFloat("_Scala", curve.Evaluate(t));
                t += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation_Token);
            }
            renderer_Brack.enabled = true;
            color.a = 1;//不透明
            materialPropertyBlock.SetColor("_BaseColor", color);
            renderer_Brack.SetPropertyBlock(materialPropertyBlock);
            
            renderer_Cutoff.sharedMaterial.SetFloat("_Scala", 0);
            renderer_Cutoff.enabled = false;

            //ローディングアニメーション
            loadAnimation.gameObject.SetActive(true);
            //ロードが早すぎても最低演出分は待機する
            var async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;
            await UniTask.Delay(1000, cancellationToken: cancellation_Token);
            SystemInfo.userProfile.LastSceneName = sceneName;
            FileAccessManager.WriteJson(SystemInfo.userProfile);
            async.allowSceneActivation = true;
        }


        private async UniTaskVoid FinishLoading()
        {
            await UniTask.Delay(300, cancellationToken: cancellation_Token);

            //まずloadingアニメーションを消す
            loadAnimation.gameObject.SetActive(false);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation_Token);

            //暗転から徐々に再開
            color = materialPropertyBlock.GetColor("_BaseColor");
            color.a = 1;//不透明

            while (color.a >= 0.0f)
            {
                color.a -= Time.deltaTime;

                materialPropertyBlock.SetColor("_BaseColor", color);
                renderer_Brack.SetPropertyBlock(materialPropertyBlock);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation_Token);
            }
            player.enabled = true;//操作可能に
        }

        /// <summary>
        /// ファイル名異常
        /// </summary>
        private void VMDLoadError()
        {
            Debug.Log("読み込み失敗発生:" + SystemInfo.userProfile.LanguageCode);

            if (loadAnimation.gameObject.activeSelf) loadAnimation.gameObject.SetActive(false);

            int index = SystemInfo.userProfile.LanguageCode - 1;
            vmdError[index].gameObject.SetActive(true);
        }
    }

}