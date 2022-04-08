using System.Collections;
using UnityEngine;

namespace UniLiveViewer
{
    public class MoveUI : MonoBehaviour
    {
        [SerializeField] private Transform targetAnchor;
        [SerializeField] private SwitchController switchController;

        //public bool isViewerMode = false;
        private bool isInit = false;

        private void Awake()
        {

        }
        private void OnEnable()
        {
            if (!isInit) return;

            if(!targetAnchor.parent.gameObject.activeSelf) targetAnchor.parent.gameObject.SetActive(true);
            //ターゲットの位置へ移動
            transform.position = targetAnchor.position;
        }

        private void OnDisable()
        {
            if (!isInit) return;
            if (targetAnchor.parent.gameObject.activeSelf) targetAnchor.parent.gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine("init");
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

        private IEnumerator init()
        {
            //最初にとめておく(マニュアルモード)
            yield return new WaitForSeconds(0.1f);

            //キャラを生成する
            switchController.initPage();
            yield return new WaitForSeconds(0.1f);

            //非表示にする
            gameObject.SetActive(false);
            isInit = true;
            yield return null;
        }
    }
}