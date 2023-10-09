using UniLiveViewer;
using VContainer;
using VContainer.Unity;
using MessagePipe;
using UniLiveViewer.SceneLoader;
using UniLiveViewer.Kari;

public class RootLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        var options = builder.RegisterMessagePipe();
        builder.RegisterMessageBroker<SummonedCount>(options);

        builder.Register<SceneChangeService>(Lifetime.Singleton);
        builder.Register<FileAccessManager>(Lifetime.Singleton);
        builder.Register<StageSettingService>(Lifetime.Singleton);

        builder.RegisterEntryPoint<SystemSettingPresenter>();
    }
}
