using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace UniLiveViewer
{
    public class SystemInfo
    {
        //レイヤー
        public static int layerNo_Default = LayerMask.NameToLayer("Default");
        public static int layerNo_VirtualHead = LayerMask.NameToLayer("VirtualHead");
        public static int layerNo_IgnoreRaycats = LayerMask.NameToLayer("Ignore Raycast");
        public static int layerNo_UI = LayerMask.NameToLayer("UI");
        public static int layerNo_FieldObject = LayerMask.NameToLayer("FieldObject");
        public static int layerNo_GrabObject = LayerMask.NameToLayer("GrabObject");
        public static int layerNo_UnRendererFeature = LayerMask.NameToLayer("UnRendererFeature");
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

        //一括ボタンカラー(仮)
        public static readonly Color btnColor_Ena_sky = new Color(0, 1, 1, 1);
        public static readonly Color btnColor_Dis = new Color(0.4f, 0.4f, 0.4f, 1);

        //召喚上限(CRS/KAGURA/VIEW/GYM)
        public static readonly int[] MAXCHARA_QUEST1 = { 2, 2, 4, 2 };
        public static readonly int[] MAXCHARA_QUEST2 = { 3, 2, 5, 3 };
        public static readonly int[] MAXCHARA_EDITOR = { 5, 5, 5, 5 };

        public static void Init()
        {
            string sName = SceneManager.GetActiveScene().name;
            if (sName == "LiveScene") sceneMode = SceneMode.CANDY_LIVE;
            else if (sName == "KAGURAScene") sceneMode = SceneMode.KAGURA_LIVE;
            else if (sName == "ViewerScene") sceneMode = SceneMode.VIEWER;
            else if (sName == "GymnasiumScene") sceneMode = SceneMode.GYMNASIUM;

            userProfile = FileReadAndWriteUtility.ReadJson();
        }
    }

    public enum SceneMode
    {
        CANDY_LIVE,
        KAGURA_LIVE,
        VIEWER,
        GYMNASIUM,
    }
    
    public enum USE_LANGUAGE  
    {
        NULL,   
        JP,     
        EN,
        KO//未使用
    }
}
