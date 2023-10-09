using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace UniLiveViewer
{
    public class TitleSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] OVRScreenFade _ovrScreenFade;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_ovrScreenFade);
            builder.RegisterEntryPoint<TitleScenePresenter>();
        }
    }
}
