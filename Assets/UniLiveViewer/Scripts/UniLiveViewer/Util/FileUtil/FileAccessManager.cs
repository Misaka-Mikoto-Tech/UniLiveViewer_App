using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer 
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FileAccessManager : MonoBehaviour
    {
        public bool isSuccess { get; private set; } = false;

        [SerializeField] TextureAssetManager _textureAssetManager;
        [SerializeField] AnimationAssetManager _animationAssetManager;

        CancellationToken cancellation_token;

        public event Action onLoadStart;
        public event Action onLoadSuccess;
        public event Action onLoadFail;
        public event Action onVMDLoadError;
        public event Action onLoadEnd;

        void Awake()
        {
            cancellation_token = this.GetCancellationTokenOnDestroy();

            _animationAssetManager = GetComponent<AnimationAssetManager>();
            _textureAssetManager = GetComponent<TextureAssetManager>();

            //Linkだと両方反応するのでelif必須→PLATFORM_OCULUSってのがあるみたい
#if UNITY_EDITOR
            Debug.Log("Windowsとして認識しています");
#elif UNITY_ANDROID
            Debug.Log("Questとして認識しています");
#endif
        }

        void Start()
        {
            Init().Forget();
        }

        async UniTask Init()
        {
            await UniTask.Delay(100, cancellationToken:cancellation_token);//他の初期化を待つ
            try
            {
                onLoadStart?.Invoke();

                TryCreateCustomFolder();
                await TryCreateReadmeFile();
                Debug.Log("readme生成完了");

                if (GlobalConfig.GetActiveSceneName() != "TitleScene")
                {
                    //VMDのファイルを確認
                    if (!_animationAssetManager.CheckOffsetFile())
                    {
                        onVMDLoadError?.Invoke();//フォーマットエラー
                        throw new Exception("CheckOffsetFile");
                    }
                    //VRMのサムネイル画像をキャッシュする
                    await _textureAssetManager.CacheThumbnails();
                }
                isSuccess = true;
                onLoadSuccess?.Invoke();
                Debug.Log("ロード成功");
            }
            catch
            {
                onLoadFail?.Invoke();
                Debug.Log("ロード失敗");
            }
            onLoadEnd?.Invoke();
        }

        /// <summary>
        /// Jsonファイルを読み込んでクラスに変換
        /// </summary>
        /// <returns></returns>
        public static UserProfile ReadJson()
        {
            UserProfile result;

            string path = PathsInfo.GetFullPath_JSON();
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
            string path = PathsInfo.GetFullPath_JSON();
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
        void TryCreateCustomFolder()
        {
            string sFullPath = "";
            try
            {
                //無ければ各種フォルダ生成
                for (int i = 0; i < PathsInfo.folder_length; i++)
                {
                    sFullPath = PathsInfo.GetFullPath((FOLDERTYPE)i) + "/";
                    CreateFolder(sFullPath);
                }
                //キャッシュフォルダ
                sFullPath = PathsInfo.GetFullPath_ThumbnailCache() + "/";
                CreateFolder(sFullPath);

                //リップシンクフォルダ
                sFullPath = PathsInfo.GetFullPath_LipSync() + "/";
                CreateFolder(sFullPath);
            }
            catch
            {
                throw new Exception("CreateCustomFolder");
            }
        }
        
        void CreateFolder(string fullPath)
        {
            try
            {
                var isExisting = Directory.Exists(fullPath);
                if (isExisting) Debug.Log("フォルダが既にあります");
                else
                {
                    Directory.CreateDirectory(fullPath);
                    Debug.Log("フォルダ作成成功");
                }
            }
            catch
            {
                Debug.Log("フォルダ作成失敗");
                throw;
            }
        }

        /// <summary>
        /// デフォルトファイル作成
        /// </summary>
        /// <returns></returns>
        async UniTask TryCreateReadmeFile()
        {
            try
            {
                await ResourcesLoadText("readme_ja", PathsInfo.GetFullPath_README(USE_LANGUAGE.JP));
                await ResourcesLoadText("readme_en", PathsInfo.GetFullPath_README(USE_LANGUAGE.EN));
                DeleteFile(PathsInfo.GetFullPath_DEFECT());
            }
            catch
            {
                throw new Exception("CreateFile");
            }
        }

        //TODO:わざわざ書き込む必要ないし解放必要では
        static async UniTask ResourcesLoadText(string fileName, string path)
        {
            TextAsset resourceFile = (TextAsset)await Resources.LoadAsync<TextAsset>(fileName);
            using (StreamWriter writer = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                writer.Write(resourceFile.text);
            }
        }

        void DeleteFile(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        public int CountVRM(string folderPath)
        {
            return Directory.GetFiles(folderPath, "*.vrm", SearchOption.TopDirectoryOnly).Length;
        }


        /// <summary>
        /// ダンスモーションの再生位置書き込み
        /// </summary>
        public static void SaveOffset()
        {
            //書き込み
            string path = PathsInfo.GetFullPath(FOLDERTYPE.SETTING) + "/";
            using (StreamWriter writer = new StreamWriter(path + "MotionOffset.txt", false, System.Text.Encoding.UTF8))
            {
                foreach (var e in SystemInfo.dicVMD_offset)
                {
                    writer.WriteLine(e.Key + "," + e.Value);
                }
            }
        }
    }
}
