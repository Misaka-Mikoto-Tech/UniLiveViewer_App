using VContainer;
using VContainer.Unity;
using UniLiveViewer;
using UnityEngine.Playables;

/// <summary>
/// PlayableDirectorが欲しいだけだったりする
/// </summary>
public class TimeLineLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<MeshGuideService>(Lifetime.Singleton);

        builder.RegisterComponentInHierarchy<TimelineController>();
        builder.RegisterComponentInHierarchy<AudioAssetManager>();
        builder.RegisterComponentInHierarchy<PlayableDirector>();
        builder.RegisterComponentInHierarchy<QuasiShadowSetting>();
        builder.RegisterComponentInHierarchy<QuasiShadowService>();

        // ここの並び順模様確認
        builder.RegisterEntryPoint<TimelinePresenter>();
        builder.RegisterEntryPoint<MeshGuidePresenter>();
        builder.RegisterEntryPoint<QuasiShadowPresenter>();
    }
}
