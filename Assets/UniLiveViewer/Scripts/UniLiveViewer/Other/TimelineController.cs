using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UniLiveViewer
{
    public class TimelineController : MonoBehaviour
    {
        public readonly string sPortalBaseAniTrack = "Animation Track_Portal";
        private const string ANITRACK1 = "Animation Track1";
        private const string ANITRACK2 = "Animation Track2";
        private const string ANITRACK3 = "Animation Track3";
        private const string ANITRACK4 = "Animation Track4";
        private const string ANITRACK5 = "Animation Track5";

        private const string assetName_MainAudio = "Main Audio";
        private readonly string[] AUDIOTRACK = { "Audio Track 1", "Audio Track 2", "Audio Track 3", "Audio Track 4" };

        private const string SUBTRACK0 = "Override 0";
        private const string SUBTRACK1 = "Override 1";
        private const string SUBTRACK2 = "Override 2";
        private const string SUBTRACK3 = "Override 3";

        private const string MAINCLIP = "DanceBase";
        private const string SUBCLIP0 = "HandExpression";
        private const string SUBCLIP1 = "HandExpression";
        private const string SUBCLIP2 = "FaceClip";
        private const string SUBCLIP3 = "LipClip";

        //ポータルキャラの確認
        public static int PORTAL_ELEMENT = 0;

        //タイムライン
        public PlayableDirector playableDirector; //ディレクタ
        private TimelineAsset timeLineAsset;//タイムラインアセットにアクセス用

        //バインドキャラを管理するクラス
        public CharaController[] trackBindChara = new CharaController[6];

        public int FieldCharaCount { get; private set; } = 0;//フィールドのキャラカウント
        public event Action FieldCharaAdded;//設置キャラ数の更新時
        public event Action FieldCharaDeleted;//設置キャラ数の更新時
        public int maxFieldChara = 1;//最大召喚数

        public bool isPortalChara() { return trackBindChara[PORTAL_ELEMENT]; }
        public FileAccessManager fileManager = null;
        [SerializeField] private AnimationClip grabHandAnime;
        
        public double AudioClip_StartTime = 0;//セットされたaudioクリップの開始再生位置
        private double motionClip_StartTime = 3;//モーションクリップの開始再生位置(デフォルト)
        private float _timelineSpeed = 1.0f;
        public float timelineSpeed
        {
            get
            {
                return _timelineSpeed;
            }
            set
            {
                _timelineSpeed = Mathf.Clamp(value, 0.0f, 3.0f);
                playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_timelineSpeed);
            }
        }

        public double AudioClip_PlaybackTime
        {
            get
            {
                //音楽クリップ内での再生時間を算出
                return playableDirector.time - AudioClip_StartTime;
            }
            set
            {
                //上限以上ならMaX値に丸める
                if (value > playableDirector.duration) value = playableDirector.duration;
                //タイムラインに反映
                playableDirector.time = AudioClip_StartTime + value;
            }
        }

        private void Awake()
        {
            if (timeLineAsset == null) timeLineAsset = playableDirector.playableAsset as TimelineAsset;
            fileManager = GameObject.FindGameObjectWithTag("AppConfig").GetComponent<FileAccessManager>();
        }

        private void Start()
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


            byte current = (byte)SystemInfo.sceneMode;
#if UNITY_EDITOR
            maxFieldChara = SystemInfo.MAXCHARA_EDITOR[current];
#elif UNITY_ANDROID
            if (UnityEngine.SystemInfo.deviceName == "Oculus Quest 2") maxFieldChara = SystemInfo.MAXCHARA_QUEST2[current];
            else if (UnityEngine.SystemInfo.deviceName == "Oculus Quest") maxFieldChara = SystemInfo.MAXCHARA_QUEST1[current];
#endif
        }

        private void Update()
        {
            //初期位置
            if (AudioClip_PlaybackTime < 0)
            {
                //マニュアルモード
                TimelineManualMode();

                //停止状態にする(UIにトリガーを送る為)
                playableDirector.Stop();

                //クリップ開始位置まで進める(重複予防)
                AudioClip_PlaybackTime = 0;
            }
        }

        public void DestoryPortalChara()
        {
            //既存キャラがいれば削除しておく
            if (trackBindChara[PORTAL_ELEMENT])
            {
                Destroy(trackBindChara[PORTAL_ELEMENT].gameObject);
                trackBindChara[PORTAL_ELEMENT] = null;
            }
        }

        /// <summary>
        /// 顔モーフの有効無効を切り替える
        /// </summary>
        /// <param name="isFace">表情か口パクか</param>
        /// <param name="isEnable"></param>
        public void SetMouthUpdate_Portal(bool isFace, bool isEnable)
        {
            var bindChara = trackBindChara[PORTAL_ELEMENT];

            if (bindChara)
            {
                var vmdPlayer = trackBindChara[PORTAL_ELEMENT].GetComponent<VMDPlayer_Custom>();

                if (bindChara.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
                {
                    //VMD再生中
                    if (vmdPlayer.morphPlayer_vrm != null)
                    {
                        //表情
                        if (isFace)
                        {
                            vmdPlayer.morphPlayer_vrm.isUpdateFace = isEnable;

                            //停止の場合は表情を初期化しておく
                            if (!isEnable) bindChara.facialSync.AllClear_BlendShape();
                        }
                        //口パク
                        else
                        {
                            vmdPlayer.morphPlayer_vrm.isUpdateMouth = isEnable;

                            //停止の場合は口を初期化しておく
                            if (!isEnable) bindChara.lipSync.AllClear_BlendShape();
                        }
                    }
                    //プリセット中
                    else
                    {
                        //表情
                        if (isFace)
                        {
                            //停止の場合は表情を初期化しておく
                            if (!isEnable) bindChara.facialSync.AllClear_BlendShape();
                            bindChara.facialSync.enabled = isEnable;
                        }
                        //口パク
                        else
                        {
                            //停止の場合は口を初期化しておく
                            if (!isEnable) bindChara.lipSync.AllClear_BlendShape();
                            bindChara.lipSync.enabled = isEnable;
                        }
                    }
                }
            }
        }

        public void ClearPortal()
        {
            IEnumerable<PlayableBinding> outputs = playableDirector.playableAsset.outputs;
            //ポータル用BaseAnimeのPlayableBindingを取得
            PlayableBinding Asset_BaseAnime = outputs.FirstOrDefault(x => x.streamName == sPortalBaseAniTrack);
            //bindを解除
            playableDirector.SetGenericBinding(Asset_BaseAnime.sourceObject, null);
            trackBindChara[PORTAL_ELEMENT] = null;
        }

        /// <summary>
        /// 新規キャラをポータル枠にバインドする
        /// </summary>
        /// <param name="bindObject"></param>
        /// <returns></returns>
        public bool NewAssetBinding_Portal(CharaController bindChara)
        {
            if (!bindChara) return false;//失敗(nullバインドの必要なし)

            IEnumerable<PlayableBinding> outputs = playableDirector.playableAsset.outputs;
            //ポータル用BaseAnimeのPlayableBindingを取得
            PlayableBinding Asset_BaseAnime = outputs.FirstOrDefault(x => x.streamName == sPortalBaseAniTrack);

            if (Asset_BaseAnime.streamName != "")
            {
                //オブジェクトをバインドする
                playableDirector.SetGenericBinding(Asset_BaseAnime.sourceObject, bindChara.gameObject);
                //CharaListにセット
                trackBindChara[PORTAL_ELEMENT] = bindChara;
                //バインド情報を付与
                bindChara.bindTrackName = sPortalBaseAniTrack;
                //chara.bindTrackName_LipSync = "LipSync Track_Portal";
            }
            else
            {
                Debug.Log("システム設定エラー、キャラ登録枠が見つかりません。PlayableBinding名を見直してください");
                return false;
            }

            //マニュアル状態なら
            if (playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //アニメーターコントローラーを解除しておく
                StartCoroutine(RemoveCharasAniCon());
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
        public void SetAnimationClip(string baseAniTrackName, DanceInfoData danceInfoData, Vector3 initPos, Vector3 initEulerAngles)
        {
            // タイムライン内のトラック一覧を取得
            if (timeLineAsset == null) timeLineAsset = playableDirector.playableAsset as TimelineAsset;
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
                animationPlayableAsset.position = initPos;
                animationPlayableAsset.rotation = Quaternion.Euler(initEulerAngles);
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
        private void SetAnimationClip_Override(TrackAsset parentTrack, DanceInfoData danceClipInfo)
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
                        if (trackBindChara[PORTAL_ELEMENT] && trackBindChara[PORTAL_ELEMENT].keepHandL_Anime)
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
                        if (trackBindChara[PORTAL_ELEMENT] && trackBindChara[PORTAL_ELEMENT].keepHandR_Anime)
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
        public string NextAudioClip(int moveCurrent)
        {
            fileManager.CurrentAudio += moveCurrent;

            //Current移動制限
            if (fileManager.CurrentAudio < 0) fileManager.CurrentAudio = fileManager.audioList.Count - 1;
            else if (fileManager.CurrentAudio >= fileManager.audioList.Count) fileManager.CurrentAudio = 0;
            //クリップ決定
            AudioClip newAudioClip = fileManager.audioList[fileManager.CurrentAudio];

            // タイムライン内のトラック一覧を取得
            if (timeLineAsset == null) timeLineAsset = playableDirector.playableAsset as TimelineAsset;
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //audioのTrackAssetを取得
            TrackAsset track = tracks.FirstOrDefault(x => x.name == assetName_MainAudio);

            if (track)
            {
                //トラック内のクリップを全取得
                IEnumerable<TimelineClip> clips = track.GetClips();

                // 指定名称のクリップを抜き出す
                TimelineClip oldAudioClip = clips.FirstOrDefault(x => x.displayName != "");
                oldAudioClip.duration = AudioClip_StartTime + fileManager.audioList[fileManager.CurrentAudio].length;//秒

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
                            oldAudioClip.duration = AudioClip_StartTime + fileManager.audioList[fileManager.CurrentAudio].length;//秒
                            (oldAudioClip.asset as AudioPlayableAsset).clip = newAudioClip;
                        }
                    }
                }

                //反映の為にディレクターをリスタートする
                TimeLineReStart();
            }

            return newAudioClip.name;
        }

        public void SetVMD_MotionOffset(string sName, int val)
        {
            if (sName.Contains(".vmd"))
            {
                SystemInfo.dicVMD_offset[sName] = val;
            }
        }

        public float GetNowAudioLength()
        {
            return fileManager.audioList[fileManager.CurrentAudio].length;
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
                if (!isGrabHand && !charaCon.keepHandL_Anime) return;
                else if (isGrabHand && charaCon.keepHandL_Anime) return;
            }
            else
            {
                if (!isGrabHand && !charaCon.keepHandR_Anime) return;
                else if (isGrabHand && charaCon.keepHandR_Anime) return;
            }

            // タイムライン内のトラック一覧を取得
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //対象のキャラTrackAssetを取得
            TrackAsset track = tracks.FirstOrDefault(x => x.name == charaCon.bindTrackName);
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
                    charaCon.keepHandL_Anime = (handClip.asset as AnimationPlayableAsset).clip;
                    (handClip.asset as AnimationPlayableAsset).clip = grabHandAnime;
                }
                //解除する
                else
                {
                    (handClip.asset as AnimationPlayableAsset).clip = charaCon.keepHandL_Anime;
                    charaCon.keepHandL_Anime = null;
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
                    charaCon.keepHandR_Anime = (handClip.asset as AnimationPlayableAsset).clip;
                    (handClip.asset as AnimationPlayableAsset).clip = grabHandAnime;
                }
                //解除する
                else
                {
                    (handClip.asset as AnimationPlayableAsset).clip = charaCon.keepHandR_Anime;
                    charaCon.keepHandR_Anime = null;
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
            TrackAsset track = tracks.FirstOrDefault(x => x.name == transferChara.bindTrackName);
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

            //##### ここから転送先処理 #####
            IEnumerable<PlayableBinding> outputs = playableDirector.playableAsset.outputs;
            //移行元のPlayableBindingをnullバインドで解除しておく
            PlayableBinding fromBaseAnime = outputs.FirstOrDefault(x => x.streamName == transferChara.bindTrackName);
            playableDirector.SetGenericBinding(fromBaseAnime.sourceObject, null);

            //移行先の既存キャラ確認
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (trackBindChara[i])
                {
                    if (trackBindChara[i].bindTrackName == transferChara.bindTrackName)
                    {
                        //転送元のリンクを解除
                        trackBindChara[i] = null;
                        break;
                    }
                }
            }

            //移行先のPlayableBindingを取得
            PlayableBinding toBaseAnime = outputs.FirstOrDefault(x => x.streamName == toTrackName);
            if (toBaseAnime.streamName == "") return false;

            //オブジェクトを移行先にバインドする
            playableDirector.SetGenericBinding(toBaseAnime.sourceObject, transferChara.gameObject);

            //バインド情報を付与
            transferChara.bindTrackName = toTrackName;
            //リストに登録
            switch (toTrackName)
            {
                case "Animation Track_Portal":
                    trackBindChara[0] = transferChara;
                    break;
                case ANITRACK1:
                    trackBindChara[1] = transferChara;
                    break;
                case ANITRACK2:
                    trackBindChara[2] = transferChara;
                    break;
                case ANITRACK3:
                    trackBindChara[3] = transferChara;
                    break;
                case ANITRACK4:
                    trackBindChara[4] = transferChara;
                    break;
                case ANITRACK5:
                    trackBindChara[5] = transferChara;
                    break;
            }

            //アニメーションを移行(取得した転送元アニメーションで新規登録)
            SetAnimationClip(toTrackName, danceInfoData, initPos, initEulerAngles);

            //RootMotionの解除
            //掴んで自由に移動させる為に必要だったが、設置後は移動モーションにカクツキが生じてしまうため解除
            //設置座標設定後に解除しないと位置が反映されないので注意(またこの変更はアニメーターの再初期化が走る)
            transferChara.GetComponent<Animator>().applyRootMotion = false;

            //表情系をリセットしておく
            transferChara.facialSync.AllClear_BlendShape();
            transferChara.lipSync.AllClear_BlendShape();

            //フィールドカウンター
            FieldCharaCount++;
            FieldCharaAdded?.Invoke();

            return true;
        }

        /// <summary>
        /// トラックバインドキャラを削除する
        /// </summary>
        /// <param name="chara"></param>
        public void DeletebindAsset(CharaController chara)
        {
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (chara == trackBindChara[i])
                {
                    trackBindChara[i] = null;
                    break;
                }
            }
            Destroy(chara.gameObject);

            //フィールドカウンター
            FieldCharaCount--;
            FieldCharaDeleted?.Invoke();
        }

        /// <summary>
        /// ID一致でバインドアセットを削除
        /// </summary>
        /// <param name="chara"></param>
        public void DeletebindAsset_CleanUp(int _id)
        {
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (trackBindChara[i] && _id == trackBindChara[i].charaInfoData.vrmID)
                {
                    Destroy(trackBindChara[i].gameObject);
                    trackBindChara[i] = null;

                    //フィールドカウンター
                    if (i != PORTAL_ELEMENT) FieldCharaCount--;
                }
            }
            FieldCharaDeleted?.Invoke();
        }

        /// <summary>
        /// Field上のラックバインドキャラを削除する
        /// </summary>
        public void DeletebindAsset_FieldAll()
        {
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (i == PORTAL_ELEMENT) continue;
                if (trackBindChara[i])
                {
                    Destroy(trackBindChara[i].gameObject);
                    trackBindChara[i] = null;
                }
            }
            //フィールドカウンター
            FieldCharaCount = 0;
            FieldCharaDeleted?.Invoke();
        }

        /// <summary>
        /// 空いているトラックを探す
        /// </summary>
        /// <returns></returns>
        public bool isFreeTrack(out string freeTrack)
        {
            freeTrack = "";
            for (int i = 1; i < trackBindChara.Length; i++)
            {
                if (trackBindChara[i]) continue;
                else if (i == 1)
                {
                    freeTrack = ANITRACK1;
                    return true;
                }
                else if (i == 2)
                {
                    freeTrack = ANITRACK2;
                    return true;
                }
                else if (i == 3)
                {
                    freeTrack = ANITRACK3;
                    return true;
                }
                else if (i == 4)
                {
                    freeTrack = ANITRACK4;
                    return true;
                }
                else if (i == 5)
                {
                    freeTrack = ANITRACK5;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// タイムラインの変更内容を強制的?に反映させる
        /// AnimationClip変更だけ反映されないためリスタートが必要
        /// ・・・ランタイムは無理?って見かけたけど試してみたらいけたっていう
        /// </summary>
        public void TimeLineReStart()
        {
            //再生時間の記録
            double keepTime = playableDirector.time;
            //初期化して入れ直し(これでいけちゃう謎)
            playableDirector.playableAsset = null;
            playableDirector.playableAsset = timeLineAsset;

            //前回の続きを指定
            playableDirector.time = keepTime;

            ////Track情報を更新する
            //TrackList_Update();

            if (playableDirector.timeUpdateMode == DirectorUpdateMode.GameTime)
            {
                //再生
                playableDirector.Play();

                //速度更新(Play後は再度呼び出さないとダメみたい)
                playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_timelineSpeed);

                //速度更新
                //TimelineSpeedUpdate();
            }
            if (playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //更新
                playableDirector.Evaluate();
            }
        }

        /// <summary>
        /// 再生状態にする
        /// </summary>
        public void TimelinePlay()
        {
            //表情系をリセットしておく
            foreach (var chara in trackBindChara)
            {
                if (!chara) continue;
                chara.facialSync.AllClear_BlendShape();
                chara.lipSync.AllClear_BlendShape();
            }

            //モードをマニュアルからゲームタイマーへ
            if (playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                playableDirector.timeUpdateMode = DirectorUpdateMode.GameTime;
            }

            //再開させる
            playableDirector.Play();

            //速度更新(Play後は再度呼び出さないとダメみたい)
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_timelineSpeed);

            //速度更新
            //TimelineSpeedUpdate();
        }

        /// <summary>
        /// マニュアル状態にする
        /// </summary>
        public void TimelineManualMode()
        {
            //マニュアルモードに
            playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;

            //AnimatorControllerを解除しておく
            StartCoroutine(RemoveCharasAniCon());

            //マニュアルモードでの更新を開始
            StartCoroutine(ManualUpdate());

            //playableDirector.Pause();
            //playableDirector.Resume();
        }

        public bool isManualMode()
        {
            return playableDirector.timeUpdateMode == DirectorUpdateMode.Manual;
        }

        /// <summary>
        /// 再生位置を初期化する
        /// </summary>
        public void TimelineBaseReturn()
        {
            playableDirector.time = 0;
            //TimelinePlay();
        }

        /// <summary>
        /// 一定間隔でマニュアルモードで更新を行う
        /// </summary>
        /// <returns></returns>
        private IEnumerator ManualUpdate()
        {
            double keepVal = AudioClip_PlaybackTime;
            //1回状態を反映させる
            playableDirector.Evaluate();
            yield return null;

            while (playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //更新されているか
                if (keepVal != AudioClip_PlaybackTime)
                {
                    //状態を反映させる
                    playableDirector.Evaluate();

                    //キープの更新
                    keepVal = AudioClip_PlaybackTime;
                }
                yield return new WaitForSeconds(0.1f);
            }

            //AnimatorControllerを戻す
            //Manual状態で戻すと一瞬初期座標に移動してチラついてしまう為、このタイミングで実行
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (!trackBindChara[i]) continue;
                trackBindChara[i].ReturnRunAnime();
            }
        }

        /// <summary>
        /// キャラ側アタッチポイントの有効状態を切り替える
        /// </summary>
        /// <param name="isActive"></param>
        public void SetActive_AttachPoint(bool isActive)
        {
            //マニュアル状態のみ
            if (playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) return;

            foreach (var chara in trackBindChara)
            {
                if (chara == null) continue;
                chara.GetComponent<AttachPointGenerator>().SetActive_AttachPoint(isActive);
            }
        }

        /// <summary>
        /// キャラのAnimatorControllerを解除する(timelineのanimatorと競合するため)
        /// </summary>
        private IEnumerator RemoveCharasAniCon()
        {
            //マニュアルモードでなければ処理しない
            if (playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) yield break;

            yield return null;//必要、VRMのAwakeが間に合わない

            //TimeLineと競合っぽいのでAnimatorControllerを解除しておく 
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (!trackBindChara[i]) continue;
                trackBindChara[i].RemoveRunAnime();
            }

            //ワンフレーム後にアニメーションの状態を1回だけ更新
            yield return null;
            playableDirector.Evaluate();
        }

        /// <summary>
        /// バインドキャラのガイドを一括で切り替える
        /// </summary>
        /// <param name="isEnable"></param>
        public void SetCharaMeshGuide(bool isEnable)
        {
            foreach (var chara in trackBindChara)
            {
                if (!chara) continue;
                if (chara != trackBindChara[PORTAL_ELEMENT])
                {
                    chara.GetComponent<MeshGuide>().SetGuide(isEnable);
                }
            }
        }
    }
}