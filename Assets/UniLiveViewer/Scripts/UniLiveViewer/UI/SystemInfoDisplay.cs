using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace UniLiveViewer
{
    public class SystemInfoDisplay : MonoBehaviour
    {
        [SerializeField] private TextMesh[] textMeshes_maxChara = new TextMesh[3];
        [SerializeField] private TextMesh[] textMeshe_memory = new TextMesh[3];

        

        // Start is called before the first frame update
        void Start()
        {
            textMeshes_maxChara[0].text = GetMaxChara(SceneMode.CANDY_LIVE).ToString();
            textMeshes_maxChara[1].text = GetMaxChara(SceneMode.KAGURA_LIVE).ToString();
            textMeshes_maxChara[2].text = GetMaxChara(SceneMode.VIEWER).ToString();
        }

        private byte GetMaxChara(SceneMode mode)
        {
            byte result = 0;
#if UNITY_EDITOR
            result = SystemInfo.MAXCHARA_EDITOR[(byte)mode];
#elif UNITY_ANDROID
            if (UnityEngine.SystemInfo.deviceName == "Oculus Quest 2") result = SystemInfo.MAXCHARA_QUEST2[(byte)mode];
            else if (UnityEngine.SystemInfo.deviceName == "Oculus Quest") result = SystemInfo.MAXCHARA_QUEST1[(byte)mode];
#endif
            return result;
        }

        // Update is called once per frame
        void Update()
        {
            textMeshe_memory[0].text = $"Reserved:{(Profiler.GetTotalReservedMemoryLong() / 1024 / 1024):0}MB";
            textMeshe_memory[1].text = $"Usable:{(Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024):0}MB";
            textMeshe_memory[2].text = $"Free:{(Profiler.GetTotalUnusedReservedMemoryLong() / 1024 / 1024):0}MB";
        }
    }
}
