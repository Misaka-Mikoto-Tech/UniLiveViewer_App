using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using static UniLiveViewer.OVRGrabber_UniLiveViewer;
using System;

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
        private bool isMoveUI = true;
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
        public event Action<bool> onSwitchMainUI;
        public event Action<bool> onPassthrough;
        private CancellationToken cancellation_token;

        [SerializeField] private AnimationCurve FadeCurve;
        private float curveTimer;
        private const int PIECE_ANGLE = 45;

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

            instance = this;

            EnablePassthrough(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            //初期座標
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
            HandStateAction(HandType.LHand, key_Lcon);
            HandStateAction(HandType.RHand, key_Rcon);

#if UNITY_EDITOR
            DebugInput();
#endif
        }

        private void HandStateAction(HandType handType, KeyConfig key)
        {
            OVRGrabber_UniLiveViewer hand = ovrGrabber[(int)handType];

            switch (hand.handState)
            {
                case HandState.GRABBED_CHARA:
                    CheckInput_GrabbedChara(handType, key, hand);
                    break;
                case HandState.CHARA_ONCIRCLE:
                    CheckInput_OnCircleChara(handType, key, hand);
                    break;
                case HandState.GRABBED_ITEM:
                    CheckInput_GrabedItem(handType, key, hand);
                    break;
                default:
                    CheckInput_Default(handType, key, hand);
                    break;
            }
        }

        public void EnablePassthrough(bool isEnable)
        {
            if (isEnable)
            {
                myCamera.clearFlags = CameraClearFlags.Color;
                myOVRManager.isInsightPassthroughEnabled = true;
            }
            else
            {
                var e = GameObject.FindGameObjectsWithTag("Passthrough");
                int max = e.Length;
                for (int i = 0; i < max; i++)
                {
                    Destroy(e[max - i - 1]);
                }

                myCamera.clearFlags = CameraClearFlags.Skybox;
                myOVRManager.isInsightPassthroughEnabled = false;
            }
            onPassthrough?.Invoke(myOVRManager.isInsightPassthroughEnabled);
        }

        private void CheckInput_GrabbedChara(HandType handType, KeyConfig key,OVRGrabber_UniLiveViewer hand)
        {
            //魔法陣と十字を表示してキャラを乗せる
            if (OVRInput.GetDown(key.action))
            {
                hand.SelectorChangeEnabled();
                Update_MeshGuide();

                if (!handUIController.handUI_CharaAdjustment[(int)handType].Show)
                {
                    handUIController.handUI_CharaAdjustment[(int)handType].Show = true;
                }
                CharaResize(0);
                MovementRestrictions();
            }
        }

        private void CheckInput_OnCircleChara(HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
        {
            //魔法陣回転
            if (OVRInput.GetDown(key.rotate_L))
            {
                hand.lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, +15, 0));
                audioSource.PlayOneShot(Sound[2]);
            }
            else if (OVRInput.GetDown(key.rotate_R))
            {
                hand.lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, -15, 0));
                audioSource.PlayOneShot(Sound[2]);
            }

            //キャラサイズ変更
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
            else curveTimer = 0;

            //魔法陣と十字を非表示にしてキャラを手元へ
            if (OVRInput.GetDown(key.action))
            {
                hand.SelectorChangeEnabled();
                Update_MeshGuide();

                if (handUIController.handUI_CharaAdjustment[(int)handType].Show)
                {
                    handUIController.handUI_CharaAdjustment[(int)handType].Show = false;
                }
                MovementRestrictions();
            }
        }

        private void CheckInput_GrabedItem(HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
        {
            if (!handUIController.handUI_ItemMatSelecter[(int)handType].Show)
            {
                //アイテムをアタッチ
                if (OVRInput.GetDown(key.trigger)) ItemAttachment(hand);
                //テクスチャ変更UIを表示
                else if (OVRInput.GetDown(key.action))
                {
                    handUIController.handUI_ItemMatSelecter[(int)handType].Show = true;
                    handUIController.InitItemMaterialSelector((int)handType, hand.grabbedObject.GetComponent<DecorationItemInfo>());
                    audioSource.PlayOneShot(Sound[0]);
                    MovementRestrictions();
                }
            }
            else
            {
                //テクスチャ変更UIを非表示
                if (OVRInput.GetDown(key.action))
                {
                    handUIController.handUI_ItemMatSelecter[(int)handType].Show = false;
                    audioSource.PlayOneShot(Sound[1]);
                    MovementRestrictions();
                }
                //テクスチャカレントの移動
                else
                {
                    Vector2 stick = OVRInput.Get(key.thumbstick);
                    if (stick.sqrMagnitude > 0.25f)
                    {
                        float rad = Mathf.Atan2(stick.x, stick.y);
                        float degree = rad * Mathf.Rad2Deg;
                        if (degree < 0 - ( PIECE_ANGLE / 2) ) degree += 360;
                        int current = (int)System.Math.Round(degree / PIECE_ANGLE);//Mathfは四捨五入ではない→.NET使用
                        if (handUIController.TrySetItemTexture((int)handType, current))
                        {
                            audioSource.PlayOneShot(Sound[2]);
                        }
                    }
                }
            }
        }

        private void CheckInput_Default(HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
        {
            //左手専用
            if (handType == HandType.LHand && handUIController.handUI_PlayerHeight.Show)
            {
                //Playerカメラの高さ調整
                if (OVRInput.GetDown(key.resize_U))
                {
                    handUIController.PlayerHeight += 0.05f;
                }
                else if (OVRInput.GetDown(key.resize_D))
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

            //魔法陣と十字の表示をスイッチ
            if (OVRInput.GetDown(key.action))
            {
                hand.SelectorChangeEnabled();
                Update_MeshGuide();
            }

            //メインメニューかサブメニューの表示をスイッチ
            if (OVRInput.GetDown(key.menuUI))
            {
                if (hand == ovrGrabber[1]) SwitchMainUI();
                else if (hand == ovrGrabber[0]) SwitchHandUI();
            }
        }

        private void CharaResize(float addVal)
        {
            var chara = timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
            chara.CustomScalar += addVal;
            handUIController.handUI_CharaAdjustment[0].textMesh.text = $"{chara.CustomScalar:0.00}";
            handUIController.handUI_CharaAdjustment[1].textMesh.text = $"{chara.CustomScalar:0.00}";
        }

        private void ItemAttachment(OVRGrabber_UniLiveViewer hand)
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

        private void DebugInput()
        {
            if (Input.GetKeyDown(uiKey_win)) SwitchMainUI();
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
        private void ChangeSummonCircle(OVRGrabber_UniLiveViewer hand)
        {
            hand.SelectorChangeEnabled();
            Update_MeshGuide();

            int i = 0;
            if (hand == ovrGrabber[1]) i = 1;
            if (handUIController.handUI_CharaAdjustment[i].Show)
            {
                handUIController.handUI_CharaAdjustment[i].Show = false;
            }
            MovementRestrictions();
        }

        private void Update_MeshGuide()
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
            timeline.GetComponent<MeshGuide>().IsShow = isSummonCircle;
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

        private void SwitchMainUI()
        {
            //UI表示の切り替え
            isMoveUI = !isMoveUI;
            onSwitchMainUI?.Invoke(isMoveUI);

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


        /// <summary>
        /// Playerインスタンスにコントローラー振動を指示
        /// </summary>
        /// <param name="touch">RTouch or LTouch</param>
        /// <param name="frequency">周波数0~1(1の方が繊細な気がする)</param>
        /// <param name="amplitude">振れ幅0~1(0で停止)</param>
        /// <param name="time">振動時間、上限2秒らしい</param>
        public static void ControllerVibration(OVRInput.Controller touch, float frequency, float amplitude, float time)
        {
            if (!SystemInfo.userProfile.TouchVibration) return;

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
