using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;


namespace UniLiveViewer
{
    public class VRMSwitchController : MonoBehaviour
    {
        /// <summary>
        /// TODO: 処せ
        /// </summary>
        public static int loadVRMID = 0;

        /// <summary>
        /// 全体のONOFF用
        /// </summary>
        [SerializeField] Transform _root;
        /// <summary>
        /// 各ページ
        /// </summary>
        [SerializeField] Transform[] _pageTransform;

        [Space(1), Header("＜1ページ＞")]
        [SerializeField] TextMesh[] _textDirectory;
        IVRMLoaderUI _vrmLoaderUI;
        [SerializeField] ThumbnailController _thumbnailCon;
        [SerializeField] LoadAnimation _anime_Loading;
        [Space(1), Header("＜2ページ＞")]
        [SerializeField] Button_Base _btnApply;
        [SerializeField] PrefabEditor _prefabEditor;
        [Space(1), Header("＜3ページ＞")]
        [SerializeField] TextMesh _textErrorResult;
        [Header("＜アタッチャー＞")]
        [SerializeField] ComponentAttacher_VRM _attacherPrefab;
        [Header("＜その他＞")]
        //特殊表情用サウンド
        [SerializeField] AudioClip[] _specialFaceAudioClip;
        //クリックSE
        AudioSource _audioSource;
        [SerializeField] AudioClip[] _sound;//ボタン音,読み込み音,クリック音
                                            //
        //VRM読み込み時イベント
        public event Action<CharaController> OnAddVRM;
        public event Action<CharaController> OnSetupComplete;

        //ファイルアクセスとサムネの管理
        FileAccessManager _fileManager;
        TextureAssetManager _textureAssetManager;
        //当たり判定
        VRMTouchColliders _touchCollider;
        CancellationToken _cancellationToken;

        public void Initialize(IVRMLoaderUI vrmLoaderUI)
        {
            _vrmLoaderUI = vrmLoaderUI;

            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = SystemInfo.soundVolume_SE;

            //コールバック登録・・・2ページ目
            _btnApply.onTrigger += (btn) => PrefabApply(btn).Forget();
            _prefabEditor.onCurrentUpdate += () => { _audioSource.PlayOneShot(_sound[0]); };//クリック音
            _cancellationToken = this.GetCancellationTokenOnDestroy();
        }

        async void Start()
        {
            var appConfig = GameObject.FindGameObjectWithTag("AppConfig").transform;
            _fileManager = appConfig.GetComponent<FileAccessManager>();
            _textureAssetManager = appConfig.GetComponent<TextureAssetManager>();
            _touchCollider = GameObject.FindGameObjectWithTag("Player").GetComponent<VRMTouchColliders>();

            //サムネ用ボタンの生成
            Button_Base[] btns = await _thumbnailCon.CreateThumbnailButtons();
            for (int i = 0; i < btns.Length; i++)
            {
                btns[i].onTrigger += (b) => LoadVRM(b).Forget();
            }

            _thumbnailCon.onGenerated += async () =>
            {
                await UniTask.Delay(500, cancellationToken: _cancellationToken);
                _audioSource.PlayOneShot(_sound[1]);
            };

            UIShow(false);
        }

        /// <summary>
        /// UIの表示状態を変更
        /// </summary>
        /// <param name="isHide"></param>
        public void UIShow(bool isEnabel)
        {
            if (_root.gameObject.activeSelf != isEnabel) _root.gameObject.SetActive(isEnabel);
        }

        /// <summary>
        /// カレントページで開き直す（初期化）
        /// </summary>
        public void InitPage(int currentPage)
        {
            //UIを表示
            UIShow(true);

            //ページスイッチ
            for (int i = 0; i < _pageTransform.Length; i++)
            {
                if (currentPage == i)
                {
                    if (!_pageTransform[i].gameObject.activeSelf) _pageTransform[i].gameObject.SetActive(true);
                }
                else
                {
                    if (_pageTransform[i].gameObject.activeSelf) _pageTransform[i].gameObject.SetActive(false);
                }
            }

            //各初期化処理
            switch (currentPage)
            {
                case 0:
                    if (!_fileManager.isSuccess) return;
                    //フォルダパスの表示を更新
                    _textDirectory[0].text = $"({PathsInfo.GetFullPath(FOLDERTYPE.CHARA)}/)";
                    _textDirectory[1].text = $"/Download...[{_fileManager.CountVRM(PathsInfo.GetFullPath_Download() + "/")} VRMs]";

                    //ローディングアニメーションを無効状態
                    _anime_Loading.gameObject.SetActive(false);

                    //サムネボタンアンカーを有効状態
                    _thumbnailCon.gameObject.SetActive(true);

                    //VRM選択ボタンを生成する

                    string[] names = _textureAssetManager.VrmNames;
                    _thumbnailCon.SetThumbnail(names).Forget();
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
        async UniTaskVoid LoadVRM(Button_Base btn)
        {
            //重複クリックできないようにボタンを無効化
            _thumbnailCon.gameObject.SetActive(false);

            //クリック音
            _audioSource.PlayOneShot(_sound[0]);
            await UniTask.Yield(PlayerLoopTiming.Update, _cancellationToken);

            //ローディングアニメーション開始
            _anime_Loading.gameObject.SetActive(true);

            GameObject vrmModel = null;
            try
            {
                //SampleUIを有効化
                _vrmLoaderUI.SetUIActive(true);
                await UniTask.Delay(10, cancellationToken: _cancellationToken);

                //指定パスのVRMのみ読み込む
                string fileName = btn.transform.name;
                string fullPath = PathsInfo.GetFullPath(FOLDERTYPE.CHARA) + "/" + fileName;

                var instance = await _vrmLoaderUI.GetURPVRMAsync(fullPath, _cancellationToken);

                //Meshが消える対策
                instance.EnableUpdateWhenOffscreen();

                //キャンセル確認
                _cancellationToken.ThrowIfCancellationRequested();

                //最低限の設定
                vrmModel = instance.gameObject;
                vrmModel.name = fileName;
                vrmModel.tag = SystemInfo.tag_GrabChara;
                vrmModel.layer = SystemInfo.layerNo_GrabObject;

                //各種component追加
                var attacher = Instantiate(_attacherPrefab.gameObject).GetComponent<ComponentAttacher_VRM>();
                await attacher.Init(vrmModel.transform, instance.SkinnedMeshRenderers, _cancellationToken);
                await attacher.Attachment(_touchCollider, _cancellationToken);

                //VRM追加した
                OnAddVRM?.Invoke(attacher.CharaCon);

                //UIを非表示にする
                UIShow(false);
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception e)
            {
                if (vrmModel) Destroy(vrmModel);

                _vrmLoaderUI.SetUIActive(false);

                //errorページ
                InitPage(2);

                Debug.Log(e);
                System.IO.StringReader rs = new System.IO.StringReader(e.ToString());
                _textErrorResult.text = $"{rs.ReadLine()}";//1行まで
                await UniTask.Delay(5000, cancellationToken: _cancellationToken);

                //UIを非表示にする
                UIShow(false);
            }
            finally
            {
                //ローディングアニメーション終了
                _anime_Loading.gameObject.SetActive(false);
            }
        }

        public void VRMEditing(CharaController _vrmModel)
        {
            //編集対象へ
            _prefabEditor.SetEditingTarget(_vrmModel);
            //マテリアル設定ページへ
            InitPage(1);
        }

        /// <summary>
        /// 設定の確定
        /// </summary>
        /// <param name="btn"></param>
        async UniTask PrefabApply(Button_Base btn)
        {
            //クリック音
            _audioSource.PlayOneShot(_sound[0]);

            var vrm = _prefabEditor.EditTarget;

            if (vrm.animationMode == CharaEnums.ANIMATION_MODE.VMD)
            {
                var vmdPlayer = vrm.GetComponent<VMDPlayer_Custom>();
                vmdPlayer.ResetBodyAndFace();
            }
            else if (vrm.animationMode == CharaEnums.ANIMATION_MODE.CLIP)
            {
                vrm.GetComponent<Animator>().enabled = false;
            }
            vrm.SetState(CharaEnums.STATE.NULL, null);

            _vrmLoaderUI.SetVRMToPrefab(vrm);
            await UniTask.Delay(1000, cancellationToken: _cancellationToken);

            vrm.SetEnabelSpringBones(false);//Prefab化で値が残ってしまうので無効化
            vrm.GetComponent<Animator>().enabled = true;
            vrm.animationMode = CharaEnums.ANIMATION_MODE.CLIP;
            vrm.gameObject.SetActive(false);
            await UniTask.Yield(_cancellationToken);

            OnSetupComplete?.Invoke(vrm);

            //UIを非表示にする
            UIShow(false);
        }

        /// <summary>
        /// VRMプレハブを削除する
        /// </summary>
        /// <param name="id"></param>
        public void ClearVRMPrefab(int id)
        {
            _vrmLoaderUI.DeleteVRMPrefab(id);
        }

        /// <summary>
        /// ダウンロードフォルダからVRMをコピーしてくる
        /// </summary>
        public async void OnClick_VRMCopy()
        {
            try
            {
                await _textureAssetManager.CopyVRMtoCharaFolder(PathsInfo.GetFullPath_Download() + "/");
                InitPage(0);//開き直して反映
            }
            catch
            {
                _textDirectory[1].text = "VRM Copy Error...";
            }
        }
    }
}