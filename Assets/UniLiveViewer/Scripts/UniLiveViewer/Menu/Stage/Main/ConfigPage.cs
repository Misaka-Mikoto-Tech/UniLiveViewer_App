using Cysharp.Threading.Tasks;
using System.Threading;
using UniLiveViewer.Player;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Stage;
using UniLiveViewer.Timeline;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu
{
    public class ConfigPage : MonoBehaviour
    {
        public static bool isSmoothVMD = false;
        [SerializeField] private MenuManager menuManager;

        [Header("＜シーン別＞")]
        [SerializeField] Transform[] sceneAnchor;
        Button_Base[] btnE = new Button_Base[5];
        [SerializeField] Transform[] btnE_ActionParent;

        [Header("＜KAGURALive専用＞")]
        [SerializeField] SliderGrabController slider_Fog = null;

        [Header("＜ViewerScene専用＞")]
        [SerializeField] TextMesh[] textMeshs_Viewer = new TextMesh[4];

        [Header("＜Gym専用＞")]
        [SerializeField] TextMesh[] textMeshs_Gym = new TextMesh[1];

        [Header("＜共用＞")]
        [SerializeField] Button_Base[] btn_General = null;
        [SerializeField] Button_Switch[] btnE_SecenChange = new Button_Switch[4];
        [SerializeField] TextMesh[] textMeshs = new TextMesh[5];
        [SerializeField] SliderGrabController slider_OutLine;
        [SerializeField] SliderGrabController slider_InitCharaSize;
        [SerializeField] SliderGrabController slider_CharaShadow;
        [SerializeField] SliderGrabController slider_VMDScale;
        [SerializeField] SliderGrabController slider_FixedFoveated;
        [Space(10)]
        [SerializeField] ScriptableRendererFeature outlineRender;
        [SerializeField] Material material_OutLine;
        [SerializeField] UniversalRendererData frd;

        TimelineController _timeline;
        QuasiShadowSetting _quasiShadowSetting;
        PassthroughService _passthroughService;

        Material _matMirrore;//LiveScene用
        BackGroundController _backGroundCon;
        StageLightManager _stageLightManager;
        CancellationToken _cancellation;

        void Awake()
        {
            var container = LifetimeScope.Find<TimelineLifetimeScope>().Container;
            _timeline = container.Resolve<TimelineController>();
            _quasiShadowSetting = container.Resolve<QuasiShadowSetting>();
            _cancellation = this.GetCancellationTokenOnDestroy();

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
                FileReadAndWriteUtility.UserProfile.InitCharaSize = float.Parse(slider_InitCharaSize.Value.ToString("f2"));
                FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            };
            slider_CharaShadow.ValueUpdate += Update_CharaShadow;
            slider_CharaShadow.UnControled += () =>
            {
                FileReadAndWriteUtility.UserProfile.CharaShadow = float.Parse(slider_CharaShadow.Value.ToString("f2"));
                FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            };
            slider_VMDScale.ValueUpdate += Update_VMDScale;
            slider_VMDScale.UnControled += () =>
            {
                FileReadAndWriteUtility.UserProfile.VMDScale = float.Parse(slider_VMDScale.Value.ToString("f3"));
                FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            };
            slider_FixedFoveated.ValueUpdate += Update_FixedFoveated;
            slider_Fog.ValueUpdate += () => { RenderSettings.fogDensity = slider_Fog.Value; };

            for (int i = 0; i < btn_General.Length; i++)
            {
                btn_General[i].onTrigger += Click_Action;
            }
            btn_General[0].isEnable = isSmoothVMD;//スムースは毎回無効化
        }
        void OnEnable()
        {
            Init().Forget();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Title分を除外で-1
            var current = (int)SceneChangeService.GetSceneType - 1;

            // TODO: UI作り直す時にまともにする
            var player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerLifetimeScope>();
            _passthroughService = player.Container.Resolve<PassthroughService>();

            //シーン応じて有効化を切り替える
            for (int i = 0; i < sceneAnchor.Length; i++)
            {
                if (i == current)
                {
                    if (!sceneAnchor[i].gameObject.activeSelf) sceneAnchor[i].gameObject.SetActive(true);
                }
                else if (sceneAnchor[i].gameObject.activeSelf)
                {
                    sceneAnchor[i].gameObject.SetActive(false);
                }
            }

            if (SceneChangeService.GetSceneType == SceneType.CANDY_LIVE)
            {
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 5; i++)
                {
                    btnE[i] = sceneAnchor[current].GetChild(i).GetComponent<Button_Base>();
                }

                btnE_ActionParent = new Transform[5];

                btnE_ActionParent[0] = GameObject.FindGameObjectWithTag("Particle").transform;
                btnE_ActionParent[1] = GameObject.FindGameObjectWithTag("LaserGun").transform;
                btnE_ActionParent[2] = GameObject.FindGameObjectWithTag("FloorMirror").transform;
                btnE_ActionParent[3] = GameObject.FindGameObjectWithTag("SonicBoom").transform;
                btnE_ActionParent[4] = GameObject.FindGameObjectWithTag("ManualUI").transform;

                _matMirrore = btnE_ActionParent[2].GetComponent<MeshRenderer>().material;

                btnE_ActionParent[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_particle);
                btnE_ActionParent[1].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_laser);
                _matMirrore.SetFloat("_Smoothness", FileReadAndWriteUtility.UserProfile.scene_crs_reflection ? 1 : 0);
                btnE_ActionParent[3].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_sonic);
                btnE_ActionParent[4].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_crs_manual);
            }
            else if (SceneChangeService.GetSceneType == SceneType.KAGURA_LIVE)
            {
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 3; i++)
                {
                    btnE[i] = sceneAnchor[current].GetChild(i).GetComponent<Button_Base>();
                }

                btnE_ActionParent = new Transform[3];

                btnE_ActionParent[0] = GameObject.FindGameObjectWithTag("Particle").transform;
                btnE_ActionParent[1] = GameObject.FindGameObjectWithTag("ReflectionProbe").transform;
                btnE_ActionParent[2] = GameObject.FindGameObjectWithTag("WaterAnchor").transform;

                btnE_ActionParent[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_particle);
                btnE_ActionParent[1].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_sea);
                btnE_ActionParent[2].transform.GetChild(0).gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_kagura_reflection);
            }
            else if (SceneChangeService.GetSceneType == SceneType.VIEWER)
            {
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 1; i++)
                {
                    btnE[i] = sceneAnchor[current].GetChild(i).GetComponent<Button_Base>();
                }

                btnE_ActionParent = new Transform[1];

                btnE_ActionParent[0] = GameObject.FindGameObjectWithTag("FloorLED").transform;
                _backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();

                btnE_ActionParent[0].gameObject.SetActive(FileReadAndWriteUtility.UserProfile.scene_view_led);
            }
            else if (SceneChangeService.GetSceneType == SceneType.GYMNASIUM)
            {
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 2; i++)
                {
                    btnE[i] = sceneAnchor[current].GetChild(i).GetComponent<Button_Base>();
                }
                _stageLightManager = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<StageLightManager>();
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
        }

        async UniTaskVoid Init()
        {
            await UniTask.Yield(PlayerLoopTiming.Update, _cancellation);//一応

            //sceneボタン初期化
            foreach (var e in btnE_SecenChange)
            {
                e.isEnable = false;
            }
            var type = SceneChangeService.GetSceneType;
            if (type == SceneType.CANDY_LIVE)
            {
                //各種有効化状態にボタンを合わせる
                btnE[0].isEnable = btnE_ActionParent[0].gameObject.activeSelf;
                btnE[1].isEnable = btnE_ActionParent[1].gameObject.activeSelf;
                btnE[3].isEnable = btnE_ActionParent[3].gameObject.activeSelf;
                btnE[4].isEnable = btnE_ActionParent[4].gameObject.activeSelf;

                //反射状態に合わせる
                btnE[2].isEnable = (_matMirrore.GetFloat("_Smoothness") == 1.0f ? true : false);
            }
            else if (type == SceneType.KAGURA_LIVE)
            {
                //各種有効化状態にボタンを合わせる
                btnE[0].isEnable = btnE_ActionParent[0].gameObject.activeSelf;
                btnE[1].isEnable = btnE_ActionParent[1].gameObject.activeSelf;
                btnE[2].isEnable = btnE_ActionParent[2].transform.GetChild(0).gameObject.activeSelf;
            }
            else if (type == SceneType.VIEWER)
            {
                //各種有効化状態にボタンを合わせる
                btnE[0].isEnable = btnE_ActionParent[0].gameObject.activeSelf;
            }
            else if (type == SceneType.VIEWER)
            {
                //各種有効化状態にボタンを合わせる
                btnE[0].isEnable = btnE_ActionParent[0].gameObject.activeSelf;
            }
            else if (type == SceneType.GYMNASIUM)
            {
                //各種有効化状態にボタンを合わせる
                btnE[0].isEnable = FileReadAndWriteUtility.UserProfile.scene_gym_whitelight;
                btnE[1].isEnable = FileReadAndWriteUtility.UserProfile.StepSE;

                Click_Setting_Gym(0);
            }

            //共用
            btn_General[1].isEnable = _passthroughService.IsInsightPassthroughEnabled();
            btn_General[2].isEnable = FileReadAndWriteUtility.UserProfile.TouchVibration;

            //キャラサイズ
            slider_InitCharaSize.Value = FileReadAndWriteUtility.UserProfile.InitCharaSize;
            Update_InitCharaSize();

            //キャラ影
            slider_CharaShadow.Value = _quasiShadowSetting.ShadowScale;
            Update_CharaShadow();

            textMeshs[3].text = $"FootShadow:\n{_quasiShadowSetting.ShadowType}";

            //VMD拡縮
            slider_VMDScale.Value = FileReadAndWriteUtility.UserProfile.VMDScale;
            Update_VMDScale();
            //固定中心窩レンダリング初期化
            Update_FixedFoveated();
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }

        void Click_Action(Button_Base btn)
        {
            //スムース
            if (btn == btn_General[0])
            {
                isSmoothVMD = btn.isEnable;
            }
            //パススルー
            else if (btn == btn_General[1])
            {
                _passthroughService.Switching(btn.isEnable);
            }
            //コントローラー振動
            else if (btn == btn_General[2])
            {
                FileReadAndWriteUtility.UserProfile.TouchVibration = btn.isEnable;
                FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            }
            //キャラ影デクリ
            else if (btn == btn_General[3])
            {
                _quasiShadowSetting.ShadowType -= 1;
                textMeshs[3].text = $"FootShadow:\n{_quasiShadowSetting.ShadowType}";
                FileReadAndWriteUtility.UserProfile.CharaShadowType = (int)_quasiShadowSetting.ShadowType;
                FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            }
            //キャラ影インクリ
            else if (btn == btn_General[4])
            {
                _quasiShadowSetting.ShadowType += 1;
                textMeshs[3].text = $"FootShadow:\n{_quasiShadowSetting.ShadowType}";
                FileReadAndWriteUtility.UserProfile.CharaShadowType = (int)_quasiShadowSetting.ShadowType;
                FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
            }

            menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// セッティングのクリック処理
        /// </summary>
        /// <param name="i"></param>
        public void Click_Setting_Live(int i)
        {
            menuManager.PlayOneShot(SoundType.BTN_CLICK);

            bool result = btnE[i].isEnable;
            switch (i)
            {
                //パーティクル
                case 0:
                    if (btnE_ActionParent[0])
                    {
                        btnE_ActionParent[0].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_crs_particle = result;
                    }
                    break;
                //レーザーガン
                case 1:
                    if (btnE_ActionParent[1])
                    {
                        btnE_ActionParent[1].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_crs_laser = result;
                    }
                    break;
                //ミラー
                case 2:
                    if (_matMirrore)
                    {
                        _matMirrore.SetFloat("_Smoothness", (result == true ? 1 : 0));
                        FileReadAndWriteUtility.UserProfile.scene_crs_reflection = result;
                    }
                    break;
                //ソニックブーム
                case 3:
                    if (btnE_ActionParent[3])
                    {
                        btnE_ActionParent[3].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_crs_sonic = result;
                    }
                    break;
                //マニュアル
                case 4:
                    if (btnE_ActionParent[4])
                    {
                        btnE_ActionParent[4].gameObject.SetActive(result);
                        btnE_ActionParent[4].GetComponent<ManualSwitch>().SetEnable(result);
                    }
                    break;
            }

            //保存する
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        /// <summary>
        /// セッティングのクリック処理
        /// </summary>
        /// <param name="i"></param>
        public void Click_Setting_KAGURA(int i)
        {
            menuManager.PlayOneShot(SoundType.BTN_CLICK);

            bool result = btnE[i].isEnable;
            switch (i)
            {
                //パーティクル
                case 0:
                    if (btnE_ActionParent[0])
                    {
                        btnE_ActionParent[0].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_kagura_particle = result;
                    }
                    break;
                //リフレクション
                case 1:
                    if (btnE_ActionParent[1])
                    {
                        btnE_ActionParent[1].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_kagura_reflection = result;
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
                        FileReadAndWriteUtility.UserProfile.scene_kagura_sea = result;
                    }
                    break;
            }
            //保存する
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
        }

        /// <summary>
        /// セッティングのクリック処理
        /// </summary>
        /// <param name="i"></param>
        public void Click_Setting_Viewer(int i)
        {
            menuManager.PlayOneShot(SoundType.BTN_CLICK);

            bool result = btnE[i].isEnable;
            switch (i)
            {
                //LED
                case 0:
                    if (btnE_ActionParent[0])
                    {
                        btnE_ActionParent[0].gameObject.SetActive(result);
                        FileReadAndWriteUtility.UserProfile.scene_view_led = result;

                        //保存する
                        FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);
                    }
                    break;
            }
        }

        public void Click_ChangeEffect(int i)
        {
            if (!_backGroundCon) return;
            string str;

            switch (i)
            {
                case 0:
                    _backGroundCon.SetCubemap(-1, out str);
                    textMeshs_Viewer[0].text = "SkyBox_" + str;
                    break;
                case 1:
                    _backGroundCon.SetCubemap(1, out str);
                    textMeshs_Viewer[0].text = "SkyBox_" + str;
                    break;
                case 2:
                    _backGroundCon.SetWormHole(-1, out str);
                    textMeshs_Viewer[1].text = "WormHole_" + str;
                    break;
                case 3:
                    _backGroundCon.SetWormHole(1, out str);
                    textMeshs_Viewer[1].text = "WormHole_" + str;
                    break;
                case 4:
                    _backGroundCon.SetParticle(-1, out str);
                    textMeshs_Viewer[2].text = "Particle_" + str;
                    break;
                case 5:
                    _backGroundCon.SetParticle(1, out str);
                    textMeshs_Viewer[2].text = "Particle_" + str;
                    break;
            }

            menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        public void Click_ChangeStageLight(int i)
        {
            if (!_stageLightManager) return;
            string str;

            switch (i)
            {
                case 0:
                    _stageLightManager.SetStageLight(-1, btnE[0].isEnable, out str);
                    textMeshs_Gym[0].text = "SpotLight_" + str;
                    break;
                case 1:
                    _stageLightManager.SetStageLight(1, btnE[0].isEnable, out str);
                    textMeshs_Gym[0].text = "SpotLight_" + str;
                    break;
            }

            menuManager.PlayOneShot(SoundType.BTN_CLICK_LIGHT);
        }

        public void Click_Setting_Gym(int i)
        {
            switch (i)
            {
                case 0:
                    _stageLightManager.SetLightColor(btnE[0].isEnable);
                    FileReadAndWriteUtility.UserProfile.scene_gym_whitelight = btnE[0].isEnable;
                    break;
                case 1:
                    _quasiShadowSetting.SetStepSE(btnE[1].isEnable);
                    FileReadAndWriteUtility.UserProfile.StepSE = btnE[1].isEnable;
                    break;
            }

            //保存する
            FileReadAndWriteUtility.WriteJson(FileReadAndWriteUtility.UserProfile);

            menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        public void Click_SceneChange(int i)
        {
            menuManager.PlayOneShot(SoundType.BTN_CLICK);

            if (!btnE_SecenChange[i]) return;

            string[] str = new string[] { "LiveScene", "KAGURAScene", "ViewerScene", "GymnasiumScene" };
            SceneChange(str[i]).Forget();
        }

        /// <summary>
        /// シーン遷移処理
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        async UniTask SceneChange(string sceneName)
        {
            await UniTask.Delay(100, cancellationToken: _cancellation);

            BlackoutCurtain.instance.StartBlackout(sceneName).Forget();
            await UniTask.Delay(200, cancellationToken: _cancellation);

            //音が割れるので止める
            _timeline.TimelineManualMode().Forget();
            await UniTask.Delay(200, cancellationToken: _cancellation);

            //UIが透けて見えるので隠す
            menuManager.gameObject.SetActive(false);
        }

        /// <summary>
        /// キャラ初期サイズ
        /// </summary>
        void Update_InitCharaSize()
        {
            textMeshs[0].text = $"{slider_InitCharaSize.Value:0.00}";
        }

        /// <summary>
        /// キャラの影サイズ
        /// </summary>
        void Update_CharaShadow()
        {
            _quasiShadowSetting.SetShadowScale(slider_CharaShadow.Value);
            textMeshs[4].text = $"{slider_CharaShadow.Value:0.00}";
        }

        /// <summary>
        /// VMD範囲
        /// </summary>
        void Update_VMDScale()
        {
            slider_VMDScale.Value = Mathf.Clamp(slider_VMDScale.Value, 0.3f, 1.0f);
            textMeshs[1].text = $"{slider_VMDScale.Value:0.000}";
        }

        /// <summary>
        /// 固定中心窩レンダリングのスライダー
        /// </summary>
        void Update_FixedFoveated()
        {
            slider_FixedFoveated.Value = Mathf.Clamp(slider_FixedFoveated.Value, 2, 4);
#if UNITY_EDITOR
            textMeshs[2].text = $"noQuest:{slider_FixedFoveated.Value}";
#elif UNITY_ANDROID
            //反映し直す
            OVRManager.fixedFoveatedRenderingLevel = (OVRManager.FixedFoveatedRenderingLevel)slider_FixedFoveated.Value;
            textMeshs[2].text = Enum.GetName(typeof(OVRManager.FixedFoveatedRenderingLevel),OVRManager.fixedFoveatedRenderingLevel);
#endif
        }
        void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I)) Click_Setting_Viewer(2);//ワームホール
            if (Input.GetKeyDown(KeyCode.K)) Click_Setting_Viewer(3);//パーティクル
        }
    }
}