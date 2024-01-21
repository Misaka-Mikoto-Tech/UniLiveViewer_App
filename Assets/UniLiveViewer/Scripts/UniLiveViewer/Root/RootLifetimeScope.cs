using UniLiveViewer.SceneLoader;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SceneChangeService>(Lifetime.Singleton);
            builder.Register<FileAccessManager>(Lifetime.Singleton);

            builder.RegisterEntryPoint<SystemSettingPresenter>();
        }
    }
}
