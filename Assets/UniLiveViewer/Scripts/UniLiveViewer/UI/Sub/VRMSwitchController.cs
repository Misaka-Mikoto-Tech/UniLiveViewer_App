using Cysharp.Threading.Tasks;
using NanaCiel;
using System;
using System.Threading;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    [RequireComponent(typeof(AudioSource))]
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

        //VRM読み込み時イベント
        public IObservable<CharaController> AddCharacterAsObservable => _addCharacterStream;
        Subject<CharaController> _addCharacterStream;

        public IObservable<CharaController> AddPrefabAsObservable => _addPrefabStream;
        Subject<CharaController> _addPrefabStream;

        public IObservable<int> OnOpenPageAsObservable => _pageStream;
        Subject<int> _pageStream;

        //ファイルアクセスとサムネの管理
        FileAccessManager _fileManager;
        //当たり判定
        VRMTouchColliders _touchCollider;


        /// <summary>
        /// 最後に生成したVRM
        /// </summary>
        GameObject _currentVrmInstance;

        public async UniTask OnStartAsync(IVRMLoaderUI vrmLoaderUI,
            FileAccessManager fileAccessManager,
            CancellationToken cancellation)
        {
            _vrmLoaderUI = vrmLoaderUI;
            _fileManager = fileAccessManager;

            _addCharacterStream = new Subject<CharaController>();
            _addPrefabStream = new Subject<CharaController>();
            _pageStream = new Subject<int>();

            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = SystemInfo.soundVolume_SE;

            //コールバック登録・・・2ページ目
            _btnApply.onTrigger += (btn) => PrefabApply(btn, cancellation).Forget();
            _prefabEditor.onCurrentUpdate += () => { _audioSource.PlayOneShot(_sound[0]); };//クリック音

            _touchCollider = LifetimeScope.Find<PlayerLifetimeScope>().Container.Resolve<VRMTouchColliders>();

            UIShow(false);

            await UniTask.CompletedTask;
        }

        public async void OnGeneratedThumbnail(CancellationToken cancellation)
        {
            await UniTask.Delay(500, cancellationToken: cancellation);
            _audioSource.PlayOneShot(_sound[1]);
        }

        public void OnClickThumbnail(string buttonName, CancellationToken cancellation)
        {
            LoadVRM(buttonName, cancellation).Forget();
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
                    break;
                case 1:
                    //prefabEditor.Init();

                    //表示を更新
                    //MaterialInfoUpdate();
                    break;
                case 2:
                    break;
            }

            _pageStream.OnNext(currentPage);
        }

        /// <summary>
        /// VRMを読み込む
        /// </summary>
        /// <param name="buttonName">該当サムネボタン</param>
        async UniTaskVoid LoadVRM(string buttonName, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            _currentVrmInstance = null;

            //クリック音
            _audioSource.PlayOneShot(_sound[0]);
            await UniTask.Yield(PlayerLoopTiming.Update, cancellation);

            cancellation.ThrowIfCancellationRequested();

            //ローディングアニメーション開始
            _anime_Loading.gameObject.SetActive(true);

            //SampleUIを有効化
            _vrmLoaderUI.SetUIActive(true);
            await UniTask.Delay(10, cancellationToken: cancellation);

            cancellation.ThrowIfCancellationRequested();

            //指定パスのVRMのみ読み込む
            var fullPath = PathsInfo.GetFullPath(FOLDERTYPE.CHARA) + "/" + buttonName;

            Debug.LogError("NOTE:ここで死ぬ、Materialエラってるがキャッチはされてない");
            var instance = await _vrmLoaderUI.GetURPVRMAsync(fullPath, cancellation)
                .OnError(_ => OnError(new Exception("Vrm Loader"), cancellation));
            cancellation.ThrowIfCancellationRequested();

            if (instance)
            {
                //Meshが消える対策
                instance.EnableUpdateWhenOffscreen();

                //最低限の設定
                _currentVrmInstance = instance.gameObject;
                _currentVrmInstance.name = buttonName;
                _currentVrmInstance.tag = Constants.TagGrabChara;
                _currentVrmInstance.layer = Constants.LayerNoGrabObject;

                var attacher = Instantiate(_attacherPrefab.gameObject).GetComponent<ComponentAttacher_VRM>();
                await attacher.Init(_currentVrmInstance.transform, instance.SkinnedMeshRenderers, cancellation)
                    .OnError(_ => OnError(new Exception("Attacher Initialize"), cancellation));
                cancellation.ThrowIfCancellationRequested();

                await attacher.Attachment(_touchCollider, cancellation)
                    .OnError(_ => OnError(new Exception("Attacher Attachment"), cancellation));
                cancellation.ThrowIfCancellationRequested();

                _addCharacterStream.OnNext(attacher.CharaCon);
                Destroy(attacher);
            }

            //UIを非表示にする
            UIShow(false);

            //ローディングアニメーション終了
            _anime_Loading.gameObject.SetActive(false);
        }

        async void OnError(Exception exception, CancellationToken cancellation)
        {
            if (_currentVrmInstance) Destroy(_currentVrmInstance);

            _vrmLoaderUI.SetUIActive(false);

            //errorページ
            InitPage(2);

            Debug.LogError(exception);
            System.IO.StringReader rs = new System.IO.StringReader(exception.ToString());
            _textErrorResult.text = $"{rs.ReadLine()}";//1行まで
            await UniTask.Delay(5000, cancellationToken: cancellation);
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
        async UniTask PrefabApply(Button_Base btn, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();

            //クリック音
            _audioSource.PlayOneShot(_sound[0]);

            var vrm = _prefabEditor.EditTarget;

            if (vrm.AnimationMode == CharaEnums.ANIMATION_MODE.VMD)
            {
                var vmdPlayer = vrm.GetComponent<VMDPlayer_Custom>();
                vmdPlayer.ResetBodyAndFace();
            }
            else if (vrm.AnimationMode == CharaEnums.ANIMATION_MODE.CLIP)
            {
                vrm.GetComponent<Animator>().enabled = false;
            }
            vrm.SetState(CharaEnums.STATE.NULL, null);

            _vrmLoaderUI.SetVRMToPrefab(vrm);

            await UniTask.Delay(1000, cancellationToken: cancellation);
            cancellation.ThrowIfCancellationRequested();

            vrm.SetEnabelSpringBones(false);//Prefab化で値が残ってしまうので無効化
            vrm.GetComponent<Animator>().enabled = true;
            vrm.AnimationMode = CharaEnums.ANIMATION_MODE.CLIP;
            vrm.gameObject.SetActive(false);

            await UniTask.Yield(cancellation);
            cancellation.ThrowIfCancellationRequested();

            _addPrefabStream.OnNext(vrm);

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
        //public async void OnClick_VRMCopy()
        //{
        //    cancellation.ThrowIfCancellationRequested();
        //    try
        //    {
        //        await _textureAssetManager.CopyVRMtoCharaFolder(PathsInfo.GetFullPath_Download() + "/", cancellation);
        //        cancellation.ThrowIfCancellationRequested();

        //        InitPage(0);//開き直して反映
        //    }
        //    catch
        //    {
        //        _textDirectory[1].text = "VRM Copy Error...";
        //    }
        //}
    }
}