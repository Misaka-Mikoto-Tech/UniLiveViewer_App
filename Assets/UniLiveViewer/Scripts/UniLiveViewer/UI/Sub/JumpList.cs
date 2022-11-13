using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniLiveViewer
{
    //本当はスクロールビューにしたい、ボコボコボタン生成めちゃ高コスト
    public class JumpList : MonoBehaviour
    {
        public enum TARGET
        {
            NULL,
            CHARA,
            ANIME,
            VMD_LIPSYNC,
            AUDIO,
        }

        public event Action<int> onSelect;
        public TARGET target = TARGET.NULL;

        [SerializeField] Button_Base Button_BasePrefab;
        [SerializeField] Transform parentAnchor;
        AudioAssetManager _audioAssetManager;
        List<Button_Base> btnList = new List<Button_Base>();

        // Start is called before the first frame update
        void Start()
        {
            var appConfig = GameObject.FindGameObjectWithTag("AppConfig").transform;
            _audioAssetManager = appConfig.GetComponent<AudioAssetManager>();
        }

        /// <summary>
        /// 必要に応じてボタンを追加生成
        /// </summary>
        public void BtnInstanceCheck(int needCount)
        {
            if (btnList.Count < needCount)
            {
                float initX = 0, initY = 0;

                Button_Base btn;
                for (int i = btnList.Count; i < needCount; i++)
                {
                    initX = i / 15 * 3;
                    initY = 2 - (i % 15 * 0.3f);

                    btn = Instantiate(Button_BasePrefab);
                    btn.onTrigger += OnClick;
                    btn.transform.parent = parentAnchor;
                    
                    btn.transform.localRotation = Quaternion.identity;

                    //Zファイティング対策
                    if((initX / 3) % 2 == 0) btn.transform.localPosition = new Vector3(initX, initY, 0);
                    else btn.transform.localPosition = new Vector3(initX, initY, -0.01f);

                    btnList.Add(btn);
                    btn = null;
                }
            }
        }

        /// <summary>
        /// ボタンにキャラ名を設定する
        /// </summary>
        /// <param name="charaInfoDatas"></param>
        public void SetCharaDate(CharaInfoData[] charaInfoDatas)
        {
            //必要ならボタンを生成
            BtnInstanceCheck(charaInfoDatas.Length);
            ;
            for (int i = 0; i < btnList.Count; i++)
            {
                if (i < charaInfoDatas.Length)
                {
                    if (charaInfoDatas[i]) btnList[i].SetTextMesh(charaInfoDatas[i].viewName);
                    else btnList[i].SetTextMesh("VRM Load");

                    if (!btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(true);
                }
                else if (charaInfoDatas.Length <= i)
                {
                    if (btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(false);
                }
            }

            target = TARGET.CHARA;
        }

        /// <summary>
        /// ボタンにアニメーション名を設定する
        /// </summary>
        /// <param name="danceInfoData"></param>
        public void SetAnimeData(DanceInfoData[] danceInfoData)
        {
            //必要ならボタンを生成
            BtnInstanceCheck(danceInfoData.Length);

            for (int i = 0; i < btnList.Count; i++)
            {
                if (i < danceInfoData.Length)
                {
                    btnList[i].SetTextMesh(danceInfoData[i].viewName);
                    if (!btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(true);
                }
                else if (danceInfoData.Length <= i)
                {
                    if (btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(false);
                }
            }

            target = TARGET.ANIME;
        }

        /// <summary>
        /// ボタンにアニメーション名を設定する
        /// </summary>
        /// <param name="danceInfoData"></param>
        public void SetLipSyncNames(string[] lipSyncNames)
        {
            //必要ならボタンを生成
            BtnInstanceCheck(lipSyncNames.Length);

            for (int i = 0; i < btnList.Count; i++)
            {
                if (i < lipSyncNames.Length)
                {
                    btnList[i].SetTextMesh(lipSyncNames[i]);
                    if (!btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(true);
                }
                else if (lipSyncNames.Length <= i)
                {
                    if (btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(false);
                }
            }

            target = TARGET.VMD_LIPSYNC;
        }

        /// <summary>
        /// ボタンにオーディオ名を設定する
        /// </summary>
        public void SetAudioDate()
        {
            //必要ならボタンを生成
            BtnInstanceCheck(_audioAssetManager.CustomAudios.Count);

            for (int i = 0; i < btnList.Count; i++)
            {
                if (i < _audioAssetManager.CustomAudios.Count)
                {
                    var name = Path.GetFileName(_audioAssetManager.CustomAudios[i]); 
                    btnList[i].SetTextMesh(name);
                    if (!btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(true);
                }
                else if (_audioAssetManager.CustomAudios.Count <= i)
                {
                    if (btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(false);
                }
            }

            target = TARGET.AUDIO;
        }

        /// <summary>
        /// リスト内のいずれかのボタンがクリックされた
        /// </summary>
        /// <param name="btn"></param>
        private void OnClick(Button_Base btn)
        {
            //ボタンを特定
            for (int i = 0; i < btnList.Count; i++)
            {
                if (btn == btnList[i])
                {
                    //カレントを渡す
                    onSelect?.Invoke(i);
                    break;
                }
            }
            gameObject.SetActive(false);
        }
    }
}