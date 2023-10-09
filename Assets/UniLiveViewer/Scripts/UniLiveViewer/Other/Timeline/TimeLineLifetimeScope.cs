using UnityEngine;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Timeline
{
    [RequireComponent(typeof(AudioAssetManager), typeof(TimelineController), typeof(PlayableDirector))]
    [RequireComponent(typeof(QuasiShadowService), typeof(QuasiShadowSetting))]
    public class TimelineLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<MeshGuideService>(Lifetime.Singleton);

            builder.RegisterComponent(GetComponent<TimelineController>());
            builder.RegisterComponent(GetComponent<AudioAssetManager>());
            builder.RegisterComponent(GetComponent<PlayableDirector>());

            builder.RegisterComponent(GetComponent<QuasiShadowService>());
            builder.RegisterComponent(GetComponent<QuasiShadowSetting>());

            builder.RegisterEntryPoint<TimelinePresenter>();
            builder.RegisterEntryPoint<MeshGuidePresenter>();
            builder.RegisterEntryPoint<QuasiShadowPresenter>();
        }
    }
}
