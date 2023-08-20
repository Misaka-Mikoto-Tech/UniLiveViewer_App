using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniLiveViewer 
{
    public static class FileReadAndWriteUtility
    {
        static string PathOffset = PathsInfo.GetFullPath(FOLDERTYPE.SETTING) + "/" + "MotionOffset.txt";
        static string PathPair = PathsInfo.GetFullPath(FOLDERTYPE.SETTING) + "/" + "MotionFacialPair.txt";

        /// <summary>
        /// モーションファイル名とoffset値
        /// </summary>
        public static IReadOnlyDictionary<string, int> GetMotionOffset => map_MotionOffset;
        static Dictionary<string, int> map_MotionOffset = new Dictionary<string, int>();
        /// <summary>
        /// モーションファイル名と表情ファイル名
        /// </summary>
        public static IReadOnlyDictionary<string, string> GetMotionFacialPair => map_MotionFacialPair;
        public static Dictionary<string, string> map_MotionFacialPair = new Dictionary<string, string>();

        /// <summary>
        /// Jsonファイルを読み込んでクラスに変換
        /// </summary>
        /// <returns></returns>
        public static UserProfile ReadJson()
        {
            UserProfile result;

            var path = PathsInfo.GetFullPath_JSON();
            var datastr = "";
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
        /// MotionOffsetファイルに情報をセット
        /// </summary>
        /// <param name="sName"></param>
        /// <param name="val"></param>
        public static void SetMotionOffset(string sName, int val)
        {
            if (!sName.Contains(".vmd")) return;
            map_MotionOffset[sName] = val;
        }

        
        /// <summary>
        /// ダンスモーションの再生位置書き込み
        /// </summary>
        public static void SaveMotionOffset()
        {
            //書き込み
            using (StreamWriter writer = new StreamWriter(PathOffset, false, System.Text.Encoding.UTF8))
            {
                foreach (var e in map_MotionOffset)
                {
                    writer.WriteLine(e.Key + "," + e.Value);
                }
            }
        }

        /// <summary>
        /// ダンスモーションのオフセット情報を読み込み直す
        /// そもそもファイルが無ければfalse
        /// </summary>
        public static bool TryLoadMotionOffset()
        {
            //初期化
            if (map_MotionOffset.Count != 0)
            {
                map_MotionOffset.Clear();
            }

            //offset情報ファイルがあれば読み込む
            if (!File.Exists(PathOffset)) return false;

            foreach (string line in File.ReadLines(PathOffset))
            {
                string[] spl = line.Split(',');
                if (spl.Length != 2) return false;
                if (spl[0] == "" || spl[1] == "") return false;
                map_MotionOffset.Add(spl[0], int.Parse(spl[1]));
            }
            return true;
        }

        /// <summary>
        /// ダンスモーションと表情のファイル名ペアを保存、書き込み
        /// </summary>
        public static void SaveMotionFacialPair(string motionfileName, string faciaFileName)
        {
            // NOTE: faciaFileNameは「No-LipSyncData」枠があるので.vmd確認しない
            if (!motionfileName.Contains(".vmd")) return;
            map_MotionFacialPair[motionfileName] = faciaFileName;

            //書き込み
            using (StreamWriter writer = new StreamWriter(PathPair, false, System.Text.Encoding.UTF8))
            {
                foreach (var e in map_MotionFacialPair)
                {
                    writer.WriteLine(e.Key + "," + e.Value);
                }
            }
        }

        /// <summary>
        /// ダンスモーションのオフセット情報を読み込み直す
        /// そもそもファイルが無ければfalse
        /// </summary>
        public static bool TryLoadMotionFacialPair()
        {
            //初期化
            if (map_MotionFacialPair.Count != 0)
            {
                map_MotionFacialPair.Clear();
            }

            //offset情報ファイルがあれば読み込む
            if (!File.Exists(PathPair)) return false;

            foreach (string line in File.ReadLines(PathPair))
            {
                string[] spl = line.Split(',');
                if (spl.Length != 2) return false;
                if (spl[0] == "" || spl[1] == "") return false;
                map_MotionFacialPair.Add(spl[0], spl[1]);
            }
            return true;
        }
    }
}
