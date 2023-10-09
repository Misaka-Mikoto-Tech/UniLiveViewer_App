using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    public class BlackoutCurtain : MonoBehaviour
    {
        [SerializeField] LoadAnimation loadAnimation;
        [SerializeField] Renderer renderer_Cutoff;
        [SerializeField] Renderer renderer_Brack;
        [SerializeField] TextMesh[] vmdError = new TextMesh[2];

        [SerializeField] AnimationCurve curve;
        public static BlackoutCurtain instance;

        
        MaterialPropertyBlock _materialPropertyBlock;
        Color _color;
        CancellationToken _cancellation;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            _cancellation = this.GetCancellationTokenOnDestroy();

            //不透明黒
            _color = new Color(0, 0, 0, 1);
            _materialPropertyBlock = new MaterialPropertyBlock();
            renderer_Brack.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor("_BaseColor", _color);
            renderer_Brack.SetPropertyBlock(_materialPropertyBlock);

            renderer_Brack.enabled = true;
            renderer_Cutoff.enabled = false;
            if (loadAnimation.gameObject.activeSelf) loadAnimation.gameObject.SetActive(false);

            foreach (var e in vmdError)
            {
                if (e.gameObject.activeSelf) e.gameObject.SetActive(false);
            }

            instance = this;
        }

        /// <summary>
        /// 演出開始
        /// </summary>
        public void Staging()
        {
            // ちゃんとカメラ渡したい
            transform.parent = Camera.main.transform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            loadAnimation.gameObject.SetActive(true);
        }

        /// <summary>
        /// エラーメッセージを表示　←警告やめるので削除予定
        /// </summary>
        public void ShowErrorMessage()
        {
            //Debug.Log("読み込み失敗発生:" + FileReadAndWriteUtility.UserProfile.LanguageCode);

            //if (loadAnimation.gameObject.activeSelf) loadAnimation.gameObject.SetActive(false);

            //int index = FileReadAndWriteUtility.UserProfile.LanguageCode - 1;
            //vmdError[index].gameObject.SetActive(true);
        }

        /// <summary>
        /// 演出終了
        /// </summary>
        /// <returns></returns>
        public async UniTaskVoid Ending()
        {
            await UniTask.Delay(300, cancellationToken: _cancellation);

            //まずloadingアニメーションを消す
            loadAnimation.gameObject.SetActive(false);
            await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);

            //暗転から徐々に再開
            _color = _materialPropertyBlock.GetColor("_BaseColor");
            _color.a = 1;//不透明

            while (_color.a >= 0.0f)
            {
                _color.a -= Time.deltaTime;

                _materialPropertyBlock.SetColor("_BaseColor", _color);
                renderer_Brack.SetPropertyBlock(_materialPropertyBlock);
                await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);
            }
        }

        /// <summary>
        /// 暗転しシーンを遷移する
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public async UniTask StartBlackout(string sceneName)
        {
            //_playerStateManager.enabled = false;// TODO: 操作不可にしないといけない
            renderer_Brack.enabled = false;
            renderer_Cutoff.enabled = true;
            if (loadAnimation.gameObject.activeSelf) loadAnimation.gameObject.SetActive(false);
            await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);

            //閉幕演出
            float t = 0;
            while (t < 2.5f)
            {
                renderer_Cutoff.sharedMaterial.SetFloat("_Scala", curve.Evaluate(t));
                t += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);
            }
            renderer_Brack.enabled = true;
            _color.a = 1;//不透明
            _materialPropertyBlock.SetColor("_BaseColor", _color);
            renderer_Brack.SetPropertyBlock(_materialPropertyBlock);
            
            renderer_Cutoff.sharedMaterial.SetFloat("_Scala", 0);
            renderer_Cutoff.enabled = false;

            //ローディングアニメーション
            loadAnimation.gameObject.SetActive(true);
            //ロードが早すぎても最低演出分は待機する
            var async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;
            await UniTask.Delay(1000, cancellationToken: _cancellation);
            FileReadAndWriteUtility.UserProfile.LastSceneName = sceneName;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            async.allowSceneActivation = true;
        }
    }

}