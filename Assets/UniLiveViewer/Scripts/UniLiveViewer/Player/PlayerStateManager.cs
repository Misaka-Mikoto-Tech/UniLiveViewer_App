using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using System;

namespace UniLiveViewer 
{
    public class PlayerStateManager : MonoBehaviour
    {
        [Header("基本")]
        SimpleCapsuleWithStickMovement _simpleCapsuleWithStickMovement;
        TimelineController _timeline;
        TimelineInfo _timelineInfo;
        public OVRManager myOVRManager;
        public Camera myCamera;

        [Header("掴み")]
        [SerializeField] OVRGrabber_UniLiveViewer[] _ovrGrabber;//左右                                                                      
        //両手で掴む
        OVRGrabbable_Custom _bothHandsGrabObj;
        Vector3 _initBothHandsDistance;
        Transform _bothHandsCenterAnchor;

        public OVRGrabbable_Custom[] bothHandsCandidate = new OVRGrabbable_Custom[2];

        [Header("UI関係")]
        bool _isMoveUI = true;
        public HandUIController handUIController;

        [Header("使用キー")]
        [SerializeField] KeyConfig key_Lcon;
        [SerializeField] KeyConfig key_Rcon;
        [Header("windows U")]
        //UI
        [SerializeField] KeyCode uiKey_win = KeyCode.U;

        [Space(10) , Header("サウンド")]
        [SerializeField] AudioClip[] Sound;//UI開く,UI閉じる
        AudioSource _audioSource;

        public static PlayerStateManager instance;
        public event Action<bool> onSwitchMainUI;
        public event Action<bool> onPassthrough;
        CancellationToken _cancellation_token;

        [SerializeField] AnimationCurve FadeCurve;
        float _curveTimer;
        const int PIECE_ANGLE = 45;

        void Awake()
        {
            _timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            _timelineInfo = _timeline.GetComponent<TimelineInfo>();
            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = SystemInfo.soundVolume_SE;

            _simpleCapsuleWithStickMovement = GetComponent<SimpleCapsuleWithStickMovement>();

            handUIController = GetComponent<HandUIController>();

            //両手掴み用
            foreach (var hand in _ovrGrabber)
            {
                hand.OnSummon += ChangeSummonCircle;
                hand.OnGrabItem += BothHandsCandidate;
                hand.OnGrabEnd += BothHandsGrabEnd;
            }
            _bothHandsCenterAnchor = new GameObject("BothHandsCenter").transform;

            _cancellation_token = this.GetCancellationTokenOnDestroy();

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
                    transform.position = new Vector3(0, 1, 5);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case SceneMode.VIEWER:
                    transform.position = new Vector3(0, 0.5f, 5.5f);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case SceneMode.GYMNASIUM:
                    transform.position = new Vector3(0, 0.5f, 5.5f);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
            }
            this.enabled = false;
        }

        void Update()
        {
            HandStateAction(PlayerEnums.HandType.LHand, key_Lcon);
            HandStateAction(PlayerEnums.HandType.RHand, key_Rcon);

#if UNITY_EDITOR
            DebugInput();
#endif
        }

        void HandStateAction(PlayerEnums.HandType handType, KeyConfig key)
        {
            OVRGrabber_UniLiveViewer hand = _ovrGrabber[(int)handType];

            switch (hand.handState)
            {
                case PlayerEnums.HandState.GRABBED_CHARA:
                    CheckInput_GrabbedChara(handType, key, hand);
                    break;
                case PlayerEnums.HandState.CHARA_ONCIRCLE:
                    CheckInput_OnCircleChara(handType, key, hand);
                    break;
                case PlayerEnums.HandState.GRABBED_ITEM:
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

        void CheckInput_GrabbedChara(PlayerEnums.HandType handType, KeyConfig key,OVRGrabber_UniLiveViewer hand)
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

        void CheckInput_OnCircleChara(PlayerEnums.HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
        {
            //魔法陣回転
            if (OVRInput.GetDown(key.rotate_L))
            {
                hand.LineSelector.GroundPointer_AddEulerAngles(new Vector3(0, +15, 0));
                _audioSource.PlayOneShot(Sound[2]);
            }
            else if (OVRInput.GetDown(key.rotate_R))
            {
                hand.LineSelector.GroundPointer_AddEulerAngles(new Vector3(0, -15, 0));
                _audioSource.PlayOneShot(Sound[2]);
            }

            //キャラサイズ変更
            if (OVRInput.Get(key.resize_D))
            {
                _curveTimer += Time.deltaTime;
                CharaResize(-0.01f * FadeCurve.Evaluate(_curveTimer));
            }
            else if (OVRInput.Get(key.resize_U))
            {
                _curveTimer += Time.deltaTime;
                CharaResize(0.01f * FadeCurve.Evaluate(_curveTimer));
            }
            else _curveTimer = 0;

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

        void CheckInput_GrabedItem(PlayerEnums.HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
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
                    _audioSource.PlayOneShot(Sound[0]);
                    MovementRestrictions();
                }
            }
            else
            {
                //テクスチャ変更UIを非表示
                if (OVRInput.GetDown(key.action))
                {
                    handUIController.handUI_ItemMatSelecter[(int)handType].Show = false;
                    _audioSource.PlayOneShot(Sound[1]);
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
                            _audioSource.PlayOneShot(Sound[5]);
                        }
                    }
                }
            }
        }

        void CheckInput_Default(PlayerEnums.HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
        {
            //左手専用
            if (handType == PlayerEnums.HandType.LHand && handUIController.handUI_PlayerHeight.Show)
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
                _audioSource.PlayOneShot(Sound[1]);

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
                if (hand == _ovrGrabber[1]) SwitchMainUI();
                else if (hand == _ovrGrabber[0]) SwitchHandUI();
            }
        }

        void CharaResize(float addVal)
        {
            var chara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
            chara.CustomScalar += addVal;
            handUIController.handUI_CharaAdjustment[0].textMesh.text = $"{chara.CustomScalar:0.00}";
            handUIController.handUI_CharaAdjustment[1].textMesh.text = $"{chara.CustomScalar:0.00}";
        }

        void ItemAttachment(OVRGrabber_UniLiveViewer hand)
        {
            var grabObj = hand.grabbedObject;
            if (grabObj && grabObj.isBothHandsGrab)
            {
                hand.FoeceGrabEnd();//強制離す

                //アタッチ成功かつマニュアルモード
                if (_timelineInfo.isManualMode() && grabObj.GetComponent<DecorationItemInfo>().TryAttachment())
                {
                    //手なら握らせる
                    if (grabObj.hitCollider.name.Contains("Hand"))
                    {
                        var targetChara = grabObj.hitCollider.GetComponent<AttachPoint>().myCharaCon;
                        _timeline.SwitchHandType(targetChara, true, grabObj.hitCollider.name.Contains("Left"));
                    }
                    _audioSource.PlayOneShot(Sound[3]);
                }
                else
                {
                    Destroy(grabObj.gameObject);
                    _audioSource.PlayOneShot(Sound[4]);
                }

                //両手がフリーか
                if (!_ovrGrabber[0].grabbedObject && !_ovrGrabber[1].grabbedObject)
                {
                    //アタッチポイントを無効化
                    _timeline.SetActive_AttachPoint(false);
                }
            }
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(uiKey_win)) SwitchMainUI();
        }

        void LateUpdate()
        {
            //両手で掴むオブジェクトがあれば座標を上書きする
            if (_bothHandsGrabObj)
            {
                //両手の中間座標
                Vector3 bothHandsDistance = (_ovrGrabber[1].GetGripPoint - _ovrGrabber[0].GetGripPoint);
                _bothHandsCenterAnchor.localScale = Vector3.one * bothHandsDistance.sqrMagnitude / _initBothHandsDistance.sqrMagnitude;
                _bothHandsCenterAnchor.position = bothHandsDistance * 0.5f + _ovrGrabber[0].GetGripPoint;
                _bothHandsCenterAnchor.forward = (_ovrGrabber[0].transform.forward + _ovrGrabber[1].transform.forward) * 0.5f;
            }
        }

        /// <summary>
        /// 召喚陣の状態をスイッチ
        /// </summary>
        /// <param name="target"></param>
        void ChangeSummonCircle(OVRGrabber_UniLiveViewer hand)
        {
            hand.SelectorChangeEnabled();
            Update_MeshGuide();

            int i = 0;
            if (hand == _ovrGrabber[1]) i = 1;
            if (handUIController.handUI_CharaAdjustment[i].Show)
            {
                handUIController.handUI_CharaAdjustment[i].Show = false;
            }
            MovementRestrictions();
        }

        void Update_MeshGuide()
        {
            //いずれかの召喚陣が出現しているか？
            bool isSummonCircle = false;
            foreach (var e in _ovrGrabber)
            {
                if (e.IsSummonCircle)
                {
                    isSummonCircle = true;
                    break;
                }
            }
            //ガイドの表示を切り替える
            _timeline.GetComponent<MeshGuide>().IsShow = isSummonCircle;
        }

        /// <summary>
        /// 何れかのHandUIが表示中は制限を課す
        /// </summary>
        void MovementRestrictions()
        {
            bool b = handUIController.IsShow_HandUI();

            if (b)
            {
                //移動と方向転換の無効化
                _simpleCapsuleWithStickMovement.EnableRotation = false;
                _simpleCapsuleWithStickMovement.EnableLinearMovement = false;
            }
            else
            {
                //移動と方向転換の有効化
                _simpleCapsuleWithStickMovement.EnableRotation = true;
                _simpleCapsuleWithStickMovement.EnableLinearMovement = true;
            }
        }

        /// <summary>
        /// 両手掴み候補として登録
        /// </summary>
        /// <param name="newHand"></param>
        void BothHandsCandidate(OVRGrabber_UniLiveViewer newHand)
        {
            if (newHand == _ovrGrabber[0])
            {
                bothHandsCandidate[0] = _ovrGrabber[0].grabbedObject;

                //直前まで反対の手で掴んでいたオブジェクトなら
                if (bothHandsCandidate[1] == bothHandsCandidate[0])
                {

                    //両手用オブジェクトとしてセット
                    _bothHandsGrabObj = bothHandsCandidate[0];
                    //初期値を記録
                    _initBothHandsDistance = (_ovrGrabber[1].GetGripPoint - _ovrGrabber[0].GetGripPoint);
                    _bothHandsCenterAnchor.position = _initBothHandsDistance * 0.5f + _ovrGrabber[0].GetGripPoint;
                    _bothHandsCenterAnchor.forward = (_ovrGrabber[0].transform.forward + _ovrGrabber[1].transform.forward) * 0.5f;
                    _bothHandsGrabObj.transform.parent = _bothHandsCenterAnchor;
                }
            }
            else if (newHand == _ovrGrabber[1])
            {
                bothHandsCandidate[1] = _ovrGrabber[1].grabbedObject;

                //直前まで反対の手で掴んでいたオブジェクトなら
                if (bothHandsCandidate[0] == bothHandsCandidate[1])
                {
                    //両手用オブジェクトとしてセット
                    _bothHandsGrabObj = bothHandsCandidate[1];
                    //初期値を記録
                    _initBothHandsDistance = (_ovrGrabber[1].GetGripPoint - _ovrGrabber[0].GetGripPoint);
                    _bothHandsCenterAnchor.position = _initBothHandsDistance * 0.5f + _ovrGrabber[0].GetGripPoint;
                    _bothHandsCenterAnchor.forward = (_ovrGrabber[0].transform.forward + _ovrGrabber[1].transform.forward) * 0.5f;
                    _bothHandsGrabObj.transform.parent = _bothHandsCenterAnchor;
                }
            }
        }

        /// <summary>
        /// 反対の手で持ち直す
        /// </summary>
        /// <param name="releasedHand"></param>
        void BothHandsGrabEnd(OVRGrabber_UniLiveViewer releasedHand)
        {
            //両手に何もなければ処理しない
            if (!bothHandsCandidate[0] && !bothHandsCandidate[1]) return;

            //初期化
            if (releasedHand == _ovrGrabber[0])
            {
                if (bothHandsCandidate[0] == bothHandsCandidate[1])
                {
                    //強制的に持たせる
                    _ovrGrabber[1].ForceGrabBegin(_bothHandsGrabObj);
                }
                bothHandsCandidate[0] = null;
            }
            else if (releasedHand == _ovrGrabber[1])
            {
                if (bothHandsCandidate[0] == bothHandsCandidate[1])
                {
                    //強制的に持たせる
                    _ovrGrabber[0].ForceGrabBegin(_bothHandsGrabObj);
                }
                bothHandsCandidate[1] = null;
            }
            //両手は終了
            if (_bothHandsGrabObj)
            {
                _bothHandsGrabObj.transform.parent = null;
                _bothHandsCenterAnchor.localScale = Vector3.one;
                _bothHandsGrabObj = null;
            }

            //両手がフリーか
            if (!bothHandsCandidate[0] && !bothHandsCandidate[1])
            {
                //アタッチポイントを無効化
                _timeline.SetActive_AttachPoint(false);
            }
        }

        void SwitchMainUI()
        {
            //UI表示の切り替え
            _isMoveUI = !_isMoveUI;
            onSwitchMainUI?.Invoke(_isMoveUI);

            //表示音
            if (_isMoveUI) _audioSource.PlayOneShot(Sound[0]);
            //非表示音
            else _audioSource.PlayOneShot(Sound[1]);
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
        /// どちらかの手でスライダーをつかんでいるか
        /// </summary>
        public bool IsSliderGrabbing()
        {
            for (int i = 0; i < _ovrGrabber.Length; i++)
            {
                if (!_ovrGrabber[i].grabbedObject) continue;
                if (_ovrGrabber[i].grabbedObject.gameObject.CompareTag(SystemInfo.tag_GrabSliderVolume))
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
        void UniTask_ControllerVibration(OVRInput.Controller touch, float frequency, float amplitude, float time)
        {
            int milliseconds = (int)(Mathf.Clamp(time, 0, 2) * 1000);

            UniTask.Void(async () =>
            {
                try
                {
                    //振動開始
                    OVRInput.SetControllerVibration(frequency, amplitude, touch);
                    await UniTask.Delay(milliseconds, cancellationToken: _cancellation_token);
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
