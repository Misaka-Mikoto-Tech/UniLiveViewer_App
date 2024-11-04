using UnityEngine;

namespace UniLiveViewer
{
    /// <summary>
    /// 注意、過剰権限
    /// https://communityforums.atmeta.com/t5/Quest-Development/Scoped-Storage-and-VR/m-p/1043602
    /// </summary>
    public class GrantStoragePermission
    {
        static bool m_FolderPermissionOverride = false;

        public void TryGranting()
        {
#if UNITY_ANDROID
            if (!UserHasManageExternalStoragePermission())
            {
                AskForManageStoragePermission();
            }
#endif
        }

        /// <summary>
        /// 管理権限を既に持っているか
        /// </summary>
        public static bool UserHasManageExternalStoragePermission()
        {
#if UNITY_EDITOR
            return true;
#elif UNITY_ANDROID
        var isExternalStorageManager = false;
        try
        {
            var environmentClass = new AndroidJavaClass("android.os.Environment");
            isExternalStorageManager = environmentClass.CallStatic<bool>("isExternalStorageManager");
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("Java Exception caught and ignored: " + e.Message);
            Debug.LogError("Assuming this means this device doesn't support isExternalStorageManager.");
        }
        return m_FolderPermissionOverride || isExternalStorageManager;
#endif

        }

        /// <summary>
        /// ストレージの管理許可を求める
        /// </summary>
        void AskForManageStoragePermission()
        {
            try
            {
                using var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                string packageName = currentActivityObject.Call<string>("getPackageName");
                using var uriClass = new AndroidJavaClass("android.net.Uri");
                using var uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null);
                using var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.MANAGE_APP_ALL_FILES_ACCESS_PERMISSION", uriObject);
                intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                currentActivityObject.Call("startActivity", intentObject);
            }
            catch (AndroidJavaException e)
            {
                m_FolderPermissionOverride = true;
                Debug.LogError("Java Exception caught and ignored: " + e.Message);
                Debug.LogError("Assuming this means we don't need android.settings.MANAGE_APP_ALL_FILES_ACCESS_PERMISSION (e.g., Android SDK < 30)");
            }
        }
    }
}