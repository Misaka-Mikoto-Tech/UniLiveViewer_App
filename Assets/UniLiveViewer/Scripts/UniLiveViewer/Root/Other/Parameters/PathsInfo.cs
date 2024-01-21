using System.IO;

namespace UniLiveViewer
{
    public class PathsInfo
    {
        static string folderPath_Persistent = UnityEngine.Application.persistentDataPath;
        //static string folderPath_Persistent = UnityEngine.Application.temporaryCachePath;

        //個別
        static string[] folderName = { "Chara", "Motion", "BGM", "Setting" };
        const string cachePath = "Cache";
        const string lipSyncPath = "Lip-sync";
        
        public static int folder_length = folderName.Length;

        const string readme_ja = "readme_ja.txt";
        const string readme_en = "readme_en.txt";
        const string defect = "不具合・Defect.txt";
        const string json = "System.json";

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
#endif
        }

        public static string GetFullPath(FolderType type)
        {
            return Path.Combine(AppFolderPath + "/", folderName[(int)type]);
        }

        public static string GetFullPath_ThumbnailCache()
        {
            return Path.Combine(AppFolderPath + "/", folderName[(int)FolderType.CHARA] + "/", cachePath);
        }

        public static string GetFullPath_LipSync()
        {
            return Path.Combine(AppFolderPath + "/", folderName[(int)FolderType.MOTION] + "/", lipSyncPath);
        }

        public static string GetFullPath_Download()
        {
            return DownloadFolderPath;
        }

        public static string GetFullPath_README(LanguageType language)
        {
            if (language == LanguageType.EN) return Path.Combine(AppFolderPath + "/", readme_en);
            else if (language == LanguageType.JP) return Path.Combine(AppFolderPath + "/", readme_ja);
            else return "";
        }
        public static string GetFullPath_DEFECT()
        {
            return Path.Combine(AppFolderPath + "/", defect);
        }

        public static string GetFullPath_JSON()
        {
            return Path.Combine(folderPath_Persistent + "/", json);
        }
    }
}
