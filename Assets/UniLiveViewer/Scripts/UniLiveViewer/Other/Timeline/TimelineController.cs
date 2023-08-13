using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UniLiveViewer
{
    // NOTE: まだカオス
    public class TimelineController : MonoBehaviour
    {
        /// <summary>
        /// ポータルキャラのindex
        /// </summary>
        public static int PORTAL_INDEX = 0;

        const string ANIMATION_TRACK_PORTAL = "Animation Track_Portal";

        Dictionary<int, string> _trackNameMap = new Dictionary<int, string>()
        {
            {0,ANIMATION_TRACK_PORTAL},
            {1,"Animation Track1"},
            {2,"Animation Track2"},
            {3,"Animation Track3"},
            {4,"Animation Track4"},
            {5,"Animation Track5"},
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

        PlayableDirector _playableDirector;
        TimelineAsset _timeLineAsset;//タイムラインアセットにアクセス用

        /// <summary>
        /// トラックにバインドされているキャラ
        /// TODO: 2重管理..うっ...改善したい
        /// </summary>
        public IReadOnlyDictionary<int, CharaController> BindCharaMap => _bindCharaMap;
        Dictionary<int, CharaController> _bindCharaMap = Enumerable.Range(0, 6).ToDictionary(i => i, i => (CharaController)null);

        public CharaController GetCharacterInPortal => _bindCharaMap[PORTAL_INDEX];

        public IReadOnlyReactiveProperty<int> FieldCharacterCount => _fieldCharacterCount;
        ReactiveProperty<int> _fieldCharacterCount = new ReactiveProperty<int>(0);

        AudioAssetManager _audioAssetManager;
        [SerializeField] AnimationClip _grabHandAnime;

        [SerializeField] double _audioClipStartTime = 0;//セットされたaudioクリップの開始再生位置
        double _motionClipStartTime = 3;//モーションクリップの開始再生位置(デフォルト)
        CancellationToken _cancellation;

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
                _PlaybackTime = _playableDirector.time - _audioClipStartTime;//参考用
                return _PlaybackTime;
            }
            set
            {
                _PlaybackTime = value;
                if (_PlaybackTime > _playableDirector.duration) _PlaybackTime = _playableDirector.duration;
                _playableDirector.time = _audioClipStartTime + _PlaybackTime;//タイムラインに反映
            }
        }

        public void OnStart(PlayableDirector playableDirector, AudioAssetManager audioAssetManager)
        {
            _playableDirector = playableDirector;
            _audioAssetManager = audioAssetManager;

            _cancellation = this.GetCancellationTokenOnDestroy();
            _timeLineAsset = _playableDirector.playableAsset as TimelineAsset;

            // タイムライン内のトラック一覧を取得
            var tracks = _timeLineAsset.GetOutputTracks();

            //メインオーディオのTrackAssetを取得
            var track = tracks.FirstOrDefault(x => x.name == assetName_MainAudio);

            if (track)
            {
                //トラック内のクリップを全取得
                var clips = track.GetClips();
                // 指定名称のクリップを抜き出す
                var danceClip = clips.FirstOrDefault(x => x.displayName == "Main Audio Clip");
                //開始位置を取得
                danceClip.start = _motionClipStartTime + 2;
                _audioClipStartTime = danceClip.start;
            }
            else
            {
                Debug.Log("メインオーディオが見つかりません");
            }

            NextAudioClip(_cancellation, true, 0).Forget();

            //開幕は停止しておく
            TimelineBaseReturn();
        }

        /// <summary>
        /// キャラクリア(※ポータル限定)
        /// NOTE: カウントしないのも必要
        /// </summary>
        public void ClearCaracter()
        {
            var portalChara = GetCharacterInPortal;
            if (!portalChara) return;
            Destroy(portalChara.gameObject);
            _bindCharaMap[PORTAL_INDEX] = null;
        }

        /// <summary>
        /// キャラ削除
        /// NOTE: UnlockBindingAssetとまとめられない？
        /// </summary>
        /// <param name="charaController"></param>
        public void TryDeleteCaracter(CharaController charaController)
        {
            if (!charaController) return;

            for (int i = 0; i < _bindCharaMap.Count; i++)
            {
                if (_bindCharaMap[i] != charaController) continue;
                Destroy(charaController.gameObject);
                _bindCharaMap[i] = null;
                break;
            }

            _fieldCharacterCount.Value -= 1;
        }

        /// <summary>
        /// nullバインドでキャラ解除
        /// NOTE: TryDeleteCaracterとまとめられない？
        /// </summary>
        /// <param name="charaController"></param>
        public void UnlockBindingAsset(CharaController charaController)
        {
            if (!charaController) return;

            // バインド解除
            var outputs = _playableDirector.playableAsset.outputs;
            var fromBaseAnime = outputs.FirstOrDefault(x => x.streamName == charaController.BindTrackName);
            _playableDirector.SetGenericBinding(fromBaseAnime.sourceObject, null);

            // 管理から削除
            for (int i = 0; i < _bindCharaMap.Count; i++)
            {
                if (!_bindCharaMap[i]) continue;
                if (_bindCharaMap[i].BindTrackName != charaController.BindTrackName) continue;
                _bindCharaMap[i] = null;
                break;
            }
        }

        /// <summary>
        /// 顔モーフの有効無効を切り替える(※ポータル限定)
        /// </summary>
        /// <param name="isFace">表情か口パクか</param>
        /// <param name="isEnable"></param>
        public void SetMouthUpdate(bool isFace, bool isEnable)
        {
            var bindChara = _bindCharaMap[PORTAL_INDEX];
            if (!bindChara) return;

            var vmdPlayer = bindChara.GetComponent<VMDPlayer_Custom>();
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

        /// <summary>
        /// 新規キャラをバインドする(※ポータル限定)
        /// </summary>
        /// <param name="bindObject"></param>
        /// <returns></returns>
        public bool BindingNewAsset(CharaController bindChara)
        {
            if (!bindChara) return false;

            // ポータル枠を削除しておく
            ClearCaracter();

            //ポータルのPlayableBindingを取得
            var outputs = _playableDirector.playableAsset.outputs;
            var assetBaseAnime = outputs.FirstOrDefault(x => x.streamName == _trackNameMap[0]);
            if (assetBaseAnime.streamName == "")
            {
                Debug.Log("システム設定エラー、キャラ登録枠が見つかりません。PlayableBinding名を見直してください");
                return false;
            }

            //オブジェクトをバインドする
            _playableDirector.SetGenericBinding(assetBaseAnime.sourceObject, bindChara.gameObject);
            _bindCharaMap[PORTAL_INDEX] = bindChara;
            bindChara.BindTrackName = _trackNameMap[0];
            //chara.bindTrackName_LipSync = "LipSync Track_Portal";

            //マニュアル状態ならアニメーターコントローラーを解除しておく
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                RemoveCharasAniCon().Forget();
            }
            return true;
        }

        /// <summary>
        /// 新規アニメーションクリップをバインドする(※ポータル限定)
        /// NOTE: 全く切り替わらなくなったらEditorでトラックごと作り直したら直った...
        /// </summary>
        /// <param name="baseAniTrackName"></param>
        /// <param name="danceInfoData"></param>
        public void BindingNewAnimationClip(DanceInfoData danceInfoData)
        {
            // タイムライン内のトラック一覧を取得
            if (_timeLineAsset == null) _timeLineAsset = _playableDirector.playableAsset as TimelineAsset;
            var tracks = _timeLineAsset.GetOutputTracks().OfType<AnimationTrack>();

            var baseAniTrackName = _trackNameMap[0];

            //BaseAnimeのTrackAssetを取得
            var track = tracks.FirstOrDefault(x => x.name == baseAniTrackName);
            if (!track) return;

            //トラック内のクリップを全取得
            var clips = track.GetClips();
            // 指定名称のクリップを抜き出す
            var danceClip = clips.FirstOrDefault(x => x.displayName == MAINCLIP);
            danceClip.start = _motionClipStartTime + danceInfoData.motionOffsetTime;

            //登録する
            var animationPlayableAsset = danceClip.asset as AnimationPlayableAsset;
            animationPlayableAsset.clip = danceInfoData.isReverse ?
                danceInfoData.baseDanceClip_reverse : danceInfoData.baseDanceClip;

            //オーバーライドアニメーションを登録する
            SetAnimationClip_Override(track, danceInfoData);

            TimeLineReStart();
        }

        /// <summary>
        /// 既存アニメーションを別枠にバインドする
        /// </summary>
        /// <param name="baseAniTrackName"></param>
        /// <param name="danceInfoData"></param>
        /// <param name="initPos"></param>
        /// <param name="initEulerAngles"></param>
        public void SetAnimationClip(string baseAniTrackName, DanceInfoData danceInfoData, Vector3 initPos, Vector3 initEulerAngles)
        {
            // タイムライン内のトラック一覧を取得
            if (_timeLineAsset == null) _timeLineAsset = _playableDirector.playableAsset as TimelineAsset;
            var tracks = _timeLineAsset.GetOutputTracks().OfType<AnimationTrack>();

            //BaseAnimeのTrackAssetを取得
            var track = tracks.FirstOrDefault(x => x.name == baseAniTrackName);
            if (!track) return;

            //トラック内のクリップを全取得
            var clips = track.GetClips();
            // 指定名称のクリップを抜き出す
            var danceClip = clips.FirstOrDefault(x => x.displayName == MAINCLIP);
            danceClip.start = _motionClipStartTime + danceInfoData.motionOffsetTime;

            //登録する
            var animationPlayableAsset = danceClip.asset as AnimationPlayableAsset;
            animationPlayableAsset.clip = danceInfoData.isReverse ?
                danceInfoData.baseDanceClip_reverse : danceInfoData.baseDanceClip;

            animationPlayableAsset.position = initPos;
            animationPlayableAsset.rotation = Quaternion.Euler(initEulerAngles);

            //オーバーライドアニメーションを登録する
            SetAnimationClip_Override(track, danceInfoData);

            TimeLineReStart();
        }

        /// <summary>
        /// 上書きするアニメーションを順番に登録する
        /// TODO: 綺麗にする
        /// </summary>
        /// <param name="parentTrack">ベースになるTrack</param>
        /// <param name="overrideAniClips">上書きしたいアニメーション</param>
        void SetAnimationClip_Override(TrackAsset parentTrack, DanceInfoData danceInfoData)
        {
            TimelineClip handClip;

            //上書きするトラックを処理する
            foreach (var subTrack in parentTrack.GetChildTracks().OfType<AnimationTrack>())
            {
                var clips = subTrack.GetClips();
                switch (subTrack.name)
                {
                    case SUBTRACK0:
                        // 指定名称のクリップを抜き出す
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP0);

                        //キャラが既に握りなら
                        if (_bindCharaMap[PORTAL_INDEX] && _bindCharaMap[PORTAL_INDEX].CachedClip_handL)
                        {

                        }
                        else
                        {
                            //登録する
                            handClip.start = _motionClipStartTime + danceInfoData.motionOffsetTime;
                            var animationPlayableAsset = handClip.asset as AnimationPlayableAsset;
                            animationPlayableAsset.clip = danceInfoData.isReverse ?
                                danceInfoData.overrideClip_reverseHand : danceInfoData.overrideClip_hand;
                        }

                        break;
                    case SUBTRACK1:
                        // 指定名称のクリップを抜き出す
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP1);

                        //キャラが既に握りなら
                        if (_bindCharaMap[PORTAL_INDEX] && _bindCharaMap[PORTAL_INDEX].CachedClip_handR)
                        {

                        }
                        else
                        {
                            //登録する
                            handClip.start = _motionClipStartTime + danceInfoData.motionOffsetTime;
                            var animationPlayableAsset = handClip.asset as AnimationPlayableAsset;
                            animationPlayableAsset.clip = danceInfoData.isReverse ?
                                danceInfoData.overrideClip_reverseHand : danceInfoData.overrideClip_hand;
                        }

                        break;
                    case SUBTRACK2:
                        // 指定名称のクリップを抜き出す
                        var faceClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP2);

                        //登録する
                        (faceClip.asset as AnimationPlayableAsset).clip = danceInfoData.overrideClip_face;
                        faceClip.start = _motionClipStartTime + danceInfoData.motionOffsetTime;
                        break;
                    case SUBTRACK3:
                        // 指定名称のクリップを抜き出す
                        var lipClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP3);

                        //登録する
                        (lipClip.asset as AnimationPlayableAsset).clip = danceInfoData.overrideClip_lip;
                        lipClip.start = _motionClipStartTime + danceInfoData.motionOffsetTime;
                        break;
                }
            }
        }

        /// <summary>
        /// 指定CurrentのBGMをセットする
        /// </summary>
        public async UniTask<string> NextAudioClip(CancellationToken token, bool isPreset, int moveCurrent)
        {
            token.ThrowIfCancellationRequested();

            var newAudioClip = await _audioAssetManager.TryGetAudioClipAsync(token, isPreset, moveCurrent);
            if (newAudioClip == null) return "";

            // タイムライン内のトラック一覧を取得
            if (_timeLineAsset is null) _timeLineAsset = _playableDirector.playableAsset as TimelineAsset;
            var tracks = _timeLineAsset.GetOutputTracks().OfType<AudioTrack>();

            //audioのTrackAssetを取得
            var track = tracks.FirstOrDefault(x => x.name == assetName_MainAudio);
            if (!track) return "";

            //トラック内のクリップを全取得
            var clips = track.GetClips();

            // 指定名称のクリップを抜き出す
            var oldAudioClip = clips.FirstOrDefault(x => x.displayName != "");
            oldAudioClip.duration = _audioClipStartTime + newAudioClip.length;//秒

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
                    // NOTE: ランタイム上手くいかなかった
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        track = tracks.FirstOrDefault(x => x.name == AUDIOTRACK[i]);
                        clips = track.GetClips();
                        oldAudioClip = clips.FirstOrDefault(x => x.displayName != "");
                        oldAudioClip.duration = _audioClipStartTime + newAudioClip.length;//秒
                        (oldAudioClip.asset as AudioPlayableAsset).clip = newAudioClip;
                    }
                }
            }
            TimeLineReStart();
            return newAudioClip.name;
        }

        /// <summary>
        /// 現在曲の長さ
        /// </summary>
        /// <param name="token"></param>
        /// <param name="isPreset"></param>
        /// <returns></returns>
        public async UniTask<float> CurrentAudioLength(CancellationToken token, bool isPreset)
        {
            token.ThrowIfCancellationRequested();
            var AudioClip = await _audioAssetManager.TryGetCurrentAudioClipAsycn(token, isPreset);
            return AudioClip == null ? 0 : AudioClip.length;
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
            var tracks = _timeLineAsset.GetOutputTracks();

            //対象のキャラTrackAssetを取得
            var track = tracks.FirstOrDefault(x => x.name == charaCon.BindTrackName);
            if (!track) return;


            if (isLeft)
            {
                //サブトラック
                var subTrack = track.GetChildTracks().FirstOrDefault(x => x.name == SUBTRACK0);
                //指定クリップ
                var handClip = subTrack.GetClips().FirstOrDefault(x => x.displayName == SUBCLIP0);
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
                var handClip = subTrack.GetClips().FirstOrDefault(x => x.displayName == SUBCLIP1);
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
        public bool TransferPlayableAsset(CharaController transferChara, int? toTrackNo, Vector3 initPos, Vector3 initEulerAngles)
        {
            // 転送元情報
            var srcData = TryGetDanceInfoData(transferChara);
            if (srcData is null) return false;

            // 表情系をリセットしておく
            transferChara.FacialSync.MorphReset();
            transferChara.LipSync.MorphReset();

            var trackName = _trackNameMap[toTrackNo.Value];

            // バインド
            UnlockBindingAsset(transferChara);
            BindingNewAsset(transferChara, trackName);

            //アニメーションを移行(取得した転送元アニメーションで新規登録)
            SetAnimationClip(trackName, srcData, initPos, initEulerAngles);

            // RootMotionの解除
            // NOTE: 掴んで自由に移動させる為に必要だったが、設置後は移動モーションにカクツキが生じてしまうため解除
            // 設置座標設定後に解除しないと位置が反映されないので注意(またこの変更はアニメーターの再初期化が走る)
            transferChara.GetComponent<Animator>().applyRootMotion = false;

            _fieldCharacterCount.Value += 1;

            Debug.Log("転送成功");
            return true;
        }

        /// <summary>
        /// 指定キャラの関連情報を取得
        /// </summary>
        /// <param name="transferChara"></param>
        DanceInfoData TryGetDanceInfoData(CharaController charaController)
        {
            var danceInfoData = new DanceInfoData();

            // 転送するキャラのTrackAssetを特定
            var tracks = _timeLineAsset.GetOutputTracks();
            var mainTrack = tracks.FirstOrDefault(x => x.name == charaController.BindTrackName);
            if (!mainTrack) return null;

            // メイン候補からダンス用クリップを特定
            // NOTE: まぁ現状1つしかないんですけど
            var clips = mainTrack.GetClips();
            var danceClip = clips.FirstOrDefault(x => x.displayName == MAINCLIP);
            var animationPlayableAsset = danceClip.asset as AnimationPlayableAsset;
            danceInfoData.baseDanceClip = animationPlayableAsset.clip;
            danceInfoData.motionOffsetTime = (float)(danceClip.start - _motionClipStartTime);

            // 各サブトラック情報をセット
            TimelineClip handClip;
            foreach (var subTrack in mainTrack.GetChildTracks())
            {
                clips = subTrack.GetClips();

                switch (subTrack.name)
                {
                    case SUBTRACK0:
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP0);
                        danceInfoData.overrideClip_hand = (handClip.asset as AnimationPlayableAsset).clip;
                        break;
                    case SUBTRACK1:
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP1);
                        danceInfoData.overrideClip_hand = (handClip.asset as AnimationPlayableAsset).clip;
                        break;
                    case SUBTRACK2:
                        var FaceClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP2);
                        danceInfoData.overrideClip_face = (FaceClip.asset as AnimationPlayableAsset).clip;
                        break;
                    case SUBTRACK3:
                        var lipClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP3);
                        danceInfoData.overrideClip_lip = (lipClip.asset as AnimationPlayableAsset).clip;
                        break;
                }
            }
            return danceInfoData;
        }

        /// <summary>
        /// 指定トラックにキャラをバインドする
        /// </summary>
        /// <param name="charaController"></param>
        /// <param name="toTrackName"></param>
        void BindingNewAsset(CharaController charaController, string toTrackName)
        {
            if (charaController is null) return;

            // 指定先PlayableBindingを取得
            var outputs = _playableDirector.playableAsset.outputs;
            var assetBaseAnime = outputs.FirstOrDefault(x => x.streamName == toTrackName);
            if (assetBaseAnime.streamName == "") return;

            // 先客がいれば削除しておく
            var destChara = _bindCharaMap.FirstOrDefault(x => x.Value != null && x.Value.BindTrackName == toTrackName).Value;
            if (destChara) TryDeleteCaracter(destChara);

            //オブジェクトを移行先にバインドする
            _playableDirector.SetGenericBinding(assetBaseAnime.sourceObject, charaController.gameObject);
            charaController.BindTrackName = toTrackName;
            var index = _trackNameMap.FirstOrDefault(x => x.Value == toTrackName).Key;
            _bindCharaMap[index] = charaController;
        }

        /// <summary>
        /// ID一致するVRMを全て削除(※Prefab削除時)
        /// </summary>
        /// <param name="id"></param>
        public void DeletebindAsset_CleanUp(int id)
        {
            for (int i = 0; i < _bindCharaMap.Count; i++)
            {
                if (_bindCharaMap[i] && id == _bindCharaMap[i].charaInfoData.vrmID)
                {
                    Destroy(_bindCharaMap[i].gameObject);
                    _bindCharaMap[i] = null;
                    if (i != PORTAL_INDEX) _fieldCharacterCount.Value -= 1;
                }
            }
        }

        /// <summary>
        /// 全ての召喚キャラをクリア
        /// </summary>
        public void DeletebindAsset_FieldAll()
        {
            for (int i = 0; i < _bindCharaMap.Count; i++)
            {
                if (i == PORTAL_INDEX) continue;
                if (_bindCharaMap[i])
                {
                    Destroy(_bindCharaMap[i].gameObject);
                    _bindCharaMap[i] = null;
                }
            }
            _fieldCharacterCount.Value = 0;
        }

        /// <summary>
        /// 空いているトラックを探す
        /// </summary>
        /// <returns></returns>
        public int? TryGetFreeTrack()
        {
            for (int i = 1; i < _bindCharaMap.Count; i++)
            {
                if (_bindCharaMap[i]) continue;
                return i;
            }
            return null;
        }

        /// <summary>
        /// タイムラインの変更内容を強制的?に反映させる
        /// AnimationClip変更だけ反映されないためリスタートが必要
        /// NOTE:ランタイムは無理?って見かけたけど試してみたらいけたっていう
        /// </summary>
        void TimeLineReStart()
        {
            //再生時間の記録
            var keepTime = _playableDirector.time;
            //初期化して入れ直し(これでいけちゃう謎)
            _playableDirector.playableAsset = null;
            _playableDirector.playableAsset = _timeLineAsset;

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
            else if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
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
            foreach (var chara in _bindCharaMap)
            {
                if (chara.Value is null) continue;
                chara.Value.FacialSync.MorphReset();
                chara.Value.LipSync.MorphReset();
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
            var keepVal = AudioClip_PlaybackTime;

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
                await UniTask.Delay(100, cancellationToken: _cancellation);
            }

            //AnimatorControllerを戻す
            //Manual状態で戻すと一瞬初期座標に移動してチラついてしまう為、このタイミングで実行
            for (int i = 0; i < _bindCharaMap.Count; i++)
            {
                if (!_bindCharaMap[i]) continue;
                _bindCharaMap[i].ReturnRunAnime();
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

            foreach (var chara in _bindCharaMap)
            {
                if (chara.Value == null) continue;
                chara.Value.GetComponent<AttachPointGenerator>().SetActive_AttachPoint(isActive);
            }
        }

        /// <summary>
        /// キャラのAnimatorControllerを解除する(timelineのanimatorと競合するため)
        /// </summary>
        async UniTask RemoveCharasAniCon()
        {
            //マニュアルモードでなければ処理しない
            if (_playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) return;

            await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);//必要、VRMのAwakeが間に合わない

            //TimeLineと競合っぽいのでAnimatorControllerを解除しておく 
            for (int i = 0; i < _bindCharaMap.Count; i++)
            {
                if (!_bindCharaMap[i]) continue;
                _bindCharaMap[i].RemoveRunAnime();
            }

            //ワンフレーム後にアニメーションの状態を1回だけ更新
            await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);
            _playableDirector.Evaluate();
        }
    }
}