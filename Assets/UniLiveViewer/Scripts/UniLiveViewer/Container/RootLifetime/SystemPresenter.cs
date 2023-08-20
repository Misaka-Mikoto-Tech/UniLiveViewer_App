using System;
using UniLiveViewer;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

public class SystemPresenter : IStartable, IDisposable
{
    readonly SceneManagerService _sceneManager;
    readonly StageSettingService _stageSetting;

    [Inject]
    public SystemPresenter(
        SceneManagerService sceneManager,
        StageSettingService stageSetting)
    {
        _sceneManager = sceneManager;
        _stageSetting = stageSetting;
    }

    public void Start()
    {
        Debug.Log("Trace: SceneManagerPresenter.Start");

        // NOTE: Linkだと両方反応するのでelif必須
        // TODO: PLATFORM_OCULUS試す
#if UNITY_EDITOR
        Debug.Log("Windowsとして認識しています");
#elif UNITY_ANDROID    
        Debug.Log("Questとして認識しています");
#endif

        // 初Scene(タイトル)では発火しない
        SceneManager.activeSceneChanged += OnSceneLoaded;

        _stageSetting.Initialize();//タイトルでも必要

        Debug.Log("Trace: SceneManagerPresenter.Start");
    }

    void OnSceneLoaded(Scene oldScene, Scene newScene)
    {
        _sceneManager.Initialize(oldScene, newScene);
        UniLiveViewer.SystemInfo.CheckMaxFieldChara();

        _stageSetting.Initialize();
    }

    void IDisposable.Dispose()
    {
        SceneManager.activeSceneChanged -= OnSceneLoaded;
    }
}
