using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer 
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class AnimationAssetManager : MonoBehaviour
    {
        CancellationToken cancellation_token;
        public IReadOnlyList<string> VmdList => _vmdList;
        List<string> _vmdList = new List<string>();
        public IReadOnlyList<string> VmdLipSyncList => _vmdLipSyncList;
        List<string> _vmdLipSyncList = new List<string>();

        void Awake()
        {
            cancellation_token = this.GetCancellationTokenOnDestroy();
        }

        /// <summary>
        /// アプリフォルダ内のVMDファイル名を取得
        /// </summary>
        public bool CheckOffsetFile()
        {
            if (!FileReadAndWriteUtility.TryLoadMotionOffset()) return false;
            if (!FileReadAndWriteUtility.TryLoadMotionFacialPair()) return false;

            _vmdList.Clear();

            string sFolderPath = PathsInfo.GetFullPath(FOLDERTYPE.MOTION) + "/";
            try
            {
                var names = Directory.GetFiles(sFolderPath, "*.vmd", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = names[i].Replace(sFolderPath, "");

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
            string sFolderPath = PathsInfo.GetFullPath_LipSync() + "/";
            try
            {
                var names = Directory.GetFiles(sFolderPath, "*.vmd", SearchOption.TopDirectoryOnly);

                //ファイルパスからファイル名の抽出
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = names[i].Replace(sFolderPath, "");
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
