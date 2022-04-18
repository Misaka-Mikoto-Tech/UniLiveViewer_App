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
            if (SystemInfo.deviceName == "Oculus Quest 2") result = SystemInfo.MAXCHARA_QUEST2[(byte)mode].ToString();
            else if (SystemInfo.deviceName == "Oculus Quest") result = SystemInfo.MAXCHARA_QUEST1[(byte)mode].ToString();
#endif
            return result;
        }

        // Update is called once per frame
        void Update()
        {
            textMeshe_memory[0].text = "予約:" + Profiler.GetTotalReservedMemoryLong().ToString();
            textMeshe_memory[1].text = "使用:" + Profiler.GetTotalAllocatedMemoryLong().ToString();
            textMeshe_memory[2].text = "空き:" + Profiler.GetTotalUnusedReservedMemoryLong().ToString();
        }
    }
}
