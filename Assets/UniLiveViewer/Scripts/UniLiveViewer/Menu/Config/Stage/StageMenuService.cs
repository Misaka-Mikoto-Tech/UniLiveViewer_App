using VContainer;

namespace UniLiveViewer.Menu.Config.Stage
{
    public class StageMenuService
    {
        readonly StageMenuSettings _settings;
        readonly AudioSourceService _audioSourceService;

        [Inject]
        public StageMenuService(
            StageMenuSettings settings,
            AudioSourceService audioSourceService)
        {
            _settings = settings;
            _audioSourceService = audioSourceService;
        }

        public void Initialize()
        {

        }
    }
}