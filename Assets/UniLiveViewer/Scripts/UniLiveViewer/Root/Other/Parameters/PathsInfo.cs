using System.IO;
using UnityEngine;

namespace UniLiveViewer
{
    public class PathsInfo
    {
        const string ThumbnailsCacheFolderName = "Cache";
        const string FacialSyncFolderName = "FacialSync";

        const string ReadmeFolderName = "readme.txt";
        const string ReadmeJAFileName = "readme_ja.txt";
        const string ReadmeENFileName = "readme_en.txt";
        const string DefectFileName = "不具合・Defect.txt";
        const string JsonFileName = "System.json";

        public static string AppFolderPath;
        public static string DownloadFolderPath;

        static PathsInfo()
        {
#if UNITY_EDITOR
            AppFolderPath = "D:/User/UniLiveViewer";
            DownloadFolderPath = "D:/User/Download";
#elif UNITY_ANDROID
            AppFolderPath = "/storage/emulated/0/UniLiveViewer";
            DownloadFolderPath = "/storage/emulated/0/Download";
            //AppFolderPath = $"{Application.persistentDataPath}/Custom";
#endif
        }

        public static string GetCharaFolderPath()
            => Path.Combine(AppFolderPath + "/", "Chara");
        public static string GetFullPath(FolderType type)
            => Path.Combine(AppFolderPath + "/", type.AsString());
        public static string GetThumbnailsFolderPath()
            => Path.Combine(AppFolderPath + "/", FolderType.Actor.AsString() + "/", ThumbnailsCacheFolderName);
        public static string GetFacialSyncFolderPath()
            => Path.Combine(AppFolderPath + "/", FolderType.Motion.AsString() + "/", FacialSyncFolderName);

        public static string GetDownloadFolderPath() => DownloadFolderPath;
        public static string GetReadmeFolderPath()
            => Path.Combine(AppFolderPath + "/", ReadmeFolderName);

        public static string GetReadmeFolderPath(SystemLanguage systemLanguage)
        {
            return systemLanguage.ToResourceIndex() switch
            {
                0 => Path.Combine(AppFolderPath + "/", ReadmeENFileName),
                1 => Path.Combine(AppFolderPath + "/", ReadmeJAFileName),
                _ => "",
            };
        }

        public static string GetDefectFolderPath()
            => Path.Combine(AppFolderPath + "/", DefectFileName);
        public static string GetJSONFolderPath()
            => Path.Combine(Application.persistentDataPath + "/", JsonFileName);
        //Application.temporaryCachePath
    }
}
