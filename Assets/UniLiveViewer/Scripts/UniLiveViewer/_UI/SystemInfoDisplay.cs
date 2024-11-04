using UnityEngine;
using UnityEngine.Profiling;

namespace UniLiveViewer
{
    public class SystemInfoDisplay : MonoBehaviour
    {
        [SerializeField] TextMesh[] textMeshe_memory = new TextMesh[3];

        void Start()
        {
        }

        void Update()
        {
            textMeshe_memory[0].text = $"Total:{(Profiler.GetTotalReservedMemoryLong() / 1024 / 1024):0}MB";
            textMeshe_memory[1].text = $"Used:{(Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024):0}MB";
            textMeshe_memory[2].text = $"Free:{(Profiler.GetTotalUnusedReservedMemoryLong() / 1024 / 1024):0}MB";
        }
    }
}
