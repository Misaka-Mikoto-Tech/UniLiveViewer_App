using MessagePipe;
using UniLiveViewer.Kari;
using UniLiveViewer.SceneLoader;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<SummonedCount>(options);

            builder.Register<SceneChangeService>(Lifetime.Singleton);
            builder.Register<FileAccessManager>(Lifetime.Singleton);

            builder.RegisterEntryPoint<SystemSettingPresenter>();
        }
    }
}
