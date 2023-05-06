using UnityEngine;
using NanaCiel;
using UnityEngine.Playables;
using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer;

namespace UniLiveViewer
{
    public class AudioPlaybackPage : MonoBehaviour
    {
        [SerializeField] MenuManager menuManager;
        [SerializeField] Button_Base[] btn_jumpList;

        [SerializeField] Button_Base[] btn_Audio = new Button_Base[2];
        [SerializeField] Button_Base btnS_Play = null;
        [SerializeField] Button_Base btnS_Stop = null;
        [SerializeField] Button_Base btnS_BaseReturn = null;
        [SerializeField] TextMesh[] textMeshs = new TextMesh[4];
        [SerializeField] SliderGrabController slider_Playback = null;
        [SerializeField] SliderGrabController slider_Speed = null;

        TimelineController _timeline;
        TimelineInfo _timelineInfo;
        PlayerStateManager _playerStateManager;
        AudioAssetManager _audioAssetManager;

        CancellationTokenSource cts;

        void Awake()
        {
            
        }

        void OnEnable()
        {
            Init();
        }

        // Start is called before the first frame update
        void Start()
        {
            var appConfig = GameObject.FindGameObjectWithTag("AppConfig").transform;
            _audioAssetManager = appConfig.GetComponent<AudioAssetManager>();

            // TODO: UI作り直す時にまともにする
            var player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLifetimeScope>();
            _playerStateManager = player.Container.Resolve<PlayerStateManager>();

            _timeline = menuManager.timeline;
            _timelineInfo = _timeline.GetComponent<TimelineInfo>();

            //再生スライダーに最大値を設定
            slider_Playback.maxValuel = (float)_timelineInfo.GetPlayableDirector.duration;

            //ジャンプリスト
            foreach (var e in btn_jumpList)
            {
                e.onTrigger += OpenJumplist;
            }
            menuManager.jumpList.onSelect += (jumpCurrent) =>
            {
                int moveIndex = 0;
                switch (menuManager.jumpList.target)
                {
                    case JumpList.TARGET.AUDIO:
                        moveIndex = jumpCurrent - _audioAssetManager.CurrentCustom;
                        ChangeAuido(moveIndex);
                        break;
                }
                menuManager.PlayOneShot(SoundType.BTN_CLICK);
            };

            //その他
            _timelineInfo.GetPlayableDirector.played += Director_Played;
            _timelineInfo.GetPlayableDirector.stopped += Director_Stoped;
            for (int i = 0; i < btn_Audio.Length; i++)
            {
                btn_Audio[i].onTrigger += MoveIndex_Auido;
            }
            slider_Playback.Controled += ManualStart;
            slider_Playback.ValueUpdate += () =>
            {
                float sec = slider_Playback.Value;
                _timeline.AudioClip_PlaybackTime = sec;//timelineに時間を反映
                textMeshs[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";//テキストに反映
            };
            slider_Speed.ValueUpdate += () =>
            {
                _timeline.TimelineSpeed = slider_Speed.Value;//スライダーの値を反映
                textMeshs[3].text = $"{slider_Speed.Value:0.00}";//テキストに反映
            };
            btnS_Play.onTrigger += Click_AudioPlayer;
            btnS_Stop.onTrigger += Click_AudioPlayer;
            btnS_BaseReturn.onTrigger += Click_AudioPlayer;
            Init();
        }

        async void Init()
        {
            if (!_timeline) return;
            if (_timelineInfo.GetPlayableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                btnS_Stop.gameObject.SetActive(false);
                btnS_Play.gameObject.SetActive(true);
            }
            else
            {
                btnS_Stop.gameObject.SetActive(true);
                btnS_Play.gameObject.SetActive(false);
            }
            //オーディオの長さ
            float sec = await _timeline.GetNowAudioLength(false);
            textMeshs[2].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
            //タイムラインの速度を表示
            slider_Speed.Value = _timeline.TimelineSpeed;
            textMeshs[3].text = $"{slider_Speed.Value:0.00}";
        }

        // Update is called once per frame
        void Update()
        {
            //再生スライダー非制御中なら
            if (!slider_Playback.isControl)
            {
                //TimeLine再生時間をスライダーにセット
                float sec = (float)_timeline.AudioClip_PlaybackTime;
                slider_Playback.Value = sec;
                //テキストに反映
                textMeshs[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
            }
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        void OpenJumplist(Button_Base btn)
        {
            if (!menuManager.jumpList.gameObject.activeSelf) menuManager.jumpList.gameObject.SetActive(true);

            if (btn == btn_jumpList[0])
            {
                menuManager.jumpList.SetAudioDate();
            }
            menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// オーディオプレイヤーのクリック処理
        /// </summary>
        /// <param name="btn"></param>
        void Click_AudioPlayer(Button_Base btn)
        {
            menuManager.PlayOneShot(SoundType.BTN_CLICK);

            //スライダー操作中は受け付けない
            if (_playerStateManager.IsSliderGrabbing(SystemInfo.tag_GrabSliderVolume)) return;

            if (btn == btnS_Stop)
            {
                //マニュアル開始
                _timeline.TimelineManualMode().Forget();
                //再生・停止ボタンの状態更新
                btnS_Stop.gameObject.SetActive(false);
                btnS_Play.gameObject.SetActive(true);
            }
            else if (btn == btnS_Play)
            {
                //タイムライン・再生
                _timeline.TimelinePlay();
                //再生・停止ボタンの状態更新
                btnS_Stop.gameObject.SetActive(true);
                btnS_Play.gameObject.SetActive(false);
            }
            else if (btn == btnS_BaseReturn)
            {
                //タイムライン・初期化
                _timeline.TimelineBaseReturn();
            }
        }

        /// <summary>
        /// 次オーディオに切り替える
        /// </summary>
        /// <param name="btn"></param>
        void MoveIndex_Auido(Button_Base btn)
        {
            for (int i = 0; i < 2; i++)
            {
                //押されたボタンの判別
                if (btn_Audio[i] == btn)
                {
                    int moveIndex = 0;
                    if (i == 0) moveIndex = -1;
                    else if (i == 1) moveIndex = 1;

                    ChangeAuido(moveIndex);

                    menuManager.PlayOneShot(SoundType.BTN_CLICK);
                    break;
                }
            }
        }

        /// <summary>
        /// オーディオを変更する
        /// </summary>
        /// <param name="moveIndex"></param>
        async void ChangeAuido(int moveIndex)
        {
            //文字画像を差し替える
            textMeshs[0].text = await _timeline.NextAudioClip(false,moveIndex);
            //サイズ調整
            textMeshs[0].fontSize = textMeshs[0].text.FontSizeMatch(600, 30, 50);
            //オーディオの長さ
            float sec = await _timeline.GetNowAudioLength(false);
            slider_Playback.maxValuel = sec;
            textMeshs[2].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
        }

        void Director_Played(PlayableDirector obj)
        {
            //停止表示
            //btnS_Stop.gameObject.SetActive(true);
            //btnS_Play.gameObject.SetActive(false);
        }
        void Director_Stoped(PlayableDirector obj)
        {
            //再生途中の一時停止は無視する
            if (_timeline.AudioClip_PlaybackTime <= 0)
            {
                //再生表示
                if (btnS_Stop) btnS_Stop.gameObject.SetActive(false);
                if (btnS_Play) btnS_Play.gameObject.SetActive(true);
            }
        }

        void ManualStart()
        {
            //ボタンの状態を制御
            btnS_Stop.gameObject.SetActive(false);
            btnS_Play.gameObject.SetActive(true);

            //マニュアルモードにする
            _timeline.TimelineManualMode().Forget();
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I)) ChangeAuido(1);
            if (Input.GetKeyDown(KeyCode.K))
            {
                //タイムライン・再生
                _timeline.TimelinePlay();
                //再生・停止ボタンの状態更新
                btnS_Stop.gameObject.SetActive(true);
                btnS_Play.gameObject.SetActive(false);
            }
        }
    }

}
