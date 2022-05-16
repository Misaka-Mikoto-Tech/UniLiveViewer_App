using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using VRM.FirstPersonSample;


namespace UniLiveViewer
{
    public class VRMSwitchController : MonoBehaviour
    {
        public static int loadVRMID = 0;

        [SerializeField] private Transform[] pageTransform;
        [SerializeField] private Transform displayAnchor;

        [Space(1)]
        [Header("＜1ページ＞")]
        [SerializeField] private TextMesh textDirectory;
        [SerializeField] private VRMRuntimeLoader_Custom runtimeLoader;//サンプルをそのまま利用する
        [SerializeField] private LoadAnimation anime_Loading;
        [SerializeField] private ThumbnailController thumbnailCon;

        [Space(1)]
        [Header("＜2ページ＞")]
        [SerializeField] private PrefabEditor prefabEditor;
        [SerializeField] private Button_Base btn_Apply;

        [Space(1)]
        [Header("＜3ページ＞")]
        [SerializeField] private TextMesh textErrorResult;

        [Header("＜アタッチャー＞")]
        [SerializeField] private ComponentAttacher_VRM attacherPrefab;

        [Header("＜その他＞")]
        //特殊表情用サウンド
        [SerializeField] private AudioClip[] specialFaceAudioClip;
        //クリックSE
        private AudioSource audioSource;
        [SerializeField] private AudioClip[] Sound;//ボタン音,読み込み音,クリック音                               
        //VRM読み込み時イベント
        public event Action<CharaController> VRMAdded;
        public event Action<CharaController> onSetupComplete;
        //ファイルアクセスとサムネの管理
        private FileAccessManager fileManager;
        //当たり判定
        private VRMTouchColliders touchCollider = null;

        private CancellationToken cancellation_token;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = SystemInfo.soundVolume_SE;

            //コールバック登録・・・2ページ目
            btn_Apply.onTrigger += (btn) => PrefabApply(btn).Forget();
            prefabEditor.onCurrentUpdate += () => { audioSource.PlayOneShot(Sound[0]); };//クリック音
            cancellation_token = this.GetCancellationTokenOnDestroy();
        }

        private async void Start()
        {
            fileManager = GameObject.FindGameObjectWithTag("AppConfig").GetComponent<FileAccessManager>();
            touchCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<VRMTouchColliders>();

            //サムネ用ボタンの生成
            Button_Base[] btns = await thumbnailCon.CreateThumbnailButtons();
            for (int i = 0; i < btns.Length; i++)
            {
                btns[i].onTrigger += (b) => LoadVRM(b).Forget();
            }

            thumbnailCon.onGenerated += async () =>
            {
                await UniTask.Delay(500, cancellationToken: cancellation_token);
                audioSource.PlayOneShot(Sound[1]);
            };

            UIShow(false);
        }

        /// <summary>
        /// UIの表示状態を変更
        /// </summary>
        /// <param name="isHide"></param>
        public void UIShow(bool isEnabel)
        {
            if (displayAnchor.gameObject.activeSelf != isEnabel) displayAnchor.gameObject.SetActive(isEnabel);
        }

        /// <summary>
        /// カレントページで開き直す（初期化）
        /// </summary>
        public void InitPage(int currentPage)
        {
            //UIを表示
            UIShow(true);

            //ページスイッチ
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

            //各初期化処理
            switch (currentPage)
            {
                case 0:
                    if (!fileManager.isSuccess) return;
                    //フォルダパスの表示を更新
                    textDirectory.text = $"({FileAccessManager.GetFullPath(FileAccessManager.FOLDERTYPE.CHARA)}/)";

                    //ローディングアニメーションを無効状態
                    anime_Loading.gameObject.SetActive(false);

                    //サムネボタンアンカーを有効状態
                    thumbnailCon.gameObject.SetActive(true);

                    //VRM選択ボタンを生成する
                    thumbnailCon.SetThumbnail(fileManager.GetAllVRMNames()).Forget();
                    break;
                case 1:
                    //prefabEditor.Init();

                    //表示を更新
                    //MaterialInfoUpdate();
                    break;
                case 2:
                    break;
            }
        }

        /// <summary>
        /// VRMを読み込む
        /// </summary>
        /// <param name="btn">該当サムネボタン</param>
        private async UniTaskVoid LoadVRM(Button_Base btn)
        {
            //重複クリックできないようにボタンを無効化
            thumbnailCon.gameObject.SetActive(false);

            //クリック音
            audioSource.PlayOneShot(Sound[0]);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation_token);

            //ローディングアニメーション開始
            anime_Loading.gameObject.SetActive(true);

            GameObject vrmModel = null;
            try
            {
                //SampleUIを有効化
                runtimeLoader.gameObject.SetActive(true);
                await UniTask.Delay(10, cancellationToken: cancellation_token);

                //指定パスのVRMのみ読み込む
                string fileName = btn.transform.name;
                string fullPath = FileAccessManager.GetFullPath(FileAccessManager.FOLDERTYPE.CHARA) + "/" + fileName;

                var instance = await runtimeLoader.OnOpenClicked_VRM(fullPath, cancellation_token);

                //Meshが消える対策
                instance.EnableUpdateWhenOffscreen();

                //キャンセル確認
                cancellation_token.ThrowIfCancellationRequested();

                //最低限の設定
                vrmModel = instance.gameObject;
                vrmModel.name = fileName;
                vrmModel.tag = SystemInfo.tag_GrabChara;
                vrmModel.layer = SystemInfo.layerNo_GrabObject;

                //各種component追加
                var attacher = Instantiate(attacherPrefab.gameObject).GetComponent<ComponentAttacher_VRM>();
                await attacher.Init(vrmModel.transform, instance.SkinnedMeshRenderers, cancellation_token);
                await attacher.Attachment(touchCollider, cancellation_token);

                //VRM追加した
                VRMAdded?.Invoke(attacher.CharaCon);

                //UIを非表示にする
                UIShow(false);
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception e)
            {
                if (vrmModel) Destroy(vrmModel);

                runtimeLoader.gameObject.SetActive(false);

                //errorページ
                InitPage(2);

                Debug.Log(e);
                System.IO.StringReader rs = new System.IO.StringReader(e.ToString());
                textErrorResult.text = $"{rs.ReadLine()}";//1行まで
                await UniTask.Delay(5000, cancellationToken: cancellation_token);

                //UIを非表示にする
                UIShow(false);
            }
            finally
            {
                //ローディングアニメーション終了
                anime_Loading.gameObject.SetActive(false);
            }
        }

        public void VRMEditing(CharaController _vrmModel)
        {
            //編集対象へ
            prefabEditor.SetEditingTarget(_vrmModel);
            //マテリアル設定ページへ
            InitPage(1);
        }

        /// <summary>
        /// 設定の確定
        /// </summary>
        /// <param name="btn"></param>
        private async UniTask PrefabApply(Button_Base btn)
        {
            //クリック音
            audioSource.PlayOneShot(Sound[0]);

            var vrm = prefabEditor.EditTarget;

            if (vrm.animationMode == CharaController.ANIMATIONMODE.VMD)
            {
                var vmdPlayer = vrm.GetComponent<VMDPlayer_Custom>();
                vmdPlayer.Clear();
            }
            else if(vrm.animationMode == CharaController.ANIMATIONMODE.CLIP)
            {
                vrm.GetComponent<Animator>().enabled = false;
            }
            vrm.SetState(CharaController.CHARASTATE.NULL, null);
            vrm.transform.parent = runtimeLoader.transform;
            vrm.transform.localPosition = Vector3.zero;
            vrm.transform.localRotation = Quaternion.identity;
            await UniTask.Delay(1000, cancellationToken: cancellation_token);

            vrm.SetEnabelSpringBones(false);//Prefab化で値が残ってしまうので無効化
            vrm.GetComponent<Animator>().enabled = true;
            vrm.animationMode = CharaController.ANIMATIONMODE.CLIP;
            vrm.gameObject.SetActive(false);
            await UniTask.Yield(cancellation_token);

            onSetupComplete?.Invoke(vrm);

            //UIを非表示にする
            UIShow(false);

        }
    }
}