using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer 
{
    public class PageController : MonoBehaviour
    {
        public event Action onSwitchPage;
        [Header("＜ページ・タブボタン＞")]
        [SerializeField] private Button_Switch[] btnTab;
        [SerializeField] private Transform[] pageAnchor;
        public Transform[] GetPageAnchor => pageAnchor;

        private CancellationToken cancellation_token;
        public int current { get; private set; }
        public Transform GetCurrentPage() { return pageAnchor[current]; } 

        [Header("＜Sound＞")]
        [SerializeField] private AudioClip[] Sound;//ボタン音
        private AudioSource audioSource;


        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = SystemInfo.soundVolume_SE;
        }

        // Start is called before the first frame update
        void Start()
        {
            if (btnTab.Length != pageAnchor.Length)
            {
                Debug.LogError("ページとボタン数が一致しません");
                return;
            }

            for (int i = 0; i < btnTab.Length; i++)
            {
                btnTab[i].onTrigger += SwitchCurrent;
            }

            cancellation_token = this.GetCancellationTokenOnDestroy();

            SwitchPages();
        }

        /// <summary>
        /// ページを切り替える
        /// </summary>
        /// <param name="btn"></param>
        private void SwitchCurrent(Button_Base btn)
        {
            //タブのボタン状態を更新する
            for (int i = 0; i < btnTab.Length; i++)
            {
                if (btnTab[i] != btn) continue;
                current = i;
                break;
            }
            //クリック音
            audioSource.PlayOneShot(Sound[0]);

            SwitchPages();
        }

        /// <summary>
        /// ページを切り替える
        /// </summary>
        /// <param name="btn"></param>
        public void ButtonShow_Passthrough(bool isEnable)
        {
            if (btnTab[5].gameObject.activeSelf == isEnable) return;
            btnTab[5].gameObject.SetActive(isEnable);
        }

        /// <summary>
        /// カレントページに切り替える
        /// </summary>
        private void SwitchPages()
        {
            bool b = false;
            for (int i = 0; i < btnTab.Length; i++)
            {
                if (!btnTab[i]) continue;
                b = current == i;
                btnTab[i].isEnable = b;
                if (pageAnchor[i].gameObject.activeSelf != b) pageAnchor[i].gameObject.SetActive(b);
            }

            onSwitchPage?.Invoke();

            UniTask.Void(async () =>
            {
                await UniTask.Delay(400, cancellationToken: cancellation_token);
                //ボタンの揺れ音
                audioSource.PlayOneShot(Sound[1]);
            });
        }

        private void Update()
        {
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        private void OnEnable()
        {
            SwitchPages();
        }

        private void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                current++;
                if (current >= pageAnchor.Length) current = 0;
                audioSource.PlayOneShot(Sound[0]);
                SwitchPages();
            }
        }
    }
}

