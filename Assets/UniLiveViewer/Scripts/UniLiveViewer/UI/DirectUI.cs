using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace UniLiveViewer
{ 
    public class DirectUI : MonoBehaviour
    {
        [SerializeField] Transform guideParent;
        [SerializeField] Transform guideTarget;

        PlayerStateManager playerStateManager;
        Renderer _renderer;
        Vector3 EndPoint = new Vector3(0, 0.7f, 5);
        Vector3 keepDistance;

        bool isInit = false;

        void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        // Start is called before the first frame update
        void Start()
        {

            playerStateManager = PlayerStateManager.instance;
            playerStateManager.onSwitchMainUI += SwitchEnable;

            switch (SystemInfo.sceneMode)
            {
                case SceneMode.CANDY_LIVE:
                    EndPoint =  new Vector3(4, 1.0f, 5.5f);
                    transform.position = EndPoint + (Vector3.up * 2);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                    break;
                case SceneMode.KAGURA_LIVE:
                    EndPoint = new Vector3(0, 1.35f, 3);
                    transform.position = EndPoint + (Vector3.up * 2);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case SceneMode.VIEWER:
                    EndPoint = new Vector3(0, 1.0f, 4);
                    transform.position = EndPoint + (Vector3.up * 2);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case SceneMode.GYMNASIUM:
                    EndPoint = new Vector3(0, 1.0f, 4);
                    transform.position = EndPoint + (Vector3.up * 2);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
            }
        }

        public void Initialize()
        {
            transform.position = EndPoint;
            DelayUIOpen().Forget();
        }

        /// <summary>
        /// 時間差でUIを表示する
        /// </summary>
        async UniTask DelayUIOpen()
        {
            //時間差でUIを表示する
            SwitchEnable(false);
            await UniTask.Delay(800);
            SwitchEnable(true);
            isInit = true;
        }

        void Update()
        {
            if (!isInit) return;
            guideTarget.position = guideParent.position;
            guideTarget.rotation = guideParent.rotation;
        }

        void SwitchEnable(bool isEnable)
        {
            if (guideParent.gameObject.activeSelf != isEnable) guideParent.gameObject.SetActive(isEnable);
            if (guideTarget.gameObject.activeSelf != isEnable) guideTarget.gameObject.SetActive(isEnable);
            _renderer.enabled = isEnable;

            if(isEnable) transform.position = (playerStateManager.transform.position - keepDistance);
            else keepDistance = playerStateManager.transform.position - transform.position;
        }
    }
}