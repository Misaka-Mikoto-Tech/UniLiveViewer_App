using System;
using UniLiveViewer.OVRCustom;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;

namespace UniLiveViewer.Player
{
    public class PlayerStateManager : MonoBehaviour
    {
        const int PIECE_ANGLE = 45;

        [Header("基本")]
        TimelineController _timeline;
        PlayableDirector _playableDirector;
        public OVRManager myOVRManager;
        public Camera myCamera;

        [Header("掴み")]
        [SerializeField] OVRGrabber_UniLiveViewer[] _ovrGrabber;//左右                                                                      
        //両手で掴む
        OVRGrabbable_Custom _bothHandsGrabObj;
        Vector3 _initBothHandsDistance;
        Transform _bothHandsCenterAnchor;

        [SerializeField]
        OVRGrabbable_Custom[] _bothHandsCandidate;

        [Header("UI関係")]
        bool _isMoveUI;
        [SerializeField]
        HandUIController _handUIController;

        [Header("使用キー")]
        [SerializeField] KeyConfig key_Lcon;
        [SerializeField] KeyConfig key_Rcon;
        [Header("windows U")]
        //UI
        [SerializeField] KeyCode uiKey_win = KeyCode.U;

        [Space(10), Header("サウンド")]
        [SerializeField] AudioClip[] Sound;//UI開く,UI閉じる
        AudioSource _audioSource;

        public IObservable<bool> MainUISwitchingAsObservable => _mainUISwitchingStream;
        Subject<bool> _mainUISwitchingStream;

        /// <summary>
        /// NOTE: 名前と一致してない仮・・・後々削除予定
        /// </summary>
        public IObservable<Unit> PlayerInputAsObservable => _playerInputStream;
        Subject<Unit> _playerInputStream;

        [SerializeField] AnimationCurve _animationCurve;
        float _curveTimer;

        MeshGuideService _meshGuide;

        void Awake()
        {
            _mainUISwitchingStream = new Subject<bool>();
            _playerInputStream = new Subject<Unit>();
            _bothHandsCandidate = new OVRGrabbable_Custom[2];
        }

        [Inject]
        void Construct(
            TimelineController timelineController,
            PlayableDirector playableDirector,
            HandUIController handUIController,
            MeshGuideService meshGuideService)
        {
            _timeline = timelineController;
            _playableDirector = playableDirector;
            _handUIController = handUIController;
            _meshGuide = meshGuideService;
        }

        public void OnStart()
        {
            _isMoveUI = true;
            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = SystemInfo.soundVolume_SE;

            //両手掴み用
            foreach (var hand in _ovrGrabber)
            {
                hand.OnSummon += ChangeSummonCircle;
                hand.OnGrabItem += BothHandsCandidate;
                hand.OnGrabEnd += BothHandsGrabEnd;
            }
            _bothHandsCenterAnchor = new GameObject("BothHandsCenter").transform;

            //初期座標
            switch (SceneChangeService.GetSceneType)
            {
                case SceneType.CANDY_LIVE:
                    transform.position = new Vector3(0, 0.4f, 6.5f);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case SceneType.KAGURA_LIVE:
                    transform.position = new Vector3(0, 1, 5);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case SceneType.VIEWER:
                    transform.position = new Vector3(0, 0.5f, 5.5f);
                    transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    break;
                case SceneType.GYMNASIUM:
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

        // TODO: interface・stateMachine
        void HandStateAction(PlayerEnums.HandType handType, KeyConfig key)
        {
            var hand = _ovrGrabber[(int)handType];

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

        void CheckInput_GrabbedChara(PlayerEnums.HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
        {
            //魔法陣と十字を表示してキャラを乗せる
            if (OVRInput.GetDown(key.action))
            {
                hand.SelectorChangeEnabled();
                Update_MeshGuide();

                if (!_handUIController.handUI_CharaAdjustment[(int)handType].Show)
                {
                    _handUIController.handUI_CharaAdjustment[(int)handType].Show = true;
                }
                CharaResize(0);
                _playerInputStream?.OnNext(Unit.Default);
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
                CharaResize(-0.01f * _animationCurve.Evaluate(_curveTimer));
            }
            else if (OVRInput.Get(key.resize_U))
            {
                _curveTimer += Time.deltaTime;
                CharaResize(0.01f * _animationCurve.Evaluate(_curveTimer));
            }
            else _curveTimer = 0;

            //魔法陣と十字を非表示にしてキャラを手元へ
            if (OVRInput.GetDown(key.action))
            {
                hand.SelectorChangeEnabled();
                Update_MeshGuide();

                if (_handUIController.handUI_CharaAdjustment[(int)handType].Show)
                {
                    _handUIController.handUI_CharaAdjustment[(int)handType].Show = false;
                }
                _playerInputStream?.OnNext(Unit.Default);
            }
        }

        void CheckInput_GrabedItem(PlayerEnums.HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
        {
            if (!_handUIController.handUI_ItemMatSelecter[(int)handType].Show)
            {
                //アイテムをアタッチ
                if (OVRInput.GetDown(key.trigger)) ItemAttachment(hand);
                //テクスチャ変更UIを表示
                else if (OVRInput.GetDown(key.action))
                {
                    _handUIController.handUI_ItemMatSelecter[(int)handType].Show = true;
                    _handUIController.InitItemMaterialSelector((int)handType, hand.grabbedObject.GetComponent<DecorationItemInfo>());
                    _audioSource.PlayOneShot(Sound[0]);
                    _playerInputStream?.OnNext(Unit.Default);
                }
            }
            else
            {
                //テクスチャ変更UIを非表示
                if (OVRInput.GetDown(key.action))
                {
                    _handUIController.handUI_ItemMatSelecter[(int)handType].Show = false;
                    _audioSource.PlayOneShot(Sound[1]);
                    _playerInputStream?.OnNext(Unit.Default);
                }
                //テクスチャカレントの移動
                else
                {
                    Vector2 stick = OVRInput.Get(key.thumbstick);
                    if (stick.sqrMagnitude > 0.25f)
                    {
                        float rad = Mathf.Atan2(stick.x, stick.y);
                        float degree = rad * Mathf.Rad2Deg;
                        if (degree < 0 - (PIECE_ANGLE / 2)) degree += 360;
                        int current = (int)System.Math.Round(degree / PIECE_ANGLE);//Mathfは四捨五入ではない→.NET使用
                        if (_handUIController.TrySetItemTexture((int)handType, current))
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
            if (handType == PlayerEnums.HandType.LHand && _handUIController.handUI_PlayerHeight.Show)
            {
                //Playerカメラの高さ調整
                if (OVRInput.GetDown(key.resize_U))
                {
                    _handUIController.PlayerHeight += 0.05f;
                }
                else if (OVRInput.GetDown(key.resize_D))
                {
                    _handUIController.PlayerHeight -= 0.05f;
                }
            }

            //アイテムを離した状態で選択はできない仕様
            if (_handUIController.handUI_ItemMatSelecter[(int)handType].Show)
            {
                _handUIController.handUI_ItemMatSelecter[(int)handType].Show = false;
                _audioSource.PlayOneShot(Sound[1]);

                _playerInputStream?.OnNext(Unit.Default);
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
            var chara = _timeline.BindCharaMap[TimelineController.PORTAL_INDEX];
            chara.CustomScalar += addVal;
            _handUIController.handUI_CharaAdjustment[0].textMesh.text = $"{chara.CustomScalar:0.00}";
            _handUIController.handUI_CharaAdjustment[1].textMesh.text = $"{chara.CustomScalar:0.00}";
        }

        void ItemAttachment(OVRGrabber_UniLiveViewer hand)
        {
            var grabObj = hand.grabbedObject;
            if (grabObj && grabObj.isBothHandsGrab)
            {
                hand.FoeceGrabEnd();//強制離す

                //アタッチ成功かつマニュアルモード
                if (_playableDirector.timeUpdateMode == DirectorUpdateMode.Manual && grabObj.GetComponent<DecorationItemInfo>().TryAttachment())
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
            if (_handUIController.handUI_CharaAdjustment[i].Show)
            {
                _handUIController.handUI_CharaAdjustment[i].Show = false;
            }
            _playerInputStream?.OnNext(Unit.Default);
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
            _meshGuide.IsShow = isSummonCircle;
        }

        /// <summary>
        /// 両手掴み候補として登録
        /// </summary>
        /// <param name="newHand"></param>
        void BothHandsCandidate(OVRGrabber_UniLiveViewer newHand)
        {
            if (newHand == _ovrGrabber[0])
            {
                _bothHandsCandidate[0] = _ovrGrabber[0].grabbedObject;

                //直前まで反対の手で掴んでいたオブジェクトなら
                if (_bothHandsCandidate[1] == _bothHandsCandidate[0])
                {

                    //両手用オブジェクトとしてセット
                    _bothHandsGrabObj = _bothHandsCandidate[0];
                    //初期値を記録
                    _initBothHandsDistance = (_ovrGrabber[1].GetGripPoint - _ovrGrabber[0].GetGripPoint);
                    _bothHandsCenterAnchor.position = _initBothHandsDistance * 0.5f + _ovrGrabber[0].GetGripPoint;
                    _bothHandsCenterAnchor.forward = (_ovrGrabber[0].transform.forward + _ovrGrabber[1].transform.forward) * 0.5f;
                    _bothHandsGrabObj.transform.parent = _bothHandsCenterAnchor;
                }
            }
            else if (newHand == _ovrGrabber[1])
            {
                _bothHandsCandidate[1] = _ovrGrabber[1].grabbedObject;

                //直前まで反対の手で掴んでいたオブジェクトなら
                if (_bothHandsCandidate[0] == _bothHandsCandidate[1])
                {
                    //両手用オブジェクトとしてセット
                    _bothHandsGrabObj = _bothHandsCandidate[1];
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
            if (!_bothHandsCandidate[0] && !_bothHandsCandidate[1]) return;

            //初期化
            if (releasedHand == _ovrGrabber[0])
            {
                if (_bothHandsCandidate[0] == _bothHandsCandidate[1])
                {
                    //強制的に持たせる
                    _ovrGrabber[1].ForceGrabBegin(_bothHandsGrabObj);
                }
                _bothHandsCandidate[0] = null;
            }
            else if (releasedHand == _ovrGrabber[1])
            {
                if (_bothHandsCandidate[0] == _bothHandsCandidate[1])
                {
                    //強制的に持たせる
                    _ovrGrabber[0].ForceGrabBegin(_bothHandsGrabObj);
                }
                _bothHandsCandidate[1] = null;
            }
            //両手は終了
            if (_bothHandsGrabObj)
            {
                _bothHandsGrabObj.transform.parent = null;
                _bothHandsCenterAnchor.localScale = Vector3.one;
                _bothHandsGrabObj = null;
            }

            //両手がフリーか
            if (!_bothHandsCandidate[0] && !_bothHandsCandidate[1])
            {
                //アタッチポイントを無効化
                _timeline.SetActive_AttachPoint(false);
            }
        }

        void SwitchMainUI()
        {
            //UI表示の切り替え
            _isMoveUI = !_isMoveUI;
            _mainUISwitchingStream?.OnNext(_isMoveUI);

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
            _handUIController.SwitchPlayerHeightUI();
            _playerInputStream?.OnNext(Unit.Default);
        }

        /// <summary>
        /// どちらかの手で指定タグオブジェクトを掴んでいるか
        /// </summary>
        public bool IsSliderGrabbing(string targetTag)
        {
            for (int i = 0; i < _ovrGrabber.Length; i++)
            {
                if (!_ovrGrabber[i].grabbedObject) continue;
                if (_ovrGrabber[i].grabbedObject.gameObject.CompareTag(targetTag))
                {
                    return true;
                }
            }
            return false;
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
