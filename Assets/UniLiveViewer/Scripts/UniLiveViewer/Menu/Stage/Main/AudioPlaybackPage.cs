using Cysharp.Threading.Tasks;
using NanaCiel;
using System.Threading;
using UniLiveViewer.Player;
using UniLiveViewer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

namespace UniLiveViewer.Menu
{
    /// <summary>
    /// TODO: 変数名
    /// </summary>
    public class AudioPlaybackPage : MonoBehaviour
    {
        [SerializeField] MenuManager _menuManager;
        [SerializeField] Button_Base[] btn_jumpList;
        [SerializeField] Button_Switch[] _switchAudio = new Button_Switch[2];
        bool _isPresetAudio;

        [SerializeField] Button_Base[] btn_Audio = new Button_Base[2];
        [SerializeField] Button_Base btnS_Play = null;
        [SerializeField] Button_Base btnS_Stop = null;
        [SerializeField] Button_Base btnS_BaseReturn = null;
        [SerializeField] TextMesh[] textMeshs = new TextMesh[4];
        [SerializeField] SliderGrabController slider_Playback = null;
        [SerializeField] SliderGrabController slider_Speed = null;

        TimelineController _timeline;
        PlayableDirector _playableDirector;
        PlayerStateManager _playerStateManager;
        AudioAssetManager _audioAssetManager;

        CancellationToken _cancellationToken;

        [Inject]
        void Construct(
            AudioAssetManager audioAssetManager,
            TimelineController timelineController,
            PlayableDirector playableDirector,
            PlayerStateManager playerStateManager)
        {
            _audioAssetManager = audioAssetManager;
            _timeline = timelineController;
            _playableDirector = playableDirector;
            _playerStateManager = playerStateManager;
        }

        public void OnStart()
        {
            _isPresetAudio = true;
            _cancellationToken = this.GetCancellationTokenOnDestroy();

            //再生スライダーに最大値を設定
            slider_Playback.maxValuel = (float)_playableDirector.duration;

            //ジャンプリスト
            foreach (var e in btn_jumpList)
            {
                e.onTrigger += OpenJumplist;
            }
            _menuManager.jumpList.onSelect += (jumpCurrent) =>
            {
                var moveIndex = 0;
                switch (_menuManager.jumpList.target)
                {
                    case JumpList.TARGET.AUDIO:
                        if (_isPresetAudio)
                        {
                            moveIndex = jumpCurrent - _audioAssetManager.CurrentPreset;
                        }
                        else
                        {
                            moveIndex = jumpCurrent - _audioAssetManager.CurrentCustom;
                        }
                        ChangeAuidoAsync(_isPresetAudio, moveIndex, _cancellationToken).Forget();
                        break;
                }
                _menuManager.PlayOneShot(SoundType.BTN_CLICK);
            };

            //その他
            _playableDirector.played += Director_Played;
            _playableDirector.stopped += Director_Stoped;
            for (int i = 0; i < btn_Audio.Length; i++)
            {
                btn_Audio[i].onTrigger += MoveIndex_Auido;
            }
            slider_Playback.Controled += ManualStart;
            slider_Playback.ValueUpdate += () =>
            {
                var sec = slider_Playback.Value;
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
            for (int i = 0; i < _switchAudio.Length; i++)
            {
                _switchAudio[i].isEnable = (i == 0);
                _switchAudio[i].onTrigger += OnClickSwitchAudio;
            }

            Init();
        }

        void OnEnable()
        {
            Init();
        }

        async void Init()
        {
            if (!_timeline) return;
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
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
            var sec = await _timeline.CurrentAudioLength(_cancellationToken, true);
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
                var sec = (float)_timeline.AudioClip_PlaybackTime;
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
            if (!_menuManager.jumpList.gameObject.activeSelf) _menuManager.jumpList.gameObject.SetActive(true);

            if (btn == btn_jumpList[0])
            {
                _menuManager.jumpList.SetAudioDate(_isPresetAudio);
            }
            _menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// オーディオプレイヤーのクリック処理
        /// </summary>
        /// <param name="btn"></param>
        void Click_AudioPlayer(Button_Base btn)
        {
            _menuManager.PlayOneShot(SoundType.BTN_CLICK);

            //スライダー操作中は受け付けない
            if (_playerStateManager.IsSliderGrabbing(Constants.TagGrabSliderVolume)) return;

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

        void OnClickSwitchAudio(Button_Base btn)
        {
            if (_switchAudio[0] == btn)
            {
                _isPresetAudio = true;
                _switchAudio[0].isEnable = true;
                _switchAudio[1].isEnable = false;
            }
            else
            {
                _isPresetAudio = false;
                _switchAudio[0].isEnable = false;
                _switchAudio[1].isEnable = true;
            }
            _menuManager.jumpList.Close();
            _menuManager.PlayOneShot(SoundType.BTN_CLICK);
            ChangeAuidoAsync(_isPresetAudio, 0, _cancellationToken).Forget();
        }

        /// <summary>
        /// 次オーディオに切り替える
        /// </summary>
        /// <param name="btn"></param>
        void MoveIndex_Auido(Button_Base btn)
        {
            for (int i = 0; i < 2; i++)
            {
                if (btn_Audio[i] != btn) continue;

                var moveIndex = i == 0 ? -1 : 1;
                ChangeAuidoAsync(moveIndex, _cancellationToken).Forget();
                _menuManager.PlayOneShot(SoundType.BTN_CLICK);
                return;
            }
        }

        /// <summary>
        /// オーディオを変更する
        /// </summary>
        /// <param name="moveIndex"></param>
        async UniTask ChangeAuidoAsync(int moveIndex, CancellationToken cancellation)
        {
            var clipName = await _timeline.NextAudioClip(cancellation, _isPresetAudio, moveIndex);
            await ChangeAuidoInternalAsync(clipName, cancellation);
        }

        /// <summary>
        /// オーディオを変更する
        /// </summary>
        /// <param name="moveIndex"></param>
        async UniTask ChangeAuidoAsync(bool isPreset, int moveIndex, CancellationToken cancellation)
        {
            var clipName = await _timeline.NextAudioClip(cancellation, isPreset, moveIndex);
            await ChangeAuidoInternalAsync(clipName, cancellation);
        }

        async UniTask ChangeAuidoInternalAsync(string clipName, CancellationToken cancellation)
        {
            textMeshs[0].text = clipName;
            textMeshs[0].fontSize = clipName.FontSizeMatch(600, 30, 50);

            if (clipName == string.Empty)
            {
                Debug.LogWarning("No custom songs.");
                return;
            }

            var sec = await _timeline.CurrentAudioLength(cancellation, _isPresetAudio);
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
            if (_timeline.AudioClip_PlaybackTime > 0) return;

            //再生表示
            if (btnS_Stop) btnS_Stop.gameObject.SetActive(false);
            if (btnS_Play) btnS_Play.gameObject.SetActive(true);
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
            if (Input.GetKeyDown(KeyCode.I)) ChangeAuidoAsync(1, _cancellationToken).Forget();
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
