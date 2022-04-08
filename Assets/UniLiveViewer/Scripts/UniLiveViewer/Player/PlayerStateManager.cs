using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniLiveViewer 
{
    //TODO:見直す、整理する
    public class PlayerStateManager : MonoBehaviour
    {
        [Header("基本")]
        [SerializeField] private SimpleCapsuleWithStickMovement simpleCapsuleWithStickMovement = null;
        //public bool isOperation = false;//開幕操作禁止用フラグ
        private static bool isSummonCircle = false;
        public static bool isGrabbedChara_OnCircle = false;
        private TimelineController timeline;
        //[SerializeField] private OVRScreenFade screenFade;

        [Header("掴み")]
        [SerializeField] private OVRGrabber_UniLiveViewer[] ovrGrabber = null;//左右                                                                      
        //両手で掴む
        private OVRGrabbable_Custom bothHandsGrabObj;
        private Vector3 initBothHandsDistance;
        private Transform bothHandsCenterAnchor;

        [Header("UI関係")]
        [SerializeField] private MoveUI moveUI;
        [SerializeField] private Transform handUI;
        private bool isMoveUI = false;
        private bool isHandUI = false;
        private TextMesh textMesh_CamHei;
        private CharacterCameraConstraint_Custom charaCam;
        [SerializeField] private Transform[] crossUI = new Transform[2];
        private TextMesh[] textMesh_cross = new TextMesh[2];
        [Header("使用キー")]
        //UI
        [SerializeField] private KeyCode uiKey_win = KeyCode.U;
        [SerializeField] private OVRInput.RawButton[] uiKey_quest = { OVRInput.RawButton.Y, OVRInput.RawButton.B };
        //回転に使用するキー
        [SerializeField]
        private OVRInput.RawButton[] roteKey_LCon = {
            OVRInput.RawButton.LThumbstickLeft,OVRInput.RawButton.LThumbstickRight
        };
        [SerializeField]
        private OVRInput.RawButton[] roteKey_Rcon = {
            OVRInput.RawButton.RThumbstickLeft,OVRInput.RawButton.RThumbstickRight
        };
        //サイズ変更に使用するキー
        [SerializeField]
        private OVRInput.RawButton[] resizeKey_LCon = {
            OVRInput.RawButton.LThumbstickDown,OVRInput.RawButton.LThumbstickUp
        };

        [SerializeField]
        private OVRInput.RawButton[] resizeKey_RCon = {
            OVRInput.RawButton.RThumbstickDown,OVRInput.RawButton.RThumbstickUp
        };
        //ラインセレクター表示切替キー
        [SerializeField]
        private OVRInput.RawButton[] lineOnKey = {
            OVRInput.RawButton.X, OVRInput.RawButton.A
        };
        //アタッチに使用
        [SerializeField]
        private OVRInput.RawButton[] actionKey = {
            OVRInput.RawButton.LIndexTrigger, OVRInput.RawButton.RIndexTrigger
        };

        [Header("サウンド")]
        private AudioSource audioSource;
        [SerializeField] private AudioClip[] Sound;//UI開く,UI閉じる

        public OVRGrabbable_Custom[] bothHandsCandidate = new OVRGrabbable_Custom[2];

        private CancellationToken cancellation_token;
        private static PlayerStateManager instance = null;

        private void Awake()
        {
            timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = GlobalConfig.soundVolume_SE;

            charaCam = GetComponent<CharacterCameraConstraint_Custom>();
            textMesh_CamHei = handUI.GetChild(0).GetComponent<TextMesh>();

            for (int i = 0; i < crossUI.Length; i++)
            {
                textMesh_cross[i] = crossUI[i].GetChild(0).GetComponent<TextMesh>();
            }

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
        }

        // Start is called before the first frame update
        void Start()
        {
            //非表を初期化
            handUI.gameObject.SetActive(isHandUI);

            for (int i = 0; i < crossUI.Length; i++)
            {
                crossUI[i].gameObject.SetActive(false);
            }

            this.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            //初期化済みでなければ処理しない
            //if (!isOperation) return;

            isGrabbedChara_OnCircle = false;
            isSummonCircle = false;

            //左手制御
            if (ovrGrabber[0].handState == OVRGrabber_UniLiveViewer.HandState.CHARA_ONCIRCLE)
            {
                isGrabbedChara_OnCircle = true;
                crossUI[0].gameObject.SetActive(true);

                //左手左回転
                if (OVRInput.GetDown(roteKey_LCon[0]))
                {
                    ovrGrabber[0].lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, +15, 0));
                    //回転音
                    audioSource.PlayOneShot(Sound[2]);
                }
                //左手右回転
                else if (OVRInput.GetDown(roteKey_LCon[1]))
                {
                    ovrGrabber[0].lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, -15, 0));
                    //回転音
                    audioSource.PlayOneShot(Sound[2]);
                }

                //左手縮小
                if (OVRInput.Get(resizeKey_LCon[0]))
                {
                    timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar += -0.005f;
                }
                //左手拡大
                else if (OVRInput.Get(resizeKey_LCon[1]))
                {
                    timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar += 0.005f;
                }

                textMesh_cross[0].text = $"{timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar:0.00}";
            }
            else
            {
                crossUI[0].gameObject.SetActive(false);
            }

            if (ovrGrabber[1].handState == OVRGrabber_UniLiveViewer.HandState.CHARA_ONCIRCLE)
            {
                isGrabbedChara_OnCircle = true;
                crossUI[1].gameObject.SetActive(true);

                //右手左回転
                if (OVRInput.GetDown(roteKey_Rcon[0]))
                {
                    ovrGrabber[1].lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, +15, 0));
                    //回転音
                    audioSource.PlayOneShot(Sound[2]);
                }
                //右手右回転
                else if (OVRInput.GetDown(roteKey_Rcon[1]))
                {
                    ovrGrabber[1].lineSelector.GroundPointer_AddEulerAngles(new Vector3(0, -15, 0));
                    //回転音
                    audioSource.PlayOneShot(Sound[2]);
                }

                //右手縮小
                if (OVRInput.Get(resizeKey_RCon[0]))
                {
                    timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar += -0.01f;
                }
                //右手拡大
                else if (OVRInput.Get(resizeKey_RCon[1]))
                {
                    timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar += 0.01f;
                }

                textMesh_cross[1].text = $"{timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].CustomScalar:0.00}";
            }
            else
            {
                crossUI[1].gameObject.SetActive(false);
            }

            //キャラが召喚陣にセットされていれば
            if (isGrabbedChara_OnCircle)
            {
                //移動と方向転換を無効化
                simpleCapsuleWithStickMovement.EnableLinearMovement = false;
                simpleCapsuleWithStickMovement.EnableRotation = false;
            }
            //初期化系
            else
            {
                simpleCapsuleWithStickMovement.EnableRotation = true;
                if (!isHandUI) simpleCapsuleWithStickMovement.EnableLinearMovement = true;

                //ハンドUI出現中
                if (isHandUI)
                {
                    //アナログスティックでカメラ位置調整
                    if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickUp))
                    {
                        charaCam.HeightOffset = Mathf.Clamp(charaCam.HeightOffset + 0.05f, 0f, 1.5f);
                        textMesh_CamHei.text = $"{charaCam.HeightOffset:0.00}";
                    }
                    if (OVRInput.GetDown(OVRInput.RawButton.LThumbstickDown))
                    {
                        charaCam.HeightOffset = Mathf.Clamp(charaCam.HeightOffset - 0.05f, 0f, 1.5f);
                        textMesh_CamHei.text = $"{charaCam.HeightOffset:0.00}";
                    }
                }
            }

            //ラインセレクター切り替え
            for (int i = 0; i < lineOnKey.Length; i++)
            {
                if (OVRInput.GetDown(lineOnKey[i]))
                {
                    ovrGrabber[i].SelectorChangeEnabled();
                    Click_SummonCircle();
                }
            }

            //UI表示
            if (OVRInput.GetDown(uiKey_quest[1]) || Input.GetKeyDown(uiKey_win))
            {
                SwitchUI();
            }
            if (OVRInput.GetDown(uiKey_quest[0]))
            {
                SwitchHandUI();
            }

            //アイテムをアタッチする
            for (int i = 0; i < actionKey.Length; i++)
            {
                if (OVRInput.GetDown(actionKey[i]))
                {
                    var grabObj = ovrGrabber[i].grabbedObject;
                    if (grabObj && grabObj.isBothHandsGrab)
                    {
                        ovrGrabber[i].FoeceGrabEnd();

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
            }
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
        public void ChangeSummonCircle(OVRGrabber_UniLiveViewer target)
        {
            //ラインセレクター切り替え
            for (int i = 0; i < lineOnKey.Length; i++)
            {
                if (ovrGrabber[i] == target)
                {
                    ovrGrabber[i].SelectorChangeEnabled();
                    Click_SummonCircle();
                    break;
                }
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

        public void SwitchUI()
        {
            //UI表示の切り替え
            isMoveUI = !moveUI.gameObject.activeSelf;
            moveUI.gameObject.SetActive(isMoveUI);

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
            isHandUI = !isHandUI;
            handUI.gameObject.SetActive(isHandUI);

            if (isHandUI)
            {
                //textMesh_CamHei.text = charaCam.HeightOffset.ToString("0.00");
                textMesh_CamHei.text = $"{charaCam.HeightOffset:0.00}";
                //移動を無効化
                simpleCapsuleWithStickMovement.EnableLinearMovement = false;
            }
            else
            {
                //移動を無効化
                simpleCapsuleWithStickMovement.EnableLinearMovement = true;
            }

            //表示音
            if (isHandUI) audioSource.PlayOneShot(Sound[0]);
            //非表示音
            else audioSource.PlayOneShot(Sound[1]);
        }

        /// <summary>
        /// どちらかの手で対象タグのオブジェクトを掴んでいるか
        /// </summary>
        public bool CheckGrabbing()
        {
            for (int i = 0; i < ovrGrabber.Length; i++)
            {
                if (!ovrGrabber[i].grabbedObject) continue;
                if (ovrGrabber[i].grabbedObject.gameObject.CompareTag(Parameters.tag_GrabSliderVolume))
                {
                    return true;
                }
            }
            return false;
        }

        private void Click_SummonCircle()
        {
            //いずれかの召喚陣が出現しているか？
            isSummonCircle = false;
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
        /// Playerインスタンスにコントローラー振動を指示
        /// </summary>
        /// <param name="touch">RTouch or LTouch</param>
        /// <param name="frequency">周波数0~1(1の方が繊細な気がする)</param>
        /// <param name="amplitude">振れ幅0~1(0で停止)</param>
        /// <param name="time">振動時間、上限2秒らしい</param>
        public static void ControllerVibration(OVRInput.Controller touch, float frequency, float amplitude, float time)
        {
            if (!GlobalConfig.isControllerVibration) return;

            if (instance) instance.UniTask_ControllerVibration(touch, frequency, amplitude, time);

            //Task.Run(async () =>
            //{
            //    //振動開始
            //    OVRInput.SetControllerVibration(frequency, amplitude, touch);
            //    //指定時間待機
            //    await Task.Delay(milliseconds);
            //    //振動停止
            //    OVRInput.SetControllerVibration(frequency, 0, touch);
            //});
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

}
