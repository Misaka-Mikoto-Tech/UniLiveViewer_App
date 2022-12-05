using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UniRx;

namespace UniLiveViewer 
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FileAccessManager : MonoBehaviour
    {
        public bool isSuccess { get; private set; } = false;

        CancellationToken cancellation_token;
        Subject<Unit> _onLoadStart = new Subject<Unit>();
        public IObservable<Unit> LoadStartAsObservable => _onLoadStart;
        Subject<Unit> _onLoadEnd = new Subject<Unit>();
        public IObservable<Unit> LoadEndAsObservable => _onLoadEnd;
        Subject<Unit> _onVMDLoadError = new Subject<Unit>();
        public IObservable<Unit> LoadErrorAsObservable => _onVMDLoadError;

        void Awake()
        {
            cancellation_token = this.GetCancellationTokenOnDestroy();
        }

        public async UniTask Initialize(AnimationAssetManager animationAssetManager, TextureAssetManager textureAssetManager)
        {
            // NOTE: Linkだと両方反応するのでelif必須
            // TODO: PLATFORM_OCULUS試す
#if UNITY_EDITOR
            Debug.Log("Windowsとして認識しています");
#elif UNITY_ANDROID
            Debug.Log("Questとして認識しています");
#endif

            await UniTask.Delay(100, cancellationToken:cancellation_token);//他の初期化を待つ
            try
            {
                _onLoadStart?.OnNext(Unit.Default);

                //フォルダ作成
                TryCreateCustomFolder();
                //リードミー作成
                await TryCreateReadmeFile();
                //タイトルシーン以外
                if (GlobalConfig.GetActiveSceneName() != "TitleScene")
                {
                    //VMDファイルを確認
                    if (!animationAssetManager.CheckOffsetFile())
                    {
                        _onVMDLoadError?.OnNext(Unit.Default);//フォーマットエラー
                        throw new Exception("CheckOffsetFile");
                    }
                    await textureAssetManager.CacheThumbnails();
                }

                isSuccess = true;
                Debug.Log("ロード成功");
                _onLoadEnd?.OnNext(Unit.Default);
            }
            catch
            {
                Debug.Log("ロード失敗");
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
        
        /// <summary>
        /// フォルダ生成
        /// </summary>
        /// <param name="fullPath"></param>
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
            Debug.Log("Readme作成完了");
        }

        //TODO:わざわざ書き込む必要ないし解放必要では
        async UniTask ResourcesLoadText(string fileName, string path)
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
    }
}
