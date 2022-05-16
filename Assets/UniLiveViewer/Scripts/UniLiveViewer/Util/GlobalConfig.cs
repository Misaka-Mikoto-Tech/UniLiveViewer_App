using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace UniLiveViewer 
{
    public static class SystemInfo
    {
        //レイヤー
        public static int layerNo_Default = LayerMask.NameToLayer("Default");
        public static int layerNo_VirtualHead = LayerMask.NameToLayer("VirtualHead");
        public static int layerNo_UI = LayerMask.NameToLayer("UI");
        public static int layerNo_FieldObject = LayerMask.NameToLayer("FieldObject");
        public static int layerNo_GrabObject = LayerMask.NameToLayer("GrabObject");
        public static int layerMask_Default = LayerMask.GetMask("Default");
        public static int layerMask_VirtualHead = LayerMask.GetMask("VirtualHead");
        public static int layerMask_StageFloor = LayerMask.GetMask("Stage_Floor");
        public static int layerMask_FieldObject = LayerMask.GetMask("FieldObject");
        //タグ
        public static readonly string tag_ItemMaterial = "ItemMaterial";
        public static readonly string tag_GrabChara = "Grab_Chara";
        public static readonly string tag_GrabSliderVolume = "Grab_Slider_Volume";

        public static UserProfile userProfile;
        public static SceneMode sceneMode;
        public static float soundVolume_SE = 0.3f;//SE音量
        public static OVRManager.FixedFoveatedRenderingLevel levelFFR = OVRManager.FixedFoveatedRenderingLevel.Medium;//中心窩レンダリング
        public static string folderPath_Persistent;//システム設定値など
        public static Dictionary<string, int> dicVMD_offset = new Dictionary<string, int>();

        //一括ボタンカラー(仮)
        public static readonly Color btnColor_Ena_sky = new Color(0, 1, 1, 1);
        public static readonly Color btnColor_Dis = new Color(0.4f, 0.4f, 0.4f, 1);

        //召喚上限(CRS/KAGURA/VIEW)
        public static readonly byte[] MAXCHARA_QUEST1 = { 2, 2, 4 };
        public static readonly byte[] MAXCHARA_QUEST2 = { 3, 2, 5 };
        public static readonly byte[] MAXCHARA_EDITOR = { 5, 5, 5 };

        public static readonly byte MAXAUDIO_EDITOR = 30;
        public static readonly byte MAXAUDIO_QUEST = 10;

        public static void Init()
        {
            string sName = SceneManager.GetActiveScene().name;
            if (sName == "LiveScene") sceneMode = SceneMode.CANDY_LIVE;
            else if (sName == "KAGURAScene") sceneMode = SceneMode.KAGURA_LIVE;
            else if (sName == "ViewerScene") sceneMode = SceneMode.VIEWER;

            userProfile = FileAccessManager.ReadJson();
        }
    }

    public enum SceneMode
    {
        CANDY_LIVE,
        KAGURA_LIVE,
        VIEWER,
    }
    
    public enum USE_LANGUAGE  
    {
        NULL,   
        JP,     
        EN,
        KO//未使用
    }

    public class GlobalConfig : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = -1;
        [SerializeField] private int shaderLOD = 1000;
        [Header("＜Debug.Log()を一括で切り替える＞")]
        [SerializeField] private bool isDebug = true;
        private TimelineController timeline = null;

        private void Awake()
        {
            SystemInfo.Init();

            if (targetFrameRate > 0) Application.targetFrameRate = targetFrameRate;
            Shader.globalMaximumLOD = shaderLOD;

#if UNITY_EDITOR
            //デバッグログを一括で有効・無効化
            Debug.unityLogger.logEnabled = isDebug;
#elif UNITY_ANDROID
            //デバッグログを一括で無効化
            Debug.unityLogger.logEnabled = false;

            //中心以外の描画のレベルを下げる(最大値)
            OVRManager.fixedFoveatedRenderingLevel = SystemInfo.levelFFR;
            //描画負荷に応じてfixedFoveatedRenderingLevelを自動的に調整する
            OVRManager.useDynamicFixedFoveatedRendering = true;
#endif
        }

        void Start()
        {
            if (GetActiveSceneName() == "TitleScene") return;

            timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            //オキュラスのホームボタントリガーの対応
            OVRManager.InputFocusLost += HomePause;
            OVRManager.InputFocusAcquired += HomeReStart;
            OVRManager.HMDUnmounted += HomePause;//HMDが外された
            OVRManager.HMDMounted += HomeReStart;//HMDが付けられた
        }

        public static string GetActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        private void HomePause()
        {
            timeline.TimelineManualMode().Forget();
            Time.timeScale = 0;
        }

        private void HomeReStart()
        {
            Time.timeScale = 1;
        }
    }
}
