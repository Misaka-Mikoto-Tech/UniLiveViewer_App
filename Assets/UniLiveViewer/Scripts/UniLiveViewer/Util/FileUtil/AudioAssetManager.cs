using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace UniLiveViewer 
{
    public class AudioAssetManager : MonoBehaviour
    {
        // TODO: UniTaskちゃんとしてない

        string _basePath;
        const string EXTENSION_MP3 = ".mp3";
        const string EXTENSION_WAV = ".wav";

        [Header("＜プリセット曲＞")]
        [SerializeField] AudioClip[] _presetAudioClips;
        public int CurrentPreset => _currentPreset;
        int _currentPreset = 0;

        [Header("＜カスタム曲＞")]
        [SerializeField] const int MAX_STACK = 5;
        [SerializeField] List<AudioClip> _stackAudioClips = new List<AudioClip>();
        public IReadOnlyList<string> CustomAudios => _customAudioNames;
        [SerializeField] List<string> _customAudioNames = new List<string>();
        
        public int CurrentCustom => _currentCustom;
        int _currentCustom = 0;
        int _currentStack = 0;

        CancellationToken cancellation_token;


        void Awake()
        {
            cancellation_token = this.GetCancellationTokenOnDestroy();
        }

        async void Start()
        {
            if (GlobalConfig.GetActiveSceneName() == "TitleScene") return;

            await UniTask.Delay(1000, cancellationToken: cancellation_token);
            _basePath = PathsInfo.GetFullPath(FOLDERTYPE.BGM) + "/";
            CustomAudioNamesUpdate();
        }

        /// <summary>
        /// カスタム曲名リストの取得
        /// </summary>
        /// <returns></returns>
        void CustomAudioNamesUpdate()
        {
            //初期化
            if (_customAudioNames.Count > 0) _customAudioNames.Clear();
            //フルパス名を追加
            _customAudioNames.AddRange(Directory.GetFiles(_basePath, $"*{EXTENSION_MP3}", SearchOption.TopDirectoryOnly));
            _customAudioNames.AddRange(Directory.GetFiles(_basePath, $"*{EXTENSION_WAV}", SearchOption.TopDirectoryOnly));
        }

        public async UniTask<AudioClip> GetCurrentAudioClip(bool isPreset)
        {
            if (isPreset)
            {
                return _presetAudioClips[_currentPreset];
            }
            else
            {
                if (!_stackAudioClips[_currentStack])
                {
                    await GetAudioClips(isPreset, 0);
                }
                return _stackAudioClips[_currentStack];
            }
        }

        /// <summary>
        /// 指定カレントのAudioClipを取得する
        /// </summary>
        /// <param name="isPreset"></param>
        /// <param name="addCurrent"></param>
        /// <returns></returns>
        public async UniTask<AudioClip> GetAudioClips(bool isPreset, int addCurrent)
        {
            if (isPreset)
            {
                _currentPreset = IndexNormalization(_currentPreset + addCurrent, _presetAudioClips.Length);
                return _presetAudioClips[_currentPreset];
            }
            else
            {
                _currentCustom = IndexNormalization(_currentCustom + addCurrent, _customAudioNames.Count);
                await AudioListUpdate(_currentCustom);
                return _stackAudioClips[_currentStack];
            }
        }

        async UniTask AudioListUpdate(int nextCurrent)
        {
            var audioClip = await TryAudioLoad(_customAudioNames[nextCurrent]);
            if (audioClip)
            {
                if (_stackAudioClips.Count >= MAX_STACK)
                {
                    Destroy(_stackAudioClips[0]);
                    _stackAudioClips.RemoveAt(0);
                }
                _stackAudioClips.Add(audioClip);

                for (int i =0;i< _stackAudioClips.Count;i++)
                {
                    if(_stackAudioClips[i].name == audioClip.name)
                    {
                        _currentStack = i;
                        break;
                    }
                }
            }
        }

        async UniTask<AudioClip> TryAudioLoad(string filePath)
        {
            var src = $"file://{filePath}";
            AudioType audioType = AudioType.MPEG;
            if (src.Contains(EXTENSION_MP3)) audioType = AudioType.MPEG;
            else if (src.Contains(EXTENSION_WAV)) audioType = AudioType.WAV;

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(src, audioType))
            {
                ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

                await www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    clip.name = Path.GetFileName(src);//ファイル名のみに変える
                    return clip;

                    //Debug.Log("--------------------------------------");
                    //Debug.Log(clip.loadInBackground);
                    //Debug.Log(clip.ambisonic);

                    //Debug.Log(clip.loadType);
                    //Debug.Log(clip.preloadAudioData);

                    //Debug.Log(clip.samples);//サンプル
                    //Debug.Log(clip.channels);//チャンネル
                    //Debug.Log(clip.frequency);//周波数

                }
            }
            return null;
        }

        int IndexNormalization(int nextIndex, int maxIndex)
        {
            if (maxIndex <= nextIndex)
            {
                return nextIndex - maxIndex;
            }
            else if (nextIndex < 0)
            {
                return maxIndex + nextIndex;
            }
            else return nextIndex;
        }
    }
}
