using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    public class StageSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<AnimationAssetManager>(Lifetime.Singleton);
            builder.Register<TextureAssetManager>(Lifetime.Singleton);

            builder.RegisterEntryPoint<StageScenePresenter>();
        }
    }
}
