using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    //�{���̓X�N���[���r���[�ɂ������A�{�R�{�R�{�^�������߂��ፂ�R�X�g
    public class JumpList : MonoBehaviour
    {
        public enum TARGET
        {
            NULL,
            CHARA,
            ANIME,
            VMD_LIPSYNC,
            AUDIO,
            ITEM
        }

        public event Action<int> onSelect;
        public TARGET target = TARGET.NULL;

        [SerializeField] private Button_Base Button_BasePrefab;
        [SerializeField] private Transform parentAnchor;
        private FileAccessManager fileManager;
        private List<Button_Base> btnList = new List<Button_Base>();

        // Start is called before the first frame update
        void Start()
        {
            fileManager = GameObject.FindGameObjectWithTag("AppConfig").GetComponent<FileAccessManager>();
        }

        /// <summary>
        /// �K�v�ɉ����ă{�^����ǉ�����
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
                    btn.transform.localPosition = new Vector3(initX, initY, 0);
                    btn.transform.localRotation = Quaternion.identity;

                    btnList.Add(btn);
                    btn = null;
                }
            }
        }

        /// <summary>
        /// �{�^���ɃL��������ݒ肷��
        /// </summary>
        /// <param name="charaInfoDatas"></param>
        public void SetCharaDate(CharaInfoData[] charaInfoDatas)
        {
            //�K�v�Ȃ�{�^���𐶐�
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
        /// �{�^���ɃA�j���[�V��������ݒ肷��
        /// </summary>
        /// <param name="danceInfoData"></param>
        public void SetAnimeData(DanceInfoData[] danceInfoData)
        {
            //�K�v�Ȃ�{�^���𐶐�
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
        /// �{�^���ɃA�j���[�V��������ݒ肷��
        /// </summary>
        /// <param name="danceInfoData"></param>
        public void SetLipSyncNames(string[] lipSyncNames)
        {
            //�K�v�Ȃ�{�^���𐶐�
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
        /// �{�^���ɃI�[�f�B�I����ݒ肷��
        /// </summary>
        public void SetAudioDate()
        {
            //�K�v�Ȃ�{�^���𐶐�
            BtnInstanceCheck(fileManager.audioList.Count);

            for (int i = 0; i < btnList.Count; i++)
            {
                if (i < fileManager.audioList.Count)
                {
                    btnList[i].SetTextMesh(fileManager.audioList[i].name);
                    if (!btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(true);
                }
                else if (fileManager.audioList.Count <= i)
                {
                    if (btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(false);
                }
            }

            target = TARGET.AUDIO;
        }

        /// <summary>
        /// �{�^���ɃA�j���[�V��������ݒ肷��
        /// </summary>
        /// <param name="danceInfoData"></param>
        public void SetItemData(DecorationItemInfo[] ItemInfo)
        {
            //�K�v�Ȃ�{�^���𐶐�
            BtnInstanceCheck(ItemInfo.Length);

            for (int i = 0; i < btnList.Count; i++)
            {
                if (i < ItemInfo.Length)
                {
                    if (GlobalConfig.systemData.LanguageCode == (int)SaveData.USE_LANGUAGE.JP) btnList[i].SetTextMesh(ItemInfo[i].itemName[1]);
                    else if (GlobalConfig.systemData.LanguageCode == (int)SaveData.USE_LANGUAGE.EN) btnList[i].SetTextMesh(ItemInfo[i].itemName[0]);
                    if (!btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(true);
                }
                else if (ItemInfo.Length <= i)
                {
                    if (btnList[i].gameObject.activeSelf) btnList[i].gameObject.SetActive(false);
                }
            }

            target = TARGET.ITEM;
        }

        /// <summary>
        /// ���X�g���̂����ꂩ�̃{�^�����N���b�N���ꂽ
        /// </summary>
        /// <param name="btn"></param>
        private void OnClick(Button_Base btn)
        {
            //�{�^�������
            for (int i = 0; i < btnList.Count; i++)
            {
                if (btn == btnList[i])
                {
                    //�J�����g��n��
                    onSelect?.Invoke(i);
                    break;
                }
            }
            gameObject.SetActive(false);
        }
    }
}