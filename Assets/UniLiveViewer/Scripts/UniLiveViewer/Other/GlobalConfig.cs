using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace UniLiveViewer 
{
    public class GlobalConfig : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = -1;
        [SerializeField] private int shaderLOD = 1000;
        [Header("＜Debug.Log()を一括で切り替える＞")]
        [SerializeField] private bool isDebug = true;
        TimelineController _timeline = null;

        void Awake()
        {
            SystemInfo.Init();

            if (targetFrameRate > 0) Application.targetFrameRate = targetFrameRate;
            Shader.globalMaximumLOD = shaderLOD;

#if UNITY_EDITOR
            //デバッグログを一括で有効・無効化
            Debug.unityLogger.logEnabled = isDebug;
#elif UNITY_ANDROID
            //デバッグログを一括で無効化
            Debug.unityLogger.logEnabled = false;

            //中心以外の描画のレベルを下げる(最大値)
            OVRManager.fixedFoveatedRenderingLevel = SystemInfo.levelFFR;
            //描画負荷に応じてfixedFoveatedRenderingLevelを自動的に調整する
            OVRManager.useDynamicFixedFoveatedRendering = true;
#endif
        }

        void Start()
        {
            if (GetActiveSceneName() == "TitleScene") return;

            _timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            //オキュラスのホームボタントリガーの対応
            OVRManager.InputFocusLost += HomePause;
            OVRManager.InputFocusAcquired += HomeReStart;
            OVRManager.HMDUnmounted += HomePause;//HMDが外された
            OVRManager.HMDMounted += HomeReStart;//HMDが付けられた
        }

        public static string GetActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        void HomePause()
        {
            _timeline.TimelineManualMode().Forget();
            Time.timeScale = 0;
        }

        void HomeReStart()
        {
            Time.timeScale = 1;
        }
    }
}
