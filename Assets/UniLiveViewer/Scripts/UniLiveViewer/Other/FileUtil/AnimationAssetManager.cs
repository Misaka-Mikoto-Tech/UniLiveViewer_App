using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniLiveViewer
{
    /// <summary>
    /// MotionOffsetがcsv管理の都合上ファイル名にカンマ禁止（ロード時点で除外する）
    /// 引用符は説明が面倒なのでしない
    /// 隠蔽しつつjsonでもいいがUI用意必至なのが悩む..先々のことも懸念でとりあえず誰でもなcsv
    /// </summary>
    public class AnimationAssetManager
    {
        public IReadOnlyList<string> VmdList => _vmdList;
        List<string> _vmdList = new List<string>();
        public IReadOnlyList<string> VmdSyncList => _vmdSyncList;
        List<string> _vmdSyncList = new List<string>();

        /// <summary>
        /// 各ファイルのリストを最新状態に
        /// </summary>
        public void Setup()
        {
            UpdateMotionList();
            UpdateSyncMotionList();
        }

        /// <summary>
        /// モーションファイルのリストを最新状態に
        /// </summary>
        void UpdateMotionList()
        {
            if (!FileReadAndWriteUtility.TryLoadMotionOffset())
            {
                Debug.Log("モーション設定ファイルなし");
            }
            _vmdList.Clear();

            var folderPath = PathsInfo.GetFullPath(FOLDERTYPE.MOTION) + "/";
            var names = Directory.GetFiles(folderPath, "*.vmd", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < names.Length; i++)
            {
                names[i] = names[i].Replace(folderPath, "");
                
                if (names[i].Contains(",")) continue;
                _vmdList.Add(names[i]);
                //既存offset情報がなければ追加
                if (!FileReadAndWriteUtility.GetMotionOffset.ContainsKey(names[i]))
                {
                    FileReadAndWriteUtility.SetMotionOffset(names[i], 0);
                }
            }
            //一旦保存
            FileReadAndWriteUtility.SaveMotionOffset();
        }

        /// <summary>
        /// モーションと同期するファイルのリストを最新状態に
        /// </summary>
        void UpdateSyncMotionList()
        {
            if (!FileReadAndWriteUtility.TryLoadMotionFacialPair())
            {
                Debug.Log("モーションペア設定ファイルなし");
            }
            _vmdSyncList.Clear();

            var folderPath = PathsInfo.GetFullPath_LipSync() + "/";
            var names = Directory.GetFiles(folderPath, "*.vmd", SearchOption.TopDirectoryOnly);

            for (int i = 0; i < names.Length; i++)
            {
                names[i] = names[i].Replace(folderPath, "");

                if (names[i].Contains(",")) continue;
                _vmdSyncList.Add(names[i]);
            }
        }
    }
}
