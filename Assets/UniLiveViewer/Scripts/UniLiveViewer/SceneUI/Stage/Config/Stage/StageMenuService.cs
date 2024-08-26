using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageMenuService
    {
        readonly StageMenuSettings _settings;

        [Inject]
        public StageMenuService(
            StageMenuSettings settings)
        {
            _settings = settings;
        }
    }
}