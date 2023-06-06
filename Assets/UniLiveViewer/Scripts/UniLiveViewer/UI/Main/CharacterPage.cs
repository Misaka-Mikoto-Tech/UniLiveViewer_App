using Cysharp.Threading.Tasks;
using NanaCiel;
using System.Threading;
using UnityEngine;
using UniRx;

namespace UniLiveViewer
{
    public class CharacterPage : MonoBehaviour
    {
        MenuManager _menuManager;

        [SerializeField] Button_Switch[] _switchChara = new Button_Switch[2];
        [SerializeField] Button_Switch[] _switchAnime = new Button_Switch[2];
        bool _isPresetChara;
        bool _isPresetAnime;


        /// <summary>
        /// キャラ、モーション、リップ
        /// </summary>
        [SerializeField] Button_Base[] _btnJumpList;

        [SerializeField] Button_Base[] _btnChara = new Button_Base[2];
        [SerializeField] Button_Base[] _btnAnime = new Button_Base[2];
        [SerializeField] Button_Base[] _btnOffset = new Button_Base[2];
        [SerializeField] Button_Switch _switchReverse;
        [SerializeField] Button_Base _btnVRMLoad;
        [SerializeField] Button_Base _btnDeleteAll;
        [SerializeField] TextMesh[] textMeshs = null;
        [SerializeField] SliderGrabController _sliderOffset;
        [SerializeField] SliderGrabController _sliderHeadLook;
        [SerializeField] SliderGrabController _sliderEyeLook;

        [Header("＜VRM用＞")]
        [SerializeField] Button_Base _btnVRMSetting;
        [SerializeField] Button_Base _btnVRMDelete;
        [SerializeField] Button_Base _btnFaceUpdate;
        [SerializeField] Button_Base _btnMouthUpdate;

        [Header("＜マニュアル用＞")]
        [SerializeField] GameObject[] _bookPrefab;
        [SerializeField] GameObject _bookGrabAnchor;

        Transform _offsetAnchor;
        Transform _vrmOptionAnchor;

        TimelineController _timeline;
        TimelineInfo _timelineInfo;
        [SerializeField] GeneratorPortal _generatorPortal;
        VRMSwitchController _vrmSelectUI;
        CancellationToken _cancellationToken;

        CompositeDisposable _disposable;

        void Awake()
        {
            _menuManager = transform.root.GetComponent<MenuManager>();
            _cancellationToken = this.GetCancellationTokenOnDestroy();

            _disposable = new CompositeDisposable();
        }

        // Start is called before the first frame update
        void Start()
        {
            _timeline = _menuManager.timeline;
            _timelineInfo = _timeline.GetComponent<TimelineInfo>();
            _vrmSelectUI = _menuManager.vrmSelectUI;

            //ジャンプリスト
            foreach (var e in _btnJumpList)
            {
                e.onTrigger += OpenJumplist;
            }

            //ジャンプリストから選択された時
            _menuManager.jumpList.onSelect += (jumpCurrent) =>
            {
                int moveIndex = 0;
                switch (_menuManager.jumpList.target)
                {
                    case JumpList.TARGET.CHARA:
                        moveIndex = jumpCurrent - _generatorPortal.CurrentChara;
                        _generatorPortal.SetChara(moveIndex).Forget();
                        break;
                    case JumpList.TARGET.ANIME:
                        moveIndex = jumpCurrent - _generatorPortal.CurrentAnime;
                        _generatorPortal.SetAnimation(moveIndex).Forget();
                        break;
                    // TODO:作業中・・・少し修正大変そう
                    case JumpList.TARGET.VMD_LIPSYNC:
                        moveIndex = jumpCurrent - _generatorPortal.CurrentVMDLipSync;
                        ChangeFacialVMD(moveIndex);
                        break;
                }
                _menuManager.PlayOneShot(SoundType.BTN_CLICK);
            };

            //その他
            for (int i = 0; i < _btnChara.Length; i++)
            {
                _btnChara[i].onTrigger += MoveIndex;
            }
            for (int i = 0; i < _btnAnime.Length; i++)
            {
                _btnAnime[i].onTrigger += MoveIndex;
            }
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

            _switchReverse.onTrigger += (b) => ChangeReverse_Anime(b).Forget();
            _switchReverse.isEnable = false;
            _timeline.FieldCharaAdded += () => { Add_FieldChara().Forget(); };
            _timeline.FieldCharaDeleted += () => { Add_FieldChara().Forget(); };
            _sliderOffset.ValueUpdate += () =>
            {
                var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
                if (!portalChara) return;
                //オフセットを設定
                FileReadAndWriteUtility.SetMotionOffset(_generatorPortal.GetNowAnimeInfo().viewName, (int)_sliderOffset.Value);
                textMeshs[3].text = $"{_sliderOffset.Value:0000}";
            };
            _sliderOffset.UnControled += () => { FileReadAndWriteUtility.SaveMotionOffset(); };
            _offsetAnchor = _sliderOffset.transform.parent;
            _sliderEyeLook.ValueUpdate += () =>
            {
                var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
                if (!portalChara) return;
                //目の向く量をセット
                portalChara.LookAt.inputWeight_Eye = _sliderEyeLook.Value;
            };
            _sliderHeadLook.ValueUpdate += () =>
            {
                var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
                if (!portalChara) return;
                //顔の向く量をセット
                portalChara.LookAt.inputWeight_Head = _sliderHeadLook.Value;
            };
            _btnVRMLoad.onTrigger += VRMLoad;
            _btnVRMSetting.onTrigger += VRMSetting;
            _btnVRMDelete.onTrigger += DeleteModel;
            _btnDeleteAll.onTrigger += DeleteModel;
            _btnFaceUpdate.onTrigger += Switch_Mouth;
            _vrmOptionAnchor = _btnFaceUpdate.transform.parent;
            _btnMouthUpdate.onTrigger += Switch_Mouth;

            _vrmSelectUI.AddCharacterAsObservable
                .Subscribe(x => 
                {
                    _generatorPortal.AddVRMList(x);//VRMを追加
                    _generatorPortal.SetChara(0).Forget();//追加されたVRMを生成する
                }).AddTo(_disposable);

            _vrmSelectUI.AddPrefabAsObservable
                .Subscribe(x =>
                {
                    _timeline.ClearCaracter();
                    //VRMのPrefabを差し替える
                    _generatorPortal.ChangeCurrentVRM(x);
                    _generatorPortal.SetChara(0).Forget();

                    //var instance = Instantiate(vrm).GetComponent<CharaController>();
                    //instance.SetState(CharaController.CHARASTATE.MINIATURE, generatorPortal.transform);
                }).AddTo(_disposable);

            _generatorPortal.GenerateEmptyCharacterAsObservable
                .Subscribe(_ =>
                {
                    //VRM選択画面を非表示(開いたまま別キャラは確認できない仕様)
                    _vrmSelectUI.UIShow(false);

                    textMeshs[0].text = "VRM Load";
                    textMeshs[0].fontSize = textMeshs[0].text.FontSizeMatch(600, 30, 50);

                    //生成ボタンの表示
                    if (_timelineInfo.FieldCharaCount < _timelineInfo.MaxFieldChara) _btnVRMLoad.gameObject.SetActive(true);
                    if (_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(false);
                }).AddTo(_disposable);

            _generatorPortal.GenerateCharacterAsObservable
                .Subscribe(_=> DrawCharaInfo()).AddTo(_disposable);
            _generatorPortal.EndAnimationSetAsObservable
                .Subscribe(_=> DrawAnimeInfo()).AddTo(_disposable);
            _generatorPortal.SubAnimationName
                .Subscribe(DrawAnimePairInfo).AddTo(_disposable);

            //VRMロードの画面とボタンを非表示
            _btnVRMLoad.gameObject.SetActive(false);

            if (_offsetAnchor.gameObject.activeSelf) _offsetAnchor.gameObject.SetActive(false);
            if (_vrmOptionAnchor.gameObject.activeSelf) _vrmOptionAnchor.gameObject.SetActive(false);

            //マニュアル生成
            Instantiate(_bookPrefab[SystemInfo.userProfile.LanguageCode - 1], _bookGrabAnchor.transform);
        }

        public void OnClickManualBook()
        {
            _menuManager.PlayOneShot(SoundType.BTN_CLICK);
            _bookGrabAnchor.SetActive(!_bookGrabAnchor.activeSelf);
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
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
                _menuManager.jumpList.SetCharaDate(_generatorPortal.GetCharasInfo());
            }
            else if (btn == _btnJumpList[1])
            {
                _menuManager.jumpList.SetAnimeData(_generatorPortal.GetDanceInfoData());
            }
            else if (btn == _btnJumpList[2])
            {
                _menuManager.jumpList.SetLipSyncNames(_generatorPortal.GetVmdLipSync());
            }
            _menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        void OnClickSwitchChara(Button_Base btn)
        {
            if(_switchChara[0] == btn)
            {
                _generatorPortal.SetCurrentCharaList(CurrentMode.PRESET);
                _switchChara[0].isEnable = true;
                _switchChara[1].isEnable = false;
                _menuManager.jumpList.Close();
            }
            else
            {
                _generatorPortal.SetCurrentCharaList(CurrentMode.CUSTOM);
                _switchChara[0].isEnable = false;
                _switchChara[1].isEnable = true;
                _menuManager.jumpList.Close();
            }
            _generatorPortal.SetChara(0).Forget();
        }

        void OnClickSwitchAnime(Button_Base btn)
        {
            if (_switchAnime[0] == btn)
            {
                _generatorPortal.SetCurrentAnimeList(CurrentMode.PRESET);
                _switchAnime[0].isEnable = true;
                _switchAnime[1].isEnable = false;
                _menuManager.jumpList.Close();
            }
            else
            {
                _generatorPortal.SetCurrentAnimeList(CurrentMode.CUSTOM);
                _switchAnime[0].isEnable = false;
                _switchAnime[1].isEnable = true;
                _menuManager.jumpList.Close();
            }
            _generatorPortal.SetAnimation(0).Forget();
        }

        /// <summary>
        /// インデックスを前後に進める
        /// </summary>
        /// <param name="btn"></param>
        void MoveIndex(Button_Base btn)
        {
            for (int i = 0; i < _btnChara.Length; i++)
            {
                if (_btnChara[i] == btn)
                {
                    var moveIndex = i == 0 ? -1 : 1;
                    //キャラを変更する
                    _generatorPortal.SetChara(moveIndex).Forget();
                    _menuManager.PlayOneShot(SoundType.BTN_CLICK);
                    return;
                }
            }

            for (int i = 0; i < _btnAnime.Length; i++)
            {
                if (_btnAnime[i] == btn)
                {
                    var moveIndex = i == 0 ? -1 : 1;
                    //アニメーションを変更する
                    _generatorPortal.SetAnimation(moveIndex).Forget();
                    _menuManager.PlayOneShot(SoundType.BTN_CLICK);
                    return;
                }
            }
        }

        /// <summary>
        /// 表情用のVMDを変更
        /// </summary>
        /// <param name="moveIndex"></param>
        void ChangeFacialVMD(int moveIndex)
        {
            //文字画像を差し替える
            _generatorPortal.SetFacialAnimation(moveIndex).Forget();
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
                    FileReadAndWriteUtility.SetMotionOffset(_generatorPortal.GetNowAnimeInfo().viewName, (int)_sliderOffset.Value);
                    textMeshs[3].text = $"{_sliderOffset.Value:0000}";
                    _menuManager.PlayOneShot(SoundType.BTN_CLICK);
                    break;
                }
            }
            FileReadAndWriteUtility.SaveMotionOffset();
        }

        /// <summary>
        /// 次反転アニメーションに切り替える
        /// </summary>
        /// <param name="btn"></param>
        async UniTaskVoid ChangeReverse_Anime(Button_Base btn)
        {
            _generatorPortal.IsAnimationReverse = _switchReverse.isEnable;

            //反転ボタンの状態に合わせる
            var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
            if (portalChara)
            {
                //キャラを生成して反転を反映させる
                await _generatorPortal.SetChara(0);
                textMeshs[1].text = _generatorPortal.GetNowAnimeInfo().viewName;
                textMeshs[1].fontSize = textMeshs[1].text.FontSizeMatch(600, 30, 50);

                //スライダーの値を反映
                portalChara.LookAt.inputWeight_Head = _sliderHeadLook.Value;
                portalChara.LookAt.inputWeight_Eye = _sliderEyeLook.Value;
            }
            _menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// VRMをロード
        /// </summary>
        /// <param name="btn"></param>
        void VRMLoad(Button_Base btn)
        {
            //ボタンを非表示にする
            _btnVRMLoad.gameObject.SetActive(false);

            //VRM選択画面を表示
            _vrmSelectUI.InitPage(0);
            _menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }
        /// <summary>
        /// VRM設定用画面を開く
        /// </summary>
        /// <param name="btn"></param>
        void VRMSetting(Button_Base btn)
        {
            var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
            if (portalChara && portalChara.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                //コピーをVRM設定画面に渡す
                _vrmSelectUI.VRMEditing(portalChara);
                //マニュアル開始
                _timeline.TimelineManualMode().Forget();
            }
            _menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// VRMキャラを削除
        /// </summary>
        /// <param name="btn"></param>
        void DeleteModel(Button_Base btn)
        {
            if (btn == _btnVRMDelete && _timeline.GetCharacterInPortal)
            {
                var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
                if (portalChara.charaInfoData.formatType != CharaInfoData.FORMATTYPE.VRM) return;

                var id = portalChara.charaInfoData.vrmID;
                //フィールド上に存在すれば削除
                _timeline.DeletebindAsset_CleanUp(id);
                //ポータル上のVRMを削除する
                _generatorPortal.DeleteCurrenVRM();
                //Prefabから削除
                _vrmSelectUI.ClearVRMPrefab(id);

                //未使用アセット削除
                Resources.UnloadUnusedAssets();
            }
            else if (btn == _btnDeleteAll)
            {
                //フィールド上を一掃
                _timeline.DeletebindAsset_FieldAll();
            }
            _menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// 口パクボタン押下
        /// </summary>
        /// <param name="btn"></param>
        void Switch_Mouth(Button_Base btn)
        {
            if (btn == _btnFaceUpdate)
            {
                //口モーフの更新を切り替える
                _timeline.SetMouthUpdate(true, _btnFaceUpdate.isEnable);
            }
            else if (btn = _btnMouthUpdate)
            {
                //口モーフの更新を切り替える
                _timeline.SetMouthUpdate(false, _btnMouthUpdate.isEnable);
            }
            _menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// キャラを追加する
        /// </summary>
        async UniTaskVoid Add_FieldChara()
        {
            if (transform.root.gameObject.activeSelf)
            {
                textMeshs[2].text = $"{_timelineInfo.FieldCharaCount}/{_timelineInfo.MaxFieldChara}";
                //負荷が高いので削除処理とフレームをずらす
                await UniTask.Delay(250, cancellationToken: _cancellationToken);
                //ポータルにキャラが存在していなければ生成しておく
                if (!_timeline.GetCharacterInPortal) _generatorPortal.SetChara(0).Forget();
            }
        }

        /// <summary>
        /// キャラ情報を更新する
        /// </summary>
        void DrawCharaInfo()
        {
            if (_btnVRMLoad.gameObject.activeSelf) _btnVRMLoad.gameObject.SetActive(false);

            var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
            if (!portalChara) return;
            if (!_generatorPortal.GetNowCharaName(out var charaName)) return;

            //VRM選択画面を非表示(開いたまま別キャラは確認できない仕様)
            _vrmSelectUI.UIShow(false);

            textMeshs[0].text = charaName;
            textMeshs[0].fontSize = textMeshs[0].text.FontSizeMatch(600, 30, 50);
            textMeshs[2].text = $"{_timelineInfo.FieldCharaCount}/{_timelineInfo.MaxFieldChara}";

            //スライダーの値を反映
            portalChara.LookAt.inputWeight_Head = _sliderHeadLook.Value;
            portalChara.LookAt.inputWeight_Eye = _sliderEyeLook.Value;

            //モーフボタン初期化
            if (portalChara.charaInfoData.formatType == CharaInfoData.FORMATTYPE.FBX)
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
        /// アニメーション情報を更新する
        /// </summary>
        void DrawAnimeInfo()
        {
            //表示更新
            textMeshs[1].text = _generatorPortal.GetNowAnimeInfo().viewName;
            textMeshs[1].fontSize = textMeshs[1].text.FontSizeMatch(600, 30, 50);

            //FBXモーション
            if (_generatorPortal.GetNowAnimeInfo().formatType == DanceInfoData.FORMATTYPE.FBX)
            {
                //反転ボタンを表示
                if (!_switchReverse.gameObject.activeSelf) _switchReverse.gameObject.SetActive(true);

                //offsetの非表示
                if (_offsetAnchor.gameObject.activeSelf) _offsetAnchor.gameObject.SetActive(false);

                //offset更新
                _sliderOffset.Value = 0;
                textMeshs[3].text = $"{_sliderOffset.Value:0000}";

                //LipSyncボタンを非表示
                if (_btnJumpList[2].gameObject.activeSelf) _btnJumpList[2].gameObject.SetActive(false);
            }
            //VMDモーション
            else if (_generatorPortal.GetNowAnimeInfo().formatType == DanceInfoData.FORMATTYPE.VMD)
            {
                //反転ボタンを消す
                if (_switchReverse.gameObject.activeSelf) _switchReverse.gameObject.SetActive(false);

                //offsetの表示
                if (!_offsetAnchor.gameObject.activeSelf) _offsetAnchor.gameObject.SetActive(true);

                //offset更新
                _sliderOffset.Value = FileReadAndWriteUtility.GetMotionOffset[_generatorPortal.GetNowAnimeInfo().viewName];
                textMeshs[3].text = $"{_sliderOffset.Value:0000}";

                //LipSyncボタンを表示
                if (!_btnJumpList[2].gameObject.activeSelf) _btnJumpList[2].gameObject.SetActive(true);
            }
        }

        void DrawAnimePairInfo(string name)
        {
            textMeshs[4].text = name;
            textMeshs[4].fontSize = textMeshs[4].text.FontSizeMatch(600, 25, 40);
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I)) _generatorPortal.SetChara(1).Forget();
            if (Input.GetKeyDown(KeyCode.K)) _generatorPortal.SetAnimation(1).Forget();
            if (Input.GetKeyDown(KeyCode.L))
            {
                //ボタンを非表示にする
                _btnVRMLoad.gameObject.SetActive(false);

                //VRM選択画面を表示
                _vrmSelectUI.InitPage(0);
            }
        }
    }

}