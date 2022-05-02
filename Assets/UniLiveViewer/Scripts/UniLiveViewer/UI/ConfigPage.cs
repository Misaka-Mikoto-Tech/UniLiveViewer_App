using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UniLiveViewer
{
    public class ConfigPage : MonoBehaviour
    {
        private MenuManager menuManager;

        [Header("＜マニュアル＞")]
        [SerializeField] private Sprite[] sprManualPrefab = new Sprite[4];
        [SerializeField] private SpriteRenderer[] sprManual = new SpriteRenderer[2];

        [Header("＜その他＞")]
        [SerializeField] private Transform[] sceneAnchor;
        [SerializeField] private Button_Switch[] btnE_SecenChange = new Button_Switch[3];
        [Space(20)]
        private Button_Base[] btnE = new Button_Base[5];
        [SerializeField] private Transform[] btnE_ActionParent;
        [SerializeField] private TextMesh[] textMeshs = new TextMesh[2];
        [SerializeField] private Material material_OutLine;
        [SerializeField] private UniversalRendererData frd;
        [SerializeField] private ScriptableRendererFeature outlineRender;
        [SerializeField] private SliderGrabController slider_OutLine = null;
        [SerializeField] private SliderGrabController slider_InitCharaSize = null;
        [SerializeField] private SliderGrabController slider_FixedFoveated = null;
        [SerializeField] private SliderGrabController slider_Fog = null;
        [SerializeField] private Button_Base[] btn_passthrough = null;

        private TimelineController timeline = null;
        private PlayerStateManager playerStateManager = null;

        private Material matMirrore;//LiveScene用
        private BackGroundController backGroundCon;
        public event Action<string> onSceneSwitch;
        private CancellationToken cancellation_token;

        private void Awake()
        {
            menuManager = transform.root.GetComponent<MenuManager>();

            timeline = menuManager.timeline;
            playerStateManager = menuManager.playerStateManager;

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
                SystemInfo.userProfile.data.InitCharaSize = float.Parse(slider_InitCharaSize.Value.ToString("f2"));
                SystemInfo.userProfile.WriteJson();
            };
            slider_FixedFoveated.ValueUpdate += Update_FixedFoveated;
            slider_Fog.ValueUpdate += () => { RenderSettings.fogDensity = slider_Fog.Value; };

            playerStateManager.myCamera.clearFlags = CameraClearFlags.Skybox;
            playerStateManager.myOVRManager.isInsightPassthroughEnabled = false;
            for (int i = 0; i < btn_passthrough.Length; i++)
            {
                btn_passthrough[i].onTrigger += PassthroughAction;
            }
        }
        private void OnEnable()
        {
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

                if (SystemInfo.userProfile.data.LanguageCode == (int)USE_LANGUAGE.JP)
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

                btnE_ActionParent[0].gameObject.SetActive(SystemInfo.userProfile.data.scene_crs_particle);
                btnE_ActionParent[1].gameObject.SetActive(SystemInfo.userProfile.data.scene_crs_laser);
                matMirrore.SetFloat("_Smoothness", SystemInfo.userProfile.data.scene_crs_reflection ? 1 : 0);
                btnE_ActionParent[3].gameObject.SetActive(SystemInfo.userProfile.data.scene_crs_sonic);
                btnE_ActionParent[4].gameObject.SetActive(SystemInfo.userProfile.data.scene_crs_manual);
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

                btnE_ActionParent[0].gameObject.SetActive(SystemInfo.userProfile.data.scene_kagura_particle);
                btnE_ActionParent[1].gameObject.SetActive(SystemInfo.userProfile.data.scene_kagura_sea);
                btnE_ActionParent[2].transform.GetChild(0).gameObject.SetActive(SystemInfo.userProfile.data.scene_kagura_reflection);
            }
            else if (SystemInfo.sceneMode == SceneMode.VIEWER)
            {
                //シーン別専用ボタンの割り当て
                for (int i = 0; i < 4; i++)
                {
                    btnE[i] = sceneAnchor[current].GetChild(i).GetComponent<Button_Base>();
                }

                btnE_ActionParent = new Transform[1];

                btnE_ActionParent[0] = GameObject.FindGameObjectWithTag("FloorLED").transform;
                backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();

                btnE_ActionParent[0].gameObject.SetActive(SystemInfo.userProfile.data.scene_view_led);
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
            await UniTask.Yield(cancellation_token);

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
            btn_passthrough[0].isEnable = playerStateManager.myOVRManager.isInsightPassthroughEnabled;

            //キャラサイズ
            slider_InitCharaSize.Value = SystemInfo.userProfile.data.InitCharaSize;
            Update_InitCharaSize();
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

        private void PassthroughAction(Button_Base btn)
        {
            if (btn == btn_passthrough[0])
            {
                //if (!b.gameObject.activeSelf) return;
                if (btn.isEnable)
                {
                    playerStateManager.myCamera.clearFlags = CameraClearFlags.Color;
                    //backGroundCon.Clear_CubemapTex();
                    //DynamicGI.UpdateEnvironment();
                }
                else
                {
                    playerStateManager.myCamera.clearFlags = CameraClearFlags.Skybox;
                    //string str;
                    //backGroundCon.SetCubemap(0, out str);
                    //DynamicGI.UpdateEnvironment();
                }
            }

            if (btn.isEnable) playerStateManager.myOVRManager.isInsightPassthroughEnabled = true;
            else playerStateManager.myOVRManager.isInsightPassthroughEnabled = false;
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
        /// <returns></returns>
        private async UniTaskVoid SceneChange(string sceneName)
        {
            await UniTask.Delay(100, cancellationToken: cancellation_token);
            onSceneSwitch?.Invoke(sceneName);
            await UniTask.Delay(400, cancellationToken: cancellation_token);//音の分だけ待つ

            //音が割れるので止める
            timeline.TimelineManualMode();
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
            textMeshs[0].text = $"{slider_InitCharaSize.Value:0.00}";
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
            textMeshs[1].text = $"noQuest:{slider_FixedFoveated.Value}";
#elif UNITY_ANDROID
        //反映し直す
        OVRManager.fixedFoveatedRenderingLevel = (OVRManager.FixedFoveatedRenderingLevel)slider_FixedFoveated.Value;
        //テキストに反映
        textMesh_Page3[1].text = Enum.GetName(typeof(OVRManager.FixedFoveatedRenderingLevel),OVRManager.fixedFoveatedRenderingLevel);
#endif
        }
        private void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I)) Click_Setting_Viewer(2);//ワームホール
            if (Input.GetKeyDown(KeyCode.K)) Click_Setting_Viewer(3);//パーティクル
        }
    }


}