using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    /// <summary>
    /// 全シーンの最初に走る
    /// </summary>
    public class SystemSettingPresenter : IInitializable
    {
        [Inject]
        public SystemSettingPresenter()
        {
        }

        void IInitializable.Initialize()
        {
#if UNITY_EDITOR
            Debug.Log("Windowsとして認識しています");
#elif UNITY_ANDROID
        Debug.Log("Questとして認識しています");
#endif
            FileReadAndWriteUtility.ReadJson();
        }
    }
}
