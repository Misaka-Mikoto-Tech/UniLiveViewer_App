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
        builder.RegisterComponentInHierarchy<TimelineController>();
        builder.RegisterComponentInHierarchy<AudioAssetManager>();
        builder.RegisterComponentInHierarchy<PlayableDirector>();
        builder.RegisterComponentInHierarchy<MeshGuide>();
        builder.RegisterComponentInHierarchy<QuasiShadow>();

        // ここの並び順模様確認
        builder.RegisterEntryPoint<TimelinePresenter>();
        builder.RegisterEntryPoint<MeshGuidePresenter>();
        builder.RegisterEntryPoint<QuasiShadowPresenter>();
    }
}
