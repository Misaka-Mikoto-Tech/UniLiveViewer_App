using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UniLiveViewer
{
    public class TimelineController : MonoBehaviour
    {
        public string PortalBaseAniTrack => sPortalBaseAniTrack;
        const string sPortalBaseAniTrack = "Animation Track_Portal";
        Dictionary<string, int> _map = new Dictionary<string, int>
        {
            {sPortalBaseAniTrack,0},
            {"Animation Track1",1},
            {"Animation Track2",2},
            {"Animation Track3",3},
            {"Animation Track4",4},
            {"Animation Track5",5},
        };

        const string assetName_MainAudio = "Main Audio";
        readonly string[] AUDIOTRACK = { "Audio Track 1", "Audio Track 2", "Audio Track 3", "Audio Track 4" };

        const string SUBTRACK0 = "Override 0";  //HandL
        const string SUBTRACK1 = "Override 1";  //HandR
        const string SUBTRACK2 = "Override 2";  //Face
        const string SUBTRACK3 = "Override 3";  //Lip

        const string MAINCLIP = "DanceBase";
        const string SUBCLIP0 = "HandExpression";
        const string SUBCLIP1 = "HandExpression";
        const string SUBCLIP2 = "FaceClip";
        const string SUBCLIP3 = "LipClip";

        /// <summary>
        /// ポータルキャラのindex
        /// </summary>
        public static int PORTAL_INDEX = 0;

        PlayableDirector _playableDirector;
        TimelineAsset timeLineAsset;//タイムラインアセットにアクセス用

        /// <summary>
        /// トラックにバインドされているキャラ
        /// </summary>
        [SerializeField] CharaController[] _bindCharacters = new CharaController[6];
        /// <summary>
        /// トラックから指定キャラを取得
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CharaController GetCharacter(int index)
        {
            return _bindCharacters[index];
        }
        /// <summary>
        /// timeline上でのキャラ上限、今6決め打ち
        /// </summary>
        /// <returns></returns>
        public int CharacterCount()
        {
            return _bindCharacters.Length;
        }
        /// <summary>
        /// フィールドの現在キャラ数
        /// </summary>
        public int FieldCharaCount => _fieldCharaCount;
        int _fieldCharaCount = 1;
        /// <summary>
        /// フィールドに存在できる最大キャラ数
        /// </summary>
        public int MaxFieldChara => _maxFieldChara;
        int _maxFieldChara = 1;
        /// <summary>
        /// ポータル枠にキャラが存在するか
        /// </summary>
        /// <returns></returns>
        public bool IsPortalChara() { return _bindCharacters[PORTAL_INDEX]; }

        public event Action FieldCharaAdded;//設置キャラ数の更新時
        public event Action FieldCharaDeleted;//設置キャラ数の更新時

        AudioAssetManager _audioAssetManager;
        [SerializeField] AnimationClip _grabHandAnime;
        
        [SerializeField] double AudioClip_StartTime = 0;//セットされたaudioクリップの開始再生位置
        double motionClip_StartTime = 3;//モーションクリップの開始再生位置(デフォルト)
        CancellationToken cancellation_Token;

        [Header("確認用露出(readonly)")]
        [SerializeField] float _timelineSpeed = 1.0f;
        [SerializeField] double _PlaybackTime = 0.0f;
        public float TimelineSpeed
        {
            get { return _timelineSpeed; }
            set
            {
                _timelineSpeed = Mathf.Clamp(value, 0.0f, 3.0f);
                _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_timelineSpeed);
            }
        }

        //AudioClip基準の再生時間を算出
        public double AudioClip_PlaybackTime
        {
            get
            {
                _PlaybackTime = _playableDirector.time - AudioClip_StartTime;//参考用
                return _PlaybackTime;
            }
            set
            {
                _PlaybackTime = value;
                if (_PlaybackTime > _playableDirector.duration) _PlaybackTime = _playableDirector.duration;
                _playableDirector.time = AudioClip_StartTime + _PlaybackTime;//タイムラインに反映
            }
        }

        void Awake()
        {
            _playableDirector = GetComponent<PlayableDirector>();
            if (timeLineAsset == null) timeLineAsset = _playableDirector.playableAsset as TimelineAsset;
            var appConfig = GameObject.FindGameObjectWithTag("AppConfig").transform;
            _audioAssetManager = appConfig.GetComponent<AudioAssetManager>();

            cancellation_Token = this.GetCancellationTokenOnDestroy();
        }

        void Start()
        {
            // タイムライン内のトラック一覧を取得
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //メインオーディオのTrackAssetを取得
            TrackAsset track = tracks.FirstOrDefault(x => x.name == assetName_MainAudio);

            if (track)
            {
                //トラック内のクリップを全取得
                IEnumerable<TimelineClip> clips = track.GetClips();
                // 指定名称のクリップを抜き出す
                TimelineClip danceClip = clips.FirstOrDefault(x => x.displayName == "Main Audio Clip");
                //開始位置を取得
                danceClip.start = motionClip_StartTime + 2;
                AudioClip_StartTime = danceClip.start;
            }
            else
            {
                Debug.Log("メインオーディオが見つかりません");
            }

            NextAudioClip(true,0).Forget();

            //開幕は停止しておく
            TimelineBaseReturn();

            byte current = (byte)SystemInfo.sceneMode;
#if UNITY_EDITOR
            _maxFieldChara = SystemInfo.MAXCHARA_EDITOR[current];
#elif UNITY_ANDROID
            if (UnityEngine.SystemInfo.deviceName == "Oculus Quest 2") _maxFieldChara = SystemInfo.MAXCHARA_QUEST2[current];
            else if (UnityEngine.SystemInfo.deviceName == "Oculus Quest") _maxFieldChara = SystemInfo.MAXCHARA_QUEST1[current];
#endif
        }

        public void DestoryPortalChara()
        {
            //既存キャラがいれば削除しておく
            if (_bindCharacters[PORTAL_INDEX])
            {
                Destroy(_bindCharacters[PORTAL_INDEX].gameObject);
                _bindCharacters[PORTAL_INDEX] = null;
            }
        }

        /// <summary>
        /// 顔モーフの有効無効を切り替える
        /// </summary>
        /// <param name="isFace">表情か口パクか</param>
        /// <param name="isEnable"></param>
        public void SetMouthUpdate_Portal(bool isFace, bool isEnable)
        {
            var bindChara = _bindCharacters[PORTAL_INDEX];
            if (!bindChara) return;

            var vmdPlayer = _bindCharacters[PORTAL_INDEX].GetComponent<VMDPlayer_Custom>();
            if (bindChara.charaInfoData.formatType != CharaInfoData.FORMATTYPE.VRM) return;

            //VMD再生中
            if (vmdPlayer.morphPlayer_vrm != null)
            {
                //表情
                if (isFace)
                {
                    vmdPlayer.morphPlayer_vrm.isUpdateFace = isEnable;
                    if (!isEnable) bindChara.FacialSync.MorphReset();
                }
                //口パク
                else
                {
                    vmdPlayer.morphPlayer_vrm.isUpdateMouth = isEnable;
                    if (!isEnable) bindChara.LipSync.MorphReset();
                }
            }
            //プリセット中
            else
            {
                //表情
                if (isFace)
                {
                    if (!isEnable) bindChara.FacialSync.MorphReset();
                    bindChara.CanFacialSync = isEnable;
                }
                //口パク
                else
                {
                    if (!isEnable) bindChara.LipSync.MorphReset();
                    bindChara.CanLipSync = isEnable;
                }
            }
        }

        public void ClearPortal()
        {
            IEnumerable<PlayableBinding> outputs = _playableDirector.playableAsset.outputs;
            //ポータル用BaseAnimeのPlayableBindingを取得
            PlayableBinding Asset_BaseAnime = outputs.FirstOrDefault(x => x.streamName == sPortalBaseAniTrack);
            //bindを解除
            _playableDirector.SetGenericBinding(Asset_BaseAnime.sourceObject, null);
            _bindCharacters[PORTAL_INDEX] = null;
        }

        /// <summary>
        /// 新規キャラをポータル枠にバインドする
        /// </summary>
        /// <param name="bindObject"></param>
        /// <returns></returns>
        public bool NewAssetBinding_Portal(CharaController bindChara)
        {
            if (!bindChara) return false;//失敗(nullバインドの必要なし)

            IEnumerable<PlayableBinding> outputs = _playableDirector.playableAsset.outputs;
            //ポータル用BaseAnimeのPlayableBindingを取得
            PlayableBinding Asset_BaseAnime = outputs.FirstOrDefault(x => x.streamName == sPortalBaseAniTrack);

            if (Asset_BaseAnime.streamName != "")
            {
                //オブジェクトをバインドする
                _playableDirector.SetGenericBinding(Asset_BaseAnime.sourceObject, bindChara.gameObject);
                //CharaListにセット
                _bindCharacters[PORTAL_INDEX] = bindChara;
                //バインド情報を付与
                bindChara.BindTrackName = sPortalBaseAniTrack;
                //chara.bindTrackName_LipSync = "LipSync Track_Portal";
            }
            else
            {
                Debug.Log("システム設定エラー、キャラ登録枠が見つかりません。PlayableBinding名を見直してください");
                return false;
            }

            //マニュアル状態なら
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //アニメーターコントローラーを解除しておく
                RemoveCharasAniCon().Forget();
            }

            return true;//成功
        }

        /// <summary>
        /// 新規アニメーションをポータル枠にバインドする
        /// </summary>
        /// <param name="baseAniTrackName"></param>
        /// <param name="baseAniClip"></param>
        /// <param name="overrideAniClips"></param>
        /// <param name="initPos"></param>
        /// <param name="initEulerAngles"></param>
        public void SetAnimationClip(string baseAniTrackName, DanceInfoData danceInfoData)
        {
            // タイムライン内のトラック一覧を取得
            if (timeLineAsset == null) timeLineAsset = _playableDirector.playableAsset as TimelineAsset;
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //BaseAnimeのTrackAssetを取得
            TrackAsset track = tracks.FirstOrDefault(x => x.name == baseAniTrackName);

            if (track)
            {
                //トラック内のクリップを全取得
                IEnumerable<TimelineClip> clips = track.GetClips();
                // 指定名称のクリップを抜き出す
                TimelineClip danceClip = clips.FirstOrDefault(x => x.displayName == MAINCLIP);
                danceClip.start = motionClip_StartTime + danceInfoData.motionOffsetTime;

                //登録する
                //danceClip.asset = baseAniClip; これダメ
                AnimationPlayableAsset animationPlayableAsset = danceClip.asset as AnimationPlayableAsset;
                if (!danceInfoData.isReverse) animationPlayableAsset.clip = danceInfoData.baseDanceClip;
                else animationPlayableAsset.clip = danceInfoData.baseDanceClip_reverse;

                //animationPlayableAsset.position = initPos;
                //animationPlayableAsset.rotation = Quaternion.Euler(initEulerAngles);

                //(danceClip.asset as AnimationPlayableAsset).clip = animationClip;

                //オーバーライドアニメーションを登録する
                SetAnimationClip_Override(track, danceInfoData);

                //反映の為にディレクターをリスタートする
                TimeLineReStart();
            }
        }

        /// <summary>
        /// 上書きするアニメーションを順番に登録する
        /// </summary>
        /// <param name="parentTrack">ベースになるTrack</param>
        /// <param name="overrideAniClips">上書きしたいアニメーション</param>
        void SetAnimationClip_Override(TrackAsset parentTrack, DanceInfoData danceClipInfo)
        {
            TimelineClip handClip;

            //上書きするトラックを処理する
            foreach (var subTrack in parentTrack.GetChildTracks())
            {
                // トラック内のクリップ一覧を取得
                IEnumerable<TimelineClip> clips = subTrack.GetClips();

                switch (subTrack.name)
                {
                    case SUBTRACK0:
                        // 指定名称のクリップを抜き出す
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP0);

                        //キャラが既に握りなら
                        if (_bindCharacters[PORTAL_INDEX] && _bindCharacters[PORTAL_INDEX].CachedClip_handL)
                        {

                        }
                        else
                        {
                            //登録する
                            if (!danceClipInfo.isReverse) (handClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_hand;
                            else (handClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_reverseHand;
                            handClip.start = motionClip_StartTime + danceClipInfo.motionOffsetTime;
                        }

                        break;
                    case SUBTRACK1:
                        // 指定名称のクリップを抜き出す
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP1);

                        //キャラが既に握りなら
                        if (_bindCharacters[PORTAL_INDEX] && _bindCharacters[PORTAL_INDEX].CachedClip_handR)
                        {

                        }
                        else
                        {
                            //登録する
                            if (!danceClipInfo.isReverse) (handClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_hand;
                            else (handClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_reverseHand;
                            handClip.start = motionClip_StartTime + danceClipInfo.motionOffsetTime;
                        }

                        break;
                    case SUBTRACK2:
                        // 指定名称のクリップを抜き出す
                        TimelineClip faceClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP2);

                        //登録する
                        (faceClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_face;
                        faceClip.start = motionClip_StartTime + danceClipInfo.motionOffsetTime;
                        break;
                    case SUBTRACK3:
                        // 指定名称のクリップを抜き出す
                        TimelineClip lipClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP3);

                        //登録する
                        (lipClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_lip;
                        lipClip.start = motionClip_StartTime + danceClipInfo.motionOffsetTime;
                        break;
                }
            }
        }

        /// <summary>
        /// 指定CurrentのBGMをセットする
        /// </summary>
        public async UniTask<string> NextAudioClip(bool isPreset, int moveCurrent)
        {
            //クリップ決定
            AudioClip newAudioClip = await _audioAssetManager.GetAudioClips(isPreset, moveCurrent);

            // タイムライン内のトラック一覧を取得
            if (timeLineAsset == null) timeLineAsset = _playableDirector.playableAsset as TimelineAsset;
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //audioのTrackAssetを取得
            TrackAsset track = tracks.FirstOrDefault(x => x.name == assetName_MainAudio);

            if (track)
            {
                //トラック内のクリップを全取得
                IEnumerable<TimelineClip> clips = track.GetClips();

                // 指定名称のクリップを抜き出す
                TimelineClip oldAudioClip = clips.FirstOrDefault(x => x.displayName != "");
                oldAudioClip.duration = AudioClip_StartTime + newAudioClip.length;//秒

                //位置を調整
                //oldAudioClip.start = dlayTime;

                //更新
                //AudioClip_StartTime = oldAudioClip.start;

                //登録する
                (oldAudioClip.asset as AudioPlayableAsset).clip = newAudioClip;

                //スペクトル用
                if (SystemInfo.sceneMode == SceneMode.CANDY_LIVE)
                {
                    if (newAudioClip.name.Contains(".mp3") || newAudioClip.name.Contains(".wav"))
                    {
                        //ランタイム上手くいかなかった
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            track = tracks.FirstOrDefault(x => x.name == AUDIOTRACK[i]);
                            clips = track.GetClips();
                            oldAudioClip = clips.FirstOrDefault(x => x.displayName != "");
                            oldAudioClip.duration = AudioClip_StartTime + newAudioClip.length;//秒
                            (oldAudioClip.asset as AudioPlayableAsset).clip = newAudioClip;
                        }
                    }
                }

                //反映の為にディレクターをリスタートする
                TimeLineReStart();
            }

            return newAudioClip.name;
        }

        public async UniTask<float> GetNowAudioLength(bool isPreset)
        {
            return (await _audioAssetManager.GetCurrentAudioClip(isPreset)).length;
        }

        /// <summary>
        /// 指定キャラの手の状態を切り替える
        /// </summary>
        /// <param name="charaCon"></param>
        /// <param name="isGrabHand">握り状態にするか</param>
        public void SwitchHandType(CharaController charaCon, bool isGrabHand, bool isLeft)
        {
            //重複排除
            if (isLeft)
            {
                if (!isGrabHand && !charaCon.CachedClip_handL) return;
                else if (isGrabHand && charaCon.CachedClip_handL) return;
            }
            else
            {
                if (!isGrabHand && !charaCon.CachedClip_handR) return;
                else if (isGrabHand && charaCon.CachedClip_handR) return;
            }

            // タイムライン内のトラック一覧を取得
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //対象のキャラTrackAssetを取得
            TrackAsset track = tracks.FirstOrDefault(x => x.name == charaCon.BindTrackName);
            if (!track) return;


            if (isLeft)
            {
                //サブトラック
                var subTrack = track.GetChildTracks().FirstOrDefault(x => x.name == SUBTRACK0);
                //指定クリップ
                TimelineClip handClip = subTrack.GetClips().FirstOrDefault(x => x.displayName == SUBCLIP0);
                //握る
                if (isGrabHand)
                {
                    charaCon.CachedClip_handL = (handClip.asset as AnimationPlayableAsset).clip;
                    (handClip.asset as AnimationPlayableAsset).clip = _grabHandAnime;
                }
                //解除する
                else
                {
                    (handClip.asset as AnimationPlayableAsset).clip = charaCon.CachedClip_handL;
                    charaCon.CachedClip_handL = null;
                }
            }
            else
            {
                //サブトラック
                var subTrack = track.GetChildTracks().FirstOrDefault(x => x.name == SUBTRACK1);
                //指定クリップ
                TimelineClip handClip = subTrack.GetClips().FirstOrDefault(x => x.displayName == SUBCLIP1);
                //握る
                if (isGrabHand)
                {
                    charaCon.CachedClip_handR = (handClip.asset as AnimationPlayableAsset).clip;
                    (handClip.asset as AnimationPlayableAsset).clip = _grabHandAnime;
                }
                //解除する
                else
                {
                    (handClip.asset as AnimationPlayableAsset).clip = charaCon.CachedClip_handR;
                    charaCon.CachedClip_handR = null;
                }
            }

            //反映の為にディレクターをリスタートする
            TimeLineReStart();
        }

        /// <summary>
        /// バインドキャラを指定移行先にバインドする
        /// </summary>
        /// <param name="transferChara"></param>
        /// <param name="toTrackName"></param>
        /// <param name="initPos"></param>
        /// <param name="initEulerAngles"></param>
        /// <returns></returns>
        public bool TransferPlayableAsset(CharaController transferChara, string toTrackName, Vector3 initPos, Vector3 initEulerAngles)
        {
            DanceInfoData danceInfoData = new DanceInfoData();

            // タイムライン内のトラック一覧を取得
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //転送するキャラのTrackAssetを取得
            TrackAsset track = tracks.FirstOrDefault(x => x.name == transferChara.BindTrackName);
            if (!track) return false;

            //トラック内のクリップを全取得
            IEnumerable<TimelineClip> clips = track.GetClips();
            // 指定名称のクリップを抜き出す
            TimelineClip danceClip = clips.FirstOrDefault(x => x.displayName == MAINCLIP);
            danceInfoData.motionOffsetTime = (float)(danceClip.start - motionClip_StartTime);

            //DanceBaseのアニメーションを取得
            danceInfoData.baseDanceClip = (danceClip.asset as AnimationPlayableAsset).clip;

            TimelineClip handClip;

            //オーバーライドアニメーションを取得
            foreach (var subTrack in track.GetChildTracks())
            {
                // トラック内のクリップ一覧を取得
                clips = subTrack.GetClips();

                switch (subTrack.name)
                {
                    case SUBTRACK0:
                        // 指定名称のクリップを抜き出す
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP0);
                        danceInfoData.overrideClip_hand = (handClip.asset as AnimationPlayableAsset).clip;
                        break;
                    case SUBTRACK1:
                        // 指定名称のクリップを抜き出す
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP1);
                        danceInfoData.overrideClip_hand = (handClip.asset as AnimationPlayableAsset).clip;
                        break;
                    case SUBTRACK2:
                        // 指定名称のクリップを抜き出す
                        TimelineClip FaceClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP2);
                        danceInfoData.overrideClip_face = (FaceClip.asset as AnimationPlayableAsset).clip;
                        break;
                    case SUBTRACK3:
                        // 指定名称のクリップを抜き出す
                        TimelineClip lipClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP3);
                        danceInfoData.overrideClip_lip = (lipClip.asset as AnimationPlayableAsset).clip;
                        break;
                }
            }

            //表情系をリセットしておく
            _bindCharacters[PORTAL_INDEX].LipSync.MorphReset();
            _bindCharacters[PORTAL_INDEX].LipSync.MorphReset();

            //##### ここから転送先処理 #####
            IEnumerable<PlayableBinding> outputs = _playableDirector.playableAsset.outputs;
            //移行元のPlayableBindingをnullバインドで解除しておく
            PlayableBinding fromBaseAnime = outputs.FirstOrDefault(x => x.streamName == transferChara.BindTrackName);
            _playableDirector.SetGenericBinding(fromBaseAnime.sourceObject, null);

            //移行先の既存キャラ確認
            for (int i = 0; i < _bindCharacters.Length; i++)
            {
                if (_bindCharacters[i])
                {
                    if (_bindCharacters[i].BindTrackName == transferChara.BindTrackName)
                    {
                        //転送元のリンクを解除
                        _bindCharacters[i] = null;
                        break;
                    }
                }
            }

            //移行先のPlayableBindingを取得
            PlayableBinding toBaseAnime = outputs.FirstOrDefault(x => x.streamName == toTrackName);
            if (toBaseAnime.streamName == "") return false;

            //オブジェクトを移行先にバインドする
            _playableDirector.SetGenericBinding(toBaseAnime.sourceObject, transferChara.gameObject);

            //バインド情報を付与
            transferChara.BindTrackName = toTrackName;

            //リストに登録
            var index = _map[toTrackName];
            _bindCharacters[index] = transferChara;

            //アニメーションを移行(取得した転送元アニメーションで新規登録)
            //SetAnimationClip(toTrackName, danceInfoData, initPos, initEulerAngles);
            SetAnimationClip(toTrackName, danceInfoData);

            //RootMotionの解除
            //掴んで自由に移動させる為に必要だったが、設置後は移動モーションにカクツキが生じてしまうため解除
            //設置座標設定後に解除しないと位置が反映されないので注意(またこの変更はアニメーターの再初期化が走る)
            transferChara.GetComponent<Animator>().applyRootMotion = false;

            //座標系はRootMotion変更後
            transferChara.transform.position = initPos;
            transferChara.transform.rotation = Quaternion.Euler(initEulerAngles);

            //フィールドカウンター
            _fieldCharaCount++;
            FieldCharaAdded?.Invoke();

            Debug.Log("転送成功");

            return true;
        }

        /// <summary>
        /// トラックバインドキャラを削除する
        /// </summary>
        /// <param name="chara"></param>
        public void DeleteBindAsset(CharaController chara)
        {
            if (!chara) return;

            for (int i = 0; i < _bindCharacters.Length; i++)
            {
                if (chara == _bindCharacters[i])
                {
                    _bindCharacters[i] = null;
                    break;
                }
            }
            Destroy(chara.gameObject);

            //フィールドカウンター
            _fieldCharaCount--;
            FieldCharaDeleted?.Invoke();
        }

        /// <summary>
        /// ID一致でバインドアセットを削除
        /// </summary>
        /// <param name="chara"></param>
        public void DeletebindAsset_CleanUp(int _id)
        {
            for (int i = 0; i < _bindCharacters.Length; i++)
            {
                if (_bindCharacters[i] && _id == _bindCharacters[i].charaInfoData.vrmID)
                {
                    Destroy(_bindCharacters[i].gameObject);
                    _bindCharacters[i] = null;

                    //フィールドカウンター
                    if (i != PORTAL_INDEX) _fieldCharaCount--;
                }
            }
            FieldCharaDeleted?.Invoke();
        }

        /// <summary>
        /// Field上のラックバインドキャラを削除する
        /// </summary>
        public void DeletebindAsset_FieldAll()
        {
            for (int i = 0; i < _bindCharacters.Length; i++)
            {
                if (i == PORTAL_INDEX) continue;
                if (_bindCharacters[i])
                {
                    Destroy(_bindCharacters[i].gameObject);
                    _bindCharacters[i] = null;
                }
            }
            //フィールドカウンター
            _fieldCharaCount = 0;
            FieldCharaDeleted?.Invoke();
        }

        /// <summary>
        /// 空いているトラックを探す
        /// </summary>
        /// <returns></returns>
        public bool isFreeTrack(out string freeTrack)
        {
            freeTrack = "";
            for (int i = 1; i < _bindCharacters.Length; i++)
            {
                if (_bindCharacters[i]) continue;
                freeTrack = _map.FirstOrDefault(x => x.Value == i).Key;
                return true;
            }
            return false;
        }

        /// <summary>
        /// タイムラインの変更内容を強制的?に反映させる
        /// AnimationClip変更だけ反映されないためリスタートが必要
        /// ・・・ランタイムは無理?って見かけたけど試してみたらいけたっていう
        /// </summary>
        void TimeLineReStart()
        {
            //再生時間の記録
            double keepTime = _playableDirector.time;
            //初期化して入れ直し(これでいけちゃう謎)
            _playableDirector.playableAsset = null;
            _playableDirector.playableAsset = timeLineAsset;

            //前回の続きを指定
            _playableDirector.time = keepTime;

            ////Track情報を更新する
            //TrackList_Update();

            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.GameTime)
            {
                //再生
                _playableDirector.Play();

                //速度更新(Play後は再度呼び出さないとダメみたい)
                _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_timelineSpeed);

                //速度更新
                //TimelineSpeedUpdate();
            }
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //更新
                _playableDirector.Evaluate();
            }
        }

        /// <summary>
        /// 再生状態にする
        /// </summary>
        public void TimelinePlay()
        {
            //表情系をリセットしておく
            foreach (var chara in _bindCharacters)
            {
                if (!chara) continue;
                chara.FacialSync.MorphReset();
                chara.LipSync.MorphReset();
            }

            //モードをマニュアルからゲームタイマーへ
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                _playableDirector.timeUpdateMode = DirectorUpdateMode.GameTime;
            }

            //再開させる
            _playableDirector.Play();

            //速度更新(Play後は再度呼び出さないとダメみたい)
            _playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_timelineSpeed);

            //速度更新
            //TimelineSpeedUpdate();
        }

        /// <summary>
        /// マニュアル状態にする
        /// </summary>
        public async UniTask TimelineManualMode()
        {
            //マニュアルモードに
            _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;

            //AnimatorControllerを解除しておく
            await RemoveCharasAniCon();

            //マニュアルモードでの更新を開始
            ManualUpdate().Forget();

            //_playableDirector.Pause();
            //_playableDirector.Resume();
        }

        /// <summary>
        /// 再生位置を初期化する
        /// </summary>
        public void TimelineBaseReturn()
        {
            TimelineManualMode().Forget();//マニュアルモード
            _playableDirector.Stop();//停止状態にする(UIにトリガーを送る為)
            AudioClip_PlaybackTime = 0;
        }

        /// <summary>
        /// 一定間隔でマニュアルモードで更新を行う
        /// </summary>
        /// <returns></returns>
        async UniTask ManualUpdate()
        {
            double keepVal = AudioClip_PlaybackTime;

            while (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //更新されているか
                if (keepVal != AudioClip_PlaybackTime)
                {
                    //状態を反映させる
                    _playableDirector.Evaluate();

                    //キープの更新
                    keepVal = AudioClip_PlaybackTime;
                }
                await UniTask.Delay(100,cancellationToken: cancellation_Token);
            }

            //AnimatorControllerを戻す
            //Manual状態で戻すと一瞬初期座標に移動してチラついてしまう為、このタイミングで実行
            for (int i = 0; i < _bindCharacters.Length; i++)
            {
                if (!_bindCharacters[i]) continue;
                _bindCharacters[i].ReturnRunAnime();
            }
        }

        /// <summary>
        /// キャラ側アタッチポイントの有効状態を切り替える
        /// </summary>
        /// <param name="isActive"></param>
        public void SetActive_AttachPoint(bool isActive)
        {
            //マニュアル状態のみ
            if (_playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) return;

            foreach (var chara in _bindCharacters)
            {
                if (chara == null) continue;
                chara.GetComponent<AttachPointGenerator>().SetActive_AttachPoint(isActive);
            }
        }

        /// <summary>
        /// キャラのAnimatorControllerを解除する(timelineのanimatorと競合するため)
        /// </summary>
        async UniTask RemoveCharasAniCon()
        {
            //マニュアルモードでなければ処理しない
            if (_playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) return;

            await UniTask.Yield(PlayerLoopTiming.Update, cancellation_Token);//必要、VRMのAwakeが間に合わない

            //TimeLineと競合っぽいのでAnimatorControllerを解除しておく 
            for (int i = 0; i < _bindCharacters.Length; i++)
            {
                if (!_bindCharacters[i]) continue;
                _bindCharacters[i].RemoveRunAnime();
            }

            //ワンフレーム後にアニメーションの状態を1回だけ更新
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation_Token);
            _playableDirector.Evaluate();
        }
    }
}