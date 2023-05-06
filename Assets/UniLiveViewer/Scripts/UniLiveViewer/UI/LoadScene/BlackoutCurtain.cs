using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace UniLiveViewer
{
    public class BlackoutCurtain : MonoBehaviour
    {
        [SerializeField] LoadAnimation loadAnimation;
        [SerializeField] Renderer renderer_Cutoff;
        [SerializeField] Renderer renderer_Brack;
        [SerializeField] TextMesh[] vmdError = new TextMesh[2];

        [SerializeField] AnimationCurve curve;
        public static BlackoutCurtain instance;

        PlayerStateManager _playerStateManager;
        MaterialPropertyBlock materialPropertyBlock;
        Color color;
        CancellationToken cancellation_Token;

        void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
            cancellation_Token = this.GetCancellationTokenOnDestroy();

            //不透明黒
            color = new Color(0, 0, 0, 1);
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

            // TODO: UI作り直す時にまともにする
            var player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLifetimeScope>();
            _playerStateManager = player.Container.Resolve<PlayerStateManager>();
        }

        /// <summary>
        /// 演出開始
        /// </summary>
        public void Staging()
        {
            transform.parent = _playerStateManager.myCamera.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            loadAnimation.gameObject.SetActive(true);
        }

        /// <summary>
        /// エラーメッセージを表示
        /// </summary>
        public void ShowErrorMessage()
        {
            Debug.Log("読み込み失敗発生:" + SystemInfo.userProfile.LanguageCode);

            if (loadAnimation.gameObject.activeSelf) loadAnimation.gameObject.SetActive(false);

            int index = SystemInfo.userProfile.LanguageCode - 1;
            vmdError[index].gameObject.SetActive(true);
        }

        /// <summary>
        /// 演出終了
        /// </summary>
        /// <returns></returns>
        public async UniTaskVoid Ending()
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
            _playerStateManager.enabled = true;//操作可能に
        }

        /// <summary>
        /// 暗転しシーンを遷移する
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public async UniTask StartBlackout(string sceneName)
        {
            _playerStateManager.enabled = false;//操作不可に
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
            FileReadAndWriteUtility.WriteJson(SystemInfo.userProfile);
            async.allowSceneActivation = true;
        }
    }

}