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
        //キャラ
        [SerializeField] List<CharaController> listChara = new List<CharaController>();
        public int currentChara { get; private set; } = 0;

        public int currentAnime { get; private set; } = 0;
        public int currentVMDLipSync { get; private set; } = 0;

        public bool isAnimationReverse = false;
        const string LIPSYNC_NONAME = "No-LipSyncData";
        const string LIPSYNC_VIEWNAME = "+ LipSyncData";

        //基本ダンスクリップに上書きするクリップ(手・顔・口パク）
        Dictionary<string, int> dicAniType = new Dictionary<string, int>() { { "HAND_L", 0 }, { "HAND_R", 1 }, { "FACE", 2 }, { "LIP", 3 } };

        [SerializeField] DanceInfoData[] danceAniClipInfo;
        [SerializeField] string[] vmdLipSync;
        public DanceInfoData[] GetDanceInfoData() { return danceAniClipInfo; }
        public string[] GetVmdLipSync() { return vmdLipSync; }
        [SerializeField] DanceInfoData DanceInfoData_VMDPrefab;//VMD用のテンプレ
        DanceInfoData[] vmdDanceClipInfo;

        //汎用
        TimelineController _timeline;
        AnimationAssetManager _animationAssetManager;

        public event Action onGeneratedChara;
        public event Action onEmptyCurrent;
        public event Action onGeneratedAnime;

        VMDPlayer_Custom vmdPlayer;
        bool retryVMD = false;
        CancellationTokenSource cts = new CancellationTokenSource();

        //読み込み済みVMD情報
        static Dictionary<string, VMD> dic_VMDReader = new Dictionary<string, VMD>();
        void Awake()
        {
            //キャラリストに空枠を追加(空をVRM読み込み枠として扱う、雑仕様)
            listChara.Add(null);
        }

        void OnDisable()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();
        }

        void OnEnable()
        {
            //リトライ処理
            if (retryVMD)
            {
                retryVMD = false;
                SetAnimation(0).Forget();
            }
            else
            {
                //キャラが存在していなければ生成しておく
                if (_timeline && !_timeline.isPortalChara())//初回は生成しない仕様
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
                vmdDanceClipInfo = new DanceInfoData[_animationAssetManager.VmdList.Count];

                for (int i = 0; i < vmdDanceClipInfo.Length; i++)
                {
                    vmdDanceClipInfo[i] = Instantiate(DanceInfoData_VMDPrefab);

                    vmdDanceClipInfo[i].motionOffsetTime = 0;
                    vmdDanceClipInfo[i].strBeforeName = _animationAssetManager.VmdList[i];
                    vmdDanceClipInfo[i].viewName = _animationAssetManager.VmdList[i];
                }
                danceAniClipInfo = danceAniClipInfo.Concat(vmdDanceClipInfo).ToArray();
            }
            //口パクVMD
            if (_animationAssetManager.VmdLipSyncList.Count > 0)
            {
                string[] lipSyncs = new string[_animationAssetManager.VmdLipSyncList.Count];
                lipSyncs = _animationAssetManager.VmdLipSyncList.ToArray();

                string[] dummy = { LIPSYNC_NONAME };
                vmdLipSync = dummy.Concat(lipSyncs).ToArray();
            }
        }

        void Start()
        {
            _timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            var appConfig = GameObject.FindGameObjectWithTag("AppConfig").transform;
            _animationAssetManager = appConfig.GetComponent<AnimationAssetManager>();
        }

        /// <summary>
        /// キャラリストにVRMを追加する
        /// </summary>
        public void AddVRMPrefab(CharaController charaCon_vrm)
        {
            //VRMを追加
            listChara[listChara.Count - 1] = charaCon_vrm;
            //無効化して管理
            charaCon_vrm.gameObject.SetActive(false);
            //リストに空枠を追加(空をVRM読み込み枠として扱う)
            listChara.Add(null);
        }

        /// <summary>
        /// CurrentのPrefabを入れ替える
        /// </summary>
        /// <param name="charaCon_vrm"></param>
        public void ChangeCurrentVRM(CharaController charaCon_vrm)
        {
            if (listChara[currentChara].charaInfoData.formatType
                != CharaInfoData.FORMATTYPE.VRM) return;

            //オリジナル以外は削除可
            if(listChara[currentChara].name.Contains("(Clone)"))
            {
                Destroy(listChara[currentChara].gameObject);
            }
            listChara[currentChara] = charaCon_vrm;

            //未使用アセット削除
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// カレントのVRMPrefabを削除する
        /// </summary>
        public void DeleteCurrenVRM()
        {
            //UI上から削除
            Destroy(listChara[currentChara].gameObject);
            listChara.RemoveAt(currentChara);
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
                currentChara += moveCurrent;

                //Current移動制限
                if (currentChara < 0) currentChara = listChara.Count - 1;
                else if (currentChara >= listChara.Count) currentChara = 0;

                //既存のポータルキャラを削除
                _timeline.DestoryPortalChara();

                //フィールド上限オーバーなら生成しない
                if (_timeline.FieldCharaCount >= _timeline.maxFieldChara) return;

                //null枠の場合も処理しない
                if (!listChara[currentChara])
                {
                    onEmptyCurrent?.Invoke();
                    return;
                }

                //キャラを生成
                charaCon = Instantiate(listChara[currentChara]);
                charaCon.transform.position = transform.position;
                charaCon.transform.localRotation = Quaternion.identity;

                await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);

                if (charaCon.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
                {
                    var matManager_f = listChara[currentChara].GetComponent<MaterialManager>();
                    var matManager_t = charaCon.GetComponent<MaterialManager>();

                    matManager_t.matLocation = matManager_f.matLocation;
                    matManager_t.info = matManager_f.info;
                }

                bool isSuccess;
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

                    //パラメーター設定
                    charaCon.SetState(CharaController.CHARASTATE.MINIATURE, transform);//ミニチュア状態

                    //Timelineのポータル枠へバインドする
                    isSuccess = _timeline.NewAssetBinding_Portal(charaCon);
                }
                else
                {
                    //パラメーター設定
                    charaCon.SetState(CharaController.CHARASTATE.MINIATURE, transform);//ミニチュア状態

                    //Timelineのポータル枠へバインドする
                    isSuccess = _timeline.NewAssetBinding_Portal(charaCon);
                }

                if (!isSuccess)
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
                currentAnime += moveCurrent;
                if (currentAnime < 0) currentAnime = danceAniClipInfo.Length - 1;
                else if (currentAnime >= danceAniClipInfo.Length) currentAnime = 0;

                //ポータルキャラを確認
                var portalChara = _timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
                if (!portalChara) return;
                await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                vmdPlayer = portalChara.GetComponent<VMDPlayer_Custom>();

                //VMD
                if (GetNowAnimeInfo().formatType == DanceInfoData.FORMATTYPE.VMD)
                {
                    //animatorを停止、VMDを再生
                    string folderPath = PathsInfo.GetFullPath(FOLDERTYPE.MOTION) + "/";//VMDのパスを取得
                    portalChara.GetComponent<Animator>().enabled = false;//Animatorが競合するので無効  
                    portalChara.animationMode = CharaController.ANIMATIONMODE.VMD;
                    portalChara.isLipSyncUpdate = false;
                    portalChara.isFacialSyncUpdate = false;
                    await VMDPlay(vmdPlayer, folderPath, GetNowAnimeInfo().viewName, cts.Token);

                    //ポータル上のキャラにアニメーション設定
                    //timeline.SetAnimationClip(timeline.sPortalBaseAniTrack, danceAniClipInfo[currentAnime], transform.position, Vector3.zero);
                    _timeline.SetAnimationClip(_timeline.sPortalBaseAniTrack, danceAniClipInfo[currentAnime]);
                }
                //プリセットアニメーション 
                else
                {
                    //反転設定
                    danceAniClipInfo[currentAnime].isReverse = isAnimationReverse;

                    //VMDを停止、animator再開
                    vmdPlayer.Clear();
                    portalChara.GetComponent<Animator>().enabled = true;
                    portalChara.animationMode = CharaController.ANIMATIONMODE.CLIP;
                    portalChara.isLipSyncUpdate = true;
                    portalChara.isFacialSyncUpdate = true;
                    //ポータル上のキャラにアニメーション設定
                    //timeline.SetAnimationClip(timeline.sPortalBaseAniTrack, danceAniClipInfo[currentAnime], transform.position, Vector3.zero);
                    _timeline.SetAnimationClip(_timeline.sPortalBaseAniTrack, danceAniClipInfo[currentAnime]);
                }
                onGeneratedAnime?.Invoke();

                //最後に表情系をリセットしておく
                await UniTask.Yield(PlayerLoopTiming.Update, cts.Token);
                portalChara._facialSync.MorphReset();
                portalChara._lipSync.MorphReset();
            }
            catch (OperationCanceledException)
            {
                retryVMD = true;
                throw;
            }
        }

        /// <summary>
        /// VMDを再生する
        /// </summary>
        /// <param name="vmpPlayer"></param>
        /// <param name="folderPath"></param>
        /// <param name="fileName"></param>
        public async UniTask VMDPlay(VMDPlayer_Custom vmdPlayer, string folderPath, string fileName, CancellationToken token)
        {
            //既存の読み込み済みリストと照合
            if (dic_VMDReader.ContainsKey(fileName))
            {
                //使いまわしてVMDプレイヤースタート
                await vmdPlayer.Starter(dic_VMDReader[fileName], folderPath, fileName, 
                    SystemInfo.userProfile.VMDScale,ConfigPage.isSmoothVMD, token);
            }
            else
            {
                //新規なら読み込んでVMDプレイヤースタート
                var newVMD = await vmdPlayer.Starter(null, folderPath, fileName, 
                    SystemInfo.userProfile.VMDScale, ConfigPage.isSmoothVMD, token);
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
            CharaInfoData[] result = new CharaInfoData[listChara.Count];

            for (int i = 0; i < listChara.Count; i++)
            {
                if (listChara[i]) result[i] = listChara[i].charaInfoData;
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
        /// 現在のアニメーションクリップ名を取得
        /// </summary>
        /// <returns></returns>
        public DanceInfoData GetNowAnimeInfo()
        {
            return danceAniClipInfo[currentAnime];
        }

        /// <summary>
        /// 現在のリップシンク名を取得
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