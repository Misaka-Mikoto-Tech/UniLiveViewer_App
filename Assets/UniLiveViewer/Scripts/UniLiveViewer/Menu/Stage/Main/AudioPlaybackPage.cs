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
        PlayableMusicService _playableMusicService;
        PlayableDirector _playableDirector;
        PlayerStateManager _playerStateManager;
        AudioAssetManager _audioAssetManager;
        AudioSourceService _audioSourceService;

        CancellationToken _cancellationToken;

        [Inject]
        public void Construct(
            AudioAssetManager audioAssetManager,
            PlayableMusicService playableMusicService,
            PlayableDirector playableDirector,
            PlayerStateManager playerStateManager,
            AudioSourceService audioSourceService)
        {
            _audioAssetManager = audioAssetManager;
            _playableMusicService = playableMusicService;
            _playableDirector = playableDirector;
            _playerStateManager = playerStateManager;
            _audioSourceService = audioSourceService;
        }

        public void OnJumpSelect((JumpList.TARGET, int) select)
        {
            var target = select.Item1;
            var index = select.Item2;

            int moveIndex = 0;
            switch (target)
            {
                case JumpList.TARGET.AUDIO:
                    if (_isPresetAudio)
                    {
                        moveIndex = index - _audioAssetManager.CurrentPreset;
                    }
                    else
                    {
                        moveIndex = index - _audioAssetManager.CurrentCustom;
                    }
                    ChangeAuidoAsync(_isPresetAudio, moveIndex, _cancellationToken).Forget();
                    break;
            }
            _audioSourceService.PlayOneShot(0);
        }

        public async UniTask StartAsync(CancellationToken cancellation)
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

            _playableDirector.played += Director_Played;
            _playableDirector.stopped += Director_Stoped;
            for (int i = 0; i < btn_Audio.Length; i++)
            {
                btn_Audio[i].onTrigger += MoveIndex_Auido;
            }
            slider_Playback.Controled += OnUpdatePlaybackSlider;
            slider_Playback.ValueUpdate += () =>
            {
                var sec = slider_Playback.Value;
                _playableMusicService.AudioClipPlaybackTime = sec;//timelineに時間を反映
                textMeshs[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";//テキストに反映
            };
            slider_Speed.ValueUpdate += () =>
            {
                _playableMusicService.TimelineSpeed = slider_Speed.Value;//スライダーの値を反映
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

            //最初のアクターが生成されるのを待つ
            await UniTask.Delay(2000, cancellationToken: cancellation);
            BaseReturnAsync(cancellation).Forget();
        }

        void OnEnable()
        {
            Init();
        }

        async void Init()
        {
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
            var sec = await _playableMusicService.CurrentAudioLengthAsync(true, _cancellationToken);
            textMeshs[2].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
            //タイムラインの速度を表示
            slider_Speed.Value = _playableMusicService.TimelineSpeed;
            textMeshs[3].text = $"{slider_Speed.Value:0.00}";
        }

        // Update is called once per frame
        void Update()
        {
            //再生スライダー非制御中なら
            if (!slider_Playback.IsGrabbed)
            {
                //TimeLine再生時間をスライダーにセット
                var sec = (float)_playableMusicService.AudioClipPlaybackTime;
                slider_Playback.Value = sec;
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
                _menuManager.jumpList.SetAudioData(_isPresetAudio);
            }
            _audioSourceService.PlayOneShot(0);
        }

        /// <summary>
        /// オーディオプレイヤーのクリック処理
        /// </summary>
        /// <param name="btn"></param>
        void Click_AudioPlayer(Button_Base btn)
        {
            _audioSourceService.PlayOneShot(0);

            //スライダー操作中は受け付けない
            if (_playerStateManager.IsSliderGrabbing(Constants.TagGrabSliderVolume)) return;

            if (btn == btnS_Stop)
            {
                var dummy = new CancellationToken();
                StopAsync(dummy).Forget();
            }
            else if (btn == btnS_Play)
            {
                var dummy = new CancellationToken();
                PlayAsync(dummy).Forget();
            }
            else if (btn == btnS_BaseReturn)
            {
                var dummy = new CancellationToken();
                BaseReturnAsync(dummy).Forget();
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
            _audioSourceService.PlayOneShot(0);
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
                _audioSourceService.PlayOneShot(0);
                return;
            }
        }

        /// <summary>
        /// オーディオを変更する
        /// </summary>
        /// <param name="moveIndex"></param>
        async UniTask ChangeAuidoAsync(int moveIndex, CancellationToken cancellation)
        {
            var clipName = await _playableMusicService.NextAudioClip(_isPresetAudio, moveIndex, cancellation);
            await ChangeAuidoInternalAsync(clipName, cancellation);
        }

        /// <summary>
        /// オーディオを変更する
        /// </summary>
        /// <param name="moveIndex"></param>
        async UniTask ChangeAuidoAsync(bool isPreset, int moveIndex, CancellationToken cancellation)
        {
            var clipName = await _playableMusicService.NextAudioClip(isPreset, moveIndex, cancellation);
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

            var sec = await _playableMusicService.CurrentAudioLengthAsync(_isPresetAudio, cancellation);
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
            if (_playableMusicService.AudioClipPlaybackTime > 0) return;

            //再生表示
            if (btnS_Stop) btnS_Stop.gameObject.SetActive(false);
            if (btnS_Play) btnS_Play.gameObject.SetActive(true);
        }

        void OnUpdatePlaybackSlider()
        {
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual) return;

            btnS_Stop.gameObject.SetActive(false);
            btnS_Play.gameObject.SetActive(true);

            var dummy = new CancellationToken();
            _playableMusicService.ManualModeAsync(dummy).Forget();
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                var dummy = new CancellationToken();
                PlayAsync(dummy).Forget();
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                var dummy = new CancellationToken();
                BaseReturnAsync(dummy).Forget();
            }
            if (Input.GetKeyDown(KeyCode.K)) ChangeAuidoAsync(1, _cancellationToken).Forget();
            if (Input.GetKeyDown(KeyCode.J)) ChangeAuidoAsync(-1, _cancellationToken).Forget();
        }

        async UniTask PlayAsync(CancellationToken cancellation)
        {
            btnS_Stop.gameObject.SetActive(true);
            btnS_Play.gameObject.SetActive(false);

            await _playableMusicService.PlayAsync(cancellation);
        }

        async UniTask StopAsync(CancellationToken cancellation)
        {
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual) return;

            btnS_Stop.gameObject.SetActive(false);
            btnS_Play.gameObject.SetActive(true);

            await _playableMusicService.ManualModeAsync(cancellation);
        }

        async UniTask BaseReturnAsync(CancellationToken cancellation)
        {
            await _playableMusicService.BaseReturnAsync(cancellation);
        }
    }

}
