using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityVMDReader;

namespace UniLiveViewer
{
    public class GeneratorPortal : MonoBehaviour
    {
        //�L����
        [SerializeField] private List<CharaController> listChara = new List<CharaController>();
        public int currentChara { get; private set; } = 0;

        public int currentAnime { get; private set; } = 0;
        public int currentVMDLipSync { get; private set; } = 0;
        public bool isAnimationReverse = false;
        private const string LIPSYNC_NONAME = "No-LipSyncData";
        private const string LIPSYNC_VIEWNAME = "+ LipSyncData";

        //��{�_���X�N���b�v�ɏ㏑������N���b�v(��E��E���p�N�j
        private Dictionary<string, int> dicAniType = new Dictionary<string, int>() { { "HAND_L", 0 }, { "HAND_R", 1 }, { "FACE", 2 }, { "LIP", 3 } };

        [SerializeField] private DanceInfoData[] danceAniClipInfo;
        [SerializeField] private string[] vmdLipSync;
        public DanceInfoData[] GetDanceInfoData() { return danceAniClipInfo; }
        public string[] GetVmdLipSync() { return vmdLipSync; }
        [SerializeField] private DanceInfoData DanceInfoData_VMDPrefab;//VMD�p�̃e���v��
        private DanceInfoData[] vmdDanceClipInfo;


        //�ėp
        private TimelineController timeline = null;
        private FileAccessManager fileManager = null;

        //�ǂݍ��ݍς�VMD���
        private static Dictionary<string, VMD> dic_VMDReader = new Dictionary<string, VMD>();

        void Awake()
        {
            if (!timeline) timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            fileManager = GameObject.FindGameObjectWithTag("AppConfig").GetComponent<FileAccessManager>();

            //�L�������X�g�ɋ�g��ǉ�(���VRM�ǂݍ��ݘg�Ƃ��Ĉ����A�G�d�l)
            listChara.Add(null);
        }

        private void Start()
        {
            //VMD�g�̓_�~�[�A�j���[�V������ǉ����Ă���
            if (fileManager.vmdList.Count > 0)
            {
                vmdDanceClipInfo = new DanceInfoData[fileManager.vmdList.Count];

                for (int i = 0; i < vmdDanceClipInfo.Length; i++)
                {
                    vmdDanceClipInfo[i] = Instantiate(DanceInfoData_VMDPrefab);

                    vmdDanceClipInfo[i].motionOffsetTime = 0;
                    vmdDanceClipInfo[i].strBeforeName = fileManager.vmdList[i];
                    vmdDanceClipInfo[i].viewName = fileManager.vmdList[i];
                }
                danceAniClipInfo = danceAniClipInfo.Concat(vmdDanceClipInfo).ToArray();
            }
            //���p�NVMD
            if (fileManager.vmdLipSyncList.Count > 0)
            {
                string[] lipSyncs = new string[fileManager.vmdLipSyncList.Count];
                lipSyncs = fileManager.vmdLipSyncList.ToArray();

                string[] dummy = { LIPSYNC_NONAME };
                vmdLipSync = dummy.Concat(lipSyncs).ToArray();
            }
        }

        /// <summary>
        /// �L�������X�g��VRM��ǉ�����
        /// </summary>
        public void AddVRMPrefab(CharaController charaCon_vrm)
        {
            //VRM��ǉ�
            listChara[listChara.Count - 1] = charaCon_vrm;
            //���X�g�ɋ�g��ǉ�(���VRM�ǂݍ��ݘg�Ƃ��Ĉ���)
            listChara.Add(null);
        }

        /// <summary>
        /// �J�����g��VRMPrefab���폜����
        /// </summary>
        public void DeleteCurrenVRM()
        {
            if (listChara[currentChara].charaInfoData.formatType
                != CharaInfoData.FORMATTYPE.VRM) return;

            string viewName = listChara[currentChara].charaInfoData.viewName;

            //Prefab����폜
            Destroy(listChara[currentChara].gameObject);
            listChara.RemoveAt(currentChara);

            currentChara--;

            //�t�B�[���h��ɑ��݂���΍폜
            timeline.DeletebindAsset_CleanUp(viewName);

            //���g�p�A�Z�b�g�T�[�`������A�s�v�Ȃ��̂��폜
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// �w��Current�̃L�������Z�b�g����
        /// </summary>
        /// <param Current�𓮂���="moveCurrent">�������K�v���Ȃ����0</param>
        public void SetChara(int moveCurrent)
        {
            currentChara += moveCurrent;

            //Current�ړ�����
            if (currentChara < 0) currentChara = listChara.Count - 1;
            else if (currentChara >= listChara.Count) currentChara = 0;

            if (!timeline) timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();

            //�����̃|�[�^���L�������폜
            timeline.DestoryPortalChara();

            //�t�B�[���h����I�[�o�[�Ȃ琶�����Ȃ�
            if (timeline.FieldCharaCount >= timeline.maxFieldChara) return;

            //null�g�̏ꍇ���������Ȃ�
            if (!listChara[currentChara]) return;

            //�L�����𐶐�
            var charaObj = Instantiate(listChara[currentChara]);
            var charaCon = charaObj.GetComponent<CharaController>();
            //VRMloader�ɃX�g�b�N���Ă���VRMPrefab��������ԂȂ̂ŗL����
            if (!charaObj.gameObject.activeSelf) charaObj.gameObject.SetActive(true);
            //�p�����[�^�[�ݒ�
            charaCon.SetState(CharaController.CHARASTATE.MINIATURE, transform);//�~�j�`���A���

            //Timeline�̃|�[�^���g�փo�C���h����
            bool isSuccess = timeline.NewAssetBinding_Portal(charaObj.gameObject);

            if (isSuccess) SetAnimation(0);//�L�����ɃA�j���[�V���������Z�b�g����
            else if (!isSuccess && charaObj) Destroy(charaObj);
        }

        /// <summary>
        /// �w��Current�̃A�j���[�V�������Z�b�g����
        /// </summary>
        /// <param Current�𓮂���="moveCurrent">�������K�v���Ȃ����0</param>
        public void SetAnimation(int moveCurrent)
        {
            //Current�ړ�����
            currentAnime += moveCurrent;
            if (currentAnime < 0) currentAnime = danceAniClipInfo.Length - 1;
            else if (currentAnime >= danceAniClipInfo.Length) currentAnime = 0;

            //�|�[�^���L�������m�F
            if (!timeline.trackBindChara[TimelineController.PORTAL_ELEMENT]) return;

            //VMD
            if (GetNowAnimeInfo().formatType == DanceInfoData.FORMATTYPE.VMD)
            {
                //�|�[�^����̃L�����ɃA�j���[�V�����ݒ�
                timeline.SetAnimationClip(timeline.sPortalBaseAniTrack, danceAniClipInfo[currentAnime], transform.position, Vector3.zero);

                //�|�[�^���L�����e��ݒ�
                var portalChara = timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
                var vmdPlayer = portalChara.GetComponent<VMDPlayer>();
                //�ǂݍ��ݓr���Ȃ�Current��߂��ď������Ȃ�
                if (!vmdPlayer.IsPlayable)
                {
                    currentAnime -= moveCurrent;
                    return;
                }
                else
                {
                    //animator���~�AVMD���Đ�
                    string folderPath = FileAccessManager.GetFullPath(FileAccessManager.FOLDERTYPE.MOTION);//VMD�̃p�X���擾
                    portalChara.GetComponent<Animator>().enabled = false;//Animator����������̂Ŗ���
                    portalChara.animationMode = CharaController.ANIMATIONMODE.VMD;
                    VMDPlay(vmdPlayer, folderPath, GetNowAnimeInfo().viewName);
                }
            }
            //�v���Z�b�g�A�j���[�V����
            else
            {
                //���]�ݒ�
                danceAniClipInfo[currentAnime].isReverse = isAnimationReverse;

                //�|�[�^���L�����̊e��ݒ�ύX
                var portalChara = timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
                //VMD���~�Aanimator�ĊJ
                var vmdPlayer = portalChara.GetComponent<VMDPlayer>();
                vmdPlayer.Clear();
                portalChara.GetComponent<Animator>().enabled = true;
                portalChara.animationMode = CharaController.ANIMATIONMODE.CLIP;

                //�|�[�^����̃L�����ɃA�j���[�V�����ݒ�
                timeline.SetAnimationClip(timeline.sPortalBaseAniTrack, danceAniClipInfo[currentAnime], transform.position, Vector3.zero);

            }
        }

        /// <summary>
        /// VMD���Đ�����
        /// </summary>
        /// <param name="vmpPlayer"></param>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        public async void VMDPlay(VMDPlayer vmdPlayer, string folderPath, string fileName)
        {
            //�����̓ǂݍ��ݍς݃��X�g�Əƍ�
            if (dic_VMDReader.ContainsKey(fileName))
            {
                //�g���܂킵��VMD�v���C���[�X�^�[�g
                await vmdPlayer.Starter(dic_VMDReader[fileName], folderPath, fileName);
            }
            else
            {
                //�V�K�Ȃ�ǂݍ����VMD�v���C���[�X�^�[�g
                var newVMD = await vmdPlayer.Starter(null, folderPath, fileName);
                //�V�KVMD��o�^
                dic_VMDReader.Add(fileName, newVMD);
            }
        }

        /// <summary>
        /// �L��������S�擾
        /// </summary>
        /// <returns></returns>
        public CharaInfoData[] GetCharasInfo()
        {
            CharaInfoData[] result = new CharaInfoData[listChara.Count];

            for (int i = 0; i < listChara.Count; i++)
            {
                if (listChara[i]) result[i] = listChara[i].charaInfoData;
                else result[i] = null;
            }
            return result;
        }

        /// <summary>
        /// ���݂̃L���������擾
        /// </summary>
        /// <returns></returns>
        public bool GetNowCharaName(out string name)
        {
            if (listChara[currentChara])
            {
                name = listChara[currentChara].charaInfoData.viewName;
                return true;
            }
            else
            {
                name = "None";
                return false;
            }
        }

        /// <summary>
        /// ���݂̃A�j���[�V�����N���b�v�����擾
        /// </summary>
        /// <returns></returns>
        public DanceInfoData GetNowAnimeInfo()
        {
            return danceAniClipInfo[currentAnime];
        }

        /// <summary>
        /// ���݂̃��b�v�V���N�����擾
        /// </summary>
        /// <returns></returns>
        public string GetNowLipSyncName()
        {
            string result = vmdLipSync[currentVMDLipSync];
            if (result == LIPSYNC_NONAME) result = LIPSYNC_VIEWNAME;
            return vmdLipSync[currentVMDLipSync];
        }
    }
}