using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniLiveViewer
{
    public class AnimationAssetManager
    {
        public IReadOnlyList<string> VmdList => _vmdList;
        List<string> _vmdList = new List<string>();
        public IReadOnlyList<string> VmdLipSyncList => _vmdLipSyncList;
        List<string> _vmdLipSyncList = new List<string>();

        /// <summary>
        /// アプリフォルダ内のVMDファイル名を取得
        /// </summary>
        public bool CheckOffsetFile()
        {
            if (!FileReadAndWriteUtility.TryLoadMotionOffset()) return false;
            if (!FileReadAndWriteUtility.TryLoadMotionFacialPair()) return false;

            _vmdList.Clear();

            var folderPath = PathsInfo.GetFullPath(FOLDERTYPE.MOTION) + "/";
            try
            {
                var names = Directory.GetFiles(folderPath, "*.vmd", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = names[i].Replace(folderPath, "");

                    //ファイル名に区切りのカンマが含まれると困る
                    if (names[i].Contains(",")) return false;
                    else
                    {
                        _vmdList.Add(names[i]);

                        //既存offset情報がなければ追加
                        if (!FileReadAndWriteUtility.GetMotionOffset.ContainsKey(names[i]))
                        {
                            FileReadAndWriteUtility.SetMotionOffset(names[i], 0);
                        }
                    }
                }
                //一旦保存
                FileReadAndWriteUtility.SaveMotionOffset();
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
            var folderPath = PathsInfo.GetFullPath_LipSync() + "/";
            try
            {
                var names = Directory.GetFiles(folderPath, "*.vmd", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = names[i].Replace(folderPath, "");
                    _vmdLipSyncList.Add(names[i]);
                }
            }
            catch
            {
                Debug.Log("VMDLipSyncファイル読み込みに失敗しました");
            }
        }
    }
}
