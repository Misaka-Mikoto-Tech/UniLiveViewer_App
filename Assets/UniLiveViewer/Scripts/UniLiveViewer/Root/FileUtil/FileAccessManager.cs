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
        public IObservable<Unit> EndLoadingAsObservable => _onEndLoadingStream;
        readonly Subject<Unit> _onEndLoadingStream = new ();
        
        /// <summary>
        /// 準備開始(シーン冒頭に呼ばれる)
        /// </summary>
        public async UniTask PreparationStartAsync(CancellationToken cancel)
        {
            RootSystemSettings._isUsedCustomFolders = GrantStoragePermission.UserHasManageExternalStoragePermission();
            if (!RootSystemSettings._isUsedCustomFolders) return;

            TryCreateCustomFolder();
            await TryCreateReadmeFileAsync(cancel);
        }

        /// <summary>
        /// 準備完了(シーン冒頭に呼ばれる)
        /// </summary>
        public void PreparationEnd()
        {
            _onEndLoadingStream?.OnNext(Unit.Default);
        }

        /// <summary>
        /// アプリ専用フォルダ作成
        /// </summary>
        void TryCreateCustomFolder()
        {
            try
            {
                var isExistsActor = TryRenameOldFolder();
                if (!isExistsActor) CreateFolder(FolderType.Actor);
                CreateFolder(FolderType.Motion);
                CreateFolder(FolderType.BGM);
                CreateFolder(FolderType.Settings);
                CreateFolder($"{PathsInfo.GetThumbnailsFolderPath()}/");
                CreateFolder($"{PathsInfo.GetFacialSyncFolderPath()}/");
            }
            catch
            {
                throw new Exception("CreateCustomFolder");
            }
        }

        /// <summary>
        /// Chara→Actorに変更しちゃう
        /// </summary>
        bool TryRenameOldFolder()
        {
            var oldFolderPath = PathsInfo.GetCharaFolderPath() + "/";
            var newFolderPath = PathsInfo.GetFullPath(FolderType.Actor) + "/";
            if (Directory.Exists(oldFolderPath))
            {
                // 新しいフォルダが存在しない場合、リネームを実行
                if (!Directory.Exists(newFolderPath))
                {
                    try
                    {
                        Directory.Move(oldFolderPath, newFolderPath);
                        Debug.Log("フォルダをCharaからActorにリネームしました。");
                    }
                    catch (IOException ex)
                    {
                        Debug.LogError($"フォルダのリネームに失敗しました: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning("新しいフォルダ名Actorが既に存在します。");
                }
                return true;
            }
            return false;
        }

        void CreateFolder(FolderType folderType)
        {
            var fullPath = PathsInfo.GetFullPath(folderType) + "/";
            CreateFolder(fullPath);
        }

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
        async UniTask TryCreateReadmeFileAsync(CancellationToken cancel)
        {
            try
            {
                await ResourcesLoadTextAsync("readme", PathsInfo.GetReadmeFolderPath(), cancel);
                DeleteFile(PathsInfo.GetReadmeFolderPath(SystemLanguage.English));
                DeleteFile(PathsInfo.GetReadmeFolderPath(SystemLanguage.Japanese));
                DeleteFile(PathsInfo.GetDefectFolderPath());
            }
            catch
            {
                throw new Exception("CreateFile");
            }
            Debug.Log("Readme作成完了");
        }

        //TODO:わざわざ書き込む必要ないし解放必要では
        async UniTask ResourcesLoadTextAsync(string fileName, string path, CancellationToken cancel)
        {
            var resourceFile = (TextAsset)await Resources.LoadAsync<TextAsset>(fileName);
            using (var writer = new StreamWriter(path, false, System.Text.Encoding.UTF8))
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
