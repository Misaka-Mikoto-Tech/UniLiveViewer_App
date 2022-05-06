using UnityEngine;

namespace UniLiveViewer
{
    public enum SoundType
    {
        BTN_CLICK,
        BTN_TAB_CLICK,
        BTN_SPRING,
    }

    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private PageController pageController;

        public JumpList jumpList => _jumpList;
        [SerializeField] private JumpList _jumpList = null;

        public TimelineController timeline => _timeline;
        public GeneratorPortal generatorPortal  => _generatorPortal;
        public VRMSwitchController vrmSelectUI => _vrmSelectUI;
        public FileAccessManager fileAccess => _fileAccess;

        private TimelineController _timeline = null;
        [SerializeField] private GeneratorPortal _generatorPortal = null;
        [SerializeField] private VRMSwitchController _vrmSelectUI = null;
        private FileAccessManager _fileAccess;

        [Header("＜Sound＞")]
        [SerializeField] private AudioClip[] Sound;//ボタン音,タブ音,ボタン揺れ音
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = SystemInfo.soundVolume_SE;

            _timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            _fileAccess = GameObject.FindGameObjectWithTag("AppConfig").gameObject.GetComponent<FileAccessManager>();

            pageController.onSwitchPage += () =>
            {
                if (jumpList.gameObject.activeSelf) jumpList.gameObject.SetActive(false);
            };
        }

        // Start is called before the first frame update
        void Start()
        {
            //音楽をセット
            timeline.NextAudioClip(0);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PlayOneShot(SoundType soundType)
        {
            audioSource.PlayOneShot(Sound[(int)soundType]);
        }
    }
}