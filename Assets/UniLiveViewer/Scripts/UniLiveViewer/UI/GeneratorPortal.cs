using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private VMDPlayer vmdPlayer;
        private bool isGenerateComplete = true;
        private bool retryVMD = false;
        

        //�ǂݍ��ݍς�VMD���
        private static Dictionary<string, VMD> dic_VMDReader = new Dictionary<string, VMD>();

        private CancellationTokenSource cts;

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
            isGenerateComplete = false;

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
            var charaCon = Instantiate(listChara[currentChara]);
            //var charaCon = charaObj.GetComponent<CharaController>(true);
            //VRMloader�ɃX�g�b�N���Ă���VRMPrefab��������ԂȂ̂ŗL����
            if (!charaCon.gameObject.activeSelf) charaCon.gameObject.SetActive(true);
            //if (charaCon.gameObject.activeSelf) charaCon.gameObject.SetActive(false);
            //�p�����[�^�[�ݒ�
            charaCon.SetState(CharaController.CHARASTATE.MINIATURE, transform);//�~�j�`���A���

            //Timeline�̃|�[�^���g�փo�C���h����
            bool isSuccess = timeline.NewAssetBinding_Portal(charaCon);

            if (isSuccess) SetAnimation(0).Forget();//�L�����ɃA�j���[�V���������Z�b�g����
            else if (!isSuccess && charaCon) Destroy(charaCon.gameObject);

            isGenerateComplete = true;
        }

        /// <summary>
        /// �w��Current�̃A�j���[�V�������Z�b�g����
        /// </summary>
        /// <param Current�𓮂���="moveCurrent">�������K�v���Ȃ����0</param>
        public async UniTask SetAnimation(int moveCurrent)
        {
            try
            {
                cts = new CancellationTokenSource();

                //Current�ړ�����
                currentAnime += moveCurrent;
                if (currentAnime < 0) currentAnime = danceAniClipInfo.Length - 1;
                else if (currentAnime >= danceAniClipInfo.Length) currentAnime = 0;

                //�|�[�^���L�������m�F
                var portalChara = timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
                if (!portalChara) return;
                //��\��
                //if (portalChara.gameObject.activeSelf) portalChara.gameObject.SetActive(false);

                vmdPlayer = portalChara.GetComponent<VMDPlayer>();

                //VMD
                if (GetNowAnimeInfo().formatType == DanceInfoData.FORMATTYPE.VMD)
                {
                    //�|�[�^����̃L�����ɃA�j���[�V�����ݒ�
                    timeline.SetAnimationClip(timeline.sPortalBaseAniTrack, danceAniClipInfo[currentAnime], transform.position, Vector3.zero);
                    await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);

                    //�Ō�ɕ\��
                    //if (!portalChara.gameObject.activeSelf) portalChara.gameObject.SetActive(true);

                    //animator���~�AVMD���Đ�
                    string folderPath = FileAccessManager.GetFullPath(FileAccessManager.FOLDERTYPE.MOTION);//VMD�̃p�X���擾
                    portalChara.GetComponent<Animator>().enabled = false;//Animator����������̂Ŗ���
                    portalChara.animationMode = CharaController.ANIMATIONMODE.VMD;
                    await VMDPlay(vmdPlayer, folderPath, GetNowAnimeInfo().viewName, cts.Token);
                }
                //�v���Z�b�g�A�j���[�V����
                else
                {
                    //���]�ݒ�
                    danceAniClipInfo[currentAnime].isReverse = isAnimationReverse;

                    //VMD���~�Aanimator�ĊJ
                    vmdPlayer.Clear();
                    portalChara.GetComponent<Animator>().enabled = true;
                    portalChara.animationMode = CharaController.ANIMATIONMODE.CLIP;

                    //�|�[�^����̃L�����ɃA�j���[�V�����ݒ�
                    timeline.SetAnimationClip(timeline.sPortalBaseAniTrack, danceAniClipInfo[currentAnime], transform.position, Vector3.zero);
                    await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);

                    //�Ō�ɕ\��
                    //if (!portalChara.gameObject.activeSelf) portalChara.gameObject.SetActive(true);
                }
            }
            catch (OperationCanceledException)
            {
                retryVMD = true;
                throw;
            }
        }

        /// <summary>
        /// VMD���Đ�����
        /// </summary>
        /// <param name="vmpPlayer"></param>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        public async UniTask VMDPlay(VMDPlayer vmdPlayer, string folderPath, string fileName,CancellationToken token)
        {
            //�����̓ǂݍ��ݍς݃��X�g�Əƍ�
            if (dic_VMDReader.ContainsKey(fileName))
            {
                //�g���܂킵��VMD�v���C���[�X�^�[�g
                await vmdPlayer.Starter(dic_VMDReader[fileName], folderPath, fileName, token);
            }
            else
            {
                //�V�K�Ȃ�ǂݍ����VMD�v���C���[�X�^�[�g
                var newVMD = await vmdPlayer.Starter(null, folderPath, fileName, token);
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

        private void OnDisable()
        {
            cts.Cancel();
        }

        private void OnEnable()
        {
            //retry����
            if (!isGenerateComplete)
            {
                isGenerateComplete = true;
                SetChara(0);
            }
            else if (retryVMD)
            {
                retryVMD = false;
                SetAnimation(0).Forget();
            }
        }
    }
}