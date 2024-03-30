using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniLiveViewer
{
    public static class FileReadAndWriteUtility
    {
        static string PathOffset = PathsInfo.GetFullPath(FolderType.SETTING) + "/" + "MotionOffset.txt";
        static string PathPair = PathsInfo.GetFullPath(FolderType.SETTING) + "/" + "MotionFacialPair.txt";

        /// <summary>
        /// モーションファイル名とoffset値
        /// </summary>
        public static IReadOnlyDictionary<string, int> GetMotionOffset => _motionOffsetMap;
        static Dictionary<string, int> _motionOffsetMap = new();

        /// <summary>
        /// 基準モーションに同期するファイル名を取得、ペアが無い場合はnull
        /// </summary>
        /// <param name="baseMotion"></param>
        /// <returns></returns>
        public static string TryGetSyncFileName(string baseMotion)
            => map_MotionFacialPair.ContainsKey(baseMotion) ? map_MotionFacialPair[baseMotion] : null;
        static Dictionary<string, string> map_MotionFacialPair = new();

        public static UserProfile UserProfile { get; private set; }

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
            UserProfile = result;
            return result;
        }

        /// <summary>
        /// Jsonファイルに書き込む
        /// </summary>
        /// <param name="lang"></param>
        public static void WriteJson(UserProfile data)
        {
            //Json形式に変換
            var path = PathsInfo.GetFullPath_JSON();
            var jsonstr = JsonUtility.ToJson(data, true);
            using (var writer = new StreamWriter(path, false))
            {
                writer.Write(jsonstr);
                //writer.Flush();
                //writer.Close();
            }
        }

        /// <summary>
        /// MotionOffsetファイルに情報をセット
        /// </summary>
        /// <param name="baseMotion"></param>
        /// <param name="val"></param>
        public static void SetMotionOffset(string baseMotion, int val)
        {
            if (!baseMotion.Contains(".vmd")) return;
            _motionOffsetMap[baseMotion] = val;
        }


        /// <summary>
        /// ダンスモーションの再生位置書き込み
        /// </summary>
        public static void SaveMotionOffset()
        {
            using (var writer = new StreamWriter(PathOffset, false, System.Text.Encoding.UTF8))
            {
                foreach (var e in _motionOffsetMap)
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
            if (_motionOffsetMap.Count != 0)
            {
                _motionOffsetMap.Clear();
            }

            //offset情報ファイルがあれば読み込む
            if (!File.Exists(PathOffset)) return false;

            foreach (var line in File.ReadLines(PathOffset))
            {
                var spl = line.Split(',');
                if (spl.Length != 2) return false;
                if (spl[0] == "" || spl[1] == "") return false;
                _motionOffsetMap.Add(spl[0], int.Parse(spl[1]));
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
            using (var writer = new StreamWriter(PathPair, false, System.Text.Encoding.UTF8))
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

            foreach (var line in File.ReadLines(PathPair))
            {
                var spl = line.Split(',');
                if (spl.Length != 2) return false;
                if (spl[0] == "" || spl[1] == "") return false;
                map_MotionFacialPair.Add(spl[0], spl[1]);
            }
            return true;
        }
    }
}
