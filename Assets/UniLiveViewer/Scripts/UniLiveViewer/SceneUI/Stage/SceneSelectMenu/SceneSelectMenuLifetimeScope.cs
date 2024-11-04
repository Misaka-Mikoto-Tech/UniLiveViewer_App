using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.SceneSelect
{
    /// <summary>
    /// まだ未使用
    /// </summary>
    public class SceneSelectMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] SceneSelectMenuSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_settings);

            builder.Register<SceneSelectMenuService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<SceneSelectMenuPresenter>();
        }
    }
}
