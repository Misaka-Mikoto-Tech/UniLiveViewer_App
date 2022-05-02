using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer
{
    public class MoveUI : MonoBehaviour
    {
        [SerializeField] private Transform targetAnchor;
        [SerializeField] private SwitchController switchController;

        //public bool isViewerMode = false;
        private bool isInit = false;

        private CancellationToken token;

        private void Awake()
        {

        }

        private void OnEnable()
        {
            if (!isInit || !targetAnchor) return;
            if (!targetAnchor.parent.gameObject.activeSelf) targetAnchor.parent.gameObject.SetActive(true);
            //ターゲットの位置へ移動
            transform.position = targetAnchor.position;

            token = this.GetCancellationTokenOnDestroy();
        }

        private void OnDisable()
        {
            if (!isInit || !targetAnchor) return;
            if (targetAnchor.parent.gameObject.activeSelf) targetAnchor.parent.gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            Init().Forget();
        }

        // Update is called once per frame
        void Update()
        {
            //if (!isViewerMode) return;

            //ポーズ中なら以下処理しない
            if (Time.timeScale == 0) return;

            //持ち手の正面に合わせる
            transform.position = targetAnchor.position;
            transform.rotation = targetAnchor.rotation;
        }

        private async UniTask Init()
        {
            //最初にとめておく(マニュアルモード)
            await UniTask.Delay(200, cancellationToken: token);
            //非表示にする
            gameObject.SetActive(false);
            isInit = true;
        }
    }
}