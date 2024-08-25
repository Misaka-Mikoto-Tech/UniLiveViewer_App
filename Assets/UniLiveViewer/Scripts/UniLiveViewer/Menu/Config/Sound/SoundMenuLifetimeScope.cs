using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Sound
{
    public class SoundMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] SoundMenuSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_settings);

            builder.Register<SoundMenuService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<SoundMenuPresenter>();
        }
    }
}
