using UnityEngine;

namespace UniLiveViewer
{
    /// <summary>
    /// まだ検証中、未使用
    /// https://developer.android.com/reference/android/content/Intent#ACTION_OPEN_DOCUMENT
    /// </summary>
    public class StorageAccessFramework : MonoBehaviour
    {
        public AndroidJavaObject _currentActivity;
        const int REQUEST_CODE = 42; // 任意のリクエストコード

        void Start()
        {
            // UnityPlayerから現在のアクティビティを取得
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            _currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            // SAFでファイルピッカーを開く
            OpenFolderPicker();

            HandleActivityResult();
        }

        //ファイル限定
        void OpenFilePicker()
        {
            // Intentを作成してSAFを使うためのコード
            var intentClass = new AndroidJavaClass("android.content.Intent");
            var intent = new AndroidJavaObject("android.content.Intent");

            intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_OPEN_DOCUMENT"));
            intent.Call<AndroidJavaObject>("addCategory", intentClass.GetStatic<string>("CATEGORY_OPENABLE"));
            intent.Call<AndroidJavaObject>("setType", "*/*"); // すべてのファイルタイプを選択

            // Activityを開始する
            _currentActivity.Call("startActivityForResult", intent, REQUEST_CODE);
        }

        // フォルダの場合
        public void OpenFolderPicker()
        {
            // インテント作成
            var intentClass = new AndroidJavaClass("android.content.Intent");
            var intent = new AndroidJavaObject("android.content.Intent");

            // フォルダ選択用のインテントを設定
            intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_OPEN_DOCUMENT_TREE"));

            // インテントを実行
            _currentActivity.Call("startActivityForResult", intent, REQUEST_CODE);
        }

        //アクセスの永続化
        public void PersistenceAccess(AndroidJavaObject folderUri)
        {
            var contentResolver = _currentActivity.Call<AndroidJavaObject>("getContentResolver");

            // Intentの静的定数を取得
            var intentClass = new AndroidJavaClass("android.content.Intent");
            var flagGrantWrite = intentClass.GetStatic<int>("FLAG_GRANT_WRITE_URI_PERMISSION");
            var flagGrantRead = intentClass.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION");

            // フラグを適用
            contentResolver.Call("takePersistableUriPermission", folderUri, flagGrantWrite | flagGrantRead);
        }

        // onActivityResultを処理するためのハンドラーを設定
        public void HandleActivityResult()
        {
            var resultHandler = new FolderPickerResultHandler(this);
        }
    }


    public class FolderPickerResultHandler : AndroidJavaProxy
    {
        StorageAccessFramework _storageAccessFramework;

        public FolderPickerResultHandler(StorageAccessFramework framework)
            : base("android.app.Activity")
        {
            _storageAccessFramework = framework;
        }

        public void onActivityResult(int requestCode, int resultCode, AndroidJavaObject data)
        {
            if (requestCode == 42 && resultCode == -1) // RESULT_OK
            {
                var folderUri = data.Call<AndroidJavaObject>("getData"); // ここでfolderUriを取得
                Debug.Log("Selected folder URI: " + folderUri.Call<string>("toString"));

                // 永続的なアクセス許可を取得
                _storageAccessFramework.PersistenceAccess(folderUri);

                //ここでサブディレクトリにとしてフォルダ作成
            }
        }
    }
}