using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Stage
{
    /// <summary>
    /// まだ未使用
    /// </summary>
    public class StageMenuLifetimeScope : LifetimeScope
    {
        [SerializeField] StageMenuSettings _settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_settings);

            builder.Register<StageMenuService>(Lifetime.Singleton);
            builder.RegisterEntryPoint<StageMenuPresenter>();
        }
    }
}
