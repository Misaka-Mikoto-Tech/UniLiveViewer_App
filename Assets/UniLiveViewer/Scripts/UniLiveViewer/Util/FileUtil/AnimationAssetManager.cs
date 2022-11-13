using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer 
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class AnimationAssetManager : MonoBehaviour
    {
        CancellationToken cancellation_token;

        public List<string> vmdList = new List<string>();
        public List<string> vmdLipSyncList = new List<string>();

        void Awake()
        {
            cancellation_token = this.GetCancellationTokenOnDestroy();
        }

        /// <summary>
        /// アプリフォルダ内のVMDファイル名を取得
        /// </summary>
        /// <returns></returns>
        public bool CheckOffsetFile()
        {
            //初期化
            if (SystemInfo.dicVMD_offset.Count != 0)
            {
                SystemInfo.dicVMD_offset.Clear();
            }

            //offset情報ファイルがあれば読み込む
            string path = PathsInfo.GetFullPath(FOLDERTYPE.SETTING) + "/MotionOffset.txt";
            if (File.Exists(path))
            {
                foreach (string line in File.ReadLines(path))
                {
                    string[] spl = line.Split(',');
                    if (spl.Length != 2) return false;
                    if(spl[0]==""|| spl[1] == "") return false;
                    SystemInfo.dicVMD_offset.Add(spl[0], int.Parse(spl[1]));
                }
            }

            //VMDファイル名を取得
            string sFolderPath = PathsInfo.GetFullPath(FOLDERTYPE.MOTION) + "/";
            try
            {
                //VMDファイルのみ検索
                var names = Directory.GetFiles(sFolderPath, "*.vmd", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = names[i].Replace(sFolderPath, "");

                    //ファイル名に区切りのカンマが含まれると困る
                    if (names[i].Contains(",")) return false;
                    else
                    {
                        vmdList.Add(names[i]);

                        //既存offset情報がなければ追加
                        if (!SystemInfo.dicVMD_offset.ContainsKey(names[i]))
                        {
                            SystemInfo.dicVMD_offset.Add(names[i], 0);
                        }
                    }
                }

                //一旦保存
                FileAccessManager.SaveOffset();
            }
            catch
            {
                Debug.Log("VMDファイル読み込みに失敗しました");
                return false;
            }

            GetAllVMDLipSyncNames();//仮でここ
            return true;
        }

        /// <summary>
        /// アプリフォルダ内のVMDファイル名を取得
        /// </summary>
        /// <returns></returns>
        void GetAllVMDLipSyncNames()
        {
            //VMDファイル名を取得
            string sFolderPath = PathsInfo.GetFullPath_LipSync() + "/";
            try
            {
                //VRMファイルのみ検索
                var names = Directory.GetFiles(sFolderPath, "*.vmd", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = names[i].Replace(sFolderPath, "");
                    vmdLipSyncList.Add(names[i]);
                }
            }
            catch
            {
                Debug.Log("VMDLipSyncファイル読み込みに失敗しました");
            }
        }

    }

}
