using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using UnityEngine;
using UniRx;
using UnityEngine.SceneManagement;

namespace UniLiveViewer 
{
    /// <summary>
    /// 専用フォルダ作成削除・ファイル数カウント
    /// </summary>
    public class FileAccessManager
    {
        /// <summary>
        /// TODO: なにこれ消したい
        /// </summary>
        public bool isSuccess { get; private set; } = false;

        Subject<Unit> _onLoadStart = new Subject<Unit>();
        public IObservable<Unit> LoadStartAsObservable => _onLoadStart;
        Subject<Unit> _onLoadEnd = new Subject<Unit>();
        public IObservable<Unit> LoadEndAsObservable => _onLoadEnd;
        //Subject<Unit> _onVMDLoadError = new Subject<Unit>();
        //public IObservable<Unit> LoadErrorAsObservable => _onVMDLoadError;

        public async UniTask PreparationStart(CancellationToken cancellation)
        {
            _onLoadStart?.OnNext(Unit.Default);

            TryCreateCustomFolder();
            await TryCreateReadmeFile(cancellation);
        }

        public void PreparationEnd()
        {
            isSuccess = true;
            _onLoadEnd?.OnNext(Unit.Default);
        }

        public async UniTask OnStartAsync(AnimationAssetManager animationAssetManager, TextureAssetManager textureAssetManager,CancellationToken cancellation)
        {
            //try
            //{
            //    _onLoadStart?.OnNext(Unit.Default);

            //    TryCreateCustomFolder();
            //    await TryCreateReadmeFile(cancellation);

            //    //タイトルシーン以外
            //    if (SceneManager.GetActiveScene().name != "TitleScene")
            //    {
            //        //VMDファイルを確認
            //        if (!animationAssetManager.Setup())
            //        {
            //            _onVMDLoadError?.OnNext(Unit.Default);//フォーマットエラー
            //            throw new Exception("CheckOffsetFile");
            //        }
            //        await textureAssetManager.CacheThumbnails(cancellation);
            //    }
            //    isSuccess = true;
            //    Debug.Log("ロード成功");
            //    _onLoadEnd?.OnNext(Unit.Default);
            //}
            //catch(Exception e)
            //{
            //    Debug.Log("ロード失敗:" + e);
            //}
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
                if (isExisting) Debug.Log($"フォルダが既に有り：{fullPath}");
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
        /// リードミー
        /// </summary>
        /// <returns></returns>
        async UniTask TryCreateReadmeFile(CancellationToken cancellation)
        {
            try
            {
                await ResourcesLoadText("readme_ja", PathsInfo.GetFullPath_README(USE_LANGUAGE.JP), cancellation);
                await ResourcesLoadText("readme_en", PathsInfo.GetFullPath_README(USE_LANGUAGE.EN), cancellation);
                DeleteFile(PathsInfo.GetFullPath_DEFECT());
            }
            catch
            {
                throw new Exception("CreateFile");
            }
            Debug.Log("Readme作成完了");
        }

        //TODO:わざわざ書き込む必要ないし解放必要では
        async UniTask ResourcesLoadText(string fileName, string path, CancellationToken cancellation)
        {
            var resourceFile = (TextAsset)await Resources.LoadAsync<TextAsset>(fileName);
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
