using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace UniLiveViewer 
{
    public class GlobalConfig : MonoBehaviour
    {
        [SerializeField] int _targetFrameRate = -1;
        [SerializeField] int _shaderLOD = 1000;
        [Header("＜Debug.Log()を一括で切り替える＞")]
        [SerializeField] bool _isDebug = true;

        void Awake()
        {
            if (_targetFrameRate > 0) Application.targetFrameRate = _targetFrameRate;
            Shader.globalMaximumLOD = _shaderLOD;

#if UNITY_EDITOR
            //デバッグログを一括で有効・無効化
            Debug.unityLogger.logEnabled = _isDebug;
#elif UNITY_ANDROID
            //デバッグログを一括で無効化
            //Debug.unityLogger.logEnabled = false;

            //中心以外の描画のレベルを下げる(最大値)
            OVRManager.fixedFoveatedRenderingLevel = SystemInfo.levelFFR;
            //描画負荷に応じてfixedFoveatedRenderingLevelを自動的に調整する
            OVRManager.useDynamicFixedFoveatedRendering = true;
#endif
        }
    }
}
