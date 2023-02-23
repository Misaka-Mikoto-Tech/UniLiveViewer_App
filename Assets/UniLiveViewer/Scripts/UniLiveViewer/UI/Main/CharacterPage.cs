using Cysharp.Threading.Tasks;
using UnityEngine;
using NanaCiel;
using System.Threading;

namespace UniLiveViewer 
{
    public class CharacterPage : MonoBehaviour
    {
        MenuManager menuManager;
        /// <summary>
        /// キャラ、モーション、リップ
        /// </summary>
        [SerializeField] Button_Base[] btn_jumpList;

        [SerializeField] Button_Base[] btn_Chara = new Button_Base[2];
        [SerializeField] Button_Base[] btn_Anime = new Button_Base[2];
        [SerializeField] Button_Base[] btn_Offset = new Button_Base[2];
        [SerializeField] Button_Switch btn_Reverse = null;
        [SerializeField] Button_Base btn_VRMLoad = null;
        [SerializeField] Button_Base btn_DeleteAll = null;
        public TextMesh[] textMeshs = null;
        [SerializeField] SliderGrabController slider_Offset = null;
        [SerializeField] SliderGrabController slider_HeadLook = null;
        [SerializeField] SliderGrabController slider_EyeLook = null;

        [Header("＜VRM用＞")]
        [SerializeField] Button_Base btn_VRMSetting = null;
        [SerializeField] Button_Base btn_VRMDelete = null;
        [SerializeField] Button_Base btn_FaceUpdate = null;
        [SerializeField] Button_Base btn_MouthUpdate = null;

        [Header("＜マニュアル用＞")]
        [SerializeField] GameObject[] bookPrefab;
        [SerializeField] GameObject bookGrabAnchor;

        Transform offsetAnchor;
        Transform VRMOptionAnchor;

        TimelineController _timeline;
        TimelineInfo _timelineInfo;
        [SerializeField] GeneratorPortal generatorPortal = null;
        VRMSwitchController vrmSelectUI = null;
        CancellationToken cancellation_token;

        void Awake()
        {
            menuManager = transform.root.GetComponent<MenuManager>();
            cancellation_token = this.GetCancellationTokenOnDestroy();
        }

        // Start is called before the first frame update
        void Start()
        {
            _timeline = menuManager.timeline;
            _timelineInfo = _timeline.GetComponent<TimelineInfo>();
            vrmSelectUI = menuManager.vrmSelectUI;

            //ジャンプリスト
            foreach (var e in btn_jumpList)
            {
                e.onTrigger += OpenJumplist;
            }

            //ジャンプリストから選択された時
            menuManager.jumpList.onSelect += (jumpCurrent) =>
            {
                int moveIndex = 0;
                switch (menuManager.jumpList.target)
                {
                    case JumpList.TARGET.CHARA:
                        moveIndex = jumpCurrent - generatorPortal.CurrentChara;
                        generatorPortal.SetChara(moveIndex).Forget();
                        break;
                    case JumpList.TARGET.ANIME:
                        moveIndex = jumpCurrent - generatorPortal.CurrentAnime;
                        generatorPortal.SetAnimation(moveIndex).Forget();
                        break;
                        // TODO:作業中・・・少し修正大変そう
                    case JumpList.TARGET.VMD_LIPSYNC:
                        moveIndex = jumpCurrent - generatorPortal.CurrentVMDLipSync;
                        ChangeFacialVMD(moveIndex);
                        break;
                }
                menuManager.PlayOneShot(SoundType.BTN_CLICK);
            };

            //その他
            for (int i = 0; i < btn_Chara.Length; i++)
            {
                btn_Chara[i].onTrigger += MoveIndex;
            }
            for (int i = 0; i < btn_Anime.Length; i++)
            {
                btn_Anime[i].onTrigger += MoveIndex;
            }
            for (int i = 0; i < btn_Offset.Length; i++)
            {
                btn_Offset[i].onTrigger += ChangeOffset_Anime;
            }
            btn_Reverse.onTrigger += (b) => ChangeReverse_Anime(b).Forget();
            btn_Reverse.isEnable = false;
            _timeline.FieldCharaAdded += () => { Add_FieldChara().Forget(); };
            _timeline.FieldCharaDeleted += () => { Add_FieldChara().Forget(); };
            slider_Offset.ValueUpdate += () =>
            {
                var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
                if (!portalChara) return;
                //オフセットを設定
                FileReadAndWriteUtility.SetMotionOffset(generatorPortal.GetNowAnimeInfo().viewName, (int)slider_Offset.Value);
                textMeshs[3].text = $"{slider_Offset.Value:0000}";
            };
            slider_Offset.UnControled += () => { FileReadAndWriteUtility.SaveMotionOffset(); };
            offsetAnchor = slider_Offset.transform.parent;
            slider_EyeLook.ValueUpdate += () =>
            {
                var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
                if (!portalChara) return;
                //目の向く量をセット
                portalChara._lookAt.inputWeight_Eye = slider_EyeLook.Value;
            };
            slider_HeadLook.ValueUpdate += () =>
            {
                var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
                if (!portalChara) return;
                //顔の向く量をセット
                portalChara._lookAt.inputWeight_Head = slider_HeadLook.Value;
            };
            btn_VRMLoad.onTrigger += VRMLoad;
            btn_VRMSetting.onTrigger += VRMSetting;
            btn_VRMDelete.onTrigger += DeleteModel;
            btn_DeleteAll.onTrigger += DeleteModel;
            btn_FaceUpdate.onTrigger += Switch_Mouth;
            VRMOptionAnchor = btn_FaceUpdate.transform.parent;
            btn_MouthUpdate.onTrigger += Switch_Mouth;
            vrmSelectUI.OnAddVRM += (vrm) =>
            {
                generatorPortal.AddVRMPrefab(vrm);//VRMを追加
                generatorPortal.SetChara(0).Forget();//追加されたVRMを生成する
            };
            vrmSelectUI.OnSetupComplete += (vrm) =>
            {
                _timeline.ClearPortal();
                //VRMのPrefabを差し替える
                generatorPortal.ChangeCurrentVRM(vrm);
                generatorPortal.SetChara(0).Forget();

                //var instance = Instantiate(vrm).GetComponent<CharaController>();
                //instance.SetState(CharaController.CHARASTATE.MINIATURE, generatorPortal.transform);
            };
            generatorPortal.onEmptyCurrent += () =>
            {
                //VRM選択画面を非表示(開いたまま別キャラは確認できない仕様)
                vrmSelectUI.UIShow(false);

                textMeshs[0].text = "VRM Load";
                textMeshs[0].fontSize = textMeshs[0].text.FontSizeMatch(600, 30, 50);

                //生成ボタンの表示
                if (_timelineInfo.FieldCharaCount < _timelineInfo.MaxFieldChara) btn_VRMLoad.gameObject.SetActive(true);
                if (VRMOptionAnchor.gameObject.activeSelf) VRMOptionAnchor.gameObject.SetActive(false);
            };
            generatorPortal.onGeneratedChara += DrawCharaInfo;
            generatorPortal.onGeneratedAnime += DrawAnimeInfo;
            generatorPortal.onSelectedAnimePair += DrawAnimePairInfo;

            //VRMロードの画面とボタンを非表示
            btn_VRMLoad.gameObject.SetActive(false);

            if (offsetAnchor.gameObject.activeSelf) offsetAnchor.gameObject.SetActive(false);
            if (VRMOptionAnchor.gameObject.activeSelf) VRMOptionAnchor.gameObject.SetActive(false);

            //マニュアル生成
            Instantiate(bookPrefab[SystemInfo.userProfile.LanguageCode - 1], bookGrabAnchor.transform);
        }


        public void OnClickManualBook()
        {
            menuManager.PlayOneShot(SoundType.BTN_CLICK);
            bookGrabAnchor.SetActive(!bookGrabAnchor.activeSelf);
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
            if (!menuManager.jumpList.gameObject.activeSelf) menuManager.jumpList.gameObject.SetActive(true);

            if (btn == btn_jumpList[0])
            {
                menuManager.jumpList.SetCharaDate(generatorPortal.GetCharasInfo());
            }
            else if (btn == btn_jumpList[1])
            {
                menuManager.jumpList.SetAnimeData(generatorPortal.GetDanceInfoData());
            }
            else if (btn == btn_jumpList[2])
            {
                menuManager.jumpList.SetLipSyncNames(generatorPortal.GetVmdLipSync());
            }
            menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// インデックスを前後に進める
        /// </summary>
        /// <param name="btn"></param>
        void MoveIndex(Button_Base btn)
        {
            for (int i = 0; i < btn_Chara.Length; i++)
            {
                //押されたボタンの判別
                if (btn_Chara[i] == btn)
                {
                    int moveIndex = 0;
                    if (i == 0) moveIndex = -1;
                    else if (i == 1) moveIndex = 1;
                    //キャラを変更する
                    generatorPortal.SetChara(moveIndex).Forget();
                    menuManager.PlayOneShot(SoundType.BTN_CLICK);
                    return;
                }
            }

            for (int i = 0; i < btn_Anime.Length; i++)
            {
                //押されたボタンの判別
                if (btn_Anime[i] == btn)
                {
                    int moveIndex = 0;
                    if (i == 0) moveIndex = -1;
                    else if (i == 1) moveIndex = 1;
                    //アニメーションを変更する
                    generatorPortal.SetAnimation(moveIndex).Forget();
                    menuManager.PlayOneShot(SoundType.BTN_CLICK);
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
            generatorPortal.SetFacialAnimation(moveIndex).Forget();
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
                if (btn_Offset[i] == btn)
                {
                    int moveIndex = 0;
                    if (i == 0) moveIndex = 1;
                    else if (i == 1) moveIndex = -1;

                    //オフセットを設定
                    slider_Offset.Value += moveIndex;
                    FileReadAndWriteUtility.SetMotionOffset(generatorPortal.GetNowAnimeInfo().viewName, (int)slider_Offset.Value);
                    textMeshs[3].text = $"{slider_Offset.Value:0000}";
                    menuManager.PlayOneShot(SoundType.BTN_CLICK);
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
            generatorPortal.IsAnimationReverse = btn_Reverse.isEnable;

            //反転ボタンの状態に合わせる
            var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
            Debug.LogWarning("こおここ");
            if (portalChara)
            {
                Debug.LogWarning("こおここ");

                //キャラを生成して反転を反映させる
                await generatorPortal.SetChara(0);
                textMeshs[1].text = generatorPortal.GetNowAnimeInfo().viewName;
                textMeshs[1].fontSize = textMeshs[1].text.FontSizeMatch(600, 30, 50);

                //スライダーの値を反映
                portalChara._lookAt.inputWeight_Head = slider_HeadLook.Value;
                portalChara._lookAt.inputWeight_Eye = slider_EyeLook.Value;
            }
            menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// VRMをロード
        /// </summary>
        /// <param name="btn"></param>
        void VRMLoad(Button_Base btn)
        {
            //ボタンを非表示にする
            btn_VRMLoad.gameObject.SetActive(false);

            //VRM選択画面を表示
            vrmSelectUI.InitPage(0);
            menuManager.PlayOneShot(SoundType.BTN_CLICK);
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
                vrmSelectUI.VRMEditing(portalChara);
                //マニュアル開始
                _timeline.TimelineManualMode().Forget();
            }
            menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// VRMキャラを削除
        /// </summary>
        /// <param name="btn"></param>
        void DeleteModel(Button_Base btn)
        {
            if (btn == btn_VRMDelete && _timelineInfo.IsPortalChara())
            {
                var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
                if (portalChara.charaInfoData.formatType != CharaInfoData.FORMATTYPE.VRM) return;

                int id = portalChara.charaInfoData.vrmID;
                //フィールド上に存在すれば削除
                _timeline.DeletebindAsset_CleanUp(id);
                //ポータル上のVRMを削除する
                generatorPortal.DeleteCurrenVRM();
                //Prefabから削除
                vrmSelectUI.ClearVRMPrefab(id);

                //未使用アセット削除
                Resources.UnloadUnusedAssets();
            }
            else if (btn == btn_DeleteAll)
            {
                //フィールド上を一掃
                _timeline.DeletebindAsset_FieldAll();
            }
            menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// 口パクボタン押下
        /// </summary>
        /// <param name="btn"></param>
        void Switch_Mouth(Button_Base btn)
        {
            if (btn == btn_FaceUpdate)
            {
                //口モーフの更新を切り替える
                _timeline.SetMouthUpdate_Portal(true, btn_FaceUpdate.isEnable);
            }
            else if (btn = btn_MouthUpdate)
            {
                //口モーフの更新を切り替える
                _timeline.SetMouthUpdate_Portal(false, btn_MouthUpdate.isEnable);
            }
            menuManager.PlayOneShot(SoundType.BTN_CLICK);
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
                await UniTask.Delay(250, cancellationToken: cancellation_token);
                //ポータルにキャラが存在していなければ生成しておく
                if (!_timelineInfo.IsPortalChara()) generatorPortal.SetChara(0).Forget();
            }
        }

        /// <summary>
        /// キャラ情報を更新する
        /// </summary>
        void DrawCharaInfo()
        {
            if (btn_VRMLoad.gameObject.activeSelf) btn_VRMLoad.gameObject.SetActive(false);

            var portalChara = _timelineInfo.GetCharacter(TimelineController.PORTAL_INDEX);
            if (!portalChara) return;

            string charaName;
            if (!generatorPortal.GetNowCharaName(out charaName)) return;

            //VRM選択画面を非表示(開いたまま別キャラは確認できない仕様)
            vrmSelectUI.UIShow(false);

            textMeshs[0].text = charaName;
            textMeshs[0].fontSize = textMeshs[0].text.FontSizeMatch(600, 30, 50);
            textMeshs[2].text = $"{_timelineInfo.FieldCharaCount}/{_timelineInfo.MaxFieldChara}";

            //スライダーの値を反映
            portalChara._lookAt.inputWeight_Head = slider_HeadLook.Value;
            portalChara._lookAt.inputWeight_Eye = slider_EyeLook.Value;

            //モーフボタン初期化
            if (portalChara.charaInfoData.formatType == CharaInfoData.FORMATTYPE.FBX)
            {
                if (VRMOptionAnchor.gameObject.activeSelf) VRMOptionAnchor.gameObject.SetActive(false);
            }
            else
            {
                if (!VRMOptionAnchor.gameObject.activeSelf) VRMOptionAnchor.gameObject.SetActive(true);
                btn_FaceUpdate.isEnable = true;
                btn_MouthUpdate.isEnable = true;
            }
        }

        /// <summary>
        /// アニメーション情報を更新する
        /// </summary>
        void DrawAnimeInfo()
        {
            //表示更新
            textMeshs[1].text = generatorPortal.GetNowAnimeInfo().viewName;
            textMeshs[1].fontSize = textMeshs[1].text.FontSizeMatch(600, 30, 50);

            //FBXモーション
            if (generatorPortal.GetNowAnimeInfo().formatType == DanceInfoData.FORMATTYPE.FBX)
            {
                //反転ボタンを表示
                if (!btn_Reverse.gameObject.activeSelf) btn_Reverse.gameObject.SetActive(true);

                //offsetの非表示
                if (offsetAnchor.gameObject.activeSelf) offsetAnchor.gameObject.SetActive(false);

                //offset更新
                slider_Offset.Value = 0;
                textMeshs[3].text = $"{slider_Offset.Value:0000}";

                //LipSyncボタンを非表示
                if (btn_jumpList[2].gameObject.activeSelf) btn_jumpList[2].gameObject.SetActive(false);
            }
            //VMDモーション
            else if (generatorPortal.GetNowAnimeInfo().formatType == DanceInfoData.FORMATTYPE.VMD)
            {
                //反転ボタンを消す
                if (btn_Reverse.gameObject.activeSelf) btn_Reverse.gameObject.SetActive(false);

                //offsetの表示
                if (!offsetAnchor.gameObject.activeSelf) offsetAnchor.gameObject.SetActive(true);

                //offset更新
                slider_Offset.Value = FileReadAndWriteUtility.GetMotionOffset[generatorPortal.GetNowAnimeInfo().viewName];
                textMeshs[3].text = $"{slider_Offset.Value:0000}";

                //LipSyncボタンを表示
                if(!btn_jumpList[2].gameObject.activeSelf) btn_jumpList[2].gameObject.SetActive(true);
            }
        }

        void DrawAnimePairInfo(string name)
        {
            textMeshs[4].text = name;
            textMeshs[4].fontSize = textMeshs[4].text.FontSizeMatch(600, 25, 40);
        }

        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I)) generatorPortal.SetChara(1).Forget();
            if (Input.GetKeyDown(KeyCode.K)) generatorPortal.SetAnimation(1).Forget();
            if (Input.GetKeyDown(KeyCode.L))
            {
                //ボタンを非表示にする
                btn_VRMLoad.gameObject.SetActive(false);

                //VRM選択画面を表示
                vrmSelectUI.InitPage(0);
            }
        }
    }

}