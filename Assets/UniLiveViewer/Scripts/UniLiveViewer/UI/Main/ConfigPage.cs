using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UniLiveViewer
{
    public class ConfigPage : MonoBehaviour
    {
        public static bool isSmoothVMD = false;
        [SerializeField] private MenuManager menuManager;

        [Header("＜マニュアル＞")]
        [SerializeField] private Sprite[] sprManualPrefab = new Sprite[4];
        [SerializeField] private SpriteRenderer[] sprManual = new SpriteRenderer[2];

        [Header("＜シーン別＞")]
        [SerializeField] private Transform[] sceneAnchor;
        private Button_Base[] btnE = new Button_Base[5];
        [SerializeField] private Transform[] btnE_ActionParent;

        [Header("＜KAGURALive専用＞")]
        [SerializeField] private SliderGrabController slider_Fog = null;

        [Header("＜ViewerScene専用＞")]
        [SerializeField] private TextMesh[] textMeshs_Viewer = new TextMesh[4];

        [Header("＜共用＞")]
        [SerializeField] private Button_Base[] btn_General = null;
        [SerializeField] private Button_Switch[] btnE_SecenChange = new Button_Switch[3];
        [SerializeField] private TextMesh[] textMeshs = new TextMesh[5];
        [SerializeField] private SliderGrabController slider_OutLine;
        [SerializeField] private SliderGrabController slider_InitCharaSize;
        [SerializeField] private SliderGrabController slider_CharaShadow;
        [SerializeField] private SliderGrabController slider_VMDScale;
        [SerializeField] private SliderGrabController slider_FixedFoveated;
        [Space(10)]
        [SerializeField] private ScriptableRendererFeature outlineRender;
        [SerializeField] private Material material_OutLine;
        [SerializeField] private UniversalRendererData frd;

        private TimelineController timeline = null;
        private QuasiShadow quasiShadow;
        private PlayerStateManager playerStateManager;

        private Material matMirrore;//LiveScene用
        private BackGroundController backGroundCon;
        private CancellationToken cancellation_token;

        private void Awake()
        {
            timeline = menuManager.timeline;
            quasiShadow = timeline.GetComponent<QuasiShadow>();
            cancellation_token = this.GetCancellationTokenOnDestroy();

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
                SystemInfo.userProfile.InitCharaSize = float.Parse(slider_InitCharaSize.Value.ToString("f2"));
                FileAccessManager.WriteJson(SystemInfo.userProfile);
            };
            slider_CharaShadow.ValueUpdate += Update_CharaShadow;
            slider_CharaShadow.UnControled += () =>
            {
                SystemInfo.userProfile.CharaShadow = float.Parse(slider_CharaShadow.Value.ToString("f2"));
                FileAccessManager.WriteJson(SystemInfo.userProfile);
            };
            slider_VMDScale.ValueUpdate += Update_VMDScale;
            slider_VMDScale.UnControled += () =>
            {
                SystemInfo.userProfile.VMDScale = float.Parse(slider_VMDScale.Value.ToString("f3"));
                FileAccessManager.WriteJson(SystemInfo.userProfile);
            };
            slider_FixedFoveated.ValueUpdate += Update_FixedFoveated;
            slider_Fog.ValueUpdate += () => { RenderSettings.fogDensity = slider_Fog.Value; };

            for (int i = 0; i < btn_General.Length; i++)
            {
                btn_General[i].onTrigger += Click_Action;
            }
            btn_General[0].isEnable = isSmoothVMD;//スムースは毎回無効化
        }
        private void OnEnable()
        {
            if(!playerStateManager) playerStateManager = PlayerStateManager.instance;

            Init().Forget();
        }

        // Start is called before the first frame update
        void Start()
        {
            int current = (int)SystemInfo.sceneMode;
            
            //シーン応じて有効化を切り替える
            for (int i = 0; i < sceneAnchor.Length; i++)
            {
                if (i == current && !sceneAnchor[i].gameObject.activeSelf) sceneAnchor[i].gameObject.SetActive(true);
                else if (sceneAnchor[i].gameObject.activeSelf) sceneAnchor[i].gameObject.SetActive(false);
            }

            if (SystemInfo.sceneMode == SceneMode.CANDY_LIVE)
            {
                var manualAnchor = GameObject.FindGameObjectWithTag("ManualUI").transform;
                sprManual[0] = manualAnchor.GetChild(0).GetComponent<SpriteRenderer>();
                sprManual[1] = manualAnchor.GetChild(1).GetComponent<SpriteRenderer>();

                if (SystemInfo.userProfile.LanguageCode == (int)USE_LANGUAGE.JP)
                {
                    sprManual[0].sprite = sprManualPrefab[1];
                    sprManual[1].sprite = sprManualPrefab[3];
                }
                else
                {
                    sprManual[0].sprite = sprManualPrefab[0];
                    sprManual[1].sprite = sprManualPrefab[2];
                }

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

                matMirrore = btnE_ActionParent[2].GetComponent<MeshRenderer>().material;

                btnE_ActionParent[0].gameObject.SetActive(SystemInfo.userProfile.scene_crs_particle);
                btnE_ActionParent[1].gameObject.SetActive(SystemInfo.userProfile.scene_crs_laser);
                matMirrore.SetFloat("_Smoothness", SystemInfo.userProfile.scene_crs_reflection ? 1 : 0);
                btnE_ActionParent[3].gameObject.SetActive(SystemInfo.userProfile.scene_crs_sonic);
                btnE_ActionParent[4].gameObject.SetActive(SystemInfo.userProfile.scene_crs_manual);
            }
            else if (SystemInfo.sceneMode == SceneMode.KAGURA_LIVE)
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

                btnE_ActionParent[0].gameObject.SetActive(SystemInfo.userProfile.scene_kagura_particle);
                btnE_ActionParent[1].gameObject.SetActive(SystemInfo.userProfile.scene_kagura_sea);
                btnE_ActionParent[2].transform.GetChild(0).gameObject.SetActive(SystemInfo.userProfile.scene_kagura_reflection);
            }
            else if (SystemInfo.sceneMode == SceneMode.VIEWER)
            {
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 1; i++)
                {
                    btnE[i] = sceneAnchor[current].GetChild(i).GetComponent<Button_Base>();
                }

                btnE_ActionParent = new Transform[1];

                btnE_ActionParent[0] = GameObject.FindGameObjectWithTag("FloorLED").transform;
                backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();

                btnE_ActionParent[0].gameObject.SetActive(SystemInfo.userProfile.scene_view_led);
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

        private async UniTaskVoid Init()
        {
            await UniTask.Yield(PlayerLoopTiming.Update,cancellation_token);//一応

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

            //共用
            btn_General[1].isEnable = playerStateManager.myOVRManager.isInsightPassthroughEnabled;
            btn_General[2].isEnable = SystemInfo.userProfile.TouchVibration;

            //キャラサイズ
            slider_InitCharaSize.Value = SystemInfo.userProfile.InitCharaSize;
            Update_InitCharaSize();

            //キャラ影
            slider_CharaShadow.Value = quasiShadow.shadowScale;
            Update_CharaShadow();

            textMeshs[3].text = $"FootShadow:\n{quasiShadow.ShadowType}";

            //VMD拡縮
            slider_VMDScale.Value = SystemInfo.userProfile.VMDScale;
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

        private void Click_Action(Button_Base btn)
        {
            //スムース
            if (btn == btn_General[0])
            {
                isSmoothVMD = btn.isEnable;
            }
            //パススルー
            else if (btn == btn_General[1])
            {
                playerStateManager.EnablePassthrough(btn.isEnable);
            }
            //コントローラー振動
            else if (btn == btn_General[2])
            {
                SystemInfo.userProfile.TouchVibration = btn.isEnable;
                FileAccessManager.WriteJson(SystemInfo.userProfile);
            }
            //キャラ影デクリ
            else if (btn == btn_General[3])
            {
                quasiShadow.ShadowType -= 1;
                textMeshs[3].text = $"FootShadow:\n{quasiShadow.ShadowType}";
                SystemInfo.userProfile.CharaShadowType = (int)quasiShadow.ShadowType;
                FileAccessManager.WriteJson(SystemInfo.userProfile);
            }
            //キャラ影インクリ
            else if (btn == btn_General[4])
            {
                quasiShadow.ShadowType += 1;
                textMeshs[3].text = $"FootShadow:\n{quasiShadow.ShadowType}";
                SystemInfo.userProfile.CharaShadowType = (int)quasiShadow.ShadowType;
                FileAccessManager.WriteJson(SystemInfo.userProfile);
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
                        SystemInfo.userProfile.scene_crs_particle = result;
                    }
                    break;
                //レーザーガン
                case 1:
                    if (btnE_ActionParent[1])
                    {
                        btnE_ActionParent[1].gameObject.SetActive(result);
                        SystemInfo.userProfile.scene_crs_laser = result;
                    }
                    break;
                //ミラー
                case 2:
                    if (matMirrore)
                    {
                        matMirrore.SetFloat("_Smoothness", (result == true ? 1 : 0));
                        SystemInfo.userProfile.scene_crs_reflection = result;
                    }
                    break;
                //ソニックブーム
                case 3:
                    if (btnE_ActionParent[3])
                    {
                        btnE_ActionParent[3].gameObject.SetActive(result);
                        SystemInfo.userProfile.scene_crs_sonic = result;
                    }
                    break;
                //マニュアル
                case 4:
                    if (btnE_ActionParent[4])
                    {
                        btnE_ActionParent[4].gameObject.SetActive(result);
                        SystemInfo.userProfile.scene_crs_manual = result;
                    }
                    break;
            }

            //保存する
            FileAccessManager.WriteJson(SystemInfo.userProfile);
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
                        SystemInfo.userProfile.scene_kagura_particle = result;
                    }
                    break;
                //リフレクション
                case 1:
                    if (btnE_ActionParent[1])
                    {
                        btnE_ActionParent[1].gameObject.SetActive(result);
                        SystemInfo.userProfile.scene_kagura_reflection = result;
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
                        SystemInfo.userProfile.scene_kagura_sea = result;
                    }
                    break;
            }
            //保存する
            FileAccessManager.WriteJson(SystemInfo.userProfile);
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
                        SystemInfo.userProfile.scene_view_led = result;

                        //保存する
                        FileAccessManager.WriteJson(SystemInfo.userProfile);
                    }
                    break;
            }
        }

        public void Click_ChangeEffect(int i)
        {
            if (!backGroundCon) return;
            string str;

            switch (i)
            {
                case 0:
                    backGroundCon.SetCubemap(-1, out str);
                    textMeshs_Viewer[0].text = "SkyBox_" + str;
                    break;
                case 1:
                    backGroundCon.SetCubemap(1, out str);
                    textMeshs_Viewer[0].text = "SkyBox_" + str;
                    break;
                case 2:
                    backGroundCon.SetWormHole(-1, out str);
                    textMeshs_Viewer[1].text = "WormHole_" + str;
                    break;
                case 3:
                    backGroundCon.SetWormHole(1, out str);
                    textMeshs_Viewer[1].text = "WormHole_" + str;
                    break;
                case 4:
                    backGroundCon.SetParticle(-1, out str);
                    textMeshs_Viewer[2].text = "Particle_" + str;
                    break;
                case 5:
                    backGroundCon.SetParticle(1, out str);
                    textMeshs_Viewer[2].text = "Particle_" + str;
                    break;
            }

            menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        public void Click_SceneChange(int i)
        {
            menuManager.PlayOneShot(SoundType.BTN_CLICK);

            if (!btnE_SecenChange[i]) return;

            string[] str = new string[] { "LiveScene", "KAGURAScene", "ViewerScene" };
            SceneChange(str[i]).Forget();
        }

        /// <summary>
        /// シーン遷移処理
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private async UniTask SceneChange(string sceneName)
        {
            await UniTask.Delay(100, cancellationToken: cancellation_token);

            BlackoutCurtain.instance.StartBlackout(sceneName).Forget();
            await UniTask.Delay(200, cancellationToken: cancellation_token);

            //音が割れるので止める
            timeline.TimelineManualMode().Forget();
            await UniTask.Delay(200, cancellationToken: cancellation_token);

            //UIが透けて見えるので隠す
            menuManager.gameObject.SetActive(false);
        }

        /// <summary>
        /// キャラ初期サイズ
        /// </summary>
        private void Update_InitCharaSize()
        {
            textMeshs[0].text = $"{slider_InitCharaSize.Value:0.00}";
        }

        /// <summary>
        /// キャラの影サイズ
        /// </summary>
        private void Update_CharaShadow()
        {
            quasiShadow.shadowScale = slider_CharaShadow.Value;
            textMeshs[4].text = $"{slider_CharaShadow.Value:0.00}";
        }

        /// <summary>
        /// VMD範囲
        /// </summary>
        private void Update_VMDScale()
        {
            slider_VMDScale.Value = Mathf.Clamp(slider_VMDScale.Value, 0.3f, 1.0f);
            textMeshs[1].text = $"{slider_VMDScale.Value:0.000}";
        }

        /// <summary>
        /// 固定中心窩レンダリングのスライダー
        /// </summary>
        private void Update_FixedFoveated()
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
        private void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I)) Click_Setting_Viewer(2);//ワームホール
            if (Input.GetKeyDown(KeyCode.K)) Click_Setting_Viewer(3);//パーティクル
        }
    }


}