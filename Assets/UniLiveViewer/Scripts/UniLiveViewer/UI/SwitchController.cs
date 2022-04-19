using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;
using NanaCiel;

namespace UniLiveViewer
{
    //TODO:ページごとに分けた方がいい、継ぎ足しの産物
    public class SwitchController : MonoBehaviour
    {
        [Header("＜マニュアル＞")]
        [SerializeField] private Sprite[] sprManualPrefab = new Sprite[4];
        [SerializeField] private SpriteRenderer[] sprManual = new SpriteRenderer[2];
        private TimelineController timeline = null;
        private GeneratorPortal generatorPortal = null;
        private PlayerStateManager playerStateManager = null;
        private VRMSwitchController vrmSelectUI = null;

        public event Action<string> onSceneSwitch;

        [Header("＜ページ・タブボタン＞")]
        public Transform[] pageTransform = new Transform[4];
        private int currentPage = 0;//現在のページ数
        [SerializeField] private Button_Switch[] btn_Tab = new Button_Switch[4];
        [SerializeField] private JumpList jumpList = null;
        [SerializeField] private Button_Base[] btn_jumpList = new Button_Base[0];

        [Header("＜Sound＞")]
        [SerializeField] private AudioClip[] Sound;//ボタン音,タブ音,ボタン揺れ音
        private AudioSource audioSource;

        [Space(1), Header("＜1ページ＞")]
        [SerializeField] private Button_Base[] btn_Chara = new Button_Base[2];
        [SerializeField] private Button_Base[] btn_Anime = new Button_Base[2];
        [SerializeField] private Button_Base[] btn_Offset = new Button_Base[2];
        [SerializeField] private Button_Switch btn_Reverse = null;
        [SerializeField] private Button_Base btn_VRMLoad = null;
        [SerializeField] private Button_Base btn_VRMSetting = null;
        [SerializeField] private Button_Base btn_VRMDelete = null;
        [SerializeField] private Button_Base btn_DeleteAll = null;
        [SerializeField] private Button_Base btn_FaceUpdate = null;
        [SerializeField] private Button_Base btn_MouthUpdate = null;
        public TextMesh[] textMesh_Page1 = new TextMesh[0];
        [SerializeField] private SliderGrabController slider_Offset = null;
        [SerializeField] private SliderGrabController slider_HeadLook = null;
        [SerializeField] private SliderGrabController slider_EyeLook = null;

        private Transform offsetAnchor;
        private Transform VRMOptionAnchor;

        [Space(1), Header("＜2ページ＞")]
        [SerializeField] private Button_Base[] btn_Audio = new Button_Base[2];
        [SerializeField] private Button_Base btnS_Play = null;
        [SerializeField] private Button_Base btnS_Stop = null;
        [SerializeField] private Button_Base btnS_BaseReturn = null;
        [SerializeField] private TextMesh[] textMesh_Page2 = new TextMesh[4];
        [SerializeField] private SliderGrabController slider_Playback = null;
        [SerializeField] private SliderGrabController slider_Speed = null;
        private FileAccessManager fileAccess;
        [SerializeField] private Button_Base[] btnS_AudioLoad = new Button_Base[2];

        [Space(1), Header("＜3ページ＞")]
        [SerializeField] private Button_Switch[] btnE_SecenChange = new Button_Switch[3];
        [Space(20)]
        [SerializeField] private Transform[] sceneUIAnchor = new Transform[3];
        private Button_Base[] btnE = new Button_Base[5];
        [SerializeField] private Transform[] btnE_ActionParent;
        [SerializeField] private TextMesh[] textMesh_Page3 = new TextMesh[2];
        [SerializeField] private Material material_OutLine;
        [SerializeField] private UniversalRendererData frd;
        [SerializeField] private ScriptableRendererFeature outlineRender;
        [SerializeField] private SliderGrabController slider_OutLine = null;
        [SerializeField] private SliderGrabController slider_InitCharaSize = null;
        [SerializeField] private SliderGrabController slider_FixedFoveated = null;
        [SerializeField] private SliderGrabController slider_Fog = null;
        [SerializeField] private Button_Base btn_passthrough = null;

        private BackGroundController backGroundCon;

        private Material matMirrore;//LiveScene用

        [Space(1)]
        [Header("＜4ページ＞")]
        [SerializeField] private Button_Base[] btn_Item = new Button_Base[2];
        [SerializeField] private DecorationItemInfo[] ItemPrefab = new DecorationItemInfo[0];
        [SerializeField] private GameObject itemMaterialPrefab;
        [SerializeField] private int currentItem = 0;
        [SerializeField] private TextMesh textMesh_Page4;
        [SerializeField] private Transform itemGeneratAnchor;
        [SerializeField] private Transform itemMaterialAnchor;

        private CancellationToken cancellation_token;

        void Awake()
        {
            //Find
            generatorPortal = GameObject.FindGameObjectWithTag("GeneratorPortal").gameObject.GetComponent<GeneratorPortal>();
            timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = SystemInfo.soundVolume_SE;
            playerStateManager = GameObject.FindGameObjectWithTag("Player").gameObject.GetComponent<PlayerStateManager>();
            vrmSelectUI = GameObject.FindGameObjectWithTag("VRMSelectUI").gameObject.GetComponent<VRMSwitchController>();
            fileAccess = GameObject.FindGameObjectWithTag("AppConfig").gameObject.GetComponent<FileAccessManager>();


            //コールバック登録・・・共有
            for (int i = 0; i < btn_Tab.Length; i++)
            {
                btn_Tab[i].onTrigger += NextPage;
            }
            timeline.playableDirector.played += Director_Played;
            timeline.playableDirector.stopped += Director_Stoped;
            //コールバック登録・・・1ページ目
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
            timeline.FieldCharaAdded += () => { Add_FieldChara().Forget(); };
            timeline.FieldCharaDeleted += () => { Add_FieldChara().Forget(); };
            slider_Offset.ValueUpdate += () =>
            {
                //キャラが不在なら終了
                if (!timeline.trackBindChara[TimelineController.PORTAL_ELEMENT]) return;
                //オフセットを設定
                timeline.SetVMD_MotionOffset(generatorPortal.GetNowAnimeInfo().viewName, (int)slider_Offset.Value);
                textMesh_Page1[3].text = $"{slider_Offset.Value:0000}";
            };
            slider_Offset.UnControled += () => { SystemInfo.userProfile.SaveOffset(); };
            offsetAnchor = slider_Offset.transform.parent;
            slider_EyeLook.ValueUpdate += () =>
            {
                //キャラが不在なら終了
                if (!timeline.trackBindChara[TimelineController.PORTAL_ELEMENT]) return;
                //目の向く量をセット
                timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].lookAtCon.inputWeight_Eye = slider_EyeLook.Value;
            };
            slider_HeadLook.ValueUpdate += () =>
            {
                //キャラが不在なら終了
                if (!timeline.trackBindChara[TimelineController.PORTAL_ELEMENT]) return;
                //顔の向く量をセット
                timeline.trackBindChara[TimelineController.PORTAL_ELEMENT].lookAtCon.inputWeight_Head = slider_HeadLook.Value;
            };
            btn_VRMLoad.onTrigger += VRMLoad;
            btn_VRMSetting.onTrigger += VRMSetting;
            btn_VRMDelete.onTrigger += DeleteModel;
            btn_DeleteAll.onTrigger += DeleteModel;
            btn_FaceUpdate.onTrigger += Switch_Mouth;
            VRMOptionAnchor = btn_FaceUpdate.transform.parent;
            btn_MouthUpdate.onTrigger += Switch_Mouth;
            vrmSelectUI.VRMAdded += (vrm) =>
            {
                generatorPortal.AddVRMPrefab(vrm);//VRMを追加
                ChangeChara(0).Forget();//追加されたVRMを生成する
            };
            btn_jumpList[0].onTrigger += (btn) =>
            {
                if (!jumpList.gameObject.activeSelf) jumpList.gameObject.SetActive(true);
                jumpList.SetCharaDate(generatorPortal.GetCharasInfo());
                audioSource.PlayOneShot(Sound[0]);//クリック音
            };
            btn_jumpList[1].onTrigger += (btn) =>
            {
                if (!jumpList.gameObject.activeSelf) jumpList.gameObject.SetActive(true);
                jumpList.SetAnimeData(generatorPortal.GetDanceInfoData());
                audioSource.PlayOneShot(Sound[0]);//クリック音
            };
            btn_jumpList[2].onTrigger += (btn) =>
            {
                if (!jumpList.gameObject.activeSelf) jumpList.gameObject.SetActive(true);
                jumpList.SetLipSyncNames(generatorPortal.GetVmdLipSync());
                audioSource.PlayOneShot(Sound[0]);//クリック音
            };
            btn_jumpList[3].onTrigger += (btn) =>
            {
                if (!jumpList.gameObject.activeSelf) jumpList.gameObject.SetActive(true);
                jumpList.SetAudioDate();
                audioSource.PlayOneShot(Sound[0]);//クリック音
            };
            btn_jumpList[4].onTrigger += (btn) =>
            {
                if (!jumpList.gameObject.activeSelf) jumpList.gameObject.SetActive(true);
                jumpList.SetItemData(ItemPrefab);
                audioSource.PlayOneShot(Sound[0]);//クリック音
            };

            jumpList.onSelect += (jumpCurrent) =>
            {
                int moveIndex = 0;
                switch (jumpList.target)
                {
                    case JumpList.TARGET.CHARA:
                        moveIndex = jumpCurrent - generatorPortal.currentChara;
                        ChangeChara(moveIndex).Forget();
                        break;
                    case JumpList.TARGET.ANIME:
                        moveIndex = jumpCurrent - generatorPortal.currentAnime;
                        ChangeAnime(moveIndex).Forget();
                        break;
                    case JumpList.TARGET.VMD_LIPSYNC:
                        moveIndex = jumpCurrent - generatorPortal.currentVMDLipSync;
                        ChangeVMDLipSync(moveIndex);
                        break;
                    case JumpList.TARGET.AUDIO:
                        moveIndex = jumpCurrent - fileAccess.CurrentAudio;
                        ChangeAuido(moveIndex);
                        break;
                    case JumpList.TARGET.ITEM:
                        moveIndex = jumpCurrent - currentItem;
                        ChangeItem(moveIndex);
                        break;
                }
                audioSource.PlayOneShot(Sound[0]);//クリック音
            };
            vrmSelectUI.onSetupComplete += (vrm) =>
            {
                timeline.ClearPortal();
                //VRMのPrefabを差し替える
                generatorPortal.ChangeCurrentVRM(vrm);
                ChangeChara(0).Forget();

                //var instance = Instantiate(vrm).GetComponent<CharaController>();
                //instance.SetState(CharaController.CHARASTATE.MINIATURE, generatorPortal.transform);
            };

            //コールバック登録・・・2ページ目
            for (int i = 0; i < btn_Audio.Length; i++)
            {
                btn_Audio[i].onTrigger += MoveIndex_Auido;
            }
            slider_Playback.Controled += ManualStart;
            slider_Playback.ValueUpdate += () =>
            {
                float sec = slider_Playback.Value;
                timeline.AudioClip_PlaybackTime = sec;//timelineに時間を反映
                textMesh_Page2[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";//テキストに反映
            };
            slider_Speed.ValueUpdate += () =>
            {
                timeline.timelineSpeed = slider_Speed.Value;//スライダーの値を反映
                textMesh_Page2[3].text = $"{slider_Speed.Value:0.00}";//テキストに反映
            };
            btnS_Play.onTrigger += Click_AudioPlayer;
            btnS_Stop.onTrigger += Click_AudioPlayer;
            btnS_BaseReturn.onTrigger += Click_AudioPlayer;
            for (int i = 0; i < btn_Anime.Length; i++)
            {
                btnS_AudioLoad[i].onTrigger += Click_AudioLoad;
            }
            //コールバック登録・・・3ページ目
            slider_OutLine.ValueUpdate += () =>
            {
                if (slider_OutLine.Value > 0)
                {
                    outlineRender.SetActive(true);//有効化
                    material_OutLine.SetFloat("_Edge", slider_OutLine.Value);//値の更新
                }
                else outlineRender.SetActive(false);//無効化
            };
            slider_InitCharaSize.ValueUpdate += Update_InitCharaSize;
            slider_InitCharaSize.UnControled += () =>
            {
                SystemInfo.userProfile.data.InitCharaSize = float.Parse(slider_InitCharaSize.Value.ToString("f2"));
                SystemInfo.userProfile.WriteJson();
            };
            slider_FixedFoveated.ValueUpdate += Update_FixedFoveated;
            slider_Fog.ValueUpdate += () => { RenderSettings.fogDensity = slider_Fog.Value; };
            //今回は実装しない
            //btn_passthrough.onTrigger += (b) => {

            //    if (b.gameObject.activeSelf)
            //    {
            //        b.isEnable = !b.isEnable;

            //        if (b)
            //        {
            //            backGroundCon.Clear_CubemapTex();
            //            DynamicGI.UpdateEnvironment();
            //        }
            //        else
            //        {
            //            string str;
            //            backGroundCon.SetCubemap(0, out str);
            //            DynamicGI.UpdateEnvironment();
            //        }
            //    }
            //};
        }

        private void OnEnable()
        {
            InitPage().Forget();
        }

        // Start is called before the first frame update
        void Start()
        {
            cancellation_token = this.GetCancellationTokenOnDestroy();

            //VRMロードの画面とボタンを非表示
            btn_VRMLoad.gameObject.SetActive(false);

            if (offsetAnchor.gameObject.activeSelf) offsetAnchor.gameObject.SetActive(false);
            if (VRMOptionAnchor.gameObject.activeSelf) VRMOptionAnchor.gameObject.SetActive(false);

            if (SystemInfo.sceneMode == SceneMode.CANDY_LIVE)
            {
                var manualAnchor = GameObject.FindGameObjectWithTag("ManualUI").transform;
                sprManual[0] = manualAnchor.GetChild(0).GetComponent<SpriteRenderer>();
                sprManual[1] = manualAnchor.GetChild(1).GetComponent<SpriteRenderer>();

                if (SystemInfo.userProfile.data.LanguageCode == (int)USE_LANGUAGE.JP)
                {
                    sprManual[0].sprite = sprManualPrefab[1];
                }
                else
                {
                    sprManual[0].sprite = sprManualPrefab[0];
                }
                if (SystemInfo.userProfile.data.LanguageCode == (int)USE_LANGUAGE.JP)
                {
                    sprManual[1].sprite = sprManualPrefab[3];
                }
                else
                {
                    sprManual[1].sprite = sprManualPrefab[2];
                }

                //シーン応じて有効化を切り替える
                for (int i = 0; i < 3; i++)
                {
                    if (i == 0) pageTransform[2].GetChild(i).gameObject.SetActive(true);
                    else pageTransform[2].GetChild(i).gameObject.SetActive(false);
                }

                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 5; i++)
                {
                    btnE[i] = sceneUIAnchor[0].GetChild(i).GetComponent<Button_Base>();
                }

                btnE_ActionParent = new Transform[5];

                btnE_ActionParent[0] = GameObject.FindGameObjectWithTag("Particle").transform;
                btnE_ActionParent[1] = GameObject.FindGameObjectWithTag("LaserGun").transform;
                btnE_ActionParent[2] = GameObject.FindGameObjectWithTag("FloorMirror").transform;
                btnE_ActionParent[3] = GameObject.FindGameObjectWithTag("SonicBoom").transform;
                btnE_ActionParent[4] = GameObject.FindGameObjectWithTag("ManualUI").transform;

                matMirrore = btnE_ActionParent[2].GetComponent<MeshRenderer>().material;

                btnE_ActionParent[0].gameObject.SetActive(SystemInfo.userProfile.data.scene_crs_particle);
                btnE_ActionParent[1].gameObject.SetActive(SystemInfo.userProfile.data.scene_crs_laser);
                matMirrore.SetFloat("_Smoothness", SystemInfo.userProfile.data.scene_crs_reflection ? 1 : 0);
                btnE_ActionParent[3].gameObject.SetActive(SystemInfo.userProfile.data.scene_crs_sonic);
                btnE_ActionParent[4].gameObject.SetActive(SystemInfo.userProfile.data.scene_crs_manual);
            }
            else if (SystemInfo.sceneMode == SceneMode.KAGURA_LIVE)
            {
                //シーン応じて有効化を切り替える
                for (int i = 0; i < 3; i++)
                {
                    if (i == 1) pageTransform[2].GetChild(i).gameObject.SetActive(true);
                    else pageTransform[2].GetChild(i).gameObject.SetActive(false);
                }

                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 3; i++)
                {
                    btnE[i] = sceneUIAnchor[1].GetChild(i).GetComponent<Button_Base>();
                }

                btnE_ActionParent = new Transform[3];

                btnE_ActionParent[0] = GameObject.FindGameObjectWithTag("Particle").transform;
                btnE_ActionParent[1] = GameObject.FindGameObjectWithTag("ReflectionProbe").transform;
                btnE_ActionParent[2] = GameObject.FindGameObjectWithTag("WaterAnchor").transform;

                btnE_ActionParent[0].gameObject.SetActive(SystemInfo.userProfile.data.scene_kagura_particle);
                btnE_ActionParent[1].gameObject.SetActive(SystemInfo.userProfile.data.scene_kagura_sea);
                btnE_ActionParent[2].transform.GetChild(0).gameObject.SetActive(SystemInfo.userProfile.data.scene_kagura_reflection);
            }
            else if (SystemInfo.sceneMode == SceneMode.VIEWER)
            {
                //シーン応じて有効化を切り替える
                for (int i = 0; i < 3; i++)
                {
                    if (i == 2) pageTransform[2].GetChild(i).gameObject.SetActive(true);
                    else pageTransform[2].GetChild(i).gameObject.SetActive(false);
                }
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 4; i++)
                {
                    btnE[i] = sceneUIAnchor[2].GetChild(i).GetComponent<Button_Base>();
                }

                btnE_ActionParent = new Transform[1];

                btnE_ActionParent[0] = GameObject.FindGameObjectWithTag("FloorLED").transform;
                backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();

                btnE_ActionParent[0].gameObject.SetActive(SystemInfo.userProfile.data.scene_view_led);
            }

            //再生スライダーに最大値を設定
            slider_Playback.maxValuel = (float)timeline.playableDirector.duration;
            //再生ボタンを非表示
            btnS_Play.gameObject.SetActive(false);
            //コールバック登録・・・4ページ
            for (int i = 0; i < btn_Item.Length; i++)
            {
                btn_Item[i].onTrigger += MoveIndex_Item;
            }

            //レンダーパイプラインからoutlineオブジェクトを取得
            foreach (var renderObj in frd.rendererFeatures)
            {
                if (renderObj.name == "Outline")
                {
                    outlineRender = renderObj;
                    break;
                }
            }
            outlineRender.SetActive(false);//無効化

            //値の更新
            slider_OutLine.Value = 0;
            material_OutLine.SetFloat("_Edge", slider_OutLine.Value);
            slider_Fog.Value = 0.03f;

            //ページを切り替える
            PageSwitching();
            //音楽をセット
            timeline.NextAudioClip(0);
        }

        // Update is called once per frame
        void Update()
        {
            //該当ページになっていなければページを切り替える
            if (!pageTransform[currentPage].gameObject.activeSelf) PageSwitching();

            //ページ毎の処理に分岐
            switch (currentPage)
            {
                case 0:
                    //モデルページ
                    Page_Model();
                    break;
                case 1:
                    //サウンドページ
                    Page_Sound();
                    break;
                case 2:
                    //エフェクトページ
                    Page_Effect();
                    break;
                case 3:
                    //アイテムページ
                    Page_Item();
                    break;
            }

            DebugInput();
        }

        private void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                currentPage++;
                if (currentPage >= pageTransform.Length) currentPage = 0;
            }

            //ページ毎の処理に分岐
            switch (currentPage)
            {
                case 0:
                    //モデルページ
                    if (Input.GetKeyDown(KeyCode.I)) ChangeChara(1).Forget();
                    if (Input.GetKeyDown(KeyCode.K)) ChangeAnime(1).Forget();
                    if (Input.GetKeyDown(KeyCode.L))
                    {
                        //ボタンを非表示にする
                        btn_VRMLoad.gameObject.SetActive(false);

                        //VRM選択画面を表示
                        vrmSelectUI.InitPage(0);
                    }
                    break;
                case 1:
                    //サウンドページ
                    if (Input.GetKeyDown(KeyCode.I)) ChangeAuido(1);
                    if (Input.GetKeyDown(KeyCode.K))
                    {
                        //タイムライン・再生
                        timeline.TimelinePlay();
                        //再生・停止ボタンの状態更新
                        btnS_Stop.gameObject.SetActive(true);
                        btnS_Play.gameObject.SetActive(false);
                    }
                    if (Input.GetKeyDown(KeyCode.L))
                    {
                        //読み込む
                        StartCoroutine(LoadCheck());
                    }
                    break;
                case 2:
                    //エフェクトページ
                    if (Input.GetKeyDown(KeyCode.I)) Click_Setting_Viewer(2);//ワームホール
                    if (Input.GetKeyDown(KeyCode.K)) Click_Setting_Viewer(3);//パーティクル
                    break;
                case 3:
                    //アイテムページ
                    if (Input.GetKeyDown(KeyCode.I)) ChangeItem(1);
                    if (Input.GetKeyDown(KeyCode.K)) ChangeItem(-1);
                    break;
            }
        }

        /// <summary>
        /// 次ページを判別する
        /// </summary>
        /// <param name="btn"></param>
        private void NextPage(Button_Base btn)
        {
            //クリック音
            audioSource.PlayOneShot(Sound[1]);

            //タブのボタン状態を更新する
            for (int i = 0; i < btn_Tab.Length; i++)
            {
                //トリガーを入れたボタンか照合
                if (btn_Tab[i] == btn)
                {
                    //currentを移動
                    currentPage = i;
                    break;
                }
            }
        }

        /// <summary>
        /// 該当ページを有効化、その他ページを無効化する
        /// </summary>
        private void PageSwitching()
        {
            for (int i = 0; i < btn_Tab.Length; i++)
            {
                if (!btn_Tab[i]) continue;
                if (currentPage == i)
                {
                    //タブを有効表示に切り替える
                    btn_Tab[i].isEnable = true;

                    //該当ページに切り替える
                    pageTransform[i].gameObject.SetActive(true);
                }
                else
                {
                    //タブを無効表示に切り替える
                    btn_Tab[i].isEnable = false;

                    //該当ページに切り替える
                    pageTransform[i].gameObject.SetActive(false);
                }
            }

            //タブを切り替えたらジャンプリストは非表示
            if (jumpList.gameObject.activeSelf)
            {
                jumpList.gameObject.SetActive(false);
            }

            //ページを初期化する    
            InitPage().Forget();
        }

        /// <summary>
        /// 各ページを開く際の初期化処理
        /// </summary>
        public async UniTask InitPage()
        {
            switch (currentPage)
            {
                case 0:
                    //ポータルにキャラが存在していなければ生成しておく
                    if (!timeline.isPortalChara())
                    {
                        await ChangeChara(0);
                        ChangeAnime_UI();
                    }

                    var bindChara = timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
                    if (bindChara)
                    {
                        //反転ボタンの状態をセット
                        btn_Reverse.isEnable = generatorPortal.isAnimationReverse;
                    }
                    //フィールド設置数のテキストセット
                    textMesh_Page1[2].text = $"{timeline.FieldCharaCount}/{timeline.maxFieldChara}";
                    break;
                case 1:
                    if (timeline.playableDirector.timeUpdateMode == DirectorUpdateMode.Manual)
                    {
                        btnS_Stop.gameObject.SetActive(false);
                        btnS_Play.gameObject.SetActive(true);
                    }
                    else
                    {
                        btnS_Stop.gameObject.SetActive(true);
                        btnS_Play.gameObject.SetActive(false);
                    }
                    //読み込み関係のボタンを初期化しておく
                    StartCoroutine(ReceptionTime(0));
                    //オーディオの長さ
                    float sec = timeline.GetNowAudioLength();
                    textMesh_Page2[2].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
                    //タイムラインの速度を表示
                    slider_Speed.Value = timeline.timelineSpeed;
                    textMesh_Page2[3].text = $"{slider_Speed.Value:0.00}";

                    break;
                case 2:
                    //sceneボタン初期化
                    foreach (var e in btnE_SecenChange)
                    {
                        e.isEnable = false;
                    }

                    if (SystemInfo.sceneMode == SceneMode.CANDY_LIVE)
                    {
                        //各種有効化状態にボタンを合わせる
                        btnE[0].isEnable = btnE_ActionParent[0].gameObject.activeSelf;
                        btnE[1].isEnable = btnE_ActionParent[1].gameObject.activeSelf;
                        btnE[3].isEnable = btnE_ActionParent[3].gameObject.activeSelf;
                        btnE[4].isEnable = btnE_ActionParent[4].gameObject.activeSelf;

                        //反射状態に合わせる
                        btnE[2].isEnable = (matMirrore.GetFloat("_Smoothness") == 1.0f ? true : false);
                    }
                    else if (SystemInfo.sceneMode == SceneMode.KAGURA_LIVE)
                    {
                        //各種有効化状態にボタンを合わせる
                        btnE[0].isEnable = btnE_ActionParent[0].gameObject.activeSelf;
                        btnE[1].isEnable = btnE_ActionParent[1].gameObject.activeSelf;
                        btnE[2].isEnable = btnE_ActionParent[2].transform.GetChild(0).gameObject.activeSelf;
                    }
                    else if (SystemInfo.sceneMode == SceneMode.VIEWER)
                    {
                        //各種有効化状態にボタンを合わせる
                        btnE[0].isEnable = btnE_ActionParent[0].gameObject.activeSelf;
                    }
                    //キャラサイズ
                    slider_InitCharaSize.Value = SystemInfo.userProfile.data.InitCharaSize;
                    Update_InitCharaSize();
                    //固定中心窩レンダリング初期化
                    Update_FixedFoveated();

                    break;
                case 3:
                    //アイテムがなければ生成
                    if (itemGeneratAnchor.childCount == 0) ChangeItem(0);
                    break;
            }

            //統括してここでボタンの揺れ音を鳴らす
            StartCoroutine(DelaySoundPlay(0.4f));
        }


        /// <summary>
        /// インデックスを前後に進める
        /// </summary>
        /// <param name="btn"></param>
        private void MoveIndex(Button_Base btn)
        {
            for (int i = 0; i < 2; i++)
            {
                //押されたボタンの判別
                if (btn_Chara[i] == btn)
                {
                    int moveIndex = 0;
                    if (i == 0) moveIndex = -1;
                    else if (i == 1) moveIndex = 1;
                    //キャラを変更する
                    ChangeChara(moveIndex).Forget();

                    //クリック音
                    audioSource.PlayOneShot(Sound[0]);
                    return;
                }
            }

            for (int i = 0; i < 2; i++)
            {
                //押されたボタンの判別
                if (btn_Anime[i] == btn)
                {
                    int moveIndex = 0;
                    if (i == 0) moveIndex = -1;
                    else if (i == 1) moveIndex = 1;
                    //アニメーションを変更する
                    ChangeAnime(moveIndex).Forget();

                    //クリック音
                    audioSource.PlayOneShot(Sound[0]);
                    return;
                }
            }
        }

        /// <summary>
        /// キャラを変更する
        /// </summary>
        /// <param name="moveIndex"></param>
        private async UniTask ChangeChara(int moveIndex)
        {
            // キャラを切り替える
            await generatorPortal.SetChara(moveIndex);

            var bindChara = timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
            string charaName;

            //何かしらのキャラ枠
            if (generatorPortal.GetNowCharaName(out charaName))
            {
                //VRMボタンを非表示にする
                if (btn_VRMLoad.gameObject.activeSelf) btn_VRMLoad.gameObject.SetActive(false);

                //VRM選択画面を非表示(開いたまま別キャラは確認できない仕様)
                vrmSelectUI.SetUIView(true);

                textMesh_Page1[0].text = charaName;
                textMesh_Page1[0].fontSize = textMesh_Page1[0].text.FontSizeMatch(600, 30, 50);

                //FieldMaxの場合はキャラが存在しない為確認
                if (bindChara)
                {
                    //スライダーの値を反映
                    bindChara.lookAtCon.inputWeight_Head = slider_HeadLook.Value;
                    bindChara.lookAtCon.inputWeight_Eye = slider_EyeLook.Value;

                    //モーフの有効化
                    btn_FaceUpdate.isEnable = true;
                    btn_MouthUpdate.isEnable = true;
                    //モーフボタン初期化
                    if (bindChara.charaInfoData.formatType == CharaInfoData.FORMATTYPE.FBX)
                    {
                        if (VRMOptionAnchor.gameObject.activeSelf) VRMOptionAnchor.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (!VRMOptionAnchor.gameObject.activeSelf) VRMOptionAnchor.gameObject.SetActive(true);
                    }
                }
            }
            //VRM Load枠
            else
            {
                textMesh_Page1[0].text = "VRM Load";
                textMesh_Page1[0].fontSize = textMesh_Page1[0].text.FontSizeMatch(600, 30, 50);

                //生成ボタンの表示
                if (timeline.FieldCharaCount < timeline.maxFieldChara) btn_VRMLoad.gameObject.SetActive(true);

                if (VRMOptionAnchor.gameObject.activeSelf) VRMOptionAnchor.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// アニメーションを変更する
        /// </summary>
        /// <param name="moveIndex"></param>
        private async UniTask ChangeAnime(int moveIndex)
        {
            await generatorPortal.SetAnimation(moveIndex);
            ChangeAnime_UI();
        }

        private void ChangeAnime_UI()
        {
            //表示更新
            textMesh_Page1[1].text = generatorPortal.GetNowAnimeInfo().viewName;
            textMesh_Page1[1].fontSize = textMesh_Page1[1].text.FontSizeMatch(600, 30, 50);

            //FBXモーション
            if (generatorPortal.GetNowAnimeInfo().formatType == DanceInfoData.FORMATTYPE.FBX)
            {
                //反転ボタンを表示
                if (!btn_Reverse.gameObject.activeSelf) btn_Reverse.gameObject.SetActive(true);

                //offsetの非表示
                if (offsetAnchor.gameObject.activeSelf) offsetAnchor.gameObject.SetActive(false);

                //offset更新
                slider_Offset.Value = 0;
                textMesh_Page1[3].text = $"{slider_Offset.Value:0000}";

                //LipSyncボタンを表示(今回は実装しない)
                //if (btn_jumpList[2].gameObject.activeSelf) btn_jumpList[2].gameObject.SetActive(false);
            }
            //VMDモーション
            else if (generatorPortal.GetNowAnimeInfo().formatType == DanceInfoData.FORMATTYPE.VMD)
            {
                //反転ボタンを消す
                if (btn_Reverse.gameObject.activeSelf) btn_Reverse.gameObject.SetActive(false);

                //offsetの表示
                if (!offsetAnchor.gameObject.activeSelf) offsetAnchor.gameObject.SetActive(true);

                //offset更新
                slider_Offset.Value = SystemInfo.dicVMD_offset[generatorPortal.GetNowAnimeInfo().viewName];
                textMesh_Page1[3].text = $"{slider_Offset.Value:0000}";

                //LipSyncボタンを表示(今回は実装しない)
                //if(!btn_jumpList[2].gameObject.activeSelf) btn_jumpList[2].gameObject.SetActive(true);
            }


            var bindChara = timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
            //モーフを有効化
            if (bindChara && bindChara.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                btn_FaceUpdate.isEnable = true;
                btn_MouthUpdate.isEnable = true;
            }
        }

        /// <summary>
        /// VMD用の口パクを変更
        /// </summary>
        /// <param name="moveIndex"></param>
        private void ChangeVMDLipSync(int moveIndex)
        {
            //文字画像を差し替える
            generatorPortal.SetAnimation(moveIndex).Forget();
            textMesh_Page1[4].text = generatorPortal.GetNowLipSyncName();
            textMesh_Page1[4].fontSize = textMesh_Page1[4].text.FontSizeMatch(600, 25, 40);

            //反映のために必要
            ChangeAnime(0).Forget();
        }

        /// <summary>
        /// オフセット値の微調整
        /// </summary>
        /// <param name="btn"></param>
        private void ChangeOffset_Anime(Button_Base btn)
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
                    timeline.SetVMD_MotionOffset(generatorPortal.GetNowAnimeInfo().viewName, (int)slider_Offset.Value);
                    textMesh_Page1[3].text = $"{slider_Offset.Value:0000}";

                    //クリック音
                    audioSource.PlayOneShot(Sound[0]);
                    break;
                }
            }

            SystemInfo.userProfile.SaveOffset();
        }

        /// <summary>
        /// 次反転アニメーションに切り替える
        /// </summary>
        /// <param name="btn"></param>
        private async UniTaskVoid ChangeReverse_Anime(Button_Base btn)
        {
            //btn_Reverse.Reverse();
            generatorPortal.isAnimationReverse = btn_Reverse.isEnable;

            //反転ボタンの状態に合わせる
            if (timeline.trackBindChara[TimelineController.PORTAL_ELEMENT])
            {
                //キャラを生成して反転を反映させる
                await generatorPortal.SetChara(0);
                textMesh_Page1[0].text = generatorPortal.GetNowAnimeInfo().viewName;
                textMesh_Page1[0].fontSize = textMesh_Page1[0].text.FontSizeMatch(600, 30, 50);

                //スライダーの値を反映
                var bindChara = timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
                bindChara.lookAtCon.inputWeight_Head = slider_HeadLook.Value;
                bindChara.lookAtCon.inputWeight_Eye = slider_EyeLook.Value;
            }
            //クリック音
            audioSource.PlayOneShot(Sound[0]);
        }

        /// <summary>
        /// VRMロードボタンが押下
        /// </summary>
        /// <param name="btn"></param>
        private void VRMLoad(Button_Base btn)
        {
            //ボタンを非表示にする
            btn_VRMLoad.gameObject.SetActive(false);

            //VRM選択画面を表示
            vrmSelectUI.InitPage(0);

            //クリック音
            audioSource.PlayOneShot(Sound[0]);
        }

        /// <summary>
        /// VRM設定用画面を開く
        /// </summary>
        /// <param name="btn"></param>
        private void VRMSetting(Button_Base btn)
        {
            var vrm = timeline.trackBindChara[TimelineController.PORTAL_ELEMENT];
            if (vrm && vrm.charaInfoData.formatType == CharaInfoData.FORMATTYPE.VRM)
            {
                //var instance = Instantiate(vrm.gameObject).GetComponent<CharaController>();

                //コピーをVRM設定画面に渡す
                vrmSelectUI.VRMEditing(vrm);
            }

            //クリック音
            audioSource.PlayOneShot(Sound[0]);
        }

        /// <summary>
        /// VRMキャラを削除
        /// </summary>
        /// <param name="btn"></param>
        private void DeleteModel(Button_Base btn)
        {
            if (btn == btn_VRMDelete && timeline.isPortalChara())
            {
                //VRMを削除する
                generatorPortal.DeleteCurrenVRM();
            }
            else if (btn == btn_DeleteAll)
            {
                //フィールド上を一掃
                timeline.DeletebindAsset_FieldAll();
            }

            ////Currentを生成
            //ChangeChara(0);

            //クリック音
            audioSource.PlayOneShot(Sound[0]);
        }

        /// <summary>
        /// 口パクボタンが押下
        /// </summary>
        /// <param name="btn"></param>
        private void Switch_Mouth(Button_Base btn)
        {
            if (btn == btn_FaceUpdate)
            {
                //口モーフの更新を切り替える
                timeline.SetMouthUpdate_Portal(true, btn_FaceUpdate.isEnable);
            }
            else if (btn = btn_MouthUpdate)
            {
                //口モーフの更新を切り替える
                timeline.SetMouthUpdate_Portal(false, btn_MouthUpdate.isEnable);
            }
            //クリック音
            audioSource.PlayOneShot(Sound[0]);
        }


        /// <summary>
        /// キャラを追加する
        /// </summary>
        private async UniTaskVoid Add_FieldChara()
        {
            if (currentPage == 0 && transform.root.gameObject.activeSelf)
            {
                //負荷が高いので削除処理とフレームをずらす
                await UniTask.Delay(250, cancellationToken: cancellation_token);

                //ポータルにキャラが存在していなければ生成しておく
                if (!timeline.isPortalChara()) await ChangeChara(0);

                //フィールド設置数のテキストセット
                textMesh_Page1[2].text = $"{timeline.FieldCharaCount}/{timeline.maxFieldChara}";
            }
        }


        /// <summary>
        /// 更新処理-1ページ目
        /// </summary>
        private void Page_Model()
        {

        }

        /// <summary>
        /// オーディオプレイヤーのクリック処理
        /// </summary>
        /// <param name="btn"></param>
        private void Click_AudioPlayer(Button_Base btn)
        {
            //クリック音
            audioSource.PlayOneShot(Sound[0]);

            //スライダー操作中は受け付けない
            if (playerStateManager.CheckGrabbing()) return;

            if (btn == btnS_Stop)
            {
                //マニュアル開始
                timeline.TimelineManualMode();
                //再生・停止ボタンの状態更新
                btnS_Stop.gameObject.SetActive(false);
                btnS_Play.gameObject.SetActive(true);
            }
            else if (btn == btnS_Play)
            {
                //タイムライン・再生
                timeline.TimelinePlay();
                //再生・停止ボタンの状態更新
                btnS_Stop.gameObject.SetActive(true);
                btnS_Play.gameObject.SetActive(false);
            }
            else if (btn == btnS_BaseReturn)
            {
                //タイムライン・初期化
                timeline.TimelineBaseReturn();
            }
        }

        /// <summary>
        /// オーディオファイルの読み込み
        /// </summary>
        /// <param name="btn"></param>
        private void Click_AudioLoad(Button_Base btn)
        {
            //クリック音
            audioSource.PlayOneShot(Sound[0]);

            //読み込みボタン
            if (btn == btnS_AudioLoad[0])
            {
                //重複防止で無効化しておく
                btnS_AudioLoad[0].gameObject.SetActive(false);
                var text = btnS_AudioLoad[1].transform.GetChild(0).GetChild(0).GetComponent<TextMesh>();
                if (SystemInfo.userProfile.data.LanguageCode == (int)USE_LANGUAGE.JP)
                {
                    text.text = $"{fileAccess.GetAudioFileCount()}件あります、よろしいですか?";
                }
                else
                {
                    text.text = $"There are {fileAccess.GetAudioFileCount()} files.Is it OK ? ";
                }
                //確認ボタンを有効化
                btnS_AudioLoad[1].gameObject.SetActive(true);
                //受付時間後にリセット
                StartCoroutine(ReceptionTime(5.0f));
            }
            //最終確認ボタン
            else if (btn == btnS_AudioLoad[1])
            {
                //コルーチンを止める
                StopCoroutine(ReceptionTime(0));
                //重複防止で無効化しておく
                btnS_AudioLoad[1].gameObject.SetActive(false);
                //読み込む
                StartCoroutine(LoadCheck());
            }
        }

        private IEnumerator ReceptionTime(float wait)
        {
            yield return new WaitForSeconds(wait);
            btnS_AudioLoad[1].gameObject.SetActive(false);
            btnS_AudioLoad[0].gameObject.SetActive(true);
        }

        private IEnumerator LoadCheck()
        {
            int moveIndex = fileAccess.presetCount - fileAccess.CurrentAudio;

            //完了街ち
            yield return StartCoroutine(fileAccess.AudioLoad());

            //読み込みの音楽の先頭にカレントを移動
            ChangeAuido(moveIndex);
        }


        /// <summary>
        /// 次オーディオに切り替える
        /// </summary>
        /// <param name="btn"></param>
        private void MoveIndex_Auido(Button_Base btn)
        {
            for (int i = 0; i < 2; i++)
            {
                //押されたボタンの判別
                if (btn_Audio[i] == btn)
                {
                    int moveIndex = 0;
                    if (i == 0) moveIndex = -1;
                    else if (i == 1) moveIndex = 1;

                    ChangeAuido(moveIndex);

                    //クリック音
                    audioSource.PlayOneShot(Sound[0]);
                    break;
                }
            }
        }

        /// <summary>
        /// オーディオを変更する
        /// </summary>
        /// <param name="moveIndex"></param>
        private void ChangeAuido(int moveIndex)
        {
            //文字画像を差し替える
            textMesh_Page2[0].text = timeline.NextAudioClip(moveIndex);
            //サイズ調整
            textMesh_Page2[0].fontSize = textMesh_Page2[0].text.FontSizeMatch(600, 30, 50);
            //オーディオの長さ
            float sec = timeline.GetNowAudioLength();
            slider_Playback.maxValuel = sec;
            textMesh_Page2[2].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
        }

        /// <summary>
        /// 更新処理-2ページ目
        /// </summary>
        private void Page_Sound()
        {
            //再生スライダー非制御中なら
            if (!slider_Playback.isControl)
            {
                //TimeLine再生時間をスライダーにセット
                float sec = (float)timeline.AudioClip_PlaybackTime;
                slider_Playback.Value = sec;
                //テキストに反映
                textMesh_Page2[1].text = $"{((int)sec / 60):00}:{((int)sec % 60):00}";
            }
        }

        /// <summary>
        /// セッティングのクリック処理
        /// </summary>
        /// <param name="i"></param>
        public void Click_Setting_Live(int i)
        {
            //クリック音
            audioSource.PlayOneShot(Sound[0]);
            //状態反転
            //btnE[i].Reverse();

            bool result = btnE[i].isEnable;
            switch (i)
            {
                //パーティクル
                case 0:
                    if (btnE_ActionParent[0])
                    {
                        btnE_ActionParent[0].gameObject.SetActive(result);
                        SystemInfo.userProfile.data.scene_crs_particle = result;
                    }
                    break;
                //レーザーガン
                case 1:
                    if (btnE_ActionParent[1])
                    {
                        btnE_ActionParent[1].gameObject.SetActive(result);
                        SystemInfo.userProfile.data.scene_crs_laser = result;
                    }
                    break;
                //ミラー
                case 2:
                    if (matMirrore)
                    {
                        matMirrore.SetFloat("_Smoothness", (result == true ? 1 : 0));
                        SystemInfo.userProfile.data.scene_crs_reflection = result;
                    }
                    break;
                //ソニックブーム
                case 3:
                    if (btnE_ActionParent[3])
                    {
                        btnE_ActionParent[3].gameObject.SetActive(result);
                        SystemInfo.userProfile.data.scene_crs_sonic = result;
                    }
                    break;
                //マニュアル
                case 4:
                    if (btnE_ActionParent[4])
                    {
                        btnE_ActionParent[4].gameObject.SetActive(result);
                        SystemInfo.userProfile.data.scene_crs_manual = result;
                    }
                    break;
            }

            //保存する
            SystemInfo.userProfile.WriteJson();
        }

        /// <summary>
        /// セッティングのクリック処理
        /// </summary>
        /// <param name="i"></param>
        public void Click_Setting_Viewer(int i)
        {
            //クリック音
            audioSource.PlayOneShot(Sound[0]);
            //状態反転
            //btnE[i].Reverse();

            bool result = btnE[i].isEnable;
            switch (i)
            {
                //LED
                case 0:
                    if (btnE_ActionParent[0])
                    {
                        btnE_ActionParent[0].gameObject.SetActive(result);
                        SystemInfo.userProfile.data.scene_view_led = result;

                        //保存する
                        SystemInfo.userProfile.WriteJson();
                    }
                    break;
                //SkyBoxを差し替える
                case 1:
                    if (backGroundCon)
                    {
                        string str;
                        backGroundCon.SetCubemap(1, out str);
                        btnE[i].collisionChecker.colorSetting[0].textMesh.text = "SkyBox_" + str;
                    }
                    break;
                //ワームホールを差し替える
                case 2:
                    if (backGroundCon)
                    {
                        string str;
                        backGroundCon.SetWormHole(1, out str);
                        btnE[i].collisionChecker.colorSetting[0].textMesh.text = "WormHole_" + str;
                    }
                    break;
                //エフェクトを差し替える
                case 3:
                    if (backGroundCon)
                    {
                        string str;
                        backGroundCon.SetParticle(1, out str);
                        btnE[i].collisionChecker.colorSetting[0].textMesh.text = "Particle_" + str;
                    }
                    break;
            }
        }

        /// <summary>
        /// セッティングのクリック処理
        /// </summary>
        /// <param name="i"></param>
        public void Click_Setting_KAGURA(int i)
        {
            //クリック音
            audioSource.PlayOneShot(Sound[0]);
            //状態反転
            //btnE[i].Reverse();

            bool result = btnE[i].isEnable;
            switch (i)
            {
                //パーティクル
                case 0:
                    if (btnE_ActionParent[0])
                    {
                        btnE_ActionParent[0].gameObject.SetActive(result);
                        SystemInfo.userProfile.data.scene_kagura_particle = result;
                    }
                    break;
                //リフレクション
                case 1:
                    if (btnE_ActionParent[1])
                    {
                        btnE_ActionParent[1].gameObject.SetActive(result);
                        SystemInfo.userProfile.data.scene_kagura_reflection = result;
                    }
                    break;
                //海切り替えテスト
                case 2:
                    if (btnE_ActionParent[2])
                    {
                        if (btnE_ActionParent[2].GetChild(0).gameObject.activeSelf)
                        {
                            btnE_ActionParent[2].GetChild(0).gameObject.SetActive(false);
                            btnE_ActionParent[2].GetChild(1).gameObject.SetActive(true);
                        }
                        else if (btnE_ActionParent[2].GetChild(1).gameObject.activeSelf)
                        {
                            btnE_ActionParent[2].GetChild(1).gameObject.SetActive(false);
                            btnE_ActionParent[2].GetChild(0).gameObject.SetActive(true);
                        }
                        SystemInfo.userProfile.data.scene_kagura_sea = result;
                    }
                    break;
            }
            //保存する
            SystemInfo.userProfile.WriteJson();
        }

        public void Click_SceneChange(int i)
        {
            //クリック音
            audioSource.PlayOneShot(Sound[0]);

            if (!btnE_SecenChange[i]) return;

            string[] str = new string[] { "LiveScene", "KAGURAScene", "ViewerScene" };
            StartCoroutine(SceneChange(str[i]));
        }

        /// <summary>
        /// 更新処理-3ページ目
        /// </summary>
        private void Page_Effect()
        {

        }

        /// <summary>
        /// アウトライン
        /// </summary>
        private void Update_OutLine()
        {
            if (slider_OutLine.Value > 0)
            {
                //有効化
                outlineRender.SetActive(true);
                //値の更新
                material_OutLine.SetFloat("_Edge", slider_OutLine.Value);
            }
            else
            {
                //無効化
                outlineRender.SetActive(false);
            }
        }

        /// <summary>
        /// 更新中
        /// </summary>
        private void Update_InitCharaSize()
        {
            //textMesh_Page3[0].text = slider_InitCharaSize.Value.ToString("0.00");
            textMesh_Page3[0].text = $"{slider_InitCharaSize.Value:0.00}";
        }


        /// <summary>
        /// 固定中心窩レンダリングのスライダー
        /// </summary>
        private void Update_FixedFoveated()
        {
            //スライダーに反映
            slider_FixedFoveated.Value = Mathf.Clamp(slider_FixedFoveated.Value, 2, 4);
#if UNITY_EDITOR
            //textMesh_Page3[1].text = "noQuest:" + slider_FixedFoveated.Value;
            textMesh_Page3[1].text = $"noQuest:{slider_FixedFoveated.Value}";
#elif UNITY_ANDROID
        //反映し直す
        OVRManager.fixedFoveatedRenderingLevel = (OVRManager.FixedFoveatedRenderingLevel)slider_FixedFoveated.Value;
        //テキストに反映
        textMesh_Page3[1].text = Enum.GetName(typeof(OVRManager.FixedFoveatedRenderingLevel),OVRManager.fixedFoveatedRenderingLevel);
#endif
        }

        /// <summary>
        /// アイテム変更ボタン
        /// </summary>
        /// <param name="btn"></param>
        private void MoveIndex_Item(Button_Base btn)
        {
            if (!btn) return;
            for (int i = 0; i < 2; i++)
            {
                //押されたボタンの判別
                if (btn_Item[i] == btn)
                {
                    int moveIndex = 0;
                    if (i == 0) moveIndex = -1;
                    else if (i == 1) moveIndex = 1;

                    ChangeItem(moveIndex);

                    //クリック音
                    audioSource.PlayOneShot(Sound[0]);
                    break;
                }
            }
        }

        /// <summary>
        /// アイテムを変更する
        /// TODO:ちゃんと作り直す
        /// </summary>
        /// <param name="btn"></param>
        private void ChangeItem(int moveIndex)
        {
            currentItem += moveIndex;

            if (currentItem < 0) currentItem = ItemPrefab.Length - 1;
            else if (currentItem >= ItemPrefab.Length) currentItem = 0;

            //アイテムが残っていれば削除
            if (itemGeneratAnchor.childCount > 0)
            {
                int max = itemGeneratAnchor.childCount - 1;
                for (int j = 0; j <= max; j++)
                {
                    //常に先頭を削除
                    Destroy(itemGeneratAnchor.GetChild(max - j).gameObject);
                }
            }
            //アイテム用マテリアルが残っていれば削除
            if (itemMaterialAnchor.childCount > 0)
            {
                int max = itemMaterialAnchor.childCount - 1;
                for (int j = 0; j <= max; j++)
                {
                    //後ろから削除
                    Destroy(itemMaterialAnchor.GetChild(max - j).GetComponent<Renderer>().material);//これよくないなぁ
                    Destroy(itemMaterialAnchor.GetChild(max - j).gameObject);
                }
            }

            //生成
            var newItem = Instantiate(ItemPrefab[currentItem].gameObject, transform.position, Quaternion.identity).transform;
            newItem.parent = itemGeneratAnchor;
            newItem.localPosition = Vector3.zero;
            newItem.localRotation = Quaternion.identity;

            var itemInfo = newItem.GetComponent<DecorationItemInfo>();
            if (itemInfo.texs.Length > 0)
            {
                //マテリアルオブジェクトの生成
                for (int j = 0; j < itemInfo.texs.Length; j++)
                {
                    var mat = Instantiate(itemMaterialPrefab, transform.position, Quaternion.identity).transform;
                    mat.GetComponent<MeshRenderer>().materials[0].SetTexture("_BaseMap", itemInfo.texs[j]);
                    mat.parent = itemMaterialAnchor;
                    mat.localPosition = new Vector3(-5, 2.5f - j, 0);
                    mat.localRotation = Quaternion.identity;
                }
            }

            //名前を表示
            if (SystemInfo.userProfile.data.LanguageCode == (int)USE_LANGUAGE.JP) textMesh_Page4.text = itemInfo.itemName[1];
            else textMesh_Page4.text = itemInfo.itemName[0];

        }

        /// <summary>
        /// 更新処理-2ページ目
        /// </summary>
        private void Page_Item()
        {

        }

        private IEnumerator DelaySoundPlay(float delay)
        {
            yield return new WaitForSeconds(delay);
            //ボタンの揺れ音
            audioSource.PlayOneShot(Sound[2]);
        }

        private void Director_Played(PlayableDirector obj)
        {
            //停止表示
            //btnS_Stop.gameObject.SetActive(true);
            //btnS_Play.gameObject.SetActive(false);
        }
        private void Director_Stoped(PlayableDirector obj)
        {
            //再生途中の一時停止は無視する
            if (timeline.AudioClip_PlaybackTime <= 0)
            {
                //再生表示
                if (btnS_Stop) btnS_Stop.gameObject.SetActive(false);
                if (btnS_Play) btnS_Play.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// シーン遷移処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator SceneChange(string sceneName)
        {
            yield return new WaitForSeconds(0.1f);
            onSceneSwitch?.Invoke(sceneName);
            yield return new WaitForSeconds(0.4f);//音の分だけ待つ
                            
            //音が割れるので止める
            timeline.TimelineManualMode();
        }

        private void ManualStart()
        {
            //マニュアルモードにする
            timeline.TimelineManualMode();

            //ボタンの状態を制御
            btnS_Stop.gameObject.SetActive(false);
            btnS_Play.gameObject.SetActive(true);
        }
    }
}