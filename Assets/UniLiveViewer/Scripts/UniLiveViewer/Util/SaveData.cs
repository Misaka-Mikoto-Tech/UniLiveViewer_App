using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniLiveViewer
{
    public class SaveData : Object
    {
        public static string folderPath_Persistent;//システム設定値など

        public enum USE_LANGUAGE
        {
            NULL,
            JP,
            EN,
            KO//未使用
        }

        public static Dictionary<string, int> dicVMD_offset = new Dictionary<string, int>();

        /// <summary>
        /// Jsonファイルを読み込んでクラスに変換
        /// </summary>
        /// <returns></returns>
        public static SystemData GetJson_SystemData()
        {
            string datastr = "";
            StreamReader reader = null;
            if (File.Exists(Application.persistentDataPath + "/System.json"))
            {
                using (reader = new StreamReader(Application.persistentDataPath + "/System.json"))
                {
                    datastr = reader.ReadToEnd();
                    reader.Close();
                }

            }
            return JsonUtility.FromJson<SystemData>(datastr);
        }

        /// <summary>
        /// Jsonファイルに書き込む
        /// </summary>
        /// <param name="lang"></param>
        public static void SetJson_SystemData(SystemData systemData)
        {
            //Json形式に変換
            string jsonstr = JsonUtility.ToJson(systemData);

            StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/System.json", false);
            writer.Write(jsonstr);
            writer.Flush();
            writer.Close();
        }
        /// <summary>
        /// ダンスモーションの再生位置書き込み
        /// </summary>
        public static void SaveOffset()
        {
            //書き込み
            string path = FileAccessManager.GetFullPath(FileAccessManager.FOLDERTYPE.SETTING);
            using (StreamWriter writer = new StreamWriter(path + "MotionOffset.txt", false, System.Text.Encoding.UTF8))
            {
                foreach (var e in dicVMD_offset)
                {
                    writer.WriteLine(e.Key + "," + e.Value);
                }
            }
        }
    }

    [System.Serializable]
    public class SystemData
    {
        public int LanguageCode = 0;
        public float InitCharaSize = 1;

        public bool scene_crs_particle = true;
        public bool scene_crs_laser = true;
        public bool scene_crs_reflection = true;
        public bool scene_crs_sonic = true;
        public bool scene_crs_manual = true;

        public bool scene_kagura_particle = true;
        public bool scene_kagura_sea = true;
        public bool scene_kagura_reflection = true;

        public bool scene_view_led = true;
    }
}