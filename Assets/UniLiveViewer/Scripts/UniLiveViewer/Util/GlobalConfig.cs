using UnityEngine;
using System.Collections;

namespace UniLiveViewer 
{
    public static class Parameters
    {
        //レイヤー
        public static int layerNo_VirtualHead;
        public static int layerNo_UI;
        public static int layerNo_FieldObject;
        public static int layerMask_VirtualHead;
        public static int layerMask_StageFloor;
        public static int layerMask_FieldObject;

        //タグ
        public static readonly string tag_ItemMaterial = "ItemMaterial";
        public static readonly string tag_GrabChara = "Grab_Chara";
        public static readonly string tag_GrabSliderVolume = "Grab_Slider_Volume";
    }

    public class GlobalConfig : MonoBehaviour
    {
        public enum SceneMode
        {
            CANDY_LIVE,
            KAGURA_LIVE,
            VIEWER,
        }
        [SerializeField] private SceneMode sceneMode = SceneMode.CANDY_LIVE;
        public static SceneMode sceneMode_static;

        public int targetFrameRate = -1;
        public int shaderLOD = 1000;
        [Header("＜Debug.Log()を一括で切り替える＞")]
        public bool isDebug = true;
        [Header("＜オキュラス固有＞")]
        public OVRManager.FixedFoveatedRenderingLevel Level;
        public static float soundVolume_SE = 0.3f;
        public static bool isControllerVibration = true;

        public static SystemData systemData;

        public static readonly Color btnColor_Ena = new Color(0, 1, 1, 1);//一旦ここで一括調整
        public static readonly Color btnColor_Dis = new Color(0.4f, 0.4f, 0.4f, 1);

        //public static float initCharaSize = 0.0f;
        public Vector3 rotete = Vector3.zero;

        private void Awake()
        {
            Parameters.layerNo_VirtualHead = LayerMask.NameToLayer("VirtualHead");
            Parameters.layerNo_UI = LayerMask.NameToLayer("UI");
            Parameters.layerNo_FieldObject = LayerMask.NameToLayer("FieldObject");

            //Parameters.layerask_VirtualHead = 1 << Parameters.layerNo_VirtualHead;// ビットシフトでもいい
            Parameters.layerMask_VirtualHead = LayerMask.GetMask("VirtualHead");
            Parameters.layerMask_StageFloor = LayerMask.GetMask("Stage_Floor");
            Parameters.layerMask_FieldObject = LayerMask.GetMask("FieldObject");


            if (targetFrameRate > 0) Application.targetFrameRate = targetFrameRate;

            Shader.globalMaximumLOD = shaderLOD;

            //Cursor.visible = false;

            sceneMode_static = sceneMode;

            //中心以外の描画のレベルを下げる(最大値)
            OVRManager.fixedFoveatedRenderingLevel = Level;

            //描画負荷に応じてfixedFoveatedRenderingLevelを自動的に調整する
            OVRManager.useDynamicFixedFoveatedRendering = true;

            systemData = SaveData.GetJson_SystemData();
            if (systemData == null) systemData = new SystemData();

#if UNITY_EDITOR
            //デバッグログを一括で有効・無効化
            Debug.unityLogger.logEnabled = isDebug;
#elif UNITY_ANDROID
            //デバッグログを一括で無効化
            Debug.unityLogger.logEnabled = false;
#endif
        }

        void Start()
        {
            //オキュラスのホームボタントリガーの対応
            OVRManager.InputFocusLost += HomePause;
            OVRManager.InputFocusAcquired += HomeReStart;
            //HMDが外された
            OVRManager.HMDUnmounted += HomePause;
            //HMDが付けられた
            OVRManager.HMDMounted += HomeReStart;
        }

        private void HomePause()
        {
            Time.timeScale = 0;
        }

        private void HomeReStart()
        {
            Time.timeScale = 1;
        }
    }
}
