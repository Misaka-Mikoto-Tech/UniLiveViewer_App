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
        const string LIPSYNC_NONAME = "[ NULL ]";

        public int CurrentChara { get; private set; } = 0;
        public int CurrentAnime { get; private set; } = 0;
        public int CurrentVMDLipSync { get; private set; } = 0;

        public bool IsAnimationReverse = false;

        [SerializeField] List<CharaController> _listChara = new List<CharaController>();
        [SerializeField] DanceInfoData[] _danceAniClipInfo;
        [SerializeField] string[] _vmdLipSync;

        public DanceInfoData[] GetDanceInfoData() { return _danceAniClipInfo; }
        public string[] GetVmdLipSync() { return _vmdLipSync; }
        [SerializeField] DanceInfoData _danceInfoData_VMDPrefab;//VMD用のテンプレ
        DanceInfoData[] _vmdDanceClipInfo;
        VMDPlayer_Custom _vmdPlayer;
        bool _retryVMD = false;

        //汎用
        TimelineController _timeline;
        TimelineInfo _timelineInfo;
        AnimationAssetManager _animationAssetManager;

        public event Action onGeneratedChara;
        public event Action onEmptyCurrent;
        public event Action onGeneratedAnime;
        public event Action<string> onSelectedAnimePair;

        CancellationTokenSource cts = new CancellationTokenSource();

        //読み込み済みVMD情報
        static Dictionary<string, VMD> dic_VMDReader = new Dictionary<string, VMD>();

        void Awake()
        {
            //キャラリストに空枠を追加(空をVRM読み込み枠として扱う、雑仕様)
            _listChara.Add(null);
        }

        void OnDisable()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
        }

        void OnEnable()
        {
            //リトライ処理
            if (_retryVMD)
            {
                _retryVMD = false;
                SetAnimation(0).Forget();
            }
            else
            {
                //キャラが存在していなければ生成しておく
                if (_timeline && !_timelineInfo.IsPortalChara())//初回は生成しない仕様
                {
                    SetChara(0).Forget();
                }
            }
        }

        public void Initialize()
        {
            //VMD枠はダミーアニメーションを追加しておく
            if (_animationAssetManager.VmdList.Count > 0)
            {
                _vmdDanceClipInfo = new DanceInfoData[_animationAssetManager.VmdList.Count];

                for (int i = 0; i < _vmdDanceClipInfo.Length; i++)
                {
                    _vmdDanceClipInfo[i] = Instantiate(_danceInfoData_VMDPrefab);

                    _vmdDanceClipInfo[i].motionOffsetTime = 0;
                    _vmdDanceClipInfo[i].strBeforeName = _animationAssetManager.VmdList[i];
                    _vmdDanceClipInfo[i].viewName = _animationAssetManager.VmdList[i];
                }
                _danceAniClipInfo = _danceAniClipInfo.Concat(_vmdDanceClipInfo).ToArray();
            }
            //口パクVMD
            if (_animationAssetManager.VmdLipSyncList.Count > 0)
            {
                var lipSyncs = new string[_animationAssetManager.VmdLipSyncList.Count];
                lipSyncs = _animationAssetManager.VmdLipSyncList.ToArray();

                string[] dummy = { LIPSYNC_NONAME };
                _vmdLipSync = dummy.Concat(lipSyncs).ToArray();
            }
        }

        void Start()
        {
            _timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            _timelineInfo = _timeline.GetComponent<TimelineInfo>();
            _animationAssetManager = GameObject.FindGameObjectWithTag("AppConfig").GetComponent<AnimationAssetManager>();
        }

        /// <summary>
        /// キャラリストにVRMを追加する
        /// </summary>
        public void AddVRMPrefab(CharaController charaCon_vrm)
        {
            //VRMを追加
            _listChara[_listChara.Count - 1] = charaCon_vrm;
            //無効化して管理
            charaCon_vrm.gameObject.SetActive(false);
            //リストに空枠を追加(空をVRM読み込み枠として扱う)
            _listChara.Add(null);
        }

        /// <summary>
        /// CurrentのPrefabを入れ替える
        /// </summary>
        /// <param name="charaCon_vrm"></param>
        public void ChangeCurrentVRM(CharaController charaCon_vrm)
        {
            if (_listChara[CurrentChara].charaInfoData.formatType
                != CharaInfoData.FORMATTYPE.VRM) return;

            //オリジナル以外は削除可
            if(_listChara[CurrentChara].name.Contains("(Clone)"))
            {
                Destroy(_listChara[CurrentChara].gameObject);
            }
            _listChara[CurrentChara] = charaCon_vrm;

            //未使用アセット削除
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// カレントのVRMPrefabを削除する
        /// </summary>
        public void DeleteCurrenVRM()
        {
            //UI上から削除
            Destroy(_listChara[CurrentChara].gameObject);
            _listChara.RemoveAt(CurrentChara);
        }

        /// <summary>
        /// 指定Currentのキャラをセットする
        /// </summary>
        /// <param Currentを動かす="moveCurrent">動かす必要がなければ0</param>
        public async UniTask SetChara(int moveCurrent)
        {
            CharaController charaCon = null;

            try
            {
                CurrentChara += moveCurrent;

                //Current移動制限
                if (CurrentChara < 0) CurrentChara = _listChara.Count - 1;
                else if (CurrentChara >= _listChara.Count) CurrentChara = 0;

                //既存のポータルキャラを削除
                _timeline.DestoryPortalChara();

                //フィールド上限オーバーなら生成しない
                if (_timelineInfo.FieldCharaCount >= _timelineInfo.MaxFieldChara) return;

                //null枠の場合も処理しない
                if (!_listChara[CurrentChara])
                {
                    onEmptyCurrent?.Invoke();
                    return;
                }

                //キャラを生成
                charaCon = Instantiate(_listChara[CurrentChara]);
                charaCon.transform.position = transform.position;
                charaCon.transform.localRotation = Quaternion.identity;

                await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);

                if (charaCon.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
                {
                    var matManager_f = _listChara[CurrentChara].GetComponent<MaterialManager>();
                    var matManager_t = charaCon.GetComponent<MaterialManager>();

                    matManager_t.matLocation = matManager_f.matLocation;
                    matManager_t.info = matManager_f.info;
                }
                
                if (cts == null) cts = new CancellationTokenSource();

                //VRM
                if (charaCon.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
                {
                    if (!charaCon.gameObject.activeSelf) charaCon.gameObject.SetActive(true);

                    charaCon._lipSync = charaCon.GetComponentInChildren<ILipSync>();
                    charaCon._facialSync = charaCon.GetComponentInChildren<IFacialSync>();
                    charaCon._lookAt = charaCon.GetComponent<LookAtBase>();
                    charaCon._headLookAt = charaCon.GetComponent<IHeadLookAt>();
                    charaCon._eyeLookAt = charaCon.GetComponent<IEyeLookAt>();
                    charaCon._lookAtVRM = charaCon.GetComponent<ILookAtVRM>();

                    //Prefab化経由は無効化されているので戻す
                    charaCon._lookAtVRM.SetEnable(true);
                    charaCon.isHeadLookAtUpdate = true;
                    charaCon.isEyeLookAtUpdate = true;

                    //揺れ物の再稼働
                    charaCon.SetEnabelSpringBones(true);
                    await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                }

                //パラメーター設定
                charaCon.SetState(CharaEnums.STATE.MINIATURE, transform);//ミニチュア状態

                //Timelineのポータル枠へバインドする
                if (!_timeline.NewAssetBinding_Portal(charaCon))
                {
                    if (charaCon) Destroy(charaCon.gameObject);
                    return;
                }

                onGeneratedChara?.Invoke();

                SetAnimation(0).Forget();//キャラにアニメーション情報をセットする
            }
            catch (OperationCanceledException)
            {
                if (charaCon) Destroy(charaCon.gameObject);
            }
        }

        /// <summary>
        /// 指定Currentのアニメーションをセットする
        /// </summary>
        /// <param Currentを動かす="moveCurrent">動かす必要がなければ0</param>
        public async UniTask SetAnimation(int moveCurrent)
        {
            try
            {
                //Current移動制限    
                CurrentAnime += moveCurrent;
                if (CurrentAnime < 0) CurrentAnime = _danceAniClipInfo.Length - 1;
                else if (CurrentAnime >= _danceAniClipInfo.Length) CurrentAnime = 0;

                //ポータルキャラを確認
                var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
                if (!portalChara) return;

                await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                _vmdPlayer = portalChara.GetComponent<VMDPlayer_Custom>();

                //VMD
                if (GetNowAnimeInfo().formatType == DanceInfoData.FORMATTYPE.VMD)
                {
                    var folderPath = PathsInfo.GetFullPath(FOLDERTYPE.MOTION) + "/";
                    var viewName = GetNowAnimeInfo().viewName;

                    // NOTE: Animatorが競合するので無効化
                    portalChara.GetComponent<Animator>().enabled = false;

                    portalChara.animationMode = CharaEnums.ANIMATION_MODE.VMD;
                    portalChara.isLipSyncUpdate = false;
                    portalChara.isFacialSyncUpdate = false;

                    await VMDPlay(_vmdPlayer, folderPath, viewName, true, cts.Token);
                }
                //プリセットアニメーション 
                else
                {
                    //反転設定
                    _danceAniClipInfo[CurrentAnime].isReverse = IsAnimationReverse;

                    //VMDを停止、animator再開
                    _vmdPlayer.ResetBodyAndFace();
                    portalChara.GetComponent<Animator>().enabled = true;
                    portalChara.animationMode = CharaEnums.ANIMATION_MODE.CLIP;
                    portalChara.isLipSyncUpdate = true;
                    portalChara.isFacialSyncUpdate = true;
                }
                //ポータル上のキャラにアニメーション設定
                _timeline.SetAnimationClip(_timelineInfo.PortalBaseAniTrack, _danceAniClipInfo[CurrentAnime]);

                onGeneratedAnime?.Invoke();

                //最後に表情系をリセットしておく
                await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                portalChara._facialSync.MorphReset();
                portalChara._lipSync.MorphReset();

                TrySetFacialAnimation();
            }
            catch (OperationCanceledException)
            {
                _retryVMD = true;
                throw;
            }
        }

        /// <summary>
        /// 照合が一致すれば表情ファイルを読み込む
        /// </summary>
        void TrySetFacialAnimation()
        {
            //VMD
            if (GetNowAnimeInfo().formatType != DanceInfoData.FORMATTYPE.VMD) return;

            //もし既存の設定があり、表情データもあれば読み込む
            var syncFileName = FileReadAndWriteUtility.GetMotionFacialPair.FirstOrDefault(
                x => x.Key == GetNowAnimeInfo().viewName).Value;
            Debug.LogWarning($"MotionFacialPair照合: {GetNowAnimeInfo().viewName}:{ syncFileName }");
            if (syncFileName == null)
            {
                onSelectedAnimePair?.Invoke(LIPSYNC_NONAME);
                return;
            }
            Debug.LogWarning(syncFileName);

            for (int i = 0; i < _vmdLipSync.Length; i++)
            {
                if (_vmdLipSync[i] != syncFileName) continue;
                int moveIndex = i - CurrentVMDLipSync;
                SetFacialAnimation(moveIndex).Forget();
                return;
            }
            onSelectedAnimePair?.Invoke($"[no file!] {syncFileName}");//一度使ったがファイル消した場合
        }

        /// <summary>
        /// 指定Currentの表情アニメーションをセットする
        /// </summary>
        /// <param Currentを動かす="moveCurrent">動かす必要がなければ0</param>
        public async UniTask SetFacialAnimation(int moveCurrent)
        {
            try
            {
                //Current移動制限
                CurrentVMDLipSync += moveCurrent;
                if (CurrentVMDLipSync < 0) CurrentVMDLipSync = _vmdLipSync.Length - 1;
                else if (CurrentVMDLipSync >= _vmdLipSync.Length) CurrentVMDLipSync = 0;

                //ポータルキャラを確認
                var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
                if (!portalChara) return;

                await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                _vmdPlayer = portalChara.GetComponent<VMDPlayer_Custom>();

                var bodyName = GetNowAnimeInfo().viewName;
                var facialName = _vmdLipSync[CurrentVMDLipSync];
                if (facialName == LIPSYNC_NONAME)
                {
                    FileReadAndWriteUtility.SaveMotionFacialPair(bodyName, facialName);
                }
                else
                {
                    var folderPath = PathsInfo.GetFullPath_LipSync() + "/";
                    await VMDPlay(_vmdPlayer, folderPath, facialName, false, cts.Token);

                    onSelectedAnimePair?.Invoke(facialName);
                    FileReadAndWriteUtility.SaveMotionFacialPair(bodyName, facialName);

                    //最後に表情系をリセットしておく
                    await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                    portalChara._facialSync.MorphReset();
                    portalChara._lipSync.MorphReset();
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }

        /// <summary>
        /// VMDを再生する
        /// </summary>
        /// <param name="vmdPlayer"></param>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        /// <param name="isNormalVMD"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        async UniTask VMDPlay(VMDPlayer_Custom vmdPlayer, string folderPath, string fileName, bool isNormalVMD, CancellationToken token)
        {
            //既存の読み込み済みリストと照合
            if (dic_VMDReader.ContainsKey(fileName))
            {
                //使いまわしてVMDプレイヤースタート
                await vmdPlayer.Starter(dic_VMDReader[fileName], folderPath, fileName,
                    SystemInfo.userProfile.VMDScale, ConfigPage.isSmoothVMD, isNormalVMD, token);
            }
            else
            {
                //新規なら読み込んでVMDプレイヤースタート
                var newVMD = await vmdPlayer.Starter(null, folderPath, fileName,
                    SystemInfo.userProfile.VMDScale, ConfigPage.isSmoothVMD, isNormalVMD, token);
                //新規VMDを登録
                dic_VMDReader.Add(fileName, newVMD);
            }
        }

        /// <summary>
        /// キャラ情報を全取得
        /// </summary>
        /// <returns></returns>
        public CharaInfoData[] GetCharasInfo()
        {
            CharaInfoData[] result = new CharaInfoData[_listChara.Count];

            for (int i = 0; i < _listChara.Count; i++)
            {
                if (_listChara[i]) result[i] = _listChara[i].charaInfoData;
                else result[i] = null;
            }
            return result;
        }

        /// <summary>
        /// 現在のキャラ名を取得
        /// </summary>
        /// <returns></returns>
        public bool GetNowCharaName(out string name)
        {
            if (_listChara[CurrentChara])
            {
                name = _listChara[CurrentChara].charaInfoData.viewName;
                return true;
            }
            else
            {
                name = "None";
                return false;
            }
        }

        /// <summary>
        /// 現在のアニメーションクリップ情報を取得
        /// </summary>
        /// <returns></returns>
        public DanceInfoData GetNowAnimeInfo()
        {
            return _danceAniClipInfo[CurrentAnime];
        }
    }
}