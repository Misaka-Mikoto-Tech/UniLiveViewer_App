using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace UniLiveViewer
{ 
    public class DirectUI : MonoBehaviour
    {
        private PlayerStateManager playerStateManager;
        private Vector3 EndPoint = new Vector3(0, 0.7f, 5);
        private bool isInit = false;
        private Vector3 keepDistance;

        private void Awake()
        {
            
        }

        // Start is called before the first frame update
        void Start()
        {
            playerStateManager = PlayerStateManager.instance;

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
            }            
            Init().Forget();
        }

        private async UniTask Init()
        {
            await UniTask.Delay(500);

            int split = 50;
            Vector3 moveSpeed = (EndPoint - transform.position) / split;

            for (int i = 0; i < split; i++)
            {
                transform.position += moveSpeed;
                await UniTask.Yield();
            }

            await UniTask.Delay(500);

            //UIを表示する
            playerStateManager.SwitchUI();

            isInit = true;
        }

        private void OnEnable()
        {
            if (!playerStateManager) return;
            if (isInit) transform.position = (playerStateManager.transform.position - keepDistance);
        }

        private void OnDisable()
        {
            if (!playerStateManager) return;
            if (isInit) keepDistance = playerStateManager.transform.position - transform.position;
        }
    }

}