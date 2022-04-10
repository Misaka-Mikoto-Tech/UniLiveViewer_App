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
            switch (GlobalConfig.sceneMode_static)
            {
                case GlobalConfig.SceneMode.CANDY_LIVE:
                    transform.position = new Vector3(4, 3, 5.5f);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                    EndPoint = new Vector3(4, 1.2f, 5.5f);
                    break;
                case GlobalConfig.SceneMode.KAGURA_LIVE:
                    transform.position = new Vector3(0, 3, 3);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    EndPoint = new Vector3(0, 1.2f, 3);
                    break;
                case GlobalConfig.SceneMode.VIEWER:
                    transform.position = new Vector3(0, 3, 4);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    EndPoint = new Vector3(0, 1.2f, 4);
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
            if (isInit) transform.position = (playerStateManager.transform.position - keepDistance);
        }

        private void OnDisable()
        {
            if (isInit) keepDistance = playerStateManager.transform.position - transform.position;
        }

    }

}