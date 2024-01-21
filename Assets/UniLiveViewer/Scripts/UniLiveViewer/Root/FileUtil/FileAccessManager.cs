using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using UniRx;
using UnityEngine;

namespace UniLiveViewer
{
    /// <summary>
    /// 専用フォルダ作成削除・ファイル数カウント
    /// </summary>
    public class FileAccessManager
    {
        public bool isSuccess { get; private set; } = false;

        Subject<Unit> _onLoadStart = new Subject<Unit>();
        public IObservable<Unit> LoadStartAsObservable => _onLoadStart;
        Subject<Unit> _onLoadEnd = new Subject<Unit>();
        public IObservable<Unit> LoadEndAsObservable => _onLoadEnd;

        /// <summary>
        /// 準備開始(シーン冒頭に呼ばれる)
        /// </summary>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        public async UniTask PreparationStart(CancellationToken cancellation)
        {
            _onLoadStart?.OnNext(Unit.Default);

            TryCreateCustomFolder();
            await TryCreateReadmeFile(cancellation);
        }

        /// <summary>
        /// 準備完了(シーン冒頭に呼ばれる)
        /// </summary>
        public void PreparationEnd()
        {
            isSuccess = true;
            _onLoadEnd?.OnNext(Unit.Default);
        }

        /// <summary>
        /// アプリ専用フォルダ作成
        /// </summary>
        void TryCreateCustomFolder()
        {
            var fullPath = (string)null;
            try
            {
                //無ければ各種フォルダ生成
                for (int i = 0; i < PathsInfo.folder_length; i++)
                {
                    fullPath = PathsInfo.GetFullPath((FolderType)i) + "/";
                    CreateFolder(fullPath);
                }
                //キャッシュフォルダ
                fullPath = PathsInfo.GetFullPath_ThumbnailCache() + "/";
                CreateFolder(fullPath);

                //リップシンクフォルダ
                fullPath = PathsInfo.GetFullPath_LipSync() + "/";
                CreateFolder(fullPath);
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
                await ResourcesLoadText("readme_ja", PathsInfo.GetFullPath_README(LanguageType.JP), cancellation);
                await ResourcesLoadText("readme_en", PathsInfo.GetFullPath_README(LanguageType.EN), cancellation);
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
