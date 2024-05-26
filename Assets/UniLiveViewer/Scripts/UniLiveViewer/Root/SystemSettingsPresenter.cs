using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    /// <summary>
    /// 全シーンの最初に走る
    /// </summary>
    public class SystemSettingsPresenter : IInitializable , IAsyncStartable
    {
        readonly SystemSettingsService _systemSettingsService;

        [Inject]
        public SystemSettingsPresenter(SystemSettingsService systemSettingsService)
        {
            _systemSettingsService = systemSettingsService;
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

        async UniTask IAsyncStartable.StartAsync(CancellationToken cancellation)
        {
            await _systemSettingsService.InitializeAsync(cancellation);
        }
    }
}
