using Cysharp.Threading.Tasks;
using MessagePipe;
using NanaCiel;
using UniLiveViewer.Actor;
using UniLiveViewer.MessagePipe;
using UniLiveViewer.Timeline;
using UniRx;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Menu
{
    public class CharacterPage : MonoBehaviour
    {
        [SerializeField] Stage.LoadAnimation _loadingAnimation;
        [SerializeField] TextMesh _pleasePushText;

        MenuManager _menuManager;
        [Header("--- Preset or Custom ---")]
        [SerializeField] Button_Switch[] _switchChara = new Button_Switch[2];
        [SerializeField] Button_Switch[] _switchAnime = new Button_Switch[2];

        int _fieldCharaCount;

        [Header("--- JumpList ---")]
        /// <summary>
        /// キャラ、モーション、リップ
        /// </summary>
        [SerializeField] Button_Base[] _btnJumpList;

        [Header("--- MoveIndex ---")]
        [SerializeField] Transform _vrmOptionAnchor;
        [SerializeField] Transform _vmdAnchor;
        [SerializeField] Button_Base[] _btnChara = new Button_Base[2];
        [SerializeField] Button_Base[] _btnAnime = new Button_Base[2];

        [Header("--- ---")]
        [SerializeField] Button_Base[] _btnOffset = new Button_Base[2];
        [SerializeField] Button_Switch _switchReverse;
        [SerializeField] Button_Base _btnDeleteAll;
        [SerializeField] TextMesh[] textMeshs = null;

        [Header("--- Slider ---")]
        [SerializeField] SliderGrabController _sliderOffset;
        [SerializeField] SliderGrabController _sliderHeadLook;
        [SerializeField] SliderGrabController _sliderEyeLook;

        [Header("--- VRM用 ---")]
        [SerializeField] Button_Base _btnVRMSetting;
        [SerializeField] Button_Base _btnVRMDelete;
        [SerializeField] Button_Base _btnVRM10Mode;//0.xと1.0切り替え
        [SerializeField] Button_Base _btnFaceUpdate;
        [SerializeField] Button_Base _btnMouthUpdate;

        
        public IReadOnlyReactiveProperty<int> FBXIndex => _fbxIndex;
        readonly ReactiveProperty<int> _fbxIndex = new();
        public IReadOnlyReactiveProperty<int> VRMIndex => _vrmIndex;
        readonly ReactiveProperty<int> _vrmIndex = new();
        CurrentMode _currentActorMode = CurrentMode.PRESET;

        public IReadOnlyReactiveProperty<bool> IsReverse => _isReverse;
        readonly ReactiveProperty<bool> _isReverse = new();

        public IReadOnlyReactiveProperty<int> ClipIndex => _clipIndex;
        readonly ReactiveProperty<int> _clipIndex = new();
        public IReadOnlyReactiveProperty<int> VMDIndex => _vmdIndex;
        readonly ReactiveProperty<int> _vmdIndex = new();
        CurrentMode _animationMode = CurrentMode.PRESET;

        PlayableBinderService _playableBinderService;
        PresetResourceData _presetResourceData;
        ActorEntityManagerService _actorEntityManagerService;
        AnimationAssetManager _animationAssetManager;
        IPublisher<AllActorOperationMessage> _allPublisher;
        IPublisher<ActorOperationMessage> _publisher;
        AudioSourceService _audioSourceService;
        BookService _bookService;

        [Inject]
        public void Construct(
            PlayableBinderService playableBinderService,
            MenuManager menuManager,
            PresetResourceData presetResourceData,
            ActorEntityManagerService actorEntityManagerService,
            AnimationAssetManager animationAssetManager,
            IPublisher<AllActorOperationMessage> actorOperationPublisher,
            IPublisher<ActorOperationMessage> publisher,
            AudioSourceService audioSourceService,
            BookService bookService)
        {
            _playableBinderService = playableBinderService;
            _menuManager = menuManager;
            _presetResourceData = presetResourceData;
            _actorEntityManagerService = actorEntityManagerService;
            _animationAssetManager = animationAssetManager;
            _allPublisher = actorOperationPublisher;
            _publisher = publisher;
            _audioSourceService = audioSourceService;
            _bookService = bookService;
        }

        async void OnEnable()
        {
            var data = _playableBinderService.BindingData[TimelineConstants.PortalIndex];
            if (data != null) return;

            //初期召喚と未生成を想定した自動生成
            await UniTask.Delay(250);
            EvaluateActorIndex(0);
        }

        public void OnStart()
        {
            _fieldCharaCount = 0;

            //ジャンプリスト
            foreach (var e in _btnJumpList)
            {
                e.onTrigger += OpenJumplist;
            }

            //その他
            for (int i = 0; i < _btnChara.Length; i++) _btnChara[i].onTrigger += MoveIndexActor;
            for (int i = 0; i < _btnAnime.Length; i++) _btnAnime[i].onTrigger += MoveIndexAnimation;

            for (int i = 0; i < _btnOffset.Length; i++)
            {
                _btnOffset[i].onTrigger += ChangeOffset_Anime;
            }

            for (int i = 0; i < _switchChara.Length; i++)
            {
                _switchChara[i].isEnable = (i == 0);
                _switchChara[i].onTrigger += OnClickSwitchChara;
            }
            for (int i = 0; i < _switchAnime.Length; i++)
            {
                _switchAnime[i].isEnable = (i == 0);
                _switchAnime[i].onTrigger += OnClickSwitchAnime;
            }

            _switchReverse.onTrigger += (b) => ChangeReverseAnimation(b);
            _switchReverse.isEnable = false;

            _playableBinderService.BindingToAsObservable
                .Subscribe(_ =>
                {
                    if (!transform.root.gameObject.activeSelf) return;
                    textMeshs[2].text = $"{_fieldCharaCount}/{SystemInfo.MaxFieldChara}";
                }).AddTo(this);

            _sliderOffset.ValueAsObservable
                .Subscribe(value =>
                {
                    var baseMotion = textMeshs[1].text;
                    FileReadAndWriteUtility.SetMotionOffset(baseMotion, (int)value);
                    textMeshs[3].text = $"{value:0000}";
                }).AddTo(this);

            _sliderOffset.EndDriveAsObservable
                .Subscribe(_ => FileReadAndWriteUtility.SaveMotionOffset()).AddTo(this);

            _sliderHeadLook.ValueAsObservable
                .Subscribe(value =>
                {
                    var data = _playableBinderService.BindingData[TimelineConstants.PortalIndex];
                    if (data == null) return;
                    //顔の向く量をセット
                    var lookAtAllocator = data.ActorEntity.ActorEntity().Value.LookAtService;
                    lookAtAllocator.SetHeadWeight(value);
                }).AddTo(this);
            _sliderEyeLook.ValueAsObservable
                .Subscribe(value =>
                {
                    var data = _playableBinderService.BindingData[TimelineConstants.PortalIndex];
                    if (data == null) return;
                    //目の向く量をセット
                    var lookAtAllocator = data.ActorEntity.ActorEntity().Value.LookAtService;
                    lookAtAllocator.SetEyeWeight(value);
                }).AddTo(this);
            //_btnVRMSetting.onTrigger += VRMSetting;
            _btnVRMDelete.onTrigger += DeleteModel;
            _btnVRM10Mode.onTrigger += ChangeVRMMode;
            _btnDeleteAll.onTrigger += DeleteAll;
            _btnFaceUpdate.onTrigger += Switch_Mouth;
            _btnMouthUpdate.onTrigger += Switch_Mouth;

            _btnVRM10Mode.isEnable = FileReadAndWriteUtility.UserProfile.IsVRM10;

            //_vrmSelectUI.AddPrefabAsObservable
            //    .Subscribe(x =>
            //    {
            //        _timeline.ClearCaracter();
            //        //VRMのPrefabを差し替える
            //        _generatorPortal.ChangeCurrentVRM(x);

            //        if (_currentActorMode == CurrentMode.CUSTOM)
            //        {
            //            _vrmIndex.SetValueAndForceNotify(_vrmIndex.Value);
            //        }
            //        //var instance = Instantiate(vrm).GetComponent<CharaController>();
            //        //instance.SetState(CharaController.CHARASTATE.MINIATURE, generatorPortal.transform);
            //    }).AddTo(_disposable);

            if (_vmdAnchor.gameObject.activeSelf) _vmdAnchor.gameObject.SetActive(false);
            if (_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(false);
        }

        public void OnJumpSelect((JumpList.TARGET, int) select)
        {
            var target = select.Item1;
            var index = select.Item2;

            switch (target)
            {
                case JumpList.TARGET.CHARA:
                    if (_currentActorMode == CurrentMode.PRESET)
                    {
                        var moveIndex = index - _fbxIndex.Value;
                        EvaluateActorIndex(moveIndex);
                    }
                    else if (_currentActorMode == CurrentMode.CUSTOM)
                    {
                        var moveIndex = index - _vrmIndex.Value;
                        EvaluateActorIndex(moveIndex);
                    }
                    break;
                case JumpList.TARGET.ANIME:
                    if (_animationMode == CurrentMode.PRESET)
                    {
                        var moveIndex = index - _clipIndex.Value;
                        EvaluateAnimationIndex(moveIndex);
                    }
                    else if (_animationMode == CurrentMode.CUSTOM)
                    {
                        var moveIndex = index - _vmdIndex.Value;
                        EvaluateAnimationIndex(moveIndex);
                    }
                    break;
                // TODO:作業中・・・少し修正大変そう
                case JumpList.TARGET.VMD_LIPSYNC:
                    if (_animationMode == CurrentMode.CUSTOM)
                    {
                        var baseMotionName = _animationAssetManager.VmdList[_vmdIndex.Value];
                        var syncMotionName = _animationAssetManager.VmdSyncList[index];
                        FileReadAndWriteUtility.SaveMotionFacialPair(baseMotionName, syncMotionName);
                        // BaseVMDのまま更新したい
                        EvaluateAnimationIndex(0);
                    }
                    break;
            }
            _audioSourceService.PlayOneShot(0);
        }

        public void OnClickManualBook()
        {
            _bookService.ChangeOpenClose();
            _audioSourceService.PlayOneShot(0);
        }

        void Update()
        {
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        public void OnUpdateCharacterCount(int fieldCharaCount)
        {
            _fieldCharaCount = fieldCharaCount;
        }

        /// <summary>
        /// 該当するジャンプListを表示する
        /// </summary>
        /// <param name="btn"></param>
        void OpenJumplist(Button_Base btn)
        {
            if (!_menuManager.jumpList.gameObject.activeSelf) _menuManager.jumpList.gameObject.SetActive(true);

            if (btn == _btnJumpList[0])
            {
                _menuManager.jumpList.SetCharaData(_currentActorMode == CurrentMode.PRESET);
            }
            else if (btn == _btnJumpList[1])
            {
                _menuManager.jumpList.SetAnimeData(_animationMode == CurrentMode.PRESET);
            }
            else if (btn == _btnJumpList[2])
            {
                _menuManager.jumpList.SetLipSyncNames();
            }
            _audioSourceService.PlayOneShot(0);
        }

        void OnClickSwitchChara(Button_Base btn)
        {
            if (_switchChara[0] == btn)
            {
                _currentActorMode = CurrentMode.PRESET;
                _switchChara[0].isEnable = true;
                _switchChara[1].isEnable = false;
            }
            else
            {
                _currentActorMode = CurrentMode.CUSTOM;
                _switchChara[0].isEnable = false;
                _switchChara[1].isEnable = true;
            }
            _menuManager.jumpList.Close();
            EvaluateActorIndex(0);
        }

        void OnClickSwitchAnime(Button_Base btn)
        {
            if (_switchAnime[0] == btn)
            {
                _animationMode = CurrentMode.PRESET;
                _switchAnime[0].isEnable = true;
                _switchAnime[1].isEnable = false;
            }
            else
            {
                _animationMode = CurrentMode.CUSTOM;
                _switchAnime[0].isEnable = false;
                _switchAnime[1].isEnable = true;
            }
            _menuManager.jumpList.Close();
            EvaluateAnimationIndex(0);
        }

        void MoveIndexActor(Button_Base btn)
        {
            for (int i = 0; i < _btnChara.Length; i++)
            {
                if (_btnChara[i] != btn) continue;
                var moveIndex = i == 0 ? -1 : 1;
                EvaluateActorIndex(moveIndex);
                return;
            }
        }

        void MoveIndexAnimation(Button_Base btn)
        {
            for (int i = 0; i < _btnAnime.Length; i++)
            {
                if (_btnAnime[i] != btn) continue;
                var moveIndex = i == 0 ? -1 : 1;
                EvaluateAnimationIndex(moveIndex);
                return;
            }
        }

        public void OnVRMLoadFrame()
        {
            var s = MenuConstants.LoadVRM;
            textMeshs[0].text = s;
            textMeshs[0].fontSize = s.FontSizeMatch(600, 30, 50);
        }

        /// <summary>
        /// 何れかのサムネボタンクリック時
        /// NOTE: サムネ表示中はCustomタブのIndex0扱い
        /// </summary>
        public void OnClickThumbnail()
        {
            _pleasePushText.gameObject.SetActive(false);
            var maxIndex = _actorEntityManagerService.NumRegisteredVRM - 1;
            var moveIndex = maxIndex - _vrmIndex.Value;
            EvaluateActorIndex(moveIndex);
        }

        void EvaluateActorIndex(int moveIndex)
        {
            _pleasePushText.gameObject.SetActive(false);//非表示で初期化しておく

            if (_currentActorMode == CurrentMode.PRESET)
            {
                var pendingIndex = _fbxIndex.Value + moveIndex;
                if (pendingIndex < 0) pendingIndex = _actorEntityManagerService.NumRegisteredFBX - 1;
                else if (_actorEntityManagerService.NumRegisteredFBX <= pendingIndex) pendingIndex = 0;

                _loadingAnimation.gameObject.SetActive(true);
                if (_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(false);

                _fbxIndex.SetValueAndForceNotify(pendingIndex);
            }
            else if (_currentActorMode == CurrentMode.CUSTOM)
            {
                var pendingIndex = _vrmIndex.Value + moveIndex;
                if (pendingIndex < 0) pendingIndex = _actorEntityManagerService.NumRegisteredVRM - 1;
                else if (_actorEntityManagerService.NumRegisteredVRM <= pendingIndex) pendingIndex = 0;

                // 0はサムネページ
                if (pendingIndex == 0) _pleasePushText.gameObject.SetActive(true);
                else _loadingAnimation.gameObject.SetActive(true);
                if (!_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(true);

                _vrmIndex.SetValueAndForceNotify(pendingIndex);
            }
            _audioSourceService.PlayOneShot(0);
        }

        /// <summary>
        /// Timelineにバインド完了した直後を想定
        /// </summary>
        /// <param name="actorEntity"></param>
        public void OnBindingNewActor(ActorEntity actorEntity)
        {
            _loadingAnimation.gameObject.SetActive(false);
            UpdateActorInfo(actorEntity);
            EvaluateAnimationIndex(0, false);//生成後にAnimation反映
        }

        void UpdateActorInfo(ActorEntity actorEntity)
        {
            var actorName = actorEntity?.CharaInfoData.viewName;
            textMeshs[0].text = actorName;
            textMeshs[0].fontSize = actorName.FontSizeMatch(600, 30, 50);
            textMeshs[2].text = $"{_fieldCharaCount}/{SystemInfo.MaxFieldChara}";

            if (actorEntity == null) return;

            actorEntity.LookAtService.SetHeadWeight(_sliderHeadLook.Value);
            actorEntity.LookAtService.SetEyeWeight(_sliderEyeLook.Value);

            //モーフボタン初期化
            if (actorEntity.CharaInfoData.ActorType == ActorType.FBX)
            {
                if (_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(false);
            }
            else
            {
                if (!_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(true);
                _btnFaceUpdate.isEnable = true;
                _btnMouthUpdate.isEnable = true;
            }
        }

        /// <summary>
        /// indexが範囲内に収まっているか評価する
        /// 問題なければUIやクリック音に反映
        /// </summary>
        void EvaluateAnimationIndex(int moveIndex, bool isPlaySE = true)
        {
            if (_animationMode == CurrentMode.PRESET)
            {
                var pendingIndex = _clipIndex.Value + moveIndex;
                if (pendingIndex < 0) pendingIndex = _presetResourceData.DanceInfoData.Count - 1;
                else if (_presetResourceData.DanceInfoData.Count <= pendingIndex) pendingIndex = 0;
                _clipIndex.SetValueAndForceNotify(pendingIndex);
            }
            else if (_animationMode == CurrentMode.CUSTOM)
            {
                var pendingIndex = _vmdIndex.Value + moveIndex;
                if (pendingIndex < 0) pendingIndex = _animationAssetManager.VmdList.Count - 1;
                else if (_animationAssetManager.VmdList.Count <= pendingIndex) pendingIndex = 0;
                _vmdIndex.SetValueAndForceNotify(pendingIndex);
            }
            if (isPlaySE) _audioSourceService.PlayOneShot(0);
        }

        public void OnBindingNewAnimation()
        {
            UpdateAnimationInfo();
        }

        void UpdateAnimationInfo()
        {
            if (_animationMode == CurrentMode.PRESET)
            {
                var data = _presetResourceData.DanceInfoData[_clipIndex.Value];
                var baseMotionName = _isReverse.Value ? data.viewName + " R" : data.viewName;

                textMeshs[1].text = baseMotionName;
                textMeshs[1].fontSize = baseMotionName.FontSizeMatch(600, 30, 50);
                //反転ボタン
                if (!_switchReverse.gameObject.activeSelf) _switchReverse.gameObject.SetActive(true);
                _sliderOffset.Value = 0;
                textMeshs[3].text = $"{_sliderOffset.Value:0000}";
                if (_vmdAnchor.gameObject.activeSelf) _vmdAnchor.gameObject.SetActive(false);
            }
            else if (_animationMode == CurrentMode.CUSTOM)
            {
                var baseMotionName = _animationAssetManager.VmdList[_vmdIndex.Value];
                textMeshs[1].text = baseMotionName;
                textMeshs[1].fontSize = baseMotionName.FontSizeMatch(600, 30, 50);
                //反転ボタン
                if (_switchReverse.gameObject.activeSelf) _switchReverse.gameObject.SetActive(false);
                _sliderOffset.Value = FileReadAndWriteUtility.GetMotionOffset[baseMotionName];
                textMeshs[3].text = $"{_sliderOffset.Value:0000}";
                if (!_vmdAnchor.gameObject.activeSelf) _vmdAnchor.gameObject.SetActive(true);
                var syncFileName = FileReadAndWriteUtility.TryGetSyncFileName(baseMotionName);
                if (syncFileName == null) syncFileName = TimelineConstants.LIPSYNC_NONAME;
                textMeshs[4].text = syncFileName;
                textMeshs[4].fontSize = syncFileName.FontSizeMatch(600, 25, 40);
            }
        }

        /// <summary>
        /// オフセット値の微調整
        /// </summary>
        /// <param name="btn"></param>
        void ChangeOffset_Anime(Button_Base btn)
        {
            for (int i = 0; i < 2; i++)
            {
                //押されたボタンの判別
                if (_btnOffset[i] == btn)
                {
                    var moveIndex = i == 0 ? -1 : 1;
                    //オフセットを設定
                    _sliderOffset.Value += moveIndex;
                    var baseMotion = textMeshs[1].name;
                    FileReadAndWriteUtility.SetMotionOffset(baseMotion, (int)_sliderOffset.Value);
                    textMeshs[3].text = $"{_sliderOffset.Value:0000}";
                    _audioSourceService.PlayOneShot(0);
                    break;
                }
            }
            FileReadAndWriteUtility.SaveMotionOffset();
        }

        void ChangeReverseAnimation(Button_Base btn)
        {
            if (_animationMode != CurrentMode.PRESET) return;

            _isReverse.Value = _switchReverse.isEnable;
            EvaluateAnimationIndex(0);
        }

        /// <summary>
        /// VRM設定用画面を開く TODO:作り直す
        /// </summary>
        /// <param name="btn"></param>
        void VRMSetting(Button_Base btn)
        {
            //var portalChara = _timeline.BindCharaMap[TimelineController.PORTAL_INDEX];
            //if (portalChara && portalChara.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            //{
            //    //コピーをVRM設定画面に渡す
            //    _vrmSelectUI.VRMEditing(portalChara);
            //    //マニュアル開始
            //    var dummy = new CancellationToken();
            //    _playableMusicService.ManualMode(dummy).Forget();
            //}
            //_menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// VRMキャラを削除
        /// </summary>
        /// <param name="btn"></param>
        void DeleteModel(Button_Base btn)
        {
            _actorEntityManagerService.DeleteVRM(_vrmIndex.Value);
            EvaluateActorIndex(-1);

            UniTask.Void(async () =>
            {
                await UniTask.Delay(1000);
                _ = Resources.UnloadUnusedAssets();//明示的に消しておく
            });
        }

        void ChangeVRMMode(Button_Base btn)
        {
            FileReadAndWriteUtility.UserProfile.IsVRM10 = btn.isEnable;
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        /// <summary>
        /// フィールド一掃
        /// </summary>
        void DeleteAll(Button_Base btn)
        {
            var message = new AllActorOperationMessage(ActorState.FIELD, ActorCommand.DELETE);
            _allPublisher.Publish(message);

            _audioSourceService.PlayOneShot(4);
        }

        /// <summary>
        /// 口パクボタン押下
        /// </summary>
        /// <param name="btn"></param>
        void Switch_Mouth(Button_Base btn)
        {
            if (btn == _btnFaceUpdate)
            {
                if (!_actorEntityManagerService.TryGetCurrentInstaceID(out var instanceId)) return;
                var command = _btnFaceUpdate.isEnable ? ActorCommand.FACILSYNC_ENEBLE : ActorCommand.FACILSYNC_DISABLE;
                var message = new ActorOperationMessage(instanceId, command);
                _publisher.Publish(message);
            }
            else if (btn == _btnMouthUpdate)
            {
                if (!_actorEntityManagerService.TryGetCurrentInstaceID(out var instanceId)) return;
                var command = _btnMouthUpdate.isEnable ? ActorCommand.LIPSYNC_ENEBLE : ActorCommand.LIPSYNC_DISABLE;
                var message = new ActorOperationMessage(instanceId, command);
                _publisher.Publish(message);
            }
            _audioSourceService.PlayOneShot(0);
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I)) EvaluateActorIndex(1);
            else if (Input.GetKeyDown(KeyCode.U)) EvaluateActorIndex(-1);
            else if (Input.GetKeyDown(KeyCode.K)) EvaluateAnimationIndex(1);
            else if (Input.GetKeyDown(KeyCode.J)) EvaluateAnimationIndex(-1);
        }
    }
}