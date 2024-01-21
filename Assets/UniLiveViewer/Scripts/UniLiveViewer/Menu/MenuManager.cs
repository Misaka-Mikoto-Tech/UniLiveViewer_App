using UnityEngine;

namespace UniLiveViewer.Menu
{
    public enum SoundType
    {
        BTN_CLICK,
        BTN_TAB_CLICK,
        BTN_SPRING,
        BTN_CLICK_LIGHT,
    }

    public class MenuManager : MonoBehaviour
    {
        [SerializeField] PageController pageController;

        public JumpList jumpList => _jumpList;
        [SerializeField] JumpList _jumpList = null;

        [Header("＜Sound＞")]
        [SerializeField] AudioClip[] Sound;//ボタン音,タブ音,ボタン揺れ音
        AudioSource audioSource;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = SystemInfo.soundVolume_SE;
        }

        void Start()
        {
            pageController.onSwitchPage += () =>
            {
                if (jumpList.gameObject.activeSelf) jumpList.gameObject.SetActive(false);
            };
        }

        public void PlayOneShot(SoundType soundType)
        {
            audioSource.PlayOneShot(Sound[(int)soundType]);
        }
    }
}