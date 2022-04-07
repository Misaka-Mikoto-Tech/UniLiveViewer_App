using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UniLiveViewer
{
    public class TimelineController : MonoBehaviour
    {
        public readonly string sPortalBaseAniTrack = "Animation Track_Portal";
        private const string ANITRACK1 = "Animation Track1";
        private const string ANITRACK2 = "Animation Track2";
        private const string ANITRACK3 = "Animation Track3";
        private const string ANITRACK4 = "Animation Track4";
        private const string ANITRACK5 = "Animation Track5";

        private const string assetName_MainAudio = "Main Audio";

        private const string SUBTRACK0 = "Override 0";
        private const string SUBTRACK1 = "Override 1";
        private const string SUBTRACK2 = "Override 2";
        private const string SUBTRACK3 = "Override 3";

        private const string SUBCLIP0 = "HandExpression";
        private const string SUBCLIP1 = "HandExpression";
        private const string SUBCLIP2 = "FaceClip";
        private const string SUBCLIP3 = "LipClip";

        //�|�[�^���L�����̊m�F
        public static int PORTAL_ELEMENT = 0;

        //�^�C�����C��
        public PlayableDirector playableDirector; //�f�B���N�^
        private TimelineAsset timeLineAsset;//�^�C�����C���A�Z�b�g�ɃA�N�Z�X�p

        //�o�C���h�L�������Ǘ�����N���X
        public CharaController[] trackBindChara = new CharaController[6];

        public int FieldCharaCount { get; private set; } = 0;//�t�B�[���h�̃L�����J�E���g
        public event Action FieldCharaUpdate;//�ݒu�L�������̍X�V��
        public int maxFieldChara = 1;//�ő叢����

        public bool isPortalChara() { return trackBindChara[PORTAL_ELEMENT]; }
        public FileAccessManager fileManager = null;
        [SerializeField] private AnimationClip grabHandAnime;
        
        public double AudioClip_StartTime = 0;//�Z�b�g���ꂽaudio�N���b�v�̊J�n�Đ��ʒu
        private double motionClip_StartTime = 3;//���[�V�����N���b�v�̊J�n�Đ��ʒu(�f�t�H���g)
        private float _timelineSpeed = 1.0f;
        public float timelineSpeed
        {
            get
            {
                return _timelineSpeed;
            }
            set
            {
                _timelineSpeed = Mathf.Clamp(value, 0.0f, 3.0f);
                playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_timelineSpeed);
            }
        }

        public double AudioClip_PlaybackTime
        {
            get
            {
                //���y�N���b�v���ł̍Đ����Ԃ��Z�o
                return playableDirector.time - AudioClip_StartTime;
            }
            set
            {
                //����ȏ�Ȃ�MaX�l�Ɋۂ߂�
                if (value > playableDirector.duration) value = playableDirector.duration;
                //�^�C�����C���ɔ��f
                playableDirector.time = AudioClip_StartTime + value;
            }
        }

        private void Awake()
        {
            if (timeLineAsset == null) timeLineAsset = playableDirector.playableAsset as TimelineAsset;
            fileManager = GameObject.FindGameObjectWithTag("AppConfig").GetComponent<FileAccessManager>();
        }

        private void Start()
        {
            // �^�C�����C�����̃g���b�N�ꗗ���擾
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //���C���I�[�f�B�I��TrackAsset���擾
            TrackAsset track = tracks.FirstOrDefault(x => x.name == assetName_MainAudio);

            if (track)
            {
                //�g���b�N���̃N���b�v��S�擾
                IEnumerable<TimelineClip> clips = track.GetClips();
                // �w�薼�̂̃N���b�v�𔲂��o��
                TimelineClip danceClip = clips.FirstOrDefault(x => x.displayName == "Main Audio Clip");
                //�J�n�ʒu���擾
                danceClip.start = motionClip_StartTime + 2;
                AudioClip_StartTime = danceClip.start;
            }
            else
            {
                Debug.Log("���C���I�[�f�B�I��������܂���");

            }
            if (GlobalConfig.sceneMode_static == GlobalConfig.SceneMode.CANDY_LIVE)
            {
#if UNITY_EDITOR
                maxFieldChara = 5;
#elif UNITY_ANDROID
            if (SystemInfo.deviceName == "Oculus Quest 2") maxFieldChara = 2;
            else if (SystemInfo.deviceName == "Oculus Quest") maxFieldChara = 2;
#endif
            }
            else if (GlobalConfig.sceneMode_static == GlobalConfig.SceneMode.KAGURA_LIVE)
            {
#if UNITY_EDITOR
                maxFieldChara = 5;
#elif UNITY_ANDROID
            if (SystemInfo.deviceName == "Oculus Quest 2") maxFieldChara = 2;
            else if (SystemInfo.deviceName == "Oculus Quest") maxFieldChara = 2;
#endif
            }
            else if (GlobalConfig.sceneMode_static == GlobalConfig.SceneMode.VIEWER)
            {
#if UNITY_EDITOR
                maxFieldChara = 5;
#elif UNITY_ANDROID
            if (SystemInfo.deviceName == "Oculus Quest 2") maxFieldChara = 5;
            else if (SystemInfo.deviceName == "Oculus Quest") maxFieldChara = 4;
#endif
            }
        }

        private void Update()
        {
            //�����ʒu
            if (AudioClip_PlaybackTime < 0)
            {
                //�}�j���A�����[�h
                TimelineManualMode();

                //��~��Ԃɂ���(UI�Ƀg���K�[�𑗂��)
                playableDirector.Stop();

                //�N���b�v�J�n�ʒu�܂Ői�߂�(�d���\�h)
                AudioClip_PlaybackTime = 0;
            }
        }

        public void DestoryPortalChara()
        {
            //�����L����������΍폜���Ă���
            if (trackBindChara[PORTAL_ELEMENT])
            {
                Destroy(trackBindChara[PORTAL_ELEMENT].gameObject);
                trackBindChara[PORTAL_ELEMENT] = null;
            }
        }

        /// <summary>
        /// �烂�[�t�̗L��������؂�ւ���
        /// </summary>
        /// <param name="isFace">�\����p�N��</param>
        /// <param name="isEnable"></param>
        public void SetMouthUpdate_Portal(bool isFace, bool isEnable)
        {
            var bindChara = trackBindChara[PORTAL_ELEMENT];

            if (bindChara)
            {
                var vmdPlayer = trackBindChara[PORTAL_ELEMENT].GetComponent<VMDPlayer>();

                if (bindChara.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
                {
                    //VMD�Đ���
                    if (vmdPlayer.morphPlayer_vrm != null)
                    {
                        //�\��
                        if (isFace)
                        {
                            vmdPlayer.morphPlayer_vrm.isUpdateFace = isEnable;

                            //��~�̏ꍇ�͌��̏�Ԃ����������Ă���
                            if (!isEnable) bindChara.facialSync.AllClear_BlendShape();
                        }
                        //���p�N
                        else
                        {
                            vmdPlayer.morphPlayer_vrm.isUpdateMouth = isEnable;

                            //��~�̏ꍇ�͌��̏�Ԃ����������Ă���
                            if (!isEnable) bindChara.lipSync.AllClear_BlendShape();
                        }
                    }
                    //�v���Z�b�g��
                    else
                    {
                        //�\��
                        if (isFace)
                        {
                            //��~�̏ꍇ�͌��̏�Ԃ����������Ă���
                            if (!isEnable) bindChara.facialSync.AllClear_BlendShape();
                            bindChara.facialSync.enabled = isEnable;
                        }
                        //���p�N
                        else
                        {
                            //��~�̏ꍇ�͌��̏�Ԃ����������Ă���
                            if (!isEnable) bindChara.lipSync.AllClear_BlendShape();
                            bindChara.lipSync.enabled = isEnable;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �V�K�L�������|�[�^���g�Ƀo�C���h����
        /// </summary>
        /// <param name="bindObject"></param>
        /// <returns></returns>
        public bool NewAssetBinding_Portal(GameObject bindObject)
        {
            if (!bindObject) return false;//���s(null�o�C���h�̕K�v�Ȃ�)
            var chara = bindObject.GetComponent<CharaController>();

            IEnumerable<PlayableBinding> outputs = playableDirector.playableAsset.outputs;
            //�|�[�^���pBaseAnime��PlayableBinding���擾
            PlayableBinding Asset_BaseAnime = outputs.FirstOrDefault(x => x.streamName == sPortalBaseAniTrack);

            if (Asset_BaseAnime.streamName != "")
            {
                //�I�u�W�F�N�g���o�C���h����
                playableDirector.SetGenericBinding(Asset_BaseAnime.sourceObject, bindObject);
                //CharaList�ɃZ�b�g
                trackBindChara[PORTAL_ELEMENT] = chara;
                //�o�C���h����t�^
                chara.bindTrackName = sPortalBaseAniTrack;
                //chara.bindTrackName_LipSync = "LipSync Track_Portal";
            }
            else
            {
                Debug.Log("�V�X�e���ݒ�G���[�A�L�����o�^�g��������܂���BPlayableBinding�����������Ă�������");
                return false;
            }

            //�}�j���A����ԂȂ�
            if (playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //�A�j���[�^�[�R���g���[���[���������Ă���
                StartCoroutine(RemoveCharasAniCon());
            }

            return true;//����
        }

        /// <summary>
        /// �V�K�A�j���[�V�������|�[�^���g�Ƀo�C���h����
        /// </summary>
        /// <param name="baseAniTrackName"></param>
        /// <param name="baseAniClip"></param>
        /// <param name="overrideAniClips"></param>
        /// <param name="initPos"></param>
        /// <param name="initEulerAngles"></param>
        public void SetAnimationClip(string baseAniTrackName, DanceInfoData danceInfoData, Vector3 initPos, Vector3 initEulerAngles)
        {
            // �^�C�����C�����̃g���b�N�ꗗ���擾
            if (timeLineAsset == null) timeLineAsset = playableDirector.playableAsset as TimelineAsset;
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //BaseAnime��TrackAsset���擾
            TrackAsset track = tracks.FirstOrDefault(x => x.name == baseAniTrackName);

            if (track)
            {
                //�g���b�N���̃N���b�v��S�擾
                IEnumerable<TimelineClip> clips = track.GetClips();
                // �w�薼�̂̃N���b�v�𔲂��o��
                TimelineClip danceClip = clips.FirstOrDefault(x => x.displayName == "DanceBase");
                danceClip.start = motionClip_StartTime + danceInfoData.motionOffsetTime;

                //�o�^����
                //danceClip.asset = baseAniClip; ����_��
                AnimationPlayableAsset animationPlayableAsset = danceClip.asset as AnimationPlayableAsset;
                if (!danceInfoData.isReverse) animationPlayableAsset.clip = danceInfoData.baseDanceClip;
                else animationPlayableAsset.clip = danceInfoData.baseDanceClip_reverse;
                animationPlayableAsset.position = initPos;
                animationPlayableAsset.rotation = Quaternion.Euler(initEulerAngles);
                //(danceClip.asset as AnimationPlayableAsset).clip = animationClip;

                //�I�[�o�[���C�h�A�j���[�V������o�^����
                SetAnimationClip_Override(track, danceInfoData);
                
                //���f�ׂ̈Ƀf�B���N�^�[�����X�^�[�g����
                TimeLineReStart();
            }
        }

        /// <summary>
        /// �㏑������A�j���[�V���������Ԃɓo�^����
        /// </summary>
        /// <param name="parentTrack">�x�[�X�ɂȂ�Track</param>
        /// <param name="overrideAniClips">�㏑���������A�j���[�V����</param>
        private void SetAnimationClip_Override(TrackAsset parentTrack, DanceInfoData danceClipInfo)
        {
            TimelineClip handClip;

            //�㏑������g���b�N����������
            foreach (var subTrack in parentTrack.GetChildTracks())
            {
                // �g���b�N���̃N���b�v�ꗗ���擾
                IEnumerable<TimelineClip> clips = subTrack.GetClips();

                switch (subTrack.name)
                {
                    case SUBTRACK0:
                        // �w�薼�̂̃N���b�v�𔲂��o��
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP0);

                        //�L���������Ɉ���Ȃ�
                        if (trackBindChara[PORTAL_ELEMENT] && trackBindChara[PORTAL_ELEMENT].keepHandL_Anime)
                        {

                        }
                        else
                        {
                            //�o�^����
                            if (!danceClipInfo.isReverse) (handClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_hand;
                            else (handClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_reverseHand;
                            handClip.start = motionClip_StartTime + danceClipInfo.motionOffsetTime;
                        }

                        break;
                    case SUBTRACK1:
                        // �w�薼�̂̃N���b�v�𔲂��o��
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP1);

                        //�L���������Ɉ���Ȃ�
                        if (trackBindChara[PORTAL_ELEMENT] && trackBindChara[PORTAL_ELEMENT].keepHandR_Anime)
                        {

                        }
                        else
                        {
                            //�o�^����
                            if (!danceClipInfo.isReverse) (handClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_hand;
                            else (handClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_reverseHand;
                            handClip.start = motionClip_StartTime + danceClipInfo.motionOffsetTime;
                        }

                        break;
                    case SUBTRACK2:
                        // �w�薼�̂̃N���b�v�𔲂��o��
                        TimelineClip faceClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP2);

                        //�o�^����
                        (faceClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_face;
                        faceClip.start = motionClip_StartTime + danceClipInfo.motionOffsetTime;
                        break;
                    case SUBTRACK3:
                        // �w�薼�̂̃N���b�v�𔲂��o��
                        TimelineClip lipClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP3);

                        //�o�^����
                        (lipClip.asset as AnimationPlayableAsset).clip = danceClipInfo.overrideClip_lip;
                        lipClip.start = motionClip_StartTime + danceClipInfo.motionOffsetTime;
                        break;
                }
            }
        }

        /// <summary>
        /// �w��Current��BGM���Z�b�g����
        /// </summary>
        public string NextAudioClip(int moveCurrent)
        {
            fileManager.CurrentAudio += moveCurrent;

            //Current�ړ�����
            if (fileManager.CurrentAudio < 0) fileManager.CurrentAudio = fileManager.audioList.Count - 1;
            else if (fileManager.CurrentAudio >= fileManager.audioList.Count) fileManager.CurrentAudio = 0;
            //�N���b�v����
            AudioClip newAudioClip = fileManager.audioList[fileManager.CurrentAudio];

            // �^�C�����C�����̃g���b�N�ꗗ���擾
            if (timeLineAsset == null) timeLineAsset = playableDirector.playableAsset as TimelineAsset;
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //audio��TrackAsset���擾
            TrackAsset track = tracks.FirstOrDefault(x => x.name == assetName_MainAudio);

            if (track)
            {
                //�g���b�N���̃N���b�v��S�擾
                IEnumerable<TimelineClip> clips = track.GetClips();

                // �w�薼�̂̃N���b�v�𔲂��o��
                TimelineClip oldAudioClip = clips.FirstOrDefault(x => x.displayName != "");
                oldAudioClip.duration = AudioClip_StartTime + fileManager.audioList[fileManager.CurrentAudio].length;//�b

                //�ʒu�𒲐�
                //oldAudioClip.start = dlayTime;

                //�X�V
                //AudioClip_StartTime = oldAudioClip.start;

                //�o�^����
                (oldAudioClip.asset as AudioPlayableAsset).clip = newAudioClip;

                //���f�ׂ̈Ƀf�B���N�^�[�����X�^�[�g����
                TimeLineReStart();
            }

            return newAudioClip.name;
        }

        public void SetVMD_MotionOffset(string sName, int val)
        {
            if (sName.Contains(".vmd"))
            {
                SaveData.dicVMD_offset[sName] = val;
            }
        }

        public float GetNowAudioLength()
        {
            return fileManager.audioList[fileManager.CurrentAudio].length;
        }

        /// <summary>
        /// �w��L�����̎�̏�Ԃ�؂�ւ���
        /// </summary>
        /// <param name="charaCon"></param>
        /// <param name="isGrabHand">�����Ԃɂ��邩</param>
        public void SwitchHandType(CharaController charaCon, bool isGrabHand, bool isLeft)
        {
            //�d���r��
            if (isLeft)
            {
                if (!isGrabHand && !charaCon.keepHandL_Anime) return;
                else if (isGrabHand && charaCon.keepHandL_Anime) return;
            }
            else
            {
                if (!isGrabHand && !charaCon.keepHandR_Anime) return;
                else if (isGrabHand && charaCon.keepHandR_Anime) return;
            }

            // �^�C�����C�����̃g���b�N�ꗗ���擾
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //�Ώۂ̃L����TrackAsset���擾
            TrackAsset track = tracks.FirstOrDefault(x => x.name == charaCon.bindTrackName);
            if (!track) return;


            if (isLeft)
            {
                //�T�u�g���b�N
                var subTrack = track.GetChildTracks().FirstOrDefault(x => x.name == SUBTRACK0);
                //�w��N���b�v
                TimelineClip handClip = subTrack.GetClips().FirstOrDefault(x => x.displayName == SUBCLIP0);
                //����
                if (isGrabHand)
                {
                    charaCon.keepHandL_Anime = (handClip.asset as AnimationPlayableAsset).clip;
                    (handClip.asset as AnimationPlayableAsset).clip = grabHandAnime;
                }
                //��������
                else
                {
                    (handClip.asset as AnimationPlayableAsset).clip = charaCon.keepHandL_Anime;
                    charaCon.keepHandL_Anime = null;
                }
            }
            else
            {
                //�T�u�g���b�N
                var subTrack = track.GetChildTracks().FirstOrDefault(x => x.name == SUBTRACK1);
                //�w��N���b�v
                TimelineClip handClip = subTrack.GetClips().FirstOrDefault(x => x.displayName == SUBCLIP1);
                //����
                if (isGrabHand)
                {
                    charaCon.keepHandR_Anime = (handClip.asset as AnimationPlayableAsset).clip;
                    (handClip.asset as AnimationPlayableAsset).clip = grabHandAnime;
                }
                //��������
                else
                {
                    (handClip.asset as AnimationPlayableAsset).clip = charaCon.keepHandR_Anime;
                    charaCon.keepHandR_Anime = null;
                }
            }

            //���f�ׂ̈Ƀf�B���N�^�[�����X�^�[�g����
            TimeLineReStart();
        }

        /// <summary>
        /// �o�C���h�L�������w��ڍs��Ƀo�C���h����
        /// </summary>
        /// <param name="transferChara"></param>
        /// <param name="toTrackName"></param>
        /// <param name="initPos"></param>
        /// <param name="initEulerAngles"></param>
        /// <returns></returns>
        public bool TransferPlayableAsset(CharaController transferChara, string toTrackName, Vector3 initPos, Vector3 initEulerAngles)
        {
            DanceInfoData danceInfoData = new DanceInfoData();

            // �^�C�����C�����̃g���b�N�ꗗ���擾
            IEnumerable<TrackAsset> tracks = timeLineAsset.GetOutputTracks();

            //�]������L������TrackAsset���擾
            TrackAsset track = tracks.FirstOrDefault(x => x.name == transferChara.bindTrackName);
            if (!track) return false;

            //�g���b�N���̃N���b�v��S�擾
            IEnumerable<TimelineClip> clips = track.GetClips();
            // �w�薼�̂̃N���b�v�𔲂��o��
            TimelineClip danceClip = clips.FirstOrDefault(x => x.displayName == "DanceBase");
            danceInfoData.motionOffsetTime = (float)(danceClip.start - motionClip_StartTime);

            //DanceBase�̃A�j���[�V�������擾
            danceInfoData.baseDanceClip = (danceClip.asset as AnimationPlayableAsset).clip;

            TimelineClip handClip;

            //�I�[�o�[���C�h�A�j���[�V�������擾
            foreach (var subTrack in track.GetChildTracks())
            {
                // �g���b�N���̃N���b�v�ꗗ���擾
                clips = subTrack.GetClips();

                switch (subTrack.name)
                {
                    case SUBTRACK0:
                        // �w�薼�̂̃N���b�v�𔲂��o��
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP0);
                        danceInfoData.overrideClip_hand = (handClip.asset as AnimationPlayableAsset).clip;
                        break;
                    case SUBTRACK1:
                        // �w�薼�̂̃N���b�v�𔲂��o��
                        handClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP1);
                        danceInfoData.overrideClip_hand = (handClip.asset as AnimationPlayableAsset).clip;
                        break;
                    case SUBTRACK2:
                        // �w�薼�̂̃N���b�v�𔲂��o��
                        TimelineClip FaceClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP2);
                        danceInfoData.overrideClip_face = (FaceClip.asset as AnimationPlayableAsset).clip;
                        break;
                    case SUBTRACK3:
                        // �w�薼�̂̃N���b�v�𔲂��o��
                        TimelineClip lipClip = clips.FirstOrDefault(x => x.displayName == SUBCLIP3);
                        danceInfoData.overrideClip_lip = (lipClip.asset as AnimationPlayableAsset).clip;
                        break;
                }
            }

            //##### ��������]���揈�� #####
            IEnumerable<PlayableBinding> outputs = playableDirector.playableAsset.outputs;
            //�ڍs����PlayableBinding��null�o�C���h�ŉ������Ă���
            PlayableBinding fromBaseAnime = outputs.FirstOrDefault(x => x.streamName == transferChara.bindTrackName);
            playableDirector.SetGenericBinding(fromBaseAnime.sourceObject, null);

            //�ڍs��̊����L�����m�F
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (trackBindChara[i])
                {
                    if (trackBindChara[i].bindTrackName == transferChara.bindTrackName)
                    {
                        //�]�����̃����N������
                        trackBindChara[i] = null;
                        break;
                    }
                }
            }

            //�ڍs���PlayableBinding���擾
            PlayableBinding toBaseAnime = outputs.FirstOrDefault(x => x.streamName == toTrackName);
            if (toBaseAnime.streamName == "") return false;

            //�I�u�W�F�N�g���ڍs��Ƀo�C���h����
            playableDirector.SetGenericBinding(toBaseAnime.sourceObject, transferChara.gameObject);

            //�o�C���h����t�^
            transferChara.bindTrackName = toTrackName;
            //���X�g�ɓo�^
            switch (toTrackName)
            {
                case "Animation Track_Portal":
                    trackBindChara[0] = transferChara;
                    break;
                case ANITRACK1:
                    trackBindChara[1] = transferChara;
                    break;
                case ANITRACK2:
                    trackBindChara[2] = transferChara;
                    break;
                case ANITRACK3:
                    trackBindChara[3] = transferChara;
                    break;
                case ANITRACK4:
                    trackBindChara[4] = transferChara;
                    break;
                case ANITRACK5:
                    trackBindChara[5] = transferChara;
                    break;
            }


            //�A�j���[�V�������ڍs(�擾�����]�����A�j���[�V�����ŐV�K�o�^)
            SetAnimationClip(toTrackName, danceInfoData, initPos, initEulerAngles);

            //RootMotion�̉���
            //�͂�Ŏ��R�Ɉړ�������ׂɕK�v���������A�ݒu��͈ړ����[�V�����ɃJ�N�c�L�������Ă��܂����߉���
            //�ݒu���W�ݒ��ɉ������Ȃ��ƈʒu�����f����Ȃ��̂Œ���(�܂����̕ύX�̓A�j���[�^�[�̍ď�����������)
            transferChara.GetComponent<Animator>().applyRootMotion = false;

            //�t�B�[���h�J�E���^�[
            FieldCharaCount++;
            FieldCharaUpdate?.Invoke();

            return true;
        }

        /// <summary>
        /// �g���b�N�o�C���h�L�������폜����
        /// </summary>
        /// <param name="chara"></param>
        public void DeletebindAsset(CharaController chara)
        {
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (chara == trackBindChara[i])
                {
                    trackBindChara[i] = null;
                    break;
                }
            }
            Destroy(chara.gameObject);

            //�t�B�[���h�J�E���^�[
            FieldCharaCount--;
            FieldCharaUpdate?.Invoke();
        }

        /// <summary>
        /// ���O��v�Ńg���b�N�o�C���h�L�������폜����
        /// </summary>
        /// <param name="chara"></param>
        public void DeletebindAsset_CleanUp(string hViewName)
        {
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (trackBindChara[i] && hViewName == trackBindChara[i].charaInfoData.viewName)
                {
                    Destroy(trackBindChara[i].gameObject);
                    trackBindChara[i] = null;

                    //�t�B�[���h�J�E���^�[
                    if (i != PORTAL_ELEMENT) FieldCharaCount--;
                }
            }
            FieldCharaUpdate?.Invoke();
        }

        /// <summary>
        /// Field��̃��b�N�o�C���h�L�������폜����
        /// </summary>
        public void DeletebindAsset_FieldAll()
        {
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (i == PORTAL_ELEMENT) continue;
                if (trackBindChara[i])
                {
                    Destroy(trackBindChara[i].gameObject);
                    trackBindChara[i] = null;
                }
            }
            //�t�B�[���h�J�E���^�[
            FieldCharaCount = 0;
            FieldCharaUpdate?.Invoke();
        }

        /// <summary>
        /// �󂢂Ă���g���b�N��T��
        /// </summary>
        /// <returns></returns>
        public bool isFreeTrack(out string freeTrack)
        {
            freeTrack = "";
            for (int i = 1; i < trackBindChara.Length; i++)
            {
                if (trackBindChara[i]) continue;
                else if (i == 1)
                {
                    freeTrack = ANITRACK1;
                    return true;
                }
                else if (i == 2)
                {
                    freeTrack = ANITRACK2;
                    return true;
                }
                else if (i == 3)
                {
                    freeTrack = ANITRACK3;
                    return true;
                }
                else if (i == 4)
                {
                    freeTrack = ANITRACK4;
                    return true;
                }
                else if (i == 5)
                {
                    freeTrack = ANITRACK5;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// �^�C�����C���̕ύX���e�������I?�ɔ��f������
        /// AnimationClip�ύX�������f����Ȃ����߃��X�^�[�g���K�v
        /// �E�E�E�����^�C���͖���?���Č����������ǎ����Ă݂��炢�������Ă���
        /// </summary>
        public void TimeLineReStart()
        {
            //�Đ����Ԃ̋L�^
            double keepTime = playableDirector.time;
            //���������ē��꒼��(����ł������Ⴄ��)
            playableDirector.playableAsset = null;
            playableDirector.playableAsset = timeLineAsset;

            //�O��̑������w��
            playableDirector.time = keepTime;

            ////Track�����X�V����
            //TrackList_Update();

            if (playableDirector.timeUpdateMode == DirectorUpdateMode.GameTime)
            {
                //�Đ�
                playableDirector.Play();

                //���x�X�V(Play��͍ēx�Ăяo���Ȃ��ƃ_���݂���)
                playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_timelineSpeed);

                //���x�X�V
                //TimelineSpeedUpdate();
            }
            if (playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //�X�V
                playableDirector.Evaluate();
            }
        }

        /// <summary>
        /// �Đ���Ԃɂ���
        /// </summary>
        public void TimelinePlay()
        {
            //���[�h���}�j���A������Q�[���^�C�}�[��
            if (playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                playableDirector.timeUpdateMode = DirectorUpdateMode.GameTime;
            }

            //�ĊJ������
            playableDirector.Play();

            //���x�X�V(Play��͍ēx�Ăяo���Ȃ��ƃ_���݂���)
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(_timelineSpeed);

            //���x�X�V
            //TimelineSpeedUpdate();
        }

        /// <summary>
        /// �}�j���A����Ԃɂ���
        /// </summary>
        public void TimelineManualMode()
        {
            //�}�j���A�����[�h��
            playableDirector.timeUpdateMode = DirectorUpdateMode.Manual;

            //AnimatorController���������Ă���
            StartCoroutine(RemoveCharasAniCon());

            //�}�j���A�����[�h�ł̍X�V���J�n
            StartCoroutine(ManualUpdate());

            //playableDirector.Pause();
            //playableDirector.Resume();
        }

        public bool isManualMode()
        {
            return playableDirector.timeUpdateMode == DirectorUpdateMode.Manual;
        }

        /// <summary>
        /// �Đ��ʒu������������
        /// </summary>
        public void TimelineBaseReturn()
        {
            playableDirector.time = 0;
            //TimelinePlay();
        }

        /// <summary>
        /// ���Ԋu�Ń}�j���A�����[�h�ōX�V���s��
        /// </summary>
        /// <returns></returns>
        private IEnumerator ManualUpdate()
        {
            double keepVal = AudioClip_PlaybackTime;
            //1���Ԃ𔽉f������
            playableDirector.Evaluate();
            yield return null;

            while (playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
            {
                //�X�V����Ă��邩
                if (keepVal != AudioClip_PlaybackTime)
                {
                    //��Ԃ𔽉f������
                    playableDirector.Evaluate();

                    //�L�[�v�̍X�V
                    keepVal = AudioClip_PlaybackTime;
                }
                yield return new WaitForSeconds(0.1f);
            }

            //AnimatorController��߂�
            //Manual��ԂŖ߂��ƈ�u�������W�Ɉړ����ă`�����Ă��܂��ׁA���̃^�C�~���O�Ŏ��s
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (!trackBindChara[i]) continue;
                trackBindChara[i].ReturnRunAnime();
            }
        }

        /// <summary>
        /// �L�������A�^�b�`�|�C���g�̗L����Ԃ�؂�ւ���
        /// </summary>
        /// <param name="isActive"></param>
        public void SetActive_AttachPoint(bool isActive)
        {
            //�}�j���A����Ԃ̂�
            if (playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) return;

            foreach (var chara in trackBindChara)
            {
                if (chara == null) continue;
                chara.GetComponent<AttachPointGenerator>().SetActive_AttachPoint(isActive);
            }
        }

        /// <summary>
        /// �L������AnimatorController�ݒ���폜����(timeline��animator�Ƌ������邽��)
        /// </summary>
        private IEnumerator RemoveCharasAniCon()
        {
            //�}�j���A�����[�h�łȂ���Ώ������Ȃ�
            if (playableDirector.timeUpdateMode != DirectorUpdateMode.Manual) yield break;

            //TimeLine�Ƌ������ۂ��̂�AnimatorController���������Ă��� 
            for (int i = 0; i < trackBindChara.Length; i++)
            {
                if (!trackBindChara[i]) continue;
                trackBindChara[i].RemoveRunAnime();
            }

            //�����t���[����ɃA�j���[�V�����̏�Ԃ�1�񂾂��X�V
            yield return null;
            playableDirector.Evaluate();
        }

        /// <summary>
        /// �o�C���h�L�����̃K�C�h���ꊇ�Ő؂�ւ���
        /// </summary>
        /// <param name="isEnable"></param>
        public void SetCharaMeshGuide(bool isEnable)
        {
            foreach (var chara in trackBindChara)
            {
                if (!chara) continue;
                if (chara != trackBindChara[PORTAL_ELEMENT])
                {
                    chara.GetComponent<MeshGuide>().SetGuide(isEnable);
                }
            }
        }
    }
}