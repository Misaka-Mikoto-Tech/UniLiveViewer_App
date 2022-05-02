using System.Collections;
using UnityEngine;
using NanaCiel;
using UnityEngine.Playables;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace UniLiveViewer
{
    public class AudioPlaybackPage : MonoBehaviour
    {
        private MenuManager menuManager;
        [SerializeField] private Button_Base[] btn_jumpList;

        [SerializeField] private Button_Base[] btn_Audio = new Button_Base[2];
        [SerializeField] private Button_Base btnS_Play = null;
        [SerializeField] private Button_Base btnS_Stop = null;
        [SerializeField] private Button_Base btnS_BaseReturn = null;
        [SerializeField] private TextMesh[] textMeshs = new TextMesh[4];
        [SerializeField] private SliderGrabController slider_Playback = null;
        [SerializeField] private SliderGrabController slider_Speed = null;
        [SerializeField] private Button_Base[] btnS_AudioLoad = new Button_Base[2];

        private TimelineController timeline = null;
        private PlayerStateManager playerStateManager = null;
        private FileAccessManager fileAccess = null;

        private CancellationToken cancellation_token;

        private void Awake()
        {
            menuManager = transform.root.GetComponent<MenuManager>();

            timeline = menuManager.timeline;
            playerStateManager = menuManager.playerStateManager;
            fileAccess = menuManager.fileAccess;

            cancellation_token = this.GetCancellationTokenOnDestroy();

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
                        moveIndex = jumpCurrent - fileAccess.CurrentAudio;
                        ChangeAuido(moveIndex);
                        break;
                }
                menuManager.PlayOneShot(SoundType.BTN_CLICK);
            };

            //その他
            timeline.playableDirector.played += Director_Played;
            timeline.playableDirector.stopped += Director_Stoped;
            for (int i = 0; i < btn_Audio.Length; i++)
            {
                btn_Audio[i].onTrigger += MoveIndex_Auido;
            }
            slider_Playback.Controled += ManualStart;
            slider_Playback.ValueUpdate += () =>
            {
                float sec = slider_Playback.Value;
                timeline.AudioClip_PlaybackTime = sec;//timelineに時間を反映
                textMeshs[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";//テキストに反映
            };
            slider_Speed.ValueUpdate += () =>
            {
                timeline.timelineSpeed = slider_Speed.Value;//スライダーの値を反映
                textMeshs[3].text = $"{slider_Speed.Value:0.00}";//テキストに反映
            };
            btnS_Play.onTrigger += Click_AudioPlayer;
            btnS_Stop.onTrigger += Click_AudioPlayer;
            btnS_BaseReturn.onTrigger += Click_AudioPlayer;
            for (int i = 0; i < btnS_AudioLoad.Length; i++)
            {
                btnS_AudioLoad[i].onTrigger += Click_AudioLoad;
            }
        }

        private void OnEnable()
        {
            Init().Forget();
        }

        // Start is called before the first frame update
        void Start()
        {
            //再生スライダーに最大値を設定
            slider_Playback.maxValuel = (float)timeline.playableDirector.duration;
        }

        private async UniTaskVoid Init()
        {
            await UniTask.Yield(cancellation_token);

            if (timeline.playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                btnS_Stop.gameObject.SetActive(false);
                btnS_Play.gameObject.SetActive(true);
            }
            else
            {
                btnS_Stop.gameObject.SetActive(true);
                btnS_Play.gameObject.SetActive(false);
            }
            //読み込み関係のボタンを初期化しておく
            StartCoroutine(ReceptionTime(0));
            //オーディオの長さ
            float sec = timeline.GetNowAudioLength();
            textMeshs[2].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
            //タイムラインの速度を表示
            slider_Speed.Value = timeline.timelineSpeed;
            textMeshs[3].text = $"{slider_Speed.Value:0.00}";
        }

        // Update is called once per frame
        void Update()
        {
            //再生スライダー非制御中なら
            if (!slider_Playback.isControl)
            {
                //TimeLine再生時間をスライダーにセット
                float sec = (float)timeline.AudioClip_PlaybackTime;
                slider_Playback.Value = sec;
                //テキストに反映
                textMeshs[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
            }
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        private void OpenJumplist(Button_Base btn)
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
        private void Click_AudioPlayer(Button_Base btn)
        {
            menuManager.PlayOneShot(SoundType.BTN_CLICK);

            //スライダー操作中は受け付けない
            if (playerStateManager.CheckGrabbing()) return;

            if (btn == btnS_Stop)
            {
                //マニュアル開始
                timeline.TimelineManualMode();
                //再生・停止ボタンの状態更新
                btnS_Stop.gameObject.SetActive(false);
                btnS_Play.gameObject.SetActive(true);
            }
            else if (btn == btnS_Play)
            {
                //タイムライン・再生
                timeline.TimelinePlay();
                //再生・停止ボタンの状態更新
                btnS_Stop.gameObject.SetActive(true);
                btnS_Play.gameObject.SetActive(false);
            }
            else if (btn == btnS_BaseReturn)
            {
                //タイムライン・初期化
                timeline.TimelineBaseReturn();
            }
        }

        /// <summary>
        /// オーディオファイルの読み込み
        /// </summary>
        /// <param name="btn"></param>
        private void Click_AudioLoad(Button_Base btn)
        {
            menuManager.PlayOneShot(SoundType.BTN_CLICK);

            //読み込みボタン
            if (btn == btnS_AudioLoad[0])
            {
                //重複防止で無効化しておく
                btnS_AudioLoad[0].gameObject.SetActive(false);
                var text = btnS_AudioLoad[1].transform.GetChild(0).GetChild(0).GetComponent<TextMesh>();
                if (SystemInfo.userProfile.data.LanguageCode == (int)USE_LANGUAGE.JP)
                {
                    text.text = $"{fileAccess.GetAudioFileCount()}件あります、よろしいですか?";
                }
                else
                {
                    text.text = $"There are {fileAccess.GetAudioFileCount()} files.Is it OK ? ";
                }
                //確認ボタンを有効化
                btnS_AudioLoad[1].gameObject.SetActive(true);

                //受付時間後にリセット
                StartCoroutine(ReceptionTime(5.0f));
            }
            //最終確認ボタン
            else if (btn == btnS_AudioLoad[1])
            {
                //コルーチンを止める
                StopCoroutine(ReceptionTime(0));
                //重複防止で無効化しておく
                btnS_AudioLoad[1].gameObject.SetActive(false);
                //読み込む
                StartCoroutine(LoadCheck());
            }
        }

        private IEnumerator ReceptionTime(float wait)
        {
            yield return new WaitForSeconds(wait);
            btnS_AudioLoad[1].gameObject.SetActive(false);
            btnS_AudioLoad[0].gameObject.SetActive(true);
        }

        private IEnumerator LoadCheck()
        {
            int moveIndex = fileAccess.presetCount - fileAccess.CurrentAudio;

            //完了街ち
            yield return StartCoroutine(fileAccess.AudioLoad());

            //読み込みの音楽の先頭にカレントを移動
            ChangeAuido(moveIndex);
        }

        /// <summary>
        /// 次オーディオに切り替える
        /// </summary>
        /// <param name="btn"></param>
        private void MoveIndex_Auido(Button_Base btn)
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
        private void ChangeAuido(int moveIndex)
        {
            //文字画像を差し替える
            textMeshs[0].text = timeline.NextAudioClip(moveIndex);
            //サイズ調整
            textMeshs[0].fontSize = textMeshs[0].text.FontSizeMatch(600, 30, 50);
            //オーディオの長さ
            float sec = timeline.GetNowAudioLength();
            slider_Playback.maxValuel = sec;
            textMeshs[2].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
        }

        private void Director_Played(PlayableDirector obj)
        {
            //停止表示
            //btnS_Stop.gameObject.SetActive(true);
            //btnS_Play.gameObject.SetActive(false);
        }
        private void Director_Stoped(PlayableDirector obj)
        {
            //再生途中の一時停止は無視する
            if (timeline.AudioClip_PlaybackTime <= 0)
            {
                //再生表示
                if (btnS_Stop) btnS_Stop.gameObject.SetActive(false);
                if (btnS_Play) btnS_Play.gameObject.SetActive(true);
            }
        }

        private void ManualStart()
        {
            //マニュアルモードにする
            timeline.TimelineManualMode();

            //ボタンの状態を制御
            btnS_Stop.gameObject.SetActive(false);
            btnS_Play.gameObject.SetActive(true);
        }

        private void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I)) ChangeAuido(1);
            if (Input.GetKeyDown(KeyCode.K))
            {
                //タイムライン・再生
                timeline.TimelinePlay();
                //再生・停止ボタンの状態更新
                btnS_Stop.gameObject.SetActive(true);
                btnS_Play.gameObject.SetActive(false);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                //読み込む
                StartCoroutine(LoadCheck());
            }
        }
    }

}
