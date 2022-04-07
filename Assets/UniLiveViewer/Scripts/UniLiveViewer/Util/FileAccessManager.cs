using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using VRM.FirstPersonSample;

namespace UniLiveViewer 
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class FileAccessManager : MonoBehaviour
    {
        public enum FOLDERTYPE
        {
            CHARA,
            MOTION,
            BGM,
            SETTING
        }

        public static string folderPath_Custom;//VMD��mp3�Ȃ�
        public string sMssage;//�f�o�b�O�p

        public static string[] folderName = { "Chara/", "Motion/", "BGM/", "Setting/" };
        public static string cachePath = "Cache/";
        public static string lipSyncPath = "Lip-sync/";

        public bool isSuccess { get; private set; } = false;

        public AudioClip[] presetAudioClip = new AudioClip[3];
        public int presetCount = 0;
        public List<AudioClip> audioList = new List<AudioClip>();
        public int CurrentAudio = 0;

        public List<string> vmdList = new List<string>();
        public List<string> vmdLipSyncList = new List<string>();

        public int CurrentMotion = 0;

        private List<string> mp3_path = new List<string>();
        private List<string> wav_path = new List<string>();
        private int AudioCount = 0;

        private CancellationToken cancellation_token;

        //�T���l�C���L���b�V���p
        //public static Dictionary<string,Texture2D> cacheThumbnails = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Sprite> cacheThumbnails = new Dictionary<string, Sprite>();
        [SerializeField] private Texture2D texDummy;
        private static SpriteRenderer _renderer;

#if UNITY_EDITOR
        private const int MAX_AUDIOCOUNT = 30;
#elif UNITY_ANDROID
    private const int MAX_AUDIOCOUNT = 10;
#endif

        public event Action onFolderCheckCompleted;
        public event Action onAudioCompleted;
        public event Action onThumbnailCompleted;

        [SerializeField] VRMRuntimeLoader_Custom vrmRuntimeLoader;


        private void Awake()
        {
            //Link���Ɨ�����������̂�elif�K�{��PLATFORM_OCULUS���Ă̂�����݂���
#if UNITY_EDITOR
            folderPath_Custom = "D:/User/UniLiveViewer/";
            sMssage = "windows�Ƃ��ĔF�����Ă��܂�";
#elif UNITY_ANDROID
        folderPath_Custom = "/storage/emulated/0/" + "UniLiveViewer/";
        sMssage = "Quest�Ƃ��ĔF�����Ă��܂�";
#endif
            //folderPath_Custom = Application.persistentDataPath + "/";
            //folderPath_Custom = Application.temporaryCachePath + "/";

            cancellation_token = this.GetCancellationTokenOnDestroy();

            presetCount = presetAudioClip.Length;

            for (int i = 0; i < presetCount; i++)
            {
                audioList.Add(presetAudioClip[i]);
            }

            _renderer = GetComponent<SpriteRenderer>();

            cancellation_token = this.GetCancellationTokenOnDestroy();

            //�A�v���t�H���_�̍쐬
            StartCoroutine(CreateFolder());

        }

        public static string GetFullPath(FOLDERTYPE type)
        {
            return folderPath_Custom + folderName[(int)type];
        }

        public static string GetFullPath_ThumbnailCache()
        {
            return folderPath_Custom + folderName[(int)FOLDERTYPE.CHARA] + cachePath;
        }

        public static string GetFullPath_LipSync()
        {
            return folderPath_Custom + folderName[(int)FOLDERTYPE.MOTION] + lipSyncPath;
        }

        /// <summary>
        /// �A�v���t�H���_��readme.txt���쐬����
        /// </summary>
        private IEnumerator CreateFolder()
        {
            string sFullPath = "";

            try
            {
                //�x�[�X�t�H���_�`�F�b�N
                for (int i = 0; i < folderName.Length; i++)
                {
                    sFullPath = folderPath_Custom + folderName[i];
                    CreateFolder(sFullPath);
                }

                //�L���b�V���t�H���_
                //sFullPath = folderPath_Custom + folderName[(int)FOLDERTYPE.CHARA] + cachePath;
                //CreateFolder(sFullPath);

                //���b�v�V���N�t�H���_
                //sFullPath = folderPath_Custom + folderName[(int)FOLDERTYPE.MOTION] + lipSyncPath;
                //CreateFolder(sFullPath);

                //VMD�̃t�@�C�������擾
                GetAllVMDNames();
                //GetAllVMDLipSyncNames();

                isSuccess = true;
            }
            catch
            {
                isSuccess = false;
            }


            if (isSuccess)
            {
                sFullPath = folderPath_Custom + "readme_ja.txt";
                yield return StartCoroutine(ResourcesLoadText("readme_ja", sFullPath));

                sFullPath = folderPath_Custom + "readme_en.txt";
                yield return StartCoroutine(ResourcesLoadText("readme_en", sFullPath));

                sFullPath = folderPath_Custom + "�s��EDefect.txt";
                yield return StartCoroutine(ResourcesLoadText("�s��EDefect", sFullPath));

                //��������
                onFolderCheckCompleted?.Invoke();

                yield return new WaitForSeconds(0.5f);

                //VRM�̃T���l�C���摜���L���b�V������
                CacheThumbnail().Forget();
            }
        }

        private void CreateFolder(string fullPath)
        {
            //�L���b�V���t�H���_
            try
            {
                var b = Directory.Exists(fullPath);
                if (b) sMssage = "�t�H���_�����ɂ���܂�";
                else
                {
                    sMssage = "�t�H���_���Ȃ��̂ō��܂�";
                    Directory.CreateDirectory(fullPath);
                    sMssage += "�E�E�E�����ł�";
                }
            }
            catch
            {
                sMssage += "�E�E�E���s�ł�";
                throw;
            }
        }


        public IEnumerator ResourcesLoadText(string fileName, string path)
        {
            var resourceFile = Resources.Load<TextAsset>(fileName);
            using (StreamWriter writer = new StreamWriter(path, false, System.Text.Encoding.UTF8))
            {
                writer.Write(resourceFile.text);
            }
            yield return null;
        }

        /// <summary>
        /// �A�v���t�H���_����VRM�t�@�C�������擾
        /// </summary>
        /// <returns></returns>
        public string[] GetAllVRMNames()
        {
            string sFolderPath = folderPath_Custom + folderName[(int)FOLDERTYPE.CHARA];

            string[] sResult = null;
            try
            {
                //VRM�t�@�C���̂݌���
                sResult = Directory.GetFiles(sFolderPath, "*.vrm", SearchOption.TopDirectoryOnly);

                //�t�@�C���p�X����t�@�C�����̒��o
                for (int i = 0; i < sResult.Length; i++)
                {
                    int j = sResult[i].LastIndexOf("/");//�������當���T�[�`�A�擪���牽�Ԗڂ�
                    int maxStr = sResult[i].Length;

                    sResult[i] = sResult[i].Substring(j, maxStr - j);
                    sResult[i] = sResult[i].Replace("/", "");
                }
            }
            catch
            {
                sMssage = "VRM�t�@�C���ǂݍ��݂Ɏ��s���܂���";
            }

            return sResult;
        }

        /// <summary>
        /// �A�v���t�H���_����VMD�t�@�C�������擾
        /// </summary>
        /// <returns></returns>
        private void GetAllVMDNames()
        {
            //������
            if (SaveData.dicVMD_offset.Count != 0)
            {
                SaveData.dicVMD_offset.Clear();
            }

            //offset���t�@�C��������Γǂݍ���
            string path = GetFullPath(FOLDERTYPE.SETTING);
            if (File.Exists(path + "MotionOffset.txt"))
            {
                foreach (string line in File.ReadLines(path + "MotionOffset.txt"))
                {
                    string[] spl = line.Split(',');
                    SaveData.dicVMD_offset.Add(spl[0], int.Parse(spl[1]));
                    //Debug.Log(spl[0] + spl[1]);
                }
            }

            //VMD�t�@�C�������擾
            string sFolderPath = GetFullPath(FOLDERTYPE.MOTION);
            try
            {
                //VRM�t�@�C���̂݌���
                var names = Directory.GetFiles(sFolderPath, "*.vmd", SearchOption.TopDirectoryOnly);

                //�t�@�C���p�X����t�@�C�����̒��o
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = names[i].Replace(sFolderPath, "");
                    vmdList.Add(names[i]);

                    //����offset��񂪂Ȃ���Βǉ�
                    if (!SaveData.dicVMD_offset.ContainsKey(names[i]))
                    {
                        SaveData.dicVMD_offset.Add(names[i], 0);
                    }
                }

                //��U�ۑ�
                SaveData.SaveOffset();
            }
            catch
            {
                sMssage = "VMD�t�@�C���ǂݍ��݂Ɏ��s���܂���";
            }
        }

        /// <summary>
        /// �A�v���t�H���_����VMD�t�@�C�������擾
        /// </summary>
        /// <returns></returns>
        private void GetAllVMDLipSyncNames()
        {
            //VMD�t�@�C�������擾
            string sFolderPath = GetFullPath(FOLDERTYPE.MOTION) + lipSyncPath;
            try
            {
                //VRM�t�@�C���̂݌���
                var names = Directory.GetFiles(sFolderPath, "*.vmd", SearchOption.TopDirectoryOnly);

                //�t�@�C���p�X����t�@�C�����̒��o
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = names[i].Replace(sFolderPath, "");
                    vmdLipSyncList.Add(names[i]);
                }
            }
            catch
            {
                sMssage = "VMDLipSync�t�@�C���ǂݍ��݂Ɏ��s���܂���";
            }
        }

        /// <summary>
        /// BGM�t�H���_�̃t�@�C�������擾
        /// </summary>
        /// <returns></returns>
        public int GetAudioFileCount()
        {
            string sFolderPath = folderPath_Custom + folderName[(int)FOLDERTYPE.BGM];

            //������
            if (mp3_path.Count > 0) mp3_path.Clear();
            if (wav_path.Count > 0) wav_path.Clear();

            //.mp3�t�@�C���������A���X�g�ɒǉ�
            mp3_path.AddRange(Directory.GetFiles(sFolderPath, "*.mp3", SearchOption.TopDirectoryOnly));
            //"*.wav"�t�@�C���������A���X�g�ɒǉ�
            wav_path.AddRange(Directory.GetFiles(sFolderPath, "*.wav", SearchOption.TopDirectoryOnly));

            return mp3_path.Count + wav_path.Count;
        }

        /// <summary>
        /// Audio�t�H���_�̃f�[�^�����[�h�AClip�ŕێ�����
        /// GetAudioFileCount()�����O�ɌĂԕK�v������̉��P������
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IEnumerator AudioLoad()
        {
            //�S���Ă΂�Ă��Ȃ���ΌĂ�
            if (mp3_path.Count == 0 && wav_path.Count == 0)
            {
                GetAudioFileCount();
            }

            //���X�g������
            if (audioList.Count > presetCount)
            {
                //Clip�̓��X�gClear�������Ə����Ȃ��̂Ōʂɏ���
                for (int i = 0; i < audioList.Count; i++)
                {
                    Destroy(audioList[i]);
                }

                audioList.Clear();
                audioList = new List<AudioClip>();
                for (int i = 0; i < presetCount; i++)
                {
                    audioList.Add(presetAudioClip[i]);
                }
                AudioCount = 0;
            }

            foreach (string str in mp3_path)
            {
                if (AudioCount < MAX_AUDIOCOUNT) AudioCount++;
                else break;
                var src = $"file://{str}";
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(src, AudioType.MPEG))
                {
                    ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogError(www.error);
                    }
                    else
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                        clip.name = str.Replace(folderPath_Custom + folderName[(int)FOLDERTYPE.BGM], "");
                        audioList.Add(clip);

                        //Debug.Log("--------------------------------------");
                        //Debug.Log(clip.loadInBackground);
                        //Debug.Log(clip.ambisonic);

                        //Debug.Log(clip.loadType);
                        //Debug.Log(clip.preloadAudioData);

                        //Debug.Log(clip.samples);//�T���v��
                        //Debug.Log(clip.channels);//�`�����l��
                        //Debug.Log(clip.frequency);//���g��

                    }
                    yield return null;
                }
            }

            foreach (string str in wav_path)
            {
                if (AudioCount < MAX_AUDIOCOUNT) AudioCount++;
                else break;
                var src = $"file://{str}";
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(src, AudioType.WAV))
                {
                    ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;

                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.LogError(www.error);
                    }
                    else
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                        clip.name = str.Replace(folderPath_Custom + folderName[(int)FOLDERTYPE.BGM], "");
                        audioList.Add(clip);
                    }
                    yield return null;
                }
            }

            //��������
            onAudioCompleted?.Invoke();
        }

        /// <summary>
        /// �b��
        /// </summary>
        private async UniTaskVoid CacheThumbnail()
        {
            string folderPath_Chara = GetFullPath(FOLDERTYPE.CHARA);
            Texture2D texture = null;
            Sprite spr = null;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);

            //�S�t�@�C�������擾
            var vrmNames = GetAllVRMNames();
            for (int i = 0; i < vrmNames.Length; i++)
            {
                if (spr) spr = null;
                if (texture) texture = null;
                //�L���b�V���摜�m�F
                //texture = cacheThumbnails.FirstOrDefault(x => x.Key == vrmNames[i]).Value;
                spr = cacheThumbnails.FirstOrDefault(x => x.Key == vrmNames[i]).Value;

                if (spr == null)
                {
                    try
                    {
                        //VRM�t�@�C������T���l�C���𒊏o����
                        texture = await vrmRuntimeLoader.GetThumbnail(folderPath_Chara + vrmNames[i], cancellation_token);

                        if (texture)
                        {
                            //�e�N�X�`�����X�v���C�g�ɕϊ�
                            spr = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                            //���X�g�ɒǉ�
                            cacheThumbnails.Add(vrmNames[i], spr);
                        }
                        else
                        {
                            //�_�~�[�摜����
                            //texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
                            texture = Instantiate(texDummy);
                            //�e�N�X�`�����X�v���C�g�ɕϊ�
                            spr = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                            //���X�g�ɒǉ�
                            cacheThumbnails.Add(vrmNames[i], spr);

                        }
                    }
                    catch (System.OperationCanceledException)
                    {
                        Debug.Log("�T���l�C���L���b�V�����ɒ��f");
                        throw;
                    }
                }
                else
                {

                }
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);
            }

            //��������
            onThumbnailCompleted?.Invoke();
        }

        /// <summary>
        /// ���@�ɂĕs�����̂Ŏ���
        /// </summary>
        //private async UniTaskVoid CacheThumbnail_test()
        //{

        //    string folderPath_Chara = GetFullPath(FOLDERTYPE.CHARA);
        //    string folderPath_Cache = GetFullPath_ThumbnailCache();
        //    Texture2D texture = null;

        //    if(cacheThumbnails != null && cacheThumbnails.Count > 0)
        //    {
        //        cacheThumbnails.Clear();
        //        cacheThumbnails = new Dictionary<string, Texture2D>();
        //    }

        //    await UniTask.Yield(PlayerLoopTiming.Update,cancellation_token);

        //    //�S�t�@�C�������擾
        //    var vrmNames = GetAllVRMNames();
        //    for (int i = 0; i < vrmNames.Length; i++)
        //    {
        //        if (texture) texture = null;
        //        //�L���b�V���摜�m�F
        //        var isCache = File.Exists(folderPath_Cache + vrmNames[i] + ".png");
        //        if (isCache)
        //        {
        //            //�L���b�V���ς݉摜�𗬗p
        //            texture = GetCacheThumbnail(vrmNames[i]);

        //            //���X�g�ɒǉ�
        //            cacheThumbnails.Add(vrmNames[i], texture);
        //        }
        //        else
        //        {
        //            try
        //            {
        //                //VRM�t�@�C������T���l�C���𒊏o����
        //                texture = await vrmRuntimeLoader.GetThumbnail(folderPath_Chara + vrmNames[i], cancellation_token);

        //                if (texture)
        //                {
        //                    //���T�C�Y
        //                    texture = ResizeTexture(texture, 256, 256);
        //                    //�F���擾(CPU���̏����ł͐F�̏������擾�ł����D�F�摜�ɂȂ�̂�)
        //                    texture = GetColorInfo(texture);
        //                    //���X�g�ɒǉ�
        //                    cacheThumbnails.Add(vrmNames[i], texture);
        //                    //�t�H���_�ɂ��ۑ�
        //                    File.WriteAllBytes(folderPath_Cache + vrmNames[i] + ".png", texture.EncodeToPNG());
        //                }
        //                else
        //                {
        //                    //�_�~�[�摜����
        //                    texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
        //                    //���X�g�ɒǉ�
        //                    cacheThumbnails.Add(vrmNames[i], texture);
        //                    //�t�H���_�ɂ��ۑ�
        //                    File.WriteAllBytes(folderPath_Cache + vrmNames[i] + ".png", texture.EncodeToPNG());
        //                }
        //            }
        //            catch (System.OperationCanceledException)
        //            {
        //                Debug.Log("�T���l�C���L���b�V�����ɒ��f");
        //                throw;
        //            }
        //        }
        //        await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);
        //    }

        //    //��������
        //    onThumbnailCompleted?.Invoke();
        //}

        /// <summary>
        /// �e�N�X�`���̃��T�C�Y
        /// </summary>
        /// <param name="srcTexture"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public static Texture2D ResizeTexture(Texture2D srcTexture, int newWidth, int newHeight)
        {
            //�w�肵�Ȃ���RGBA32�ɂȂ��Ă��܂����̂ňꉞ
            var resizedTexture = new Texture2D(newWidth, newHeight, TextureFormat.ARGB32, false);
            Graphics.ConvertTexture(srcTexture, resizedTexture);
            return resizedTexture;
        }

        /// <summary>
        /// �V�F�[�_�[���ŕ`�悵�ăe�N�X�`���ɏ������ނ��Ԃ�i��̃R�s�y�j
        /// �f�v�X�n�𖳌������Ȃ���Quest���@�ł͓����Ȃ������̂ŗp��
        /// </summary>
        /// <param name="texture2D"></param>
        /// <returns></returns>
        public static Texture2D GetColorInfo(Texture2D texture2D)
        {
            //Texture mainTexture = _renderer.material.mainTexture;
            //RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = new RenderTexture(texture2D.width, texture2D.height, 0);//��O�f�v�X
            Graphics.Blit(texture2D, renderTexture, _renderer.sharedMaterial);//�f�v�X�����������}�e���A��

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.ReleaseTemporary(renderTexture);

            //Color[] pixels = texture2D.GetPixels();
            //RenderTexture.active = currentRT;

            return texture2D;
        }

        /// <summary>
        /// �L���b�V�������T���l�C�����擾
        /// </summary>
        /// <param name="filePath">.png</param>
        /// <returns></returns>
        private static Texture2D GetCacheThumbnail(string fileName)
        {
            string filePath = GetFullPath_ThumbnailCache() + fileName + ".png";
            byte[] bytes = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(64, 64);
            texture.LoadImage(bytes);
            return texture;
        }
    }

}
