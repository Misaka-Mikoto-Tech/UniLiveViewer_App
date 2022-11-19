using Cysharp.Threading.Tasks;
using System;
using System.IO;
using UnityEngine;

namespace UniLiveViewer 
{
    public static class FileReadAndWriteUtility
    {
        /// <summary>
        /// Jsonファイルを読み込んでクラスに変換
        /// </summary>
        /// <returns></returns>
        public static UserProfile ReadJson()
        {
            UserProfile result;

            string path = PathsInfo.GetFullPath_JSON();
            string datastr = "";
            StreamReader reader = null;
            if (File.Exists(path))
            {
                using (reader = new StreamReader(path))
                {
                    datastr = reader.ReadToEnd();
                    //reader.Close();
                }
                result = JsonUtility.FromJson<UserProfile>(datastr);
            }
            else
            {
                //新規作成して読み込み直す
                result = new UserProfile();
                WriteJson(result);
            }
            return result;
        }

        /// <summary>
        /// Jsonファイルに書き込む
        /// </summary>
        /// <param name="lang"></param>
        public static void WriteJson(UserProfile data)
        {
            //Json形式に変換
            string path = PathsInfo.GetFullPath_JSON();
            string jsonstr = JsonUtility.ToJson(data, true);
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.Write(jsonstr);
                //writer.Flush();
                //writer.Close();
            }
        }

        /// <summary>
        /// ダンスモーションの再生位置書き込み
        /// </summary>
        public static void SaveOffset()
        {
            //書き込み
            string path = PathsInfo.GetFullPath(FOLDERTYPE.SETTING) + "/";
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
