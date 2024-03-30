using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Actor
{
    public class ActorMenuPresenter : IStartable
    {
        readonly ActorMenuService _sceneSelectMenuService;

        [Inject]
        public ActorMenuPresenter(
            ActorMenuService sceneSelectMenuService)
        {
            _sceneSelectMenuService = sceneSelectMenuService;
        }

        void IStartable.Start()
        {
            _sceneSelectMenuService.Initialize();
        }
    }
}