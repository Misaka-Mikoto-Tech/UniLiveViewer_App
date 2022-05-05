using UnityEngine;
using System;

namespace UniLiveViewer
{
    //作りが雑(不具合あり)
    public class OVRGrabber_UniLiveViewer : OVRGrabber_Custom
    {
        public enum HandState
        {
            DEFAULT,//何もしていない、何もつかんでいない
            GRABBED_CHARA,//キャラを掴んでいる
            GRABBED_ITEM,//アイテムを掴んでいる
            GRABBED_OTHER,//キャラ以外を掴んでいる
            SUMMONCIRCLE,//召喚陣ON
            CHARA_ONCIRCLE//キャラを掴んでいるかつ召喚陣ON
        }

        //対応するセレクター
        public LineSelector lineSelector = null;

        public bool IsSummonCircle { get; private set; } = false;
        public HandState handState = HandState.DEFAULT;

        private TimelineController timeline = null;
        private GeneratorPortal generatorPortal;
        public Transform handMeshRoot;

        private AudioSource audioSource;
        [SerializeField] private AudioClip[] Sound;//掴み,離す,生成,削除

        public event Action<OVRGrabber_UniLiveViewer> OnSummon;
        public event Action<OVRGrabber_UniLiveViewer> OnGrabItem;
        public event Action<OVRGrabber_UniLiveViewer> OnGrabEnd;

        public Vector3 GetGripPoint
        {
            get { return m_gripTransform.position; }
        }

        protected override void Awake()
        {
            base.Awake();

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TitleScene") return;

            generatorPortal = GameObject.FindGameObjectWithTag("GeneratorPortal").gameObject.GetComponent<GeneratorPortal>();
            timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = SystemInfo.soundVolume_SE;

            //textMesh_CrossUI = crossUI.GetChild(0).GetComponent<TextMesh>();
            //crossUI.gameObject.SetActive(false);
        }

        public override void Update()
        {
            base.Update();

            //掴み＆セレクター表示ならセレクター操作モード
            if (handState == HandState.SUMMONCIRCLE)
            {
                //色更新
                lineSelector.SetMaterial(false);
            }
        }

        /// <summary>
        /// 強制的に持つ
        /// </summary>
        public void ForceGrabBegin(OVRGrabbable_Custom grabbedObj)
        {
            if(m_grabbedObj && m_grabbedObj == grabbedObj)
            {
                //リセット目的で一旦離す
                OffhandGrabbed(m_grabbedObj);
            }

            //コライダー候補を指定オブジェクトのみにする
            m_grabCandidates.Clear();
            m_grabCandidates[grabbedObj] = 0;

            //掴み直す
            GrabBegin();
            m_grabCandidates.Clear();
        }

        public void FoeceGrabEnd()
        {
            if (m_grabbedObj) GrabEnd();
        }

        /// <summary>
        /// 掴みの基本機能、掴んだオブジェクトの采配、掴みモーションによる削除機能
        /// </summary>
        protected override void GrabBegin()
        {
            //基本的な手に触れているものを掴む処理
            if(!IsSummonCircle) base.GrabBegin();

            //何かを掴んだ場合
            if (grabbedObject)
            {
                //キャラ以外
                if (!grabbedObject.gameObject.CompareTag(SystemInfo.tag_GrabChara))
                {
                    // TODO:ここ仕様から練り直したい
                    //両手掴みはアイテムという仕様で
                    if (grabbedObject.isBothHandsGrab)
                    {
                        handState = HandState.GRABBED_ITEM;
                        grabbedObject.GetComponent<MeshRenderer>().enabled = true;
                        grabbedObject.transform.parent = transform;
                        timeline.SetActive_AttachPoint(true);

                        //item掴んだ
                        OnGrabItem?.Invoke(this);
                    }
                    else
                    {
                        handState = HandState.GRABBED_OTHER;
                    }
                }
                //キャラ
                else
                {
                    //召喚陣の状態
                    if (IsSummonCircle)
                    {
                        //召喚陣の上に乗せる
                        var chara = grabbedObject.gameObject.GetComponent<CharaController>();
                        chara.SetState(CharaController.CHARASTATE.ON_CIRCLE, lineSelector.LineEndAnchor);

                        handState = HandState.CHARA_ONCIRCLE;
                    }
                    else
                    {
                        //手に持たせる
                        var chara = grabbedObject.gameObject.GetComponent<CharaController>();
                        chara.SetState(CharaController.CHARASTATE.HOLD, null);

                        handState = HandState.GRABBED_CHARA;
                    }
                    //掴み音
                    audioSource.PlayOneShot(Sound[0]);
                }
            }
            //掴んでいない場合
            else
            {
                //召喚陣の状態
                if (IsSummonCircle)
                {
                    //セレクターにオブジェクトが触れているか(layer的にキャラ)
                    if (!lineSelector.hitCollider.collider) return;

                    //対象オブジェクトをフィールドから削除する
                    timeline.DeletebindAsset(lineSelector.hitCollider.transform.GetComponent<CharaController>());

                    //色更新
                    lineSelector.SetMaterial(true);

                    //削除音
                    audioSource.PlayOneShot(Sound[3]);
                    handState = HandState.SUMMONCIRCLE;
                }
            }
        }

        /// <summary>
        /// 離す基本機能、離したオブジェクトの采配
        /// </summary>
        protected override void GrabEnd()
        {
            if (!m_grabbedObj)
            {
                //掴みを離す基本機能
                base.GrabEnd();
            }
            else
            {
                CharaController keepChara = null;

                //非表示なら表示に戻す
                if (!handMeshRoot.gameObject.activeSelf) handMeshRoot.gameObject.SetActive(true);

                switch (handState)
                {
                    //キャラ以外を掴んでいれば
                    case HandState.GRABBED_OTHER:
                        //掴みを離す基本機能
                        base.GrabEnd();
                        handState = HandState.DEFAULT;
                        break;
                    case HandState.GRABBED_ITEM:
                        var grabbedObj = m_grabbedObj;//解除後確認の為キープ
                        grabbedObj.transform.parent = null;
                        //掴みを離す基本機能
                        base.GrabEnd();
                        handState = HandState.DEFAULT;
                        //まだ捕まれていなければ枠非表示
                        if(!grabbedObj.isGrabbed) grabbedObj.GetComponent<MeshRenderer>().enabled = false;
                        break;
                    //キャラを掴んでいる
                    case HandState.GRABBED_CHARA:
                        keepChara = m_grabbedObj.gameObject.GetComponent<CharaController>();
                        //掴みを離す基本機能
                        base.GrabEnd();
                        //Portalに戻す
                        PortalBack(keepChara);
                        //離す音
                        audioSource.PlayOneShot(Sound[1]);
                        handState = HandState.DEFAULT;
                        break;
                    //キャラを掴んでいるかつ召喚陣ON
                    case HandState.CHARA_ONCIRCLE:
                        keepChara = m_grabbedObj.gameObject.GetComponent<CharaController>();
                        //掴みを離す基本機能
                        base.GrabEnd();

                        //フリートラックがなければPortalに戻す
                        string freeTrack;
                        if (!timeline.isFreeTrack(out freeTrack))
                        {
                            PortalBack(keepChara);
                            //離す音
                            audioSource.PlayOneShot(Sound[1]);
                            return;
                        }

                        //セレクターの座標と角度を取得
                        Vector3 pos = lineSelector.LineEndAnchor.position;
                        Vector3 eulerAngles = lineSelector.LineEndAnchor.localRotation.eulerAngles;

                        //移行に失敗ならPortalに戻す
                        if (!timeline.TransferPlayableAsset(keepChara, freeTrack, pos, eulerAngles))
                        {
                            PortalBack(keepChara);
                            //離す音
                            audioSource.PlayOneShot(Sound[1]);
                            return;
                        }

                        //キャラの状態をフィールドに設定
                        keepChara.SetState(CharaController.CHARASTATE.FIELD, null);

                        if (keepChara.charaInfoData.charaType == CharaInfoData.CHARATYPE.UnityChanSSU)
                        {
                            //しっぽタッチ音を有効にする
                            keepChara.GetComponent<TouchSound>().enabled = true;
                        }
                        else
                        {
                            //不具合の為無効化
                            //特殊演出のフラグを有効にする
                            //keepChara.GetComponent<SpecialFacial>().enabled = true;
                        }

                        //生成音
                        audioSource.PlayOneShot(Sound[2]);

                        handState = HandState.SUMMONCIRCLE;

                        //召喚した
                        OnSummon?.Invoke(this);

                        break;
                    default:
                        //掴みを離す基本機能
                        base.GrabEnd();
                        handState = HandState.DEFAULT;
                        break;
                }
            }
            //離した
            OnGrabEnd?.Invoke(this);
        }


        /// <summary>
        /// 掴んでいたものをポータルに戻す
        /// </summary>
        /// <param name="keepChara"></param>
        private void PortalBack(CharaController keepChara)
        {
            // ポータルに戻す
            if (generatorPortal.gameObject.activeSelf)
            {
                keepChara.SetState(CharaController.CHARASTATE.MINIATURE, generatorPortal.transform);
            }
            ////ポータル枠のキャラをnull初期化　不要
            //else timeline.NewAssetBinding_Portal(null);
        }

        protected override void OffhandGrabbed(OVRGrabbable_Custom grabbable)
        {
            base.OffhandGrabbed(grabbable);

            handState = HandState.DEFAULT;
        }

        /// <summary>
        /// セレクターの有効・無効状態を切り替える
        /// </summary>
        public void SelectorChangeEnabled()
        {
            CharaController chara = null;

            //現在の手の状態で分岐
            switch (handState)
            {
                //キャラを掴んでいるかつ召喚陣ON
                case HandState.CHARA_ONCIRCLE:
                    //手元に戻す
                    handState = HandState.GRABBED_CHARA;
                    chara = grabbedObject.gameObject.GetComponent<CharaController>();
                    chara.SetState(CharaController.CHARASTATE.HOLD, null);
                    break;
                //キャラを掴んでいる
                case HandState.GRABBED_CHARA:
                    //召喚陣上に設置
                    handState = HandState.CHARA_ONCIRCLE;
                    chara = grabbedObject.gameObject.GetComponent<CharaController>();
                    chara.SetState(CharaController.CHARASTATE.ON_CIRCLE, lineSelector.LineEndAnchor);
                    break;
                //何もしていない
                case HandState.DEFAULT:
                    handState = HandState.SUMMONCIRCLE;
                    break;
                //召喚陣ON
                case HandState.SUMMONCIRCLE:
                    handState = HandState.DEFAULT;
                    break;
            }

            //召喚陣の状態を反転
            IsSummonCircle = !lineSelector.gameObject.activeSelf;
            lineSelector.gameObject.SetActive(IsSummonCircle);
        }
    }
}
