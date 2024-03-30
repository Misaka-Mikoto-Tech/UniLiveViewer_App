using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Dance
{
    public class DanceMenuPresenter : IStartable
    {
        readonly DanceMenuService _sceneSelectMenuService;

        [Inject]
        public DanceMenuPresenter(
            DanceMenuService sceneSelectMenuService)
        {
            _sceneSelectMenuService = sceneSelectMenuService;
        }

        void IStartable.Start()
        {
            _sceneSelectMenuService.Initialize();
        }
    }
}