using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// シーン遷移とフォルダ内アセット関連
    /// </summary>
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
