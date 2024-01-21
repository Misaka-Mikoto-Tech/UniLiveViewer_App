using UniLiveViewer.SceneLoader;
using UnityEngine;
using UnityEngine.Profiling;

namespace UniLiveViewer
{
    public class SystemInfoDisplay : MonoBehaviour
    {
        [SerializeField] TextMesh[] textMeshes_maxChara = new TextMesh[3];
        [SerializeField] TextMesh[] textMeshe_memory = new TextMesh[3];

        void Start()
        {
            textMeshes_maxChara[0].text = GetMaxChara(SceneType.CANDY_LIVE).ToString();
            textMeshes_maxChara[1].text = GetMaxChara(SceneType.KAGURA_LIVE).ToString();
            textMeshes_maxChara[2].text = GetMaxChara(SceneType.VIEWER).ToString();
            textMeshes_maxChara[3].text = GetMaxChara(SceneType.GYMNASIUM).ToString();
        }

        int GetMaxChara(SceneType mode)
        {
            int result = 0;
#if UNITY_EDITOR
            result = SystemInfo.MAXCHARA_EDITOR[(int)mode];
#elif UNITY_ANDROID
            if (UnityEngine.SystemInfo.deviceName == "Oculus Quest 2") result = SystemInfo.MAXCHARA_QUEST2[(int)mode];
            else if (UnityEngine.SystemInfo.deviceName == "Oculus Quest") result = SystemInfo.MAXCHARA_QUEST1[(int)mode];
#endif
            return result;
        }

        void Update()
        {
            textMeshe_memory[0].text = $"Total:{(Profiler.GetTotalReservedMemoryLong() / 1024 / 1024):0}MB";
            textMeshe_memory[1].text = $"Used:{(Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024):0}MB";
            textMeshe_memory[2].text = $"Free:{(Profiler.GetTotalUnusedReservedMemoryLong() / 1024 / 1024):0}MB";
        }
    }
}
