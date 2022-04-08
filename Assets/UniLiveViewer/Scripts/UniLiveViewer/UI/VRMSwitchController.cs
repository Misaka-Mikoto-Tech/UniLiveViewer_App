using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using VRM.FirstPersonSample;
using NanaCiel;

namespace UniLiveViewer
{
    //[RequireComponent(typeof(FileAccessManager))]
    public class VRMSwitchController : MonoBehaviour
    {
        [SerializeField] private Transform[] pageTransform;
        [SerializeField] private Transform displayAnchor;

        [Space(1)]
        [Header("��1�y�[�W��")]
        [SerializeField] private TextMesh textDirectory;
        [SerializeField] private VRMRuntimeLoader_Custom runtimeLoader;//�T���v�������̂܂ܗ��p����
        [SerializeField] private Button_Base btnPrefab;
        private List<TextMesh> btnTexts = new List<TextMesh>();
        [SerializeField] private Transform btnParent;
        [SerializeField] private LoadAnimation anime_Loading;

        [Space(1)]
        [Header("��2�y�[�W��")]
        [SerializeField] private Transform vrmPresetAnchor;//�}�e���A���������p�̃L�������W�A���J�[
        [SerializeField] private RollSelector rollSelector_Material;
        [SerializeField] private Button_Switch[] btn_SuefaceType = new Button_Switch[2];
        [SerializeField] private Button_Switch[] btn_RenderFace = new Button_Switch[3];
        [SerializeField] private SliderGrabController slider_Transparent = null;
        [SerializeField] private Button_Base btn_AllReset;
        [SerializeField] private Button_Base btn_SetOK;
        private GameObject vrmModel;
        private MaterialConverter converter;

        [Header("���A�^�b�`���[��")]
        [SerializeField] private ComponentAttacher_VRM attacherPrefab;

        [Header("�����̑���")]
        //����\��p�T�E���h
        [SerializeField] private AudioClip[] specialFaceAudioClip;
        //�N���b�NSE
        private AudioSource audioSource;
        [SerializeField] private AudioClip[] Sound;//�{�^����,�ǂݍ��݉�,�N���b�N��                               
        //VRM�ǂݍ��ݎ��C�x���g
        public event Action<CharaController> VRMAdded;
        //�t�@�C���A�N�Z�X�ƃT���l�̊Ǘ�
        private FileAccessManager fileManager;
        //private Dictionary<string, Sprite> dicVRMSprite = new Dictionary<string, Sprite>();
        //�����蔻��
        private VRMTouchColliders touchCollider = null;

        private int currentPage = 0;
        private CancellationToken cancellation_token;
        private int[] randomBox;
        private string[] vrmNames;

        private void Awake()
        {
            fileManager = GameObject.FindGameObjectWithTag("AppConfig").GetComponent<FileAccessManager>();
            touchCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<VRMTouchColliders>();

            audioSource = GetComponent<AudioSource>();
            audioSource.volume = GlobalConfig.soundVolume_SE;

            //�R�[���o�b�N�o�^�E�E�E2�y�[�W��
            foreach (var e in btn_SuefaceType)
            {
                e.onTrigger += MaterialSetting_Change;
            }
            foreach (var e in btn_RenderFace)
            {
                e.onTrigger += MaterialSetting_Change;
            }
            slider_Transparent.ValueUpdate += MaterialSetting_TransparentColor;
            rollSelector_Material.onTouch += MaterialInfoUpdate;
            btn_AllReset.onTrigger += MaterialSetting_AllReset;
            btn_SetOK.onTrigger += MaterialSetting_SetOK;

            cancellation_token = this.GetCancellationTokenOnDestroy();

            //�T���l�p�{�^���̐���
            CreateThumbnailButtons(cancellation_token).Forget();
        }

        /// <summary>
        /// �T���l�p�̋�{�^������
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTaskVoid CreateThumbnailButtons(CancellationToken token)
        {

            Vector2 btnPos;
            Button_Base btn;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    //���W����
                    btnPos.x = -0.3f + (j * 0.15f);
                    btnPos.y = 0 - (i * 0.15f);

                    //����
                    btn = Instantiate(btnPrefab);

                    //�e�ݒ�
                    btn.transform.parent = btnParent;
                    btn.transform.localPosition = new Vector3(btnPos.x, btnPos.y, 0);
                    btn.transform.localRotation = Quaternion.identity;

                    //�R�[���o�b�N�o�^
                    btn.onTrigger += (b) => LoadVRM(b).Forget();

                    //�e�L�X�g���b�V�������X�g�ɉ�����
                    btnTexts.Add(btn.transform.GetChild(1).GetComponent<TextMesh>());
                    btn = null;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        /// <summary>
        /// UI�̕\����Ԃ�ύX
        /// </summary>
        /// <param name="isHide"></param>
        public void SetUIView(bool isHide)
        {
            if (isHide)
            {
                if (displayAnchor.gameObject.activeSelf) displayAnchor.gameObject.SetActive(false);
            }
            else
            {
                if (!displayAnchor.gameObject.activeSelf) displayAnchor.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// �J�����g�y�[�W�ŊJ�������i�������j
        /// </summary>
        public void initPage()
        {
            //UI��\��
            SetUIView(false);

            //�y�[�W�X�C�b�`
            for (int i = 0; i < pageTransform.Length; i++)
            {
                if (currentPage == i)
                {
                    if (!pageTransform[i].gameObject.activeSelf) pageTransform[i].gameObject.SetActive(true);
                }
                else
                {
                    if (pageTransform[i].gameObject.activeSelf) pageTransform[i].gameObject.SetActive(false);
                }
            }

            //�e����������
            switch (currentPage)
            {
                case 0:
                    if (fileManager.isSuccess)
                    {
                        //�t�H���_�p�X�̕\�����X�V
                        textDirectory.text = "(" + FileAccessManager.folderPath_Custom + ")";

                        //���[�f�B���O�A�j���[�V�����𖳌����
                        anime_Loading.gameObject.SetActive(false);

                        //�T���l�{�^���A���J�[��L�����
                        btnParent.gameObject.SetActive(true);

                        //VRM�I���{�^���𐶐�����
                        SetThumbnail(cancellation_token).Forget();
                    }
                    break;
                case 1:
                    //VRM�����݂��Ă����UI���X�V
                    if (vrmPresetAnchor.GetChild(0))
                    {
                        //��]�Z���N�^�[�Ƀ}�e���A�����̃��X�g��n��
                        List<string> strList = new List<string>();
                        foreach (var e in converter.materials)
                        {
                            strList.Add(e.name);
                        }
                        rollSelector_Material.init(strList);

                        //�\�����X�V
                        MaterialInfoUpdate();
                    }
                    break;
            }
        }



        /// <summary>
        /// VRM�̐������T���l�{�^���𐶐�����
        /// </summary>
        private async UniTaskVoid SetThumbnail(CancellationToken token)
        {
            //��U�S����\��
            for (int i = 0; i < btnTexts.Count; i++)
            {
                if (btnTexts[i].transform.parent.gameObject.activeSelf)
                {
                    btnTexts[i].transform.parent.gameObject.SetActive(false);
                }
            }

            if (randomBox == null)
            {
                //�SVRM�t�@�C�������擾
                var array = fileManager.GetAllVRMNames();
                //�ő�15���Ɋۂ߂�
                if (array.Length > 15)
                {
                    vrmNames = array.Take(15).ToArray();
                }
                else
                {
                    vrmNames = array;
                }
                //�����_���z���ݒ�
                randomBox = new int[vrmNames.Length];
                for (int i = 0; i < randomBox.Length; i++) randomBox[i] = i;
                randomBox = Shuffle(randomBox);
            }
            await UniTask.Delay(10, cancellationToken: token);

            Button_Base baseButton;
            Sprite spr = null;
            //Texture2D texture = null;
            int index = 0;

            //�K�v�ȃ{�^���̂ݗL�������Đݒ肷��
            for (int i = 0; i < vrmNames.Length; i++)
            {
                //�����_���ȃ{�^����
                index = randomBox[i];

                UniTask.Void(async () =>
                {
                    baseButton = btnTexts[index].transform.parent.GetComponent<Button_Base>();

                    if (!baseButton.gameObject.activeSelf)
                    {
                        baseButton.gameObject.SetActive(true);
                    }
                    //�I�u�W�F�N�g����ύX
                    baseButton.name = vrmNames[index];
                    //�{�^���̕\������ύX
                    btnTexts[index].text = vrmNames[index];
                    //�����T�C�Y�𒲐�����
                    btnTexts[index].fontSize = btnTexts[index].text.FontSizeMatch(500, 25, 40);

                    //�T���l�C�����擾
                    try
                    {
                        spr = FileAccessManager.cacheThumbnails[vrmNames[index]];

                        if (spr)
                        {
                            //�T���l�̗e�ʂŗL���𔻒�
                            //float size = texture.GetRawTextureData().LongLength;
                            //�T���l�C����������
                            //if (size < 10)
                            //    {
                            //        //default�̂܂�
                            //    }
                            //    //�T���l�C���L�蔻��
                            //    else
                            //    {
                            //    }

                            //�X�v���C�g���Z�b�g
                            baseButton.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = spr;
                            //baseButton.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", texture);
                        }
                        else
                        {
                            //default�̂܂�
                        }
                    }
                    catch
                    {
                        if (!spr)
                        {
                            //Debug.Log("���W�b�N�G���[�B�A�v���𗧂��グ��ɃL���b�V���摜���폜�����H");
                            //�΍�Ƃ��ă{�^�����\��
                            if (btnTexts[index].transform.parent.gameObject.activeSelf)
                            {
                                btnTexts[index].transform.parent.gameObject.SetActive(false);
                            }
                        }
                    }
                    finally
                    {
                        spr = null;

                        if (i % 2 == 0)
                        {
                            //���ԍ��œǂݍ��݉���炷
                            await UniTask.Delay(500, cancellationToken: token);
                            //yield return new WaitForSeconds(0.5f);
                            audioSource.PlayOneShot(Sound[1]);
                            //StartCoroutine(DelaySound());
                        }
                    }
                });

                //���ԍ��œǂݍ��܂���
                //await Task.Delay(200);
                if (i % 2 == 0) await UniTask.Delay(250, cancellationToken: token);
            }
        }

        /// <summary>
        /// �����_���V���b�t���i�����_����2�v�f���������V���b�t������Ȃ��v�f�����肦��j
        /// </summary>
        /// <param name="num"></param>
        int[] Shuffle(int[] inputArray)
        {
            for (int i = 0; i < inputArray.Length; i++)
            {
                int temp = inputArray[i];
                int randomIndex = UnityEngine.Random.Range(0, inputArray.Length);
                inputArray[i] = inputArray[randomIndex];
                inputArray[randomIndex] = temp;
            }
            return inputArray;
        }

        //private void UniTask_Test(Vector2 setPos,string fileName)
        //{

        //    //���O�ƍ��œo�^�ς݂̃X�v���C�g������Ύ擾
        //    Sprite spr = dicVRMSprite.FirstOrDefault(x => x.Key == fileName).Value;
        //    if (spr)
        //    {
        //        //�{�^����SpriteRender�ɃZ�b�g
        //        btn.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = spr;
        //    }
        //    else
        //    {
        //        //VRM�t�@�C������T���l�̃e�N�X�`�����擾
        //        //string fullPath = FileAccessManager.GetFullPath(FileAccessManager.FOLDERTYPE.CHARA) + vrmNames[i];
        //        //string fullPath = FileAccessManager.GetFullPath_ThumbnailCash() +  + ".png";
        //        var texture = FileAccessManager.GetCacheThumbnail(fileName);
        //        //var texture = await runtimeLoader.GetThumbnail(fullPath);

        //        if (texture)
        //        {
        //            float size = texture.GetRawTextureData().LongLength;

        //            //�T���l�C����������
        //            if (size < 10)
        //            {
        //                //default�̃X�v���C�g���擾
        //                spr = btn.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite;
        //            }
        //            //�T���l�C���L�蔻��
        //            else
        //            {
        //                //�e�N�X�`�����X�v���C�g�ɕϊ�
        //                spr = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        //                //�{�^����SpriteRender�ɃZ�b�g
        //                btn.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = spr;
        //            }
        //        }
        //        else
        //        {
        //            Debug.Log("�v���O�����G���[�B�L���b�V������");

        //            //default�̃X�v���C�g���擾
        //            //spr = btn.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite;
        //        }
        //        //���O�ƃT���l��o�^����
        //        dicVRMSprite.Add(fileName, spr);
        //    }
        //    //�R�[���o�b�N�o�^
        //    btn.GetComponent<Button_Base>().onTrigger += OpenVRM;
        //}

        /// <summary>
        /// VRM��ǂݍ���
        /// </summary>
        /// <param name="btn">�Y���T���l�{�^��</param>
        private async UniTaskVoid LoadVRM(Button_Base btn)
        {
            //�d���N���b�N�ł��Ȃ��悤�Ƀ{�^���𖳌���
            btnParent.gameObject.SetActive(false);

            //�N���b�N��
            audioSource.PlayOneShot(Sound[0]);

            //���[�f�B���O�A�j���[�V��������U��\��
            //if (anime_Loading.gameObject.activeSelf) anime_Loading.gameObject.SetActive(false);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);

            //���[�f�B���O�A�j���[�V�����J�n
            anime_Loading.gameObject.SetActive(true);

            try
            {
                //VRM�ݒ�
                await SetVRM(btn, cancellation_token);
            }
            catch (Exception e)
            {
                Debug.Log("���[�h���s:" + e);
            }
            finally
            {
                //���[�f�B���O�A�j���[�V�����I��
                anime_Loading.gameObject.SetActive(false);
            }
        }

        /// <summary>
        ///
        /// ��vrmModel�擾��ɔ񓯊��ɂ�����炸wait�����ނƃ��f���̗h�ꕨ���������ȏ�ԂɂȂ錴���s��
        /// </summary>
        /// <param name="btn"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async UniTask SetVRM(Button_Base btn, CancellationToken token)
        {
            try
            {
                //SampleUI��L����
                runtimeLoader.gameObject.SetActive(true);

                //await Task.Delay(10);
                await UniTask.Delay(10, cancellationToken: token);

                //�w��p�X��VRM�̂ݓǂݍ���
                //string fileName = btn.transform.GetChild(1).GetComponent<TextMesh>().text;
                string fileName = btn.transform.name;
                string fullPath = FileAccessManager.GetFullPath(FileAccessManager.FOLDERTYPE.CHARA) + fileName;

                //if (vrmModel) vrmModel = null;
                vrmModel = await runtimeLoader.OnOpenClicked_VRM(fullPath, token);

                //�L�����Z���m�F
                token.ThrowIfCancellationRequested();

                //�Œ���̐ݒ�
                vrmModel.name = fileName;
                vrmModel.tag = "Grab_Chara";
                vrmModel.layer = LayerMask.NameToLayer("GrabObject");
                var characon = vrmModel.AddComponent<CharaController>();

                //�e��component�ǉ�
                var attacher = Instantiate(attacherPrefab.gameObject).GetComponent<ComponentAttacher_VRM>();
                await attacher.init(vrmModel.transform, touchCollider,token);

                Destroy(attacher.gameObject);

                //�}�e���A���R���o�[�^�[�̒ǉ�
                if (converter) converter = null;
                converter = vrmModel.AddComponent<MaterialConverter>();
                converter.InitMaterials();

                //�ǂݍ���VRM
                if (characon)
                {
                    // TODO:�V�F�[�_�[�ǂ����邩

                    //�������K�v�ȃ}�e���A�������邩
                    if (converter.materials != null && converter.materials.Count > 0)
                    {
                        //VRM���v���Z�b�g�A���J�[�Ɉړ�
                        vrmModel.transform.parent = vrmPresetAnchor;
                        vrmModel.transform.localPosition = Vector3.zero;
                        vrmModel.transform.localRotation = Quaternion.identity;

                        //�}�e���A���ݒ�y�[�W��
                        currentPage = 1;
                        initPage();
                    }
                    else
                    {
                        //�y�[�W���ŏ���
                        currentPage = 0;

                        //��������Ɨh����̂����[�Ȉʒu�Ōł܂�(����Ȉʒu�ɗ��������܂ŃC���X�^���X�����֎~)
                        await UniTask.Delay(500, cancellationToken: token);

                        //VRM�ǉ�����
                        VRMAdded?.Invoke(characon);

                        vrmModel.gameObject.SetActive(false);//���������Ă���

                        //UI���\���ɂ���
                        SetUIView(true);

                        vrmModel = null;//�Ǘ�������
                    }

                }
            }
            catch
            {
                if (vrmModel) Destroy(vrmModel);
                vrmModel = null;

                //�y�[�W���ŏ���
                currentPage = 0;

                //�T���v��UI�𖳌���Ԃ�(�e��Disable����VRM�̊eAwake������Ȃ�)
                runtimeLoader.gameObject.SetActive(false);

                //UI���\���ɂ���
                SetUIView(true);

                Debug.Log("VRM�ǂݍ��݃G���[");
                throw;
            }
            finally
            {

            }
        }

        /// <summary>
        /// �}�e���A���̕\�������X�V����
        /// </summary>
        private void MaterialInfoUpdate()
        {
            //�N���b�N��
            audioSource.PlayOneShot(Sound[0]);

            int current = rollSelector_Material.current;
            var type = (MaterialConverter.SurfaceType)converter.materials[current].GetFloat("_Surface");
            var face = (MaterialConverter.RenderFace)converter.materials[current].GetFloat("_Cull");
            var color = converter.materials[current].GetColor("_BaseColor");

            //button�ɔ��f
            if (type == MaterialConverter.SurfaceType.Opaque)
            {
                btn_SuefaceType[0].isEnable = true;
                btn_SuefaceType[1].isEnable = false;
                //�X���C�_�[������
                slider_Transparent.gameObject.SetActive(false);
            }
            else if (type == MaterialConverter.SurfaceType.Transparent)
            {
                btn_SuefaceType[0].isEnable = false;
                btn_SuefaceType[1].isEnable = true;
                //�X���C�_�[�L����
                slider_Transparent.gameObject.SetActive(true);
                slider_Transparent.Value = color.a;
            }
            //button�ɔ��f
            if (face == MaterialConverter.RenderFace.Front)
            {
                btn_RenderFace[0].isEnable = true;
                btn_RenderFace[1].isEnable = false;
                btn_RenderFace[2].isEnable = false;
            }
            else if (face == MaterialConverter.RenderFace.Back)
            {
                btn_RenderFace[0].isEnable = false;
                btn_RenderFace[1].isEnable = true;
                btn_RenderFace[2].isEnable = false;
            }
            else if (face == MaterialConverter.RenderFace.Both)
            {
                btn_RenderFace[0].isEnable = false;
                btn_RenderFace[1].isEnable = false;
                btn_RenderFace[2].isEnable = true;
            }
        }

        /// <summary>
        /// �}�e���A���ݒ��ύX
        /// </summary>
        /// <param name="btn"></param>
        private void MaterialSetting_Change(Button_Base btn)
        {
            int current = rollSelector_Material.current;

            if (btn == btn_SuefaceType[0])
            {
                converter.SetSurface(current, MaterialConverter.SurfaceType.Opaque);
            }
            else if (btn == btn_SuefaceType[1])
            {
                converter.SetSurface(current, MaterialConverter.SurfaceType.Transparent);
            }
            else if (btn == btn_RenderFace[0])
            {
                converter.SetRenderFace(current, MaterialConverter.RenderFace.Front);
            }
            else if (btn == btn_RenderFace[1])
            {
                converter.SetRenderFace(current, MaterialConverter.RenderFace.Back);
            }
            else if (btn == btn_RenderFace[2])
            {
                converter.SetRenderFace(current, MaterialConverter.RenderFace.Both);
            }

            //UI�\�����X�V
            MaterialInfoUpdate();
        }

        /// <summary>
        /// �}�e���A���̓����F��ݒ�
        /// </summary>
        private void MaterialSetting_TransparentColor()
        {
            //�������X�V
            converter.SetColor_Transparent(rollSelector_Material.current, slider_Transparent.Value);
        }

        /// <summary>
        /// �}�e���A���ݒ�����Z�b�g
        /// </summary>
        /// <param name="btn"></param>
        private void MaterialSetting_AllReset(Button_Base btn)
        {
            //�N���b�N��
            audioSource.PlayOneShot(Sound[0]);

            //�}�e���A�������Z�b�g
            converter.ResetMaterials();

            //UI�\�����X�V
            MaterialInfoUpdate();
        }

        /// <summary>
        /// �}�e���A���ݒ�̊m��
        /// </summary>
        /// <param name="btn"></param>
        private void MaterialSetting_SetOK(Button_Base btn)
        {
            //�y�[�W���ŏ���
            currentPage = 0;

            //�N���b�N��
            audioSource.PlayOneShot(Sound[0]);

            vrmModel.transform.parent = runtimeLoader.transform;
            vrmModel.transform.localPosition = Vector3.zero;
            vrmModel.transform.localRotation = Quaternion.identity;

            vrmModel.gameObject.SetActive(false);//���������Ă���

            //UI���\���ɂ���
            SetUIView(true);

            //VRM�ǉ�����
            VRMAdded?.Invoke(vrmModel.GetComponent<CharaController>());

            vrmModel = null;//�Ǘ�������
        }
    }
}