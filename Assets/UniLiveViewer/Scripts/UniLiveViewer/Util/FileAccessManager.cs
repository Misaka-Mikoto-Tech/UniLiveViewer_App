using Cysharp.Threading.Tasks;
using System;
using System.Collections;
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

        public static string folderPath_Custom;//VMDやmp3など
        public string sMssage;//デバッグ用

        public static string[] folderName = { "Chara/", "Motion/", "BGM/", "Setting/" };
        public static string cachePath = "Cache/";
        public static string lipSyncPath = "Lip-sync/";

        public bool isSuccess { get; private set; } = false;

        public AudioClip[] presetAudioClip = new AudioClip[3];
        public int presetCount = 0;
        public List<AudioClip> audioList = new List<AudioClip>();
        public int CurrentAudio = 0;

        public List<string> vmdList = new List<string>();
        public List<string> vmdLipSyncList = new List<string>();

        public int CurrentMotion = 0;

        private List<string> mp3_path = new List<string>();
        private List<string> wav_path = new List<string>();
        private int AudioCount = 0;

        private CancellationToken cancellation_token;

        //サムネイルキャッシュ用
        //public static Dictionary<string,Texture2D> cacheThumbnails = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Sprite> cacheThumbnails = new Dictionary<string, Sprite>();
        [SerializeField] private Texture2D texDummy;
        private static SpriteRenderer _renderer;

#if UNITY_EDITOR
        private const int MAX_AUDIOCOUNT = 30;
#elif UNITY_ANDROID
        private const int MAX_AUDIOCOUNT = 10;
#endif

        public event Action onFolderCheckCompleted;
        public event Action onAudioCompleted;
        public event Action onThumbnailCompleted;

        [SerializeField] VRMRuntimeLoader_Custom vrmRuntimeLoader;


        private void Awake()
        {
            //Linkだと両方反応するのでelif必須→PLATFORM_OCULUSってのがあるみたい
#if UNITY_EDITOR
            folderPath_Custom = "D:/User/UniLiveViewer/";
            sMssage = "windowsとして認識しています";
#elif UNITY_ANDROID
            folderPath_Custom = "/storage/emulated/0/" + "UniLiveViewer/";
            sMssage = "Questとして認識しています";
#endif
            //folderPath_Custom = Application.persistentDataPath + "/";
            //folderPath_Custom = Application.temporaryCachePath + "/";

            cancellation_token = this.GetCancellationTokenOnDestroy();

            presetCount = presetAudioClip.Length;

            for (int i = 0; i < presetCount; i++)
            {
                audioList.Add(presetAudioClip[i]);
            }

            _renderer = GetComponent<SpriteRenderer>();

            cancellation_token = this.GetCancellationTokenOnDestroy();

            //アプリフォルダの作成
            StartCoroutine(CreateFolder());

        }

        public static string GetFullPath(FOLDERTYPE type)
        {
            return folderPath_Custom + folderName[(int)type];
        }

        public static string GetFullPath_ThumbnailCache()
        {
            return folderPath_Custom + folderName[(int)FOLDERTYPE.CHARA] + cachePath;
        }

        public static string GetFullPath_LipSync()
        {
            return folderPath_Custom + folderName[(int)FOLDERTYPE.MOTION] + lipSyncPath;
        }

        /// <summary>
        /// アプリフォルダとreadme.txtを作成する
        /// </summary>
        private IEnumerator CreateFolder()
        {
            string sFullPath = "";

            try
            {
                //ベースフォルダチェック
                for (int i = 0; i < folderName.Length; i++)
                {
                    sFullPath = folderPath_Custom + folderName[i];
                    CreateFolder(sFullPath);
                }

                //キャッシュフォルダ
                //sFullPath = folderPath_Custom + folderName[(int)FOLDERTYPE.CHARA] + cachePath;
                //CreateFolder(sFullPath);

                //リップシンクフォルダ
                //sFullPath = folderPath_Custom + folderName[(int)FOLDERTYPE.MOTION] + lipSyncPath;
                //CreateFolder(sFullPath);

                //VMDのファイル名を取得
                GetAllVMDNames();
                //GetAllVMDLipSyncNames();

                isSuccess = true;
            }
            catch
            {
                isSuccess = false;
            }


            if (isSuccess)
            {
                sFullPath = folderPath_Custom + "readme_ja.txt";
                yield return StartCoroutine(ResourcesLoadText("readme_ja", sFullPath));

                sFullPath = folderPath_Custom + "readme_en.txt";
                yield return StartCoroutine(ResourcesLoadText("readme_en", sFullPath));

                sFullPath = folderPath_Custom + "不具合・Defect.txt";
                DeleteFile(sFullPath);

                //完了した
                onFolderCheckCompleted?.Invoke();

                yield return new WaitForSeconds(0.5f);

                //VRMのサムネイル画像をキャッシュする
                CacheThumbnail().Forget();
            }
        }

        private void CreateFolder(string fullPath)
        {
            //キャッシュフォルダ
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
        public IEnumerator ResourcesLoadText(string fileName, string path)
        {
            var resourceFile = Resources.Load<TextAsset>(fileName);
            using (StreamWriter writer = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                writer.Write(resourceFile.text);
            }
            yield return null;
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
            string sFolderPath = folderPath_Custom + folderName[(int)FOLDERTYPE.CHARA];

            string[] sResult = null;
            try
            {
                //VRMファイルのみ検索
                sResult = Directory.GetFiles(sFolderPath, "*.vrm", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < sResult.Length; i++)
                {
                    int j = sResult[i].LastIndexOf("/");//末尾から文字サーチ、先頭から何番目か
                    int maxStr = sResult[i].Length;

                    sResult[i] = sResult[i].Substring(j, maxStr - j);
                    sResult[i] = sResult[i].Replace("/", "");
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
        private void GetAllVMDNames()
        {
            //初期化
            if (SaveData.dicVMD_offset.Count != 0)
            {
                SaveData.dicVMD_offset.Clear();
            }

            //offset情報ファイルがあれば読み込む
            string path = GetFullPath(FOLDERTYPE.SETTING);
            if (File.Exists(path + "MotionOffset.txt"))
            {
                foreach (string line in File.ReadLines(path + "MotionOffset.txt"))
                {
                    string[] spl = line.Split(',');
                    SaveData.dicVMD_offset.Add(spl[0], int.Parse(spl[1]));
                }
            }

            //VMDファイル名を取得
            string sFolderPath = GetFullPath(FOLDERTYPE.MOTION);
            try
            {
                //VRMファイルのみ検索
                var names = Directory.GetFiles(sFolderPath, "*.vmd", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = names[i].Replace(sFolderPath, "");
                    vmdList.Add(names[i]);

                    //既存offset情報がなければ追加
                    if (!SaveData.dicVMD_offset.ContainsKey(names[i]))
                    {
                        SaveData.dicVMD_offset.Add(names[i], 0);
                    }
                }

                //一旦保存
                SaveData.SaveOffset();
            }
            catch
            {
                sMssage = "VMDファイル読み込みに失敗しました";
            }
        }

        /// <summary>
        /// アプリフォルダ内のVMDファイル名を取得
        /// </summary>
        /// <returns></returns>
        private void GetAllVMDLipSyncNames()
        {
            //VMDファイル名を取得
            string sFolderPath = GetFullPath(FOLDERTYPE.MOTION) + lipSyncPath;
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
            string sFolderPath = folderPath_Custom + folderName[(int)FOLDERTYPE.BGM];

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
        public IEnumerator AudioLoad()
        {
            //全く呼ばれていなければ呼ぶ
            if (mp3_path.Count == 0 && wav_path.Count == 0)
            {
                GetAudioFileCount();
            }

            //リスト初期化
            if (audioList.Count > presetCount)
            {
                //ClipはリストClearだけだと消えないので個別に消す
                for (int i = 0; i < audioList.Count; i++)
                {
                    Destroy(audioList[i]);
                }

                audioList.Clear();
                audioList = new List<AudioClip>();
                for (int i = 0; i < presetCount; i++)
                {
                    audioList.Add(presetAudioClip[i]);
                }
                AudioCount = 0;
            }

            foreach (string str in mp3_path)
            {
                if (AudioCount < MAX_AUDIOCOUNT) AudioCount++;
                else break;
                var src = $"file://{str}";
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(src, AudioType.MPEG))
                {
                    ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogError(www.error);
                    }
                    else
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                        clip.name = str.Replace(folderPath_Custom + folderName[(int)FOLDERTYPE.BGM], "");
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
                    yield return null;
                }
            }

            foreach (string str in wav_path)
            {
                if (AudioCount < MAX_AUDIOCOUNT) AudioCount++;
                else break;
                var src = $"file://{str}";
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(src, AudioType.WAV))
                {
                    ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogError(www.error);
                    }
                    else
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                        clip.name = str.Replace(folderPath_Custom + folderName[(int)FOLDERTYPE.BGM], "");
                        audioList.Add(clip);
                    }
                    yield return null;
                }
            }

            //完了した
            onAudioCompleted?.Invoke();
        }

        /// <summary>
        /// 暫定
        /// </summary>
        private async UniTaskVoid CacheThumbnail()
        {
            string folderPath_Chara = GetFullPath(FOLDERTYPE.CHARA);
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

                if (spr == null)
                {
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
                    }
                    catch (System.OperationCanceledException)
                    {
                        Debug.Log("サムネイルキャッシュ中に中断");
                        throw;
                    }
                }
                else
                {

                }
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);
            }

            //完了した
            onThumbnailCompleted?.Invoke();
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
            Graphics.Blit(texture2D, renderTexture, _renderer.sharedMaterial);//デプス無効化したマテリアル

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.ReleaseTemporary(renderTexture);

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
            string filePath = GetFullPath_ThumbnailCache() + fileName + ".png";
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(64, 64);
            texture.LoadImage(bytes);
            return texture;
        }
    }

}
