using System.Collections;
using UnityEngine;

namespace UniLiveViewer
{ 
    public class DirectUI : MonoBehaviour
    {
        PlayerStateManager playerStateManager;
        private Vector3 EndPoint = new Vector3(0, 0.7f, 5);
        private bool isInit = false;
        private Vector3 keepDistance;

        private void Awake()
        {
            playerStateManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateManager>();
        }

        // Start is called before the first frame update
        void Start()
        {
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
            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            yield return new WaitForSeconds(0.5f);

            int split = 50;
            Vector3 moveSpeed = (EndPoint - transform.position) / split;

            for (int i = 0; i < split; i++)
            {
                transform.position += moveSpeed;
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

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