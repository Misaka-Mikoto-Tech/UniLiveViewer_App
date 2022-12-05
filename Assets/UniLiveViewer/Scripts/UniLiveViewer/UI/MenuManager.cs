using UnityEngine;

namespace UniLiveViewer
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

        public TimelineController timeline => _timeline;
        public VRMSwitchController vrmSelectUI => _vrmSelectUI;

        TimelineController _timeline = null;
        [SerializeField] VRMSwitchController _vrmSelectUI = null;

        [Header("＜Sound＞")]
        [SerializeField] AudioClip[] Sound;//ボタン音,タブ音,ボタン揺れ音
        AudioSource audioSource;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = SystemInfo.soundVolume_SE;

            _timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();    
        }

        // Start is called before the first frame update
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