using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using VRM.FirstPersonSample;

namespace UniLiveViewer 
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FileAccessManager : MonoBehaviour
    {
        public enum FOLDERTYPE
        {
            CHARA,
            MOTION,
            BGM,
            SETTING
        }

        //path
        private static string folderPath_Custom;
        private static string folderPath_Persistent;
        private static string[] folderName = { "Chara", "Motion", "BGM", "Setting" };
        private static string cachePath = "Cache";
        private static string lipSyncPath = "Lip-sync";

        public bool isSuccess { get; private set; } = false;
        public int PresetCount { get; private set; } = 0;
        private int currentAudio = 0;
        public int CurrentAudio
        {
            get
            {
                return currentAudio;
            }
            set
            {
                currentAudio = value;
                if(currentAudio < 0) currentAudio = audioList.Count - 1;
                else if (CurrentAudio >= audioList.Count) CurrentAudio = 0;
            }
        }
        public int AudioCount { get; private set; } = 0;
        private byte maxAudioCount = 0;

        public AudioClip[] presetAudioClip = new AudioClip[3];
        public List<AudioClip> audioList = new List<AudioClip>();
        
        public List<string> vmdList = new List<string>();
        public List<string> vmdLipSyncList = new List<string>();

        [SerializeField] private VRMRuntimeLoader_Custom vrmRuntimeLoader;

        private List<string> mp3_path = new List<string>();
        private List<string> wav_path = new List<string>();
        private CancellationToken cancellation_token;

        //サムネイルキャッシュ用
        //public static Dictionary<string,Texture2D> cacheThumbnails = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Sprite> cacheThumbnails = new Dictionary<string, Sprite>();
        [SerializeField] private Texture2D texDummy;
        private static SpriteRenderer _renderer;

        public event Action onLoadStart;
        public event Action onLoadSuccess;
        public event Action onLoadFail;
        public event Action onAudioCompleted;
        public event Action onVMDLoadError;
        public event Action onLoadEnd;

        [SerializeField] private string sMssage;//デバッグ用

        private void Awake()
        {
            cancellation_token = this.GetCancellationTokenOnDestroy();
            _renderer = GetComponent<SpriteRenderer>();
            PresetCount = presetAudioClip.Length;

            //Linkだと両方反応するのでelif必須→PLATFORM_OCULUSってのがあるみたい
#if UNITY_EDITOR
            folderPath_Custom = "D:/User/UniLiveViewer";
            sMssage = "windowsとして認識しています";
            maxAudioCount = SystemInfo.MAXAUDIO_EDITOR;
#elif UNITY_ANDROID
            folderPath_Custom = "/storage/emulated/0/UniLiveViewer";
            sMssage = "Questとして認識しています";
            maxAudioCount = SystemInfo.MAXAUDIO_QUEST;
#endif
            folderPath_Persistent = Application.persistentDataPath;
            //folderPath_Persistent = Application.temporaryCachePath;

            for (int i = 0; i < PresetCount; i++)
            {
                audioList.Add(presetAudioClip[i]);
            }
        }

        private void Start()
        {
            Init().Forget();
        }

        private async UniTask Init()
        {
            await UniTask.Delay(100, cancellationToken:cancellation_token);//他の初期化を待つ
            try
            {
                onLoadStart?.Invoke();//ロード開始

                TryCreateCustomFolder();//アプリフォルダ
                await TryCreateFile();//アプリファイル

                if (GlobalConfig.GetActiveSceneName() != "TitleScene")
                {
                    //VMDのファイルを確認
                    if (!CheckOffsetFile())
                    {
                        onVMDLoadError?.Invoke();//フォーマットエラー
                        throw new Exception("CheckOffsetFile");
                    }
                    //VRMのサムネイル画像をキャッシュする
                    await CacheThumbnail();
                }
                isSuccess = true;
                onLoadSuccess?.Invoke();//成功
            }
            catch
            {
                onLoadFail?.Invoke();//失敗
            }
            onLoadEnd?.Invoke();
        }


        public static string GetFullPath(FOLDERTYPE type)
        {
            return Path.Combine(folderPath_Custom + "/",folderName[(int)type]);
        }

        public static string GetFullPath_ThumbnailCache()
        {
            return Path.Combine(folderPath_Custom + "/", folderName[(int)FOLDERTYPE.CHARA], cachePath);
        }

        public static string GetFullPath_LipSync()
        {
            return Path.Combine(folderPath_Custom + "/", folderName[(int)FOLDERTYPE.MOTION], lipSyncPath);
        }

        /// <summary>
        /// Jsonファイルを読み込んでクラスに変換
        /// </summary>
        /// <returns></returns>
        public static UserProfile ReadJson()
        {
            UserProfile result;

            string path = Path.Combine(folderPath_Persistent + "/", "System.json");
            string datastr = "";
            StreamReader reader = null;
            if (File.Exists(path))
            {
                using (reader = new StreamReader(path))
                {
                    datastr = reader.ReadToEnd();
                    //reader.Close();
                }
                result = JsonUtility.FromJson<UserProfile>(datastr);
            }
            else
            {
                //新規作成して読み込み直す
                result = new UserProfile();
                WriteJson(result);
            }
            return result;
        }

        /// <summary>
        /// Jsonファイルに書き込む
        /// </summary>
        /// <param name="lang"></param>
        public static void WriteJson(UserProfile data)
        {
            //Json形式に変換
            string path = Path.Combine(folderPath_Persistent + "/", "System.json");
            string jsonstr = JsonUtility.ToJson(data, true);
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.Write(jsonstr);
                //writer.Flush();
                //writer.Close();
            }
        }


        /// <summary>
        /// アプリ専用フォルダ作成
        /// </summary>
        private void TryCreateCustomFolder()
        {
            string sFullPath = "";
            try
            {
                //無ければ各種フォルダ生成
                for (int i = 0; i < folderName.Length; i++)
                {
                    sFullPath = GetFullPath((FOLDERTYPE)i) + "/";
                    CreateFolder(sFullPath);
                }
#if UNITY_EDITOR
                //キャッシュフォルダ
                sFullPath = GetFullPath_ThumbnailCache() + "/";
                CreateFolder(sFullPath);
#endif
                //リップシンクフォルダ
                //sFullPath = GetFullPath_LipSync() + "/";
                //CreateFolder(sFullPath);         
            }
            catch
            {
                throw new Exception("CreateCustomFolder");
            }
        }
        
        private void CreateFolder(string fullPath)
        {
            try
            {
                var b = Directory.Exists(fullPath);
                if (b) sMssage = "フォルダが既にあります";
                else
                {
                    sMssage = "フォルダがないので作ります";
                    Directory.CreateDirectory(fullPath);
                    sMssage += "・・・成功です";
                }
            }
            catch
            {
                sMssage += "・・・失敗です";
                throw;
            }
        }

        /// <summary>
        /// デフォルトファイル作成
        /// </summary>
        /// <returns></returns>
        private async UniTask TryCreateFile()
        {
            string sFullPath;
            try
            {
                //テキストファイル
                
                sFullPath = Path.Combine(folderPath_Custom + "/", "readme_ja.txt");
                await ResourcesLoadText("readme_ja", sFullPath);

                sFullPath = Path.Combine(folderPath_Custom + "/", "readme_en.txt");
                await ResourcesLoadText("readme_en", sFullPath);

                sFullPath = Path.Combine(folderPath_Custom + "/", "不具合・Defect.txt"); 
                DeleteFile(sFullPath);
            }
            catch
            {
                throw new Exception("CreateFile");
            }
        }


        private static async UniTask ResourcesLoadText(string fileName, string path)
        {
            TextAsset resourceFile = (TextAsset)await Resources.LoadAsync<TextAsset>(fileName);
            using (StreamWriter writer = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                writer.Write(resourceFile.text);
            }
        }

        public void DeleteFile(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        /// <summary>
        /// アプリフォルダ内のVRMファイル名を取得
        /// </summary>
        /// <returns></returns>
        public string[] GetAllVRMNames()
        {
            string sFolderPath = GetFullPath(FOLDERTYPE.CHARA) + "/";

            string[] sResult = null;
            try
            {
                //VRMファイルのみ検索
                sResult = Directory.GetFiles(sFolderPath, "*.vrm", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < sResult.Length; i++)
                {
                    sResult[i] = Path.GetFileName(sResult[i]);

                    //int j = sResult[i].LastIndexOf("/");//末尾から文字サーチ、先頭から何番目か
                    //int maxStr = sResult[i].Length;

                    //sResult[i] = sResult[i].Substring(j, maxStr - j);
                    //sResult[i] = sResult[i].Replace("/", "");
                }
            }
            catch
            {
                sMssage = "VRMファイル読み込みに失敗しました";
            }

            return sResult;
        }

        /// <summary>
        /// アプリフォルダ内のVMDファイル名を取得
        /// </summary>
        /// <returns></returns>
        private bool CheckOffsetFile()
        {
            //初期化
            if (SystemInfo.dicVMD_offset.Count != 0)
            {
                SystemInfo.dicVMD_offset.Clear();
            }

            //offset情報ファイルがあれば読み込む
            string path = GetFullPath(FOLDERTYPE.SETTING) + "/MotionOffset.txt";
            if (File.Exists(path))
            {
                foreach (string line in File.ReadLines(path))
                {
                    string[] spl = line.Split(',');
                    if (spl.Length != 2) return false;
                    if(spl[0]==""|| spl[1] == "") return false;
                    SystemInfo.dicVMD_offset.Add(spl[0], int.Parse(spl[1]));
                }
            }

            //VMDファイル名を取得
            string sFolderPath = GetFullPath(FOLDERTYPE.MOTION) + "/";
            try
            {
                //VMDファイルのみ検索
                var names = Directory.GetFiles(sFolderPath, "*.vmd", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = names[i].Replace(sFolderPath, "");

                    //ファイル名に区切りのカンマが含まれると困る
                    if (names[i].Contains(",")) return false;
                    else
                    {
                        vmdList.Add(names[i]);

                        //既存offset情報がなければ追加
                        if (!SystemInfo.dicVMD_offset.ContainsKey(names[i]))
                        {
                            SystemInfo.dicVMD_offset.Add(names[i], 0);
                        }
                    }
                }

                //一旦保存
                SaveOffset();
            }
            catch
            {
                sMssage = "VMDファイル読み込みに失敗しました";
                return false;
            }
            return true;
        }

        /// <summary>
        /// ダンスモーションの再生位置書き込み
        /// </summary>
        public static void SaveOffset()
        {
            //書き込み
            string path = GetFullPath(FOLDERTYPE.SETTING) + "/";
            using (StreamWriter writer = new StreamWriter(path + "MotionOffset.txt", false, System.Text.Encoding.UTF8))
            {
                foreach (var e in SystemInfo.dicVMD_offset)
                {
                    writer.WriteLine(e.Key + "," + e.Value);
                }
            }
        }

        /// <summary>
        /// アプリフォルダ内のVMDファイル名を取得
        /// </summary>
        /// <returns></returns>
        private void GetAllVMDLipSyncNames()
        {
            //VMDファイル名を取得
            string sFolderPath = GetFullPath_LipSync() + "/";
            try
            {
                //VRMファイルのみ検索
                var names = Directory.GetFiles(sFolderPath, "*.vmd", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = names[i].Replace(sFolderPath, "");
                    vmdLipSyncList.Add(names[i]);
                }
            }
            catch
            {
                sMssage = "VMDLipSyncファイル読み込みに失敗しました";
            }
        }

        /// <summary>
        /// BGMフォルダのファイル数を取得
        /// </summary>
        /// <returns></returns>
        public int GetAudioFileCount()
        {
            string sFolderPath = GetFullPath(FOLDERTYPE.BGM) + "/";

            //初期化
            if (mp3_path.Count > 0) mp3_path.Clear();
            if (wav_path.Count > 0) wav_path.Clear();

            //.mp3ファイルを検索、リストに追加
            mp3_path.AddRange(Directory.GetFiles(sFolderPath, "*.mp3", SearchOption.TopDirectoryOnly));
            //"*.wav"ファイルを検索、リストに追加
            wav_path.AddRange(Directory.GetFiles(sFolderPath, "*.wav", SearchOption.TopDirectoryOnly));

            return mp3_path.Count + wav_path.Count;
        }

        /// <summary>
        /// Audioフォルダのデータをロード、Clipで保持する
        /// GetAudioFileCount()を事前に呼ぶ必要があるの改善したい
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async UniTask AudioLoad()
        {
            //全く呼ばれていなければ呼ぶ
            if (mp3_path.Count == 0 && wav_path.Count == 0)
            {
                GetAudioFileCount();
            }

            //リスト初期化
            if (audioList.Count > PresetCount)
            {
                //ClipはリストClearだけだと消えないので個別に消す
                for (int i = 0; i < audioList.Count; i++)
                {
                    Destroy(audioList[i]);
                }

                audioList.Clear();
                audioList = new List<AudioClip>();
                for (int i = 0; i < PresetCount; i++)
                {
                    audioList.Add(presetAudioClip[i]);
                }
                AudioCount = 0;
            }

            string oldPath = GetFullPath(FOLDERTYPE.BGM) + "/";

            foreach (string str in mp3_path)
            {
                if (AudioCount < maxAudioCount) AudioCount++;
                else break;
                var src = $"file://{str}";
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(src, AudioType.MPEG))
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
                        clip.name = str.Replace(oldPath, "");
                        audioList.Add(clip);

                        //Debug.Log("--------------------------------------");
                        //Debug.Log(clip.loadInBackground);
                        //Debug.Log(clip.ambisonic);

                        //Debug.Log(clip.loadType);
                        //Debug.Log(clip.preloadAudioData);

                        //Debug.Log(clip.samples);//サンプル
                        //Debug.Log(clip.channels);//チャンネル
                        //Debug.Log(clip.frequency);//周波数

                    }
                    await UniTask.Yield(cancellation_token);
                }
            }

            foreach (string str in wav_path)
            {
                if (AudioCount < maxAudioCount) AudioCount++;
                else break;
                var src = $"file://{str}";
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(src, AudioType.WAV))
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
                        clip.name = str.Replace(oldPath, "");
                        audioList.Add(clip);
                    }
                    await UniTask.Yield(cancellation_token);
                }
            }

            //完了した
            onAudioCompleted?.Invoke();
        }

        /// <summary>
        /// 暫定
        /// </summary>
        private async UniTask CacheThumbnail()
        {

            if (!vrmRuntimeLoader) return;

            string folderPath_Chara = GetFullPath(FOLDERTYPE.CHARA) + "/";
            Texture2D texture = null;
            Sprite spr = null;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);

            //全ファイル名を取得
            var vrmNames = GetAllVRMNames();

            for (int i = 0; i < vrmNames.Length; i++)
            {
                if (spr) spr = null;
                if (texture) texture = null;

                //キャッシュ画像確認
                //texture = cacheThumbnails.FirstOrDefault(x => x.Key == vrmNames[i]).Value;

                spr = cacheThumbnails.FirstOrDefault(x => x.Key == vrmNames[i]).Value;
                if (spr != null) continue;

                try
                {
                    //VRMファイルからサムネイルを抽出する
                    texture = await vrmRuntimeLoader.GetThumbnail(folderPath_Chara + vrmNames[i], cancellation_token);

                    if (texture)
                    {
                        //テクスチャ→スプライトに変換
                        spr = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //リストに追加
                        cacheThumbnails.Add(vrmNames[i], spr);
                    }
                    else
                    {
                        //ダミー画像生成
                        //texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
                        texture = Instantiate(texDummy);
                        //テクスチャ→スプライトに変換
                        spr = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        //リストに追加
                        cacheThumbnails.Add(vrmNames[i], spr);
                    }

                    //エディタのみキャッシュ用を出力
#if UNITY_EDITOR
                    if (texture) texture = GetColorInfo(ResizeTexture(texture, 256, 256));
                    else texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
                    File.WriteAllBytes(Path.Combine(GetFullPath_ThumbnailCache() + "/", $"{vrmNames[i]}.png"), texture.EncodeToPNG());
#endif
                }
                catch (System.OperationCanceledException)
                {
                    Debug.Log("サムネイルキャッシュ中に中断");
                    throw;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);
            }
        }

        /// <summary>
        /// 実機にて不具合あるので次回
        /// </summary>
        //private async UniTaskVoid CacheThumbnail_test()
        //{

        //    string folderPath_Chara = GetFullPath(FOLDERTYPE.CHARA);
        //    string folderPath_Cache = GetFullPath_ThumbnailCache();
        //    Texture2D texture = null;

        //    if(cacheThumbnails != null && cacheThumbnails.Count > 0)
        //    {
        //        cacheThumbnails.Clear();
        //        cacheThumbnails = new Dictionary<string, Texture2D>();
        //    }

        //    await UniTask.Yield(PlayerLoopTiming.Update,cancellation_token);

        //    //全ファイル名を取得
        //    var vrmNames = GetAllVRMNames();
        //    for (int i = 0; i < vrmNames.Length; i++)
        //    {
        //        if (texture) texture = null;
        //        //キャッシュ画像確認
        //        var isCache = File.Exists(folderPath_Cache + vrmNames[i] + ".png");
        //        if (isCache)
        //        {
        //            //キャッシュ済み画像を流用
        //            texture = GetCacheThumbnail(vrmNames[i]);

        //            //リストに追加
        //            cacheThumbnails.Add(vrmNames[i], texture);
        //        }
        //        else
        //        {
        //            try
        //            {
        //                //VRMファイルからサムネイルを抽出する
        //                texture = await vrmRuntimeLoader.GetThumbnail(folderPath_Chara + vrmNames[i], cancellation_token);

        //                if (texture)
        //                {
        //                    //リサイズ
        //                    texture = ResizeTexture(texture, 256, 256);
        //                    //色情報取得(CPU側の処理では色の情報やらを取得できず灰色画像になるので)
        //                    texture = GetColorInfo(texture);
        //                    //リストに追加
        //                    cacheThumbnails.Add(vrmNames[i], texture);
        //                    //フォルダにも保存
        //                    File.WriteAllBytes(folderPath_Cache + vrmNames[i] + ".png", texture.EncodeToPNG());
        //                }
        //                else
        //                {
        //                    //ダミー画像生成
        //                    texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
        //                    //リストに追加
        //                    cacheThumbnails.Add(vrmNames[i], texture);
        //                    //フォルダにも保存
        //                    File.WriteAllBytes(folderPath_Cache + vrmNames[i] + ".png", texture.EncodeToPNG());
        //                }
        //            }
        //            catch (System.OperationCanceledException)
        //            {
        //                Debug.Log("サムネイルキャッシュ中に中断");
        //                throw;
        //            }
        //        }
        //        await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);
        //    }

        //    //完了した
        //    onThumbnailCompleted?.Invoke();
        //}

        /// <summary>
        /// テクスチャのリサイズ
        /// </summary>
        /// <param name="srcTexture"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public static Texture2D ResizeTexture(Texture2D srcTexture, int newWidth, int newHeight)
        {
            //指定しないとRGBA32になってしまったので一応
            var resizedTexture = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false);
            Graphics.ConvertTexture(srcTexture, resizedTexture);
            return resizedTexture;
        }

        /// <summary>
        /// シェーダー側で描画してテクスチャに書き込むたぶん（大体コピペ）
        /// デプス系を無効化しないとQuest実機では動かなかったので用意
        /// </summary>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        public static Texture2D GetColorInfo(Texture2D texture2D)
        {
            //Texture mainTexture = _renderer.material.mainTexture;
            //RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(texture2D.width, texture2D.height, 0);//第三デプス
            //Linear→Gamma
            Graphics.Blit(texture2D, renderTexture, _renderer.sharedMaterial);//デプス無効化したマテリアル
            //RenderTexture情報→texture2Dへ
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            //RenderTexture.ReleaseTemporary(renderTexture);


            //Color[] pixels = texture2D.GetPixels();
            //RenderTexture.active = currentRT;

            return texture2D;
        }

        /// <summary>
        /// キャッシュしたサムネイルを取得
        /// </summary>
        /// <param name="filePath">.png</param>
        /// <returns></returns>
        private static Texture2D GetCacheThumbnail(string fileName)
        {
            string filePath = Path.Combine(GetFullPath_ThumbnailCache() + "/", $"{fileName}.png");
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(64, 64);
            texture.LoadImage(bytes);
            return texture;
        }
    }

}
