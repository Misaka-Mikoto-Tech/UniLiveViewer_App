using MessagePipe;
using System;
using System.Collections.Generic;
using System.Linq;
using UniLiveViewer.Actor;
using UniLiveViewer.Actor.AttachPoint;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.OVRCustom;
using UniLiveViewer.Timeline;
using UniLiveViewer.ValueObject;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;
using static UniLiveViewer.PlayerConfigData;

namespace UniLiveViewer.Player
{
    /// <summary>
    /// TODO: 仕様整理から
    /// </summary>
    public class PlayerStateManager : MonoBehaviour
    {
        const int PIECE_ANGLE = 45;

        public IObservable<bool> MainMenuSwitchingAsObservable => _mainUISwitchingStream;
        readonly Subject<bool> _mainUISwitchingStream = new();

        public IReadOnlyReactiveProperty<bool> CameraHeightMenuShow => _cameraHeightMenuShow;
        ReactiveProperty<bool> _cameraHeightMenuShow = new();

        //TODO: 雑
        public IReadOnlyReactiveProperty<bool>[] IsItemMaterialSelection => _isItemMaterialSelection;
        ReactiveProperty<bool>[] _isItemMaterialSelection = Enumerable.Range(0, 2).Select(_ => new ReactiveProperty<bool>()).ToArray();
        public IReadOnlyReactiveProperty<int>[] ItemMaterialSelection => _itemMaterialSelection;
        ReactiveProperty<int>[] _itemMaterialSelection = Enumerable.Range(0, 2).Select(_ => new ReactiveProperty<int>()).ToArray();

        public IReadOnlyReactiveProperty<PlayerEnums.HandState> HandState => _handsState;
        ReactiveProperty<PlayerEnums.HandState> _handsState = new(PlayerEnums.HandState.DEFAULT);

        /// <summary>
        /// どちらの手からも握っていたアイテムを開放した
        /// </summary>
        public IObservable<Unit> CompletelyReleasedItemAsObservable => _completelyReleasedItemStream;
        readonly Subject<Unit> _completelyReleasedItemStream = new();

        [Header("-----UI関係------")]
        bool _isMoveUI = true;

        /// <summary>
        /// NonLinearなActor拡縮用
        /// </summary>
        [SerializeField] AnimationCurve _animationCurve;
        float _curveTimer;

        [Header("デバッグ：Windows M")]
        [SerializeField] KeyCode uiKey_win = KeyCode.M;

        /// <summary>
        /// inspectorで両手確認用
        /// </summary>
        [Header("-----握り確認用------")]
        [SerializeField] OVRGrabbable_Custom[] _bothHandsCandidate = new OVRGrabbable_Custom[2];

        /// <summary>
        /// TODO: GetComponentしちゃってる
        /// </summary>
        AudioSourceService _audioSourceService;

        //両手で掴む
        OVRGrabbable_Custom _bothHandsGrabObj;
        Vector3 _initBothHandsDistance;
        Transform _bothHandsCenterAnchor;

        IPublisher<AllActorOptionMessage> _allPublisher;
        IPublisher<ActorResizeMessage> _publisher;
        KeyConfig _leftKeyConfig;
        KeyConfig _rightKeyConfig;

        PlayableAnimationClipService _playableAnimationClipService;
        PlayableDirector _playableDirector;
        List<OVRGrabber_UniLiveViewer> _ovrGrabbers;

        readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(
            IPublisher<AllActorOptionMessage> actorOperationPublisher,
            IPublisher<ActorResizeMessage> publisher,
            PlayerConfigData playerConfigData,
            PlayableAnimationClipService playableAnimationClipService,
            PlayableDirector playableDirector,
            List<OVRGrabber_UniLiveViewer> ovrGrabbers)
        {
            _allPublisher = actorOperationPublisher;
            _publisher = publisher;
            _leftKeyConfig = playerConfigData.LeftKeyConfig;
            _rightKeyConfig = playerConfigData.RightKeyConfig;
            _playableAnimationClipService = playableAnimationClipService;
            _playableDirector = playableDirector;
            _ovrGrabbers = ovrGrabbers;
        }

        public void OnStart()
        {
            _audioSourceService = GetComponent<AudioSourceService>();

            //両手掴み用
            foreach (var ovrGrabber in _ovrGrabbers)
            {
                ovrGrabber.HandActionStateAsObservable
                    .Subscribe(x =>
                    {
                        if (x.Target == HandTargetType.Item)
                        {
                            if (x.Action == HandActionState.Grab) BothHandsCandidate(ovrGrabber);
                            if (x.Action == HandActionState.Release) BothHandsGrabEnd(ovrGrabber);
                        }
                        else if (x.Target == HandTargetType.Actor)
                        {
                            if (x.Action == HandActionState.Release) ChangeSummonCircle(x.HandType);
                        }
                    }).AddTo(_disposables);

                ovrGrabber.HandState
                    .Subscribe(x =>
                    {

                    }).AddTo(_disposables);
            }
            _bothHandsCenterAnchor = new GameObject("BothHandsCenter").transform;

            this.enabled = false;
        }

        void Update()
        {
            HandStateAction(PlayerEnums.HandType.LHand, _leftKeyConfig);
            HandStateAction(PlayerEnums.HandType.RHand, _rightKeyConfig);

#if UNITY_EDITOR
            DebugInput();
#endif
        }

        // TODO: interface・stateMachine
        void HandStateAction(PlayerEnums.HandType handType, KeyConfig key)
        {
            var hand = _ovrGrabbers[(int)handType];

            switch (hand.HandState.Value)
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

            void CheckInput_GrabbedChara(PlayerEnums.HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
            {
                //魔法陣と十字を表示してキャラを乗せる
                if (!OVRInput.GetDown(key.action)) return;

                hand.SelectorChangeEnabled();
                Update_MeshGuide();
                // TODO: どうにかする
                var instanceId = hand.GrabbedObj.Value.GetComponent<ActorLifetimeScope>().InstanceId;
                ResizeActor(instanceId, 0);
            }

            void CheckInput_OnCircleChara(PlayerEnums.HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
            {
                //魔法陣回転
                if (OVRInput.GetDown(key.rotate_L))
                {
                    hand.AddEulerAnglesGroundPointer(new Vector3(0, +15, 0));
                    _audioSourceService.PlayOneShot(2);
                }
                else if (OVRInput.GetDown(key.rotate_R))
                {
                    hand.AddEulerAnglesGroundPointer(new Vector3(0, -15, 0));
                    _audioSourceService.PlayOneShot(2);
                }

                //キャラサイズ変更
                // TODO: GetComponentどうにかする
                if (OVRInput.Get(key.resize_D))
                {
                    _curveTimer += Time.deltaTime;
                    var instanceId = hand.GrabbedObj.Value.GetComponent<ActorLifetimeScope>().InstanceId;
                    ResizeActor(instanceId, -0.01f * _animationCurve.Evaluate(_curveTimer));
                }
                else if (OVRInput.Get(key.resize_U))
                {
                    _curveTimer += Time.deltaTime;
                    var instanceId = hand.GrabbedObj.Value.GetComponent<ActorLifetimeScope>().InstanceId;
                    ResizeActor(instanceId, 0.01f * _animationCurve.Evaluate(_curveTimer));
                }
                else _curveTimer = 0;

                //魔法陣と十字を非表示にしてキャラを手元へ
                if (!OVRInput.GetDown(key.action)) return;
                hand.SelectorChangeEnabled();
                Update_MeshGuide();
            }

            void CheckInput_GrabedItem(PlayerEnums.HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
            {
                var index = handType == PlayerEnums.HandType.LHand ? 0 : 1;

                if (!_isItemMaterialSelection[index].Value)
                {
                    //アイテムをアタッチ
                    if (OVRInput.GetDown(key.trigger))
                    {
                        if (TryAttachmentItem(hand)) _audioSourceService.PlayOneShot(3);
                        else _audioSourceService.PlayOneShot(4);

                        if (IsBothHandsFree())
                        {
                            _completelyReleasedItemStream.OnNext(Unit.Default);
                        }
                    }
                    //テクスチャ変更UIを表示
                    else if (OVRInput.GetDown(key.action))
                    {
                        _isItemMaterialSelection[index].Value = true;
                    }
                }
                else
                {
                    //テクスチャ変更UIを非表示
                    if (OVRInput.GetDown(key.action))
                    {
                        _isItemMaterialSelection[index].Value = false;
                    }
                    //テクスチャカレントの移動
                    else
                    {
                        var stick = OVRInput.Get(key.thumbstick);
                        if (stick.sqrMagnitude <= 0.25f) return;
                        var rad = Mathf.Atan2(stick.x, stick.y);
                        var degree = rad * Mathf.Rad2Deg;
                        if (degree < 0 - (PIECE_ANGLE / 2)) degree += 360;
                        var current = (int)Math.Round(degree / PIECE_ANGLE);//Mathfは四捨五入ではない→.NET使用
                        _itemMaterialSelection[index].Value = current;
                    }
                }
            }

            void CheckInput_Default(PlayerEnums.HandType handType, KeyConfig key, OVRGrabber_UniLiveViewer hand)
            {
                var index = handType == PlayerEnums.HandType.LHand ? 0 : 1;

                //アイテムを離した状態で選択はできない仕様
                if (_isItemMaterialSelection[index].Value)
                {
                    _isItemMaterialSelection[index].Value = false;
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
                    if (hand == _ovrGrabbers[1]) SwitchMainUI();
                    else if (hand == _ovrGrabbers[0])
                    {
                        _cameraHeightMenuShow.Value = !_cameraHeightMenuShow.Value;
                    }
                }
            }
        }

        /// <summary>
        /// 両手がフリーか
        /// </summary>
        /// <returns></returns>
        bool IsBothHandsFree() => !_ovrGrabbers[0].GrabbedObj.Value && !_ovrGrabbers[1].GrabbedObj.Value;

        void ResizeActor(InstanceId instanceId, float addScale)
        {
            _publisher.Publish(new ActorResizeMessage(instanceId, addScale));
        }

        /// <summary>
        /// TODO:無駄に複雑なので仕様整理したい
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        bool TryAttachmentItem(OVRGrabber_UniLiveViewer hand)
        {
            var grabObj = hand.GrabbedObj.Value;
            if (grabObj == null || !grabObj.isBothHandsGrab)
            {
                return false;
            }

            //結果によらず先に離しちゃう
            hand.FoeceGrabEnd();

            if (_playableDirector.timeUpdateMode != DirectorUpdateMode.Manual)
            {
                Destroy(grabObj.gameObject);
                return false;
            }

            if (grabObj.HitCollider == null || !grabObj.HitCollider.TryGetComponent<AttachPoint>(out var ap))
            {
                Destroy(grabObj.gameObject);
                return false;
            }

            if (!grabObj.TryGetComponent<DecorationItemInfo>(out var decoration))
            {
                Destroy(grabObj.gameObject);
                return false;
            }

            if (!decoration.TryAttachment())
            {
                Destroy(grabObj.gameObject);
                return false;
            }

            //手の時だけ特殊
            if (ap.HumanBodyBones == HumanBodyBones.LeftHand ||
                ap.HumanBodyBones == HumanBodyBones.RightHand)
            {
                _playableAnimationClipService.SetHandAnimation(ap.InstanceId, ap.HumanBodyBones, true);
            }
            return true;
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(uiKey_win)) SwitchMainUI();
        }

        public void OnLateTick()
        {
            //両手で掴むオブジェクトがあれば座標を上書きする
            if (!_bothHandsGrabObj) return;
            //両手の中間座標
            var bothHandsDistance = (_ovrGrabbers[1].GetGripPoint - _ovrGrabbers[0].GetGripPoint);
            _bothHandsCenterAnchor.localScale = Vector3.one * bothHandsDistance.sqrMagnitude / _initBothHandsDistance.sqrMagnitude;
            _bothHandsCenterAnchor.position = bothHandsDistance * 0.5f + _ovrGrabbers[0].GetGripPoint;
            _bothHandsCenterAnchor.forward = (_ovrGrabbers[0].transform.forward + _ovrGrabbers[1].transform.forward) * 0.5f;
        }

        /// <summary>
        /// 召喚陣の状態をスイッチ
        /// </summary>
        void ChangeSummonCircle(PlayerEnums.HandType handType)
        {
            Update_MeshGuide();
        }

        void Update_MeshGuide()
        {
            //いずれかの召喚陣が出現しているか？
            var isSummonCircle = false;
            foreach (var e in _ovrGrabbers)
            {
                if (!e.IsSummonCircle) continue;
                isSummonCircle = true;
                break;
            }

            var command = isSummonCircle ? ActorOptionCommand.GUID_ANCHOR_ENEBLE : ActorOptionCommand.GUID_ANCHOR_DISABLE;
            var fieldMessage = new AllActorOptionMessage(ActorState.FIELD, command);
            _allPublisher.Publish(fieldMessage);

            // 掴んでいる対象向け（TODO:本当はinstanceIDでやるべきだがリファクタが先）
            _allPublisher.Publish(new AllActorOptionMessage(ActorState.ON_CIRCLE, ActorOptionCommand.GUID_ANCHOR_ENEBLE));
            _allPublisher.Publish(new AllActorOptionMessage(ActorState.HOLD, ActorOptionCommand.GUID_ANCHOR_DISABLE));
            // MEMO: 掴みながらUIは消せないのでminiatureは不要
        }

        /// <summary>
        /// 両手掴み候補として登録
        /// </summary>
        /// <param name="newHand"></param>
        void BothHandsCandidate(OVRGrabber_UniLiveViewer newHand)
        {
            if (newHand == _ovrGrabbers[0])
            {
                _bothHandsCandidate[0] = _ovrGrabbers[0].GrabbedObj.Value;

                //直前まで反対の手で掴んでいたオブジェクトなら
                if (_bothHandsCandidate[1] != _bothHandsCandidate[0]) return;
                //両手用オブジェクトとしてセット
                _bothHandsGrabObj = _bothHandsCandidate[0];
                //初期値を記録
                _initBothHandsDistance = (_ovrGrabbers[1].GetGripPoint - _ovrGrabbers[0].GetGripPoint);
                _bothHandsCenterAnchor.position = _initBothHandsDistance * 0.5f + _ovrGrabbers[0].GetGripPoint;
                _bothHandsCenterAnchor.forward = (_ovrGrabbers[0].transform.forward + _ovrGrabbers[1].transform.forward) * 0.5f;
                _bothHandsGrabObj.transform.parent = _bothHandsCenterAnchor;
            }
            else if (newHand == _ovrGrabbers[1])
            {
                _bothHandsCandidate[1] = _ovrGrabbers[1].GrabbedObj.Value;

                //直前まで反対の手で掴んでいたオブジェクトなら
                if (_bothHandsCandidate[0] != _bothHandsCandidate[1]) return;
                //両手用オブジェクトとしてセット
                _bothHandsGrabObj = _bothHandsCandidate[1];
                //初期値を記録
                _initBothHandsDistance = (_ovrGrabbers[1].GetGripPoint - _ovrGrabbers[0].GetGripPoint);
                _bothHandsCenterAnchor.position = _initBothHandsDistance * 0.5f + _ovrGrabbers[0].GetGripPoint;
                _bothHandsCenterAnchor.forward = (_ovrGrabbers[0].transform.forward + _ovrGrabbers[1].transform.forward) * 0.5f;
                _bothHandsGrabObj.transform.parent = _bothHandsCenterAnchor;
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
            if (releasedHand == _ovrGrabbers[0])
            {
                if (_bothHandsCandidate[0] == _bothHandsCandidate[1])
                {
                    _ovrGrabbers[1].ForceGrabBegin(_bothHandsGrabObj);
                }
                _bothHandsCandidate[0] = null;
            }
            else if (releasedHand == _ovrGrabbers[1])
            {
                if (_bothHandsCandidate[0] == _bothHandsCandidate[1])
                {
                    _ovrGrabbers[0].ForceGrabBegin(_bothHandsGrabObj);
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

            if (IsBothHandsFree()) _completelyReleasedItemStream.OnNext(Unit.Default);
        }

        /// <summary>
        /// UI表示の切り替え
        /// </summary>
        void SwitchMainUI()
        {
            _isMoveUI = !_isMoveUI;
            _mainUISwitchingStream.OnNext(_isMoveUI);

            if (_isMoveUI) _audioSourceService.PlayOneShot(0);//表示音
            else _audioSourceService.PlayOneShot(1);//非表示音
        }

        /// <summary>
        /// どちらかの手で指定タグオブジェクトを掴んでいるか
        /// </summary>
        public bool IsSliderGrabbing(string targetTag)
        {
            for (int i = 0; i < _ovrGrabbers.Count; i++)
            {
                if (!_ovrGrabbers[i].GrabbedObj.Value) continue;
                if (!_ovrGrabbers[i].GrabbedObj.Value.gameObject.CompareTag(targetTag)) continue;
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
