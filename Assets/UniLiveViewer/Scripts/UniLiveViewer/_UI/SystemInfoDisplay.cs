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
            _textMaxActor[0].text = SystemInfo.GetMaxFieldActor(SceneType.CANDY_LIVE).ToString();
            _textMaxActor[1].text = SystemInfo.GetMaxFieldActor(SceneType.KAGURA_LIVE).ToString();
            _textMaxActor[2].text = SystemInfo.GetMaxFieldActor(SceneType.VIEWER).ToString();
            _textMaxActor[3].text = SystemInfo.GetMaxFieldActor(SceneType.GYMNASIUM).ToString();
            _textMaxActor[4].text = SystemInfo.GetMaxFieldActor(SceneType.FANTASY_VILLAGE).ToString();
        }

        void Update()
        {
            textMeshe_memory[0].text = $"Total:{(Profiler.GetTotalReservedMemoryLong() / 1024 / 1024):0}MB";
            textMeshe_memory[1].text = $"Used:{(Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024):0}MB";
            textMeshe_memory[2].text = $"Free:{(Profiler.GetTotalUnusedReservedMemoryLong() / 1024 / 1024):0}MB";
        }
    }
}
