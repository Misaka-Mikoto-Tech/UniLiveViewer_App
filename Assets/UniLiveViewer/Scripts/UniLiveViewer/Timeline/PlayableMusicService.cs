using Cysharp.Threading.Tasks;
using MessagePipe;
using NanaCiel;
using System.Linq;
using System.Threading;
using UniLiveViewer.Actor;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.SceneLoader;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using VContainer;

namespace UniLiveViewer.Timeline
{
    /// <summary>
    /// 
    /// TODO: コメントアウト部分の購読化とか
    /// </summary>
    public class PlayableMusicService
    {
        const double _motionClipStartTime = 3;//モーションクリップの開始再生位置(デフォルト)
        const string AssetNameMainAudio = "Main Audio";
        readonly string[] AUDIOTRACK = {
            "Audio Track1",
            "Audio Track2",
            "Audio Track3",
            "Audio Track4"
        };

        /// <summary>
        /// Timelineの再生速度
        /// </summary>
        public float TimelineSpeed
        {
            get { return _timelineSpeed; }
            set
            {
                _timelineSpeed = Mathf.Clamp(value, 0.0f, 3.0f);
                _playableDirector.SetSpeedTimeline(_timelineSpeed);
            }
        }
        float _timelineSpeed;

        //AudioClip基準の再生時間を算出
        public double AudioClipPlaybackTime
        {
            get
            {
                _playbackTime = _playableDirector.time - _audioClipStartTime;//参考用
                return _playbackTime;
            }
            //変更時はマニュアルモードにすること
            set
            {
                if (_playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) return;
                _playbackTime = value;
                if (_playbackTime > _playableDirector.duration) _playbackTime = _playableDirector.duration;
                _playableDirector.time = _audioClipStartTime + _playbackTime;//タイムラインに反映
            }
        }
        double _audioClipStartTime = 0;//セットされたaudioクリップの開始再生位置
        double _playbackTime = 0.0f;

        readonly IPublisher<AllActorOperationMessage> _allPublisher;
        readonly IPublisher<AttachPointMessage> _attachPointPublisher;
        readonly AudioAssetManager _audioAssetManager;
        readonly PlayableDirector _playableDirector;
        readonly SpectrumConverter _spectrumConverter;
        readonly TimelineAsset _timelineAsset;

        [Inject]
        public PlayableMusicService(
            IPublisher<AllActorOperationMessage> allPublisher,
            IPublisher<AttachPointMessage> attachPointPublisher,
            AudioAssetManager audioAssetManager,
            SpectrumConverter spectrumConverter,
            PlayableDirector playableDirector)
        {
            _allPublisher = allPublisher;
            _attachPointPublisher = attachPointPublisher;
            _audioAssetManager = audioAssetManager;
            _spectrumConverter = spectrumConverter;
            _playableDirector = playableDirector;

            _timelineAsset = _playableDirector.playableAsset as TimelineAsset;
        }

        public async UniTask OnStartAsync(CancellationToken cancellation)
        {
            TimelineSpeed = 1.0f;//起点大事

            // タイムライン内のトラック一覧を取得
            var tracks = _timelineAsset.GetOutputTracks();
            //メインオーディオのTrackAssetを取得
            var track = tracks.FirstOrDefault(x => x.name == AssetNameMainAudio);

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

            var audioTracks = _timelineAsset.GetOutputTracks().OfType<AudioTrack>();
            var audioTrack = audioTracks.FirstOrDefault(x => x.name == AssetNameMainAudio);
            if (audioTrack)
            {
                var audioSource = _playableDirector.GetGenericBinding(audioTrack) as AudioSource;
                _spectrumConverter.Initialize(audioSource);
            }

            await NextAudioClip(true, 0, cancellation);
        }

        /// <summary>
        /// 再生状態にする
        /// </summary>
        public async UniTask PlayAsync(CancellationToken cancellation)
        {
            //モードをマニュアルからゲームタイマーへ
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                _playableDirector.timeUpdateMode = DirectorUpdateMode.GameTime;
            }
            _playableDirector.ResumeTimeline();
            _spectrumConverter.Setup(NowAudioClip());

            //後にmessage
            await UniTask.Yield(cancellation);
            var allActorOperationMessage = new AllActorOperationMessage(ActorState.NULL, ActorCommand.TIMELINE_PLAY);
            _allPublisher.Publish(allActorOperationMessage);
            var attachPointMessage = new AttachPointMessage(false);
            _attachPointPublisher.Publish(attachPointMessage);
        }

        /// <summary>
        /// 再生位置を初期化する
        /// </summary>
        public async UniTask BaseReturnAsync(CancellationToken cancellation)
        {
            _playableDirector.StopTimeline();

            await ManualModeAsync(cancellation);
            AudioClipPlaybackTime = 0;
        }

        /// <summary>
        /// マニュアル状態にする
        /// </summary>
        public async UniTask ManualModeAsync(CancellationToken cancellation)
        {
            if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual) return;

            //先にmessage
            var message = new AllActorOperationMessage(ActorState.NULL, ActorCommand.TIMELINE_NONPLAY);
            _allPublisher.Publish(message);
            await UniTask.Yield(cancellation);

            //マニュアルモードに
            _playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;

            //マニュアルモードでの更新を開始
            ManualUpdateAsync(cancellation).Forget();
        }

        /// <summary>
        /// 一定間隔でマニュアルモードで更新を行う
        /// </summary>
        async UniTask ManualUpdateAsync(CancellationToken cancellation)
        {
            var keepVal = AudioClipPlaybackTime;

            _playableDirector.Evaluate();//一度反映しておく

            while (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //更新されているか
                if (keepVal != AudioClipPlaybackTime)
                {
                    //状態を反映させる
                    _playableDirector.Evaluate();

                    //キープの更新
                    keepVal = AudioClipPlaybackTime;
                }
                await UniTask.Delay(100, cancellationToken: cancellation);
            }
        }

        /// <summary>
        /// 現在曲の長さ
        /// </summary>
        public async UniTask<float> CurrentAudioLengthAsync(bool isPreset, CancellationToken cancellation)
        {
            var AudioClip = await _audioAssetManager.TryGetCurrentAudioClipAsycn(isPreset, cancellation);
            return AudioClip == null ? 0 : AudioClip.length;
        }

        /// <summary>
        /// 指定CurrentのBGMをセットする
        /// </summary>
        public async UniTask<string> NextAudioClip(bool isPreset, int moveCurrent, CancellationToken cancellation)
        {
            var nextAudioClip = await _audioAssetManager.TryGetAudioClipAsync(cancellation, isPreset, moveCurrent);
            if (nextAudioClip == null) return "";

            var audioTracks = _timelineAsset.GetOutputTracks().OfType<AudioTrack>();
            var audioTrack = audioTracks.FirstOrDefault(x => x.name == AssetNameMainAudio);
            if (!audioTrack) return "";

            //トラック内のクリップを全取得
            var timelineClips = audioTrack.GetClips();
            var oldAudioClip = timelineClips.FirstOrDefault(x => x.displayName != "");
            oldAudioClip.duration = _audioClipStartTime + nextAudioClip.length;//秒

            //登録する
            (oldAudioClip.asset as AudioPlayableAsset).clip = nextAudioClip;

            //スペクトル用
            if (SceneChangeService.GetSceneType == SceneType.CANDY_LIVE)
            {
                if (nextAudioClip.name.Contains(".mp3") || nextAudioClip.name.Contains(".wav"))
                {
                    // NOTE: ランタイム上手くいかなかった
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        audioTrack = audioTracks.FirstOrDefault(x => x.name == AUDIOTRACK[i]);
                        timelineClips = audioTrack.GetClips();
                        oldAudioClip = timelineClips.FirstOrDefault(x => x.displayName != "");
                        oldAudioClip.duration = _audioClipStartTime + nextAudioClip.length;//秒
                        (oldAudioClip.asset as AudioPlayableAsset).clip = nextAudioClip;
                    }
                }
            }

            _playableDirector.ResumeTimeline();
            _spectrumConverter.Setup(nextAudioClip);

            return nextAudioClip.name;
        }

        AudioClip NowAudioClip()
        {
            var audioTracks = _timelineAsset.GetOutputTracks().OfType<AudioTrack>();
            var audioTrack = audioTracks.FirstOrDefault(x => x.name == AssetNameMainAudio);
            if (!audioTrack) return null;

            //トラック内のクリップを全取得
            var timelineClips = audioTrack.GetClips();
            var audioClip = timelineClips.FirstOrDefault(x => x.displayName != "");
            return (audioClip.asset as AudioPlayableAsset).clip;
        }
    }
}