using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using static UniLiveViewer.OVRGrabber_UniLiveViewer;

namespace UniLiveViewer 
{
    public class PlayerStateManager : MonoBehaviour
    {
        public enum HandType
        {
            LHand = 0,
            RHand = 1
        }

        [Header("基本")]
        [SerializeField] private SimpleCapsuleWithStickMovement simpleCapsuleWithStickMovement = null;
        private TimelineController timeline;
        public OVRManager myOVRManager;
        public Camera myCamera;

        [Header("掴み")]
        [SerializeField] private OVRGrabber_UniLiveViewer[] ovrGrabber = null;//左右                                                                      
        //両手で掴む
        private OVRGrabbable_Custom bothHandsGrabObj;
        private Vector3 initBothHandsDistance;
        private Transform bothHandsCenterAnchor;

        public OVRGrabbable_Custom[] bothHandsCandidate = new OVRGrabbable_Custom[2];

        [Header("UI関係")]
        [SerializeField] private MoveUI mainUI;
        private bool isMoveUI = false;
        public HandUIController handUIController;

        [Header("使用キー")]
        [SerializeField] private KeyConfig key_Lcon;
        [SerializeField] private KeyConfig key_Rcon;
        [Header("windows U")]
        //UI
        [SerializeField] private KeyCode uiKey_win = KeyCode.U;

        [Space(10) , Header("サウンド")]
        [SerializeField] private AudioClip[] Sound;//UI開く,UI閉じる
        private AudioSource audioSource;

        public static PlayerStateManager instance;
        private CancellationToken cancellation_token;

        [SerializeField] private AnimationCurve FadeCurve;
        private float curveTimer;

        private void Awake()
        {
            timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = SystemInfo.soundVolume_SE;

            handUIController = GetComponent<HandUIController>();

            //両手掴み用
            foreach (var hand in ovrGrabber)
            {
                hand.OnSummon += ChangeSummonCircle;
                hand.OnGrabItem += BothHandsCandidate;
                hand.OnGrabEnd += BothHandsGrabEnd;
            }
            bothHandsCenterAnchor = new GameObject("BothHandsCenter").transform;

            cancellation_token = this.GetCancellationTokenOnDestroy();

            myCamera.clearFlags = CameraClearFlags.Skybox;
            myOVRManager.isInsightPassthroughEnabled = false;

            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            switch (SystemInfo.sceneMode)
            {
                case SceneMode.CANDY_LIVE:
                    transform.position = new Vector3(0, 0.4f, 6.5f);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case SceneMode.KAGURA_LIVE:
                    transform.position = new Vector3(0, 1, 4);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case SceneMode.VIEWER:
                    transform.position = new Vector3(0, 0.5f, 5.5f);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
            }
            this.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            CheckInputKey(HandType.LHand, key_Lcon);
            CheckInputKey(HandType.RHand, key_Rcon);

            DebugInput();
        }

        private void CharaResize(float addVal)
        {
            var chara = timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
            chara.CustomScalar += addVal;
            handUIController.handUI_CharaAdjustment[0].textMesh.text = $"{chara.CustomScalar:0.00}";
            handUIController.handUI_CharaAdjustment[1].textMesh.text = $"{chara.CustomScalar:0.00}";
        }

        private void CheckInputKey(HandType handType, KeyConfig key)
        {
            OVRGrabber_UniLiveViewer hand = ovrGrabber[(int)handType];

            switch (hand.handState)
            {
                case HandState.GRABBED_CHARA:
                    if (OVRInput.GetDown(key.action))
                    {
                        hand.SelectorChangeEnabled();
                        Click_SummonCircle();

                        if (!handUIController.handUI_CharaAdjustment[(int)handType].Show)
                        {
                            handUIController.handUI_CharaAdjustment[(int)handType].Show = true;
                        }
                        CharaResize(0);
                        MovementRestrictions();
                    }
                    break;

                case HandState.CHARA_ONCIRCLE:
                    if (OVRInput.GetDown(key.rotate_L))
                    {
                        hand.lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, +15, 0));
                        audioSource.PlayOneShot(Sound[2]);//回転音
                    }
                    else if (OVRInput.GetDown(key.rotate_R))
                    {
                        hand.lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, -15, 0));
                        audioSource.PlayOneShot(Sound[2]);//回転音
                    }

                    
                    if (OVRInput.Get(key.resize_D))
                    {
                        curveTimer += Time.deltaTime;
                        CharaResize(-0.01f * FadeCurve.Evaluate(curveTimer));
                    }
                    else if (OVRInput.Get(key.resize_U))
                    {
                        curveTimer += Time.deltaTime;
                        CharaResize(0.01f * FadeCurve.Evaluate(curveTimer));
                    }
                    else
                    {
                        curveTimer = 0;
                    }

                    if (OVRInput.GetDown(key.action))
                    {
                        hand.SelectorChangeEnabled();
                        Click_SummonCircle();

                        if (handUIController.handUI_CharaAdjustment[(int)handType].Show)
                        {
                            handUIController.handUI_CharaAdjustment[(int)handType].Show = false;
                        }
                        MovementRestrictions();
                    }
                    
                    break;
                case HandState.GRABBED_ITEM:

                    if(!handUIController.handUI_ItemMatSelecter[(int)handType].Show)
                    {
                        if (OVRInput.GetDown(key.trigger)) ItemAttachment(hand);
                    }
                    else
                    {
                        //TODO:雑、直す
                        Vector2 stick = OVRInput.Get(key.thumbstick);
                        if(stick.sqrMagnitude > 0)
                        {
                            float rad = Mathf.Atan2(stick.x, stick.y);
                            float degree = rad * Mathf.Rad2Deg;
                            if (degree < 0) degree += 360;
                            //Mathfは四捨五入ではない→.NET使用
                            handUIController.SetCurrent_ItemMaterial((int)handType, (int)System.Math.Round(degree / 45));
                            var tex = handUIController.GetTexture_ItemMaterial((int)handType);
                            if (tex)
                            {
                                hand.grabbedObject.GetComponent<DecorationItemInfo>().SetTexture(tex);
                                audioSource.PlayOneShot(Sound[2]);
                            }
                        }
                    }

                    if (OVRInput.GetDown(key.action))
                    {
                        if (!handUIController.handUI_ItemMatSelecter[(int)handType].Show)
                        {
                            handUIController.handUI_ItemMatSelecter[(int)handType].Show = true;
                            handUIController.InitItemMaterialSelector((int)handType, hand.grabbedObject.GetComponent<DecorationItemInfo>());
                            audioSource.PlayOneShot(Sound[0]);
                        }
                        else
                        {
                            handUIController.handUI_ItemMatSelecter[(int)handType].Show = false;
                            audioSource.PlayOneShot(Sound[1]);
                        }
                        MovementRestrictions();
                    }
                    break;
                default:
                    if (handType == HandType.LHand && handUIController.handUI_PlayerHeight.Show)
                    {
                        //アナログスティックでカメラ位置調整
                        if (OVRInput.GetDown(key.resize_U))
                        {
                            handUIController.PlayerHeight += 0.05f;
                        }
                        if (OVRInput.GetDown(key.resize_D))
                        {
                            handUIController.PlayerHeight -= 0.05f;
                        }
                    }

                    //アイテムを離した状態で選択はできない仕様
                    if (handUIController.handUI_ItemMatSelecter[(int)handType].Show)
                    {
                        handUIController.handUI_ItemMatSelecter[(int)handType].Show = false;
                        audioSource.PlayOneShot(Sound[1]);

                        MovementRestrictions();
                    }

                    if (OVRInput.GetDown(key.action))
                    {
                        hand.SelectorChangeEnabled();
                        Click_SummonCircle();
                    }

                    if (OVRInput.GetDown(key.menuUI))
                    {
                        if (hand == ovrGrabber[1]) SwitchUI();
                        else if (hand == ovrGrabber[0]) SwitchHandUI();
                    }
                    break;
            }
        }

        public void ItemAttachment(OVRGrabber_UniLiveViewer hand)
        {
            var grabObj = hand.grabbedObject;
            if (grabObj && grabObj.isBothHandsGrab)
            {
                hand.FoeceGrabEnd();

                //アタッチ対象ありかつマニュアルモード
                if (grabObj.hitCollider && timeline.isManualMode())
                {
                    //手なら握らせる
                    if (grabObj.hitCollider.name.Contains("Hand"))
                    {
                        var targetChara = grabObj.hitCollider.GetComponent<AttachPoint>().myCharaCon;
                        timeline.SwitchHandType(targetChara, true, grabObj.hitCollider.name.Contains("Left"));
                    }

                    //アタッチする
                    grabObj.AttachToHitCollider();
                    audioSource.PlayOneShot(Sound[3]);
                }
                //アタッチ先がなければ削除
                else
                {
                    Destroy(grabObj.gameObject);
                    audioSource.PlayOneShot(Sound[4]);
                }

                //両手がフリーか
                if (!ovrGrabber[0].grabbedObject && !ovrGrabber[1].grabbedObject)
                {
                    //アタッチポイントを無効化
                    timeline.SetActive_AttachPoint(false);
                }
            }
        }

        public void DebugInput()
        {
            if (Input.GetKeyDown(uiKey_win)) SwitchUI();
        }

        private void LateUpdate()
        {
            //両手で掴むオブジェクトがあれば座標を上書きする
            if (bothHandsGrabObj)
            {
                //両手の中間座標
                Vector3 bothHandsDistance = (ovrGrabber[1].GetGripPoint - ovrGrabber[0].GetGripPoint);
                bothHandsCenterAnchor.localScale = Vector3.one * bothHandsDistance.sqrMagnitude / initBothHandsDistance.sqrMagnitude;
                bothHandsCenterAnchor.position = bothHandsDistance * 0.5f + ovrGrabber[0].GetGripPoint;
                bothHandsCenterAnchor.forward = (ovrGrabber[0].transform.forward + ovrGrabber[1].transform.forward) * 0.5f;
            }
        }

        /// <summary>
        /// 召喚陣の状態をスイッチ
        /// </summary>
        /// <param name="target"></param>
        public void ChangeSummonCircle(OVRGrabber_UniLiveViewer hand)
        {
            hand.SelectorChangeEnabled();
            Click_SummonCircle();

            int i = 0;
            if (hand == ovrGrabber[1]) i = 1;
            if (handUIController.handUI_CharaAdjustment[i].Show)
            {
                handUIController.handUI_CharaAdjustment[i].Show = false;
            }
            MovementRestrictions();
        }

        /// <summary>
        /// 両手掴み候補として登録
        /// </summary>
        /// <param name="newHand"></param>
        private void BothHandsCandidate(OVRGrabber_UniLiveViewer newHand)
        {
            if (newHand == ovrGrabber[0])
            {
                bothHandsCandidate[0] = ovrGrabber[0].grabbedObject;

                //直前まで反対の手で掴んでいたオブジェクトなら
                if (bothHandsCandidate[1] == bothHandsCandidate[0])
                {

                    //両手用オブジェクトとしてセット
                    bothHandsGrabObj = bothHandsCandidate[0];
                    //初期値を記録
                    initBothHandsDistance = (ovrGrabber[1].GetGripPoint - ovrGrabber[0].GetGripPoint);
                    bothHandsCenterAnchor.position = initBothHandsDistance * 0.5f + ovrGrabber[0].GetGripPoint;
                    bothHandsCenterAnchor.forward = (ovrGrabber[0].transform.forward + ovrGrabber[1].transform.forward) * 0.5f;
                    bothHandsGrabObj.transform.parent = bothHandsCenterAnchor;
                }
            }
            else if (newHand == ovrGrabber[1])
            {
                bothHandsCandidate[1] = ovrGrabber[1].grabbedObject;

                //直前まで反対の手で掴んでいたオブジェクトなら
                if (bothHandsCandidate[0] == bothHandsCandidate[1])
                {
                    //両手用オブジェクトとしてセット
                    bothHandsGrabObj = bothHandsCandidate[1];
                    //初期値を記録
                    initBothHandsDistance = (ovrGrabber[1].GetGripPoint - ovrGrabber[0].GetGripPoint);
                    bothHandsCenterAnchor.position = initBothHandsDistance * 0.5f + ovrGrabber[0].GetGripPoint;
                    bothHandsCenterAnchor.forward = (ovrGrabber[0].transform.forward + ovrGrabber[1].transform.forward) * 0.5f;
                    bothHandsGrabObj.transform.parent = bothHandsCenterAnchor;
                }
            }
        }

        /// <summary>
        /// 反対の手で持ち直す
        /// </summary>
        /// <param name="releasedHand"></param>
        private void BothHandsGrabEnd(OVRGrabber_UniLiveViewer releasedHand)
        {
            //両手に何もなければ処理しない
            if (!bothHandsCandidate[0] && !bothHandsCandidate[1]) return;

            //初期化
            if (releasedHand == ovrGrabber[0])
            {
                if (bothHandsCandidate[0] == bothHandsCandidate[1])
                {
                    //強制的に持たせる
                    ovrGrabber[1].ForceGrabBegin(bothHandsGrabObj);
                }
                bothHandsCandidate[0] = null;
            }
            else if (releasedHand == ovrGrabber[1])
            {
                if (bothHandsCandidate[0] == bothHandsCandidate[1])
                {
                    //強制的に持たせる
                    ovrGrabber[0].ForceGrabBegin(bothHandsGrabObj);
                }
                bothHandsCandidate[1] = null;
            }
            //両手は終了
            if (bothHandsGrabObj)
            {
                bothHandsGrabObj.transform.parent = null;
                bothHandsCenterAnchor.localScale = Vector3.one;
                bothHandsGrabObj = null;
            }

            //両手がフリーか
            if (!bothHandsCandidate[0] && !bothHandsCandidate[1])
            {
                //アタッチポイントを無効化
                timeline.SetActive_AttachPoint(false);
            }
        }

        public void SwitchUI()
        {
            //UI表示の切り替え
            isMoveUI = !mainUI.gameObject.activeSelf;
            mainUI.gameObject.SetActive(isMoveUI);

            //表示音
            if (isMoveUI) audioSource.PlayOneShot(Sound[0]);
            //非表示音
            else audioSource.PlayOneShot(Sound[1]);
        }

        /// <summary>
        /// カメラの高さUI
        /// </summary>
        public void SwitchHandUI()
        {
            //UI表示の切り替え
            handUIController.SwitchPlayerHeightUI();
            MovementRestrictions();
        }

        /// <summary>
        /// どちらかの手で対象タグのオブジェクトを掴んでいるか
        /// </summary>
        public bool CheckGrabbing()
        {
            for (int i = 0; i < ovrGrabber.Length; i++)
            {
                if (!ovrGrabber[i].grabbedObject) continue;
                if (ovrGrabber[i].grabbedObject.gameObject.CompareTag(SystemInfo.tag_GrabSliderVolume))
                {
                    return true;
                }
            }
            return false;
        }

        private void Click_SummonCircle()
        {
            //いずれかの召喚陣が出現しているか？
            bool isSummonCircle = false;
            foreach (var e in ovrGrabber)
            {
                if (e.IsSummonCircle)
                {
                    isSummonCircle = true;
                    break;
                }
            }
            //ガイドの表示を切り替える
            timeline.SetCharaMeshGuide(isSummonCircle);
        }

        /// <summary>
        /// 何れかのHandUIが表示中は制限を課す
        /// </summary>
        private void MovementRestrictions()
        {
            bool b = handUIController.IsShow_HandUI();

            if (b)
            {
                //移動と方向転換の無効化
                simpleCapsuleWithStickMovement.EnableRotation = false;
                simpleCapsuleWithStickMovement.EnableLinearMovement = false;
            }
            else
            {
                //移動と方向転換の有効化
                simpleCapsuleWithStickMovement.EnableRotation = true;
                simpleCapsuleWithStickMovement.EnableLinearMovement = true;
            }
        }

        /// <summary>
        /// Playerインスタンスにコントローラー振動を指示
        /// </summary>
        /// <param name="touch">RTouch or LTouch</param>
        /// <param name="frequency">周波数0~1(1の方が繊細な気がする)</param>
        /// <param name="amplitude">振れ幅0~1(0で停止)</param>
        /// <param name="time">振動時間、上限2秒らしい</param>
        public static void ControllerVibration(OVRInput.Controller touch, float frequency, float amplitude, float time)
        {
            if (!SystemInfo.isControllerVibration) return;

            if (instance) instance.UniTask_ControllerVibration(touch, frequency, amplitude, time);
        }

        /// <summary>
        /// 振動開始から終了までのタスクを実行する
        /// </summary>
        /// <param name="touch">RTouch or LTouch</param>
        /// <param name="frequency">周波数0~1(1の方が繊細な気がする)</param>
        /// <param name="amplitude">振れ幅0~1(0で停止)</param>
        /// <param name="time">振動時間、上限2秒らしい</param>
        private void UniTask_ControllerVibration(OVRInput.Controller touch, float frequency, float amplitude, float time)
        {
            int milliseconds = (int)(Mathf.Clamp(time, 0, 2) * 1000);

            UniTask.Void(async () =>
            {
                try
                {
                    //振動開始
                    OVRInput.SetControllerVibration(frequency, amplitude, touch);
                    await UniTask.Delay(milliseconds, cancellationToken: cancellation_token);
                }
                catch (System.OperationCanceledException)
                {
                    Debug.Log("振動中にPlayerが削除");
                }
                finally
                {
                    //振動停止
                    OVRInput.SetControllerVibration(frequency, 0, touch);
                }
            });
        }
    }

    [System.Serializable]
    public class KeyConfig
    {
        [Header("アナログスティック")]
        public OVRInput.RawAxis2D thumbstick;
        [Header("プレイヤーや魔法陣の回転")]
        public OVRInput.RawButton rotate_L;
        public OVRInput.RawButton rotate_R;
        [Header("キャラのリサイズ")]
        public OVRInput.RawButton resize_D;
        public OVRInput.RawButton resize_U;
        [Header("アクション(ラインセレクターなど)")]
        public OVRInput.RawButton action;
        [Header("メイン・サブUI")]
        public OVRInput.RawButton menuUI;
        [Header("アタッチなど")]
        public OVRInput.RawButton trigger;
    }
}
