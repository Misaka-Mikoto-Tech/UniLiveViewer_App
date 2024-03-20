using UniLiveViewer.SceneLoader;

namespace UniLiveViewer
{
    public static class SystemInfo
    {
        //public static UserProfile UserProfile { get; private set; }

        public static float soundVolume_SE = 0.3f;//SE音量
        public static OVRManager.FixedFoveatedRenderingLevel levelFFR = OVRManager.FixedFoveatedRenderingLevel.Medium;//中心窩レンダリング
        public static string folderPath_Persistent;//システム設定値など

        //召喚上限(Title/CRS/KAGURA/VIEW/GYM/VILLAGE)
        public static readonly int[] MAXCHARA_QUEST1 = { 0, 2, 2, 4, 2, 2 };
        public static readonly int[] MAXCHARA_QUEST2 = { 0, 3, 2, 5, 3, 3 };
        public static readonly int[] MAXCHARA_QUEST3 = { 0, 3, 2, 5, 3, 3 };
        public static readonly int[] MAXCHARA_EDITOR = { 0, 5, 5, 5, 5, 5 };

        /// <summary>
        /// フィールドに存在できる最大キャラ数
        /// </summary>
        public static int MaxFieldChara => _maxFieldChara;
        static int _maxFieldChara;

        public static void CheckMaxFieldChara(SceneType sceneType)
        {
            // SDK前提だがLinqまで識別できる
            // UnityEngine.SystemInfo.deviceNameもある[Meta Quest/Meta Quest 2]
            var type = OVRPlugin.GetSystemHeadsetType();
            switch (type)
            {
                case OVRPlugin.SystemHeadset.Oculus_Quest:
                    _maxFieldChara = MAXCHARA_QUEST1[(int)sceneType];
                    break;
                case OVRPlugin.SystemHeadset.Oculus_Link_Quest:
                    _maxFieldChara = MAXCHARA_QUEST1[(int)sceneType];
                    break;
                case OVRPlugin.SystemHeadset.Oculus_Quest_2:
                    _maxFieldChara = MAXCHARA_QUEST2[(int)sceneType];
                    break;
                case OVRPlugin.SystemHeadset.Oculus_Link_Quest_2:
                    _maxFieldChara = MAXCHARA_QUEST2[(int)sceneType];
                    break;
                default:
                    _maxFieldChara = MAXCHARA_EDITOR[(int)sceneType];
                    break;
            }
        }
    }
}
