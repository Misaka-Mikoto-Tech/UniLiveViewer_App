using UniLiveViewer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class SystemSettingPresenter : IStartable
{
    readonly StageSettingService _stageSettingService;

    [Inject]
    public SystemSettingPresenter(
        StageSettingService stageSettingService)
    {
        _stageSettingService = stageSettingService;
    }

    void IStartable.Start()
    {
#if UNITY_EDITOR
        Debug.Log("Windowsとして認識しています");
#elif UNITY_ANDROID    
        Debug.Log("Questとして認識しています");
#endif

        FileReadAndWriteUtility.ReadJson();
        //_stageSettingService.Initialize();
    }
}
