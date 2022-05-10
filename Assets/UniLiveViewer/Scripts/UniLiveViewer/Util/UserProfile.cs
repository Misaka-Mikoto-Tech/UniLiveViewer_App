using System.IO;
using UnityEngine;

namespace UniLiveViewer
{
    public class UserProfile
    {
        public class Data
        {
            public int LanguageCode = 0;
            public float InitCharaSize = 1.00f;
            public float CharaShadow = 1.00f;
            public int CharaShadowType = 1;
            public float VMDScale = 0.750f;
            public bool TouchVibration = true;

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

        public Data data;

        /// <summary>
        /// Jsonファイルを読み込んでクラスに変換
        /// </summary>
        /// <returns></returns>
        public void ReadJson()
        {
            string datastr = "";
            StreamReader reader = null;
            if (File.Exists(FileAccessManager.folderPath_Persistent + "System.json"))
            {
                using (reader = new StreamReader(FileAccessManager.folderPath_Persistent + "System.json"))
                {
                    datastr = reader.ReadToEnd();
                    //reader.Close();
                }
                data = JsonUtility.FromJson<Data>(datastr);
            }
            else
            {
                //新規作成して読み込み直す
                data = new Data();
                WriteJson();
                ReadJson();
            }
        }

        /// <summary>
        /// Jsonファイルに書き込む
        /// </summary>
        /// <param name="lang"></param>
        public void WriteJson()
        {

            //Json形式に変換
            string jsonstr = JsonUtility.ToJson(data,true);
            using (StreamWriter writer = new StreamWriter(FileAccessManager.folderPath_Persistent + "System.json", false))
            {
                writer.Write(jsonstr);
                //writer.Flush();
                //writer.Close();
            }
        }
        /// <summary>
        /// ダンスモーションの再生位置書き込み
        /// </summary>
        public void SaveOffset()
        {
            //書き込み
            string path = FileAccessManager.GetFullPath(FOLDERTYPE.SETTING);
            using (StreamWriter writer = new StreamWriter(path + "MotionOffset.txt", false, System.Text.Encoding.UTF8))
            {
                foreach (var e in SystemInfo.dicVMD_offset)
                {
                    writer.WriteLine(e.Key + "," + e.Value);
                }
            }
        }
    }
}