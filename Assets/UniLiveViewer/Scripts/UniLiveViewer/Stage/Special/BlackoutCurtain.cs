using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// シーン遷移時のPlayerの視界を遮る
    /// TODO: まだ仮
    /// </summary>
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
            _cancellation = this.GetCancellationTokenOnDestroy();

            //不透明黒
            _color = new Color(0, 0, 0, 1);
            _materialPropertyBlock = new MaterialPropertyBlock();
            renderer_Brack.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetColor("_BaseColor", _color);
            renderer_Brack.SetPropertyBlock(_materialPropertyBlock);

            renderer_Brack.enabled = true;
            renderer_Cutoff.enabled = false;

            foreach (var e in vmdError)
            {
                if (e.gameObject.activeSelf) e.gameObject.SetActive(false);
            }

            // 演出開始
            loadAnimation.gameObject.SetActive(true);

            instance = this;
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
        /// 暗転させる
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        public async UniTask FadeoutAsync(CancellationToken cancellation)
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
            await UniTask.Delay(200, cancellationToken: cancellation);
        }
    }

}