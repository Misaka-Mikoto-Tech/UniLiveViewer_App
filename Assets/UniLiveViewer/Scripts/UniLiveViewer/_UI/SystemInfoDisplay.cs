using UniLiveViewer.SceneLoader;
using UnityEngine;
using UnityEngine.Profiling;

namespace UniLiveViewer
{
    public class SystemInfoDisplay : MonoBehaviour
    {
        [SerializeField] TextMesh[] _textMaxActor = new TextMesh[5];
        [SerializeField] TextMesh[] textMeshe_memory = new TextMesh[3];

        void Start()
        {
            _textMaxActor[0].text = GetMaxActor(SceneType.CANDY_LIVE).ToString();
            _textMaxActor[1].text = GetMaxActor(SceneType.KAGURA_LIVE).ToString();
            _textMaxActor[2].text = GetMaxActor(SceneType.VIEWER).ToString();
            _textMaxActor[3].text = GetMaxActor(SceneType.GYMNASIUM).ToString();
            _textMaxActor[4].text = GetMaxActor(SceneType.FANTASY_VILLAGE).ToString();
        }

        int GetMaxActor(SceneType mode)
        {
            int result = 0;
#if UNITY_EDITOR
            result = SystemInfo.MAXCHARA_EDITOR[(int)mode];
#elif UNITY_ANDROID
            if (UnityEngine.SystemInfo.deviceName.Contains("Oculus") || UnityEngine.SystemInfo.deviceName.Contains("Meta"))
            {
                if(UnityEngine.SystemInfo.deviceName.Contains("Quest 3")) result = SystemInfo.MAXCHARA_QUEST3[(int)mode];
                else if (UnityEngine.SystemInfo.deviceName.Contains("Quest 2")) result = SystemInfo.MAXCHARA_QUEST2[(int)mode];
                else  if (UnityEngine.SystemInfo.deviceName.Contains("Quest")) result = SystemInfo.MAXCHARA_QUEST1[(int)mode];
            }
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
