using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageMenuPresenter : IStartable
    {
        readonly StageMenuService _sceneSelectMenuService;

        [Inject]
        public StageMenuPresenter(
            StageMenuService sceneSelectMenuService)
        {
            _sceneSelectMenuService = sceneSelectMenuService;
        }

        void IStartable.Start()
        {

        }
    }
}