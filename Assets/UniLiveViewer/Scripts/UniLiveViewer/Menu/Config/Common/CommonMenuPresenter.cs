using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.Menu.Config.Common
{
    public class CommonMenuPresenter : IStartable
    {
        readonly CommonMenuService _sceneSelectMenuService;

        [Inject]
        public CommonMenuPresenter(
            CommonMenuService sceneSelectMenuService)
        {
            _sceneSelectMenuService = sceneSelectMenuService;
        }

        void IStartable.Start()
        {
            _sceneSelectMenuService.Initialize();
        }
    }
}