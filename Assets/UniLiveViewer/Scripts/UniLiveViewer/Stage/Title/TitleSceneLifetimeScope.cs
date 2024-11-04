using VContainer;
using VContainer.Unity;
using UnityEngine;
using MessagePipe;

namespace UniLiveViewer.Stage.Title
{
    public class TitleSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] AudioSource _mainAudioSource;
        [SerializeField] TitleSceneSettings _titleSceneSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<SceneTransitionMessage>(options);

            builder.RegisterComponent(_mainAudioSource);
            builder.RegisterComponent(_titleSceneSettings);

            builder.RegisterEntryPoint<TitleScenePresenter>();
        }
    }
}
