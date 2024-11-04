using VContainer;
using VContainer.Unity;

namespace UniLiveViewer.General
{
    public class PositionHandlerPresenter : IStartable, ILateTickable
    {
        readonly PositionHandlerSrcAnchor _srcAnchor;
        readonly PositionHandlerService _positionHandlerService;

        [Inject]
        public PositionHandlerPresenter(
            PositionHandlerSrcAnchor srcAnchor,
            PositionHandlerService positionHandlerService)
        {
            _srcAnchor = srcAnchor;
            _positionHandlerService = positionHandlerService;
        }

        void IStartable.Start()
        {

        }

        void ILateTickable.LateTick()
        {
            if (!_srcAnchor.gameObject.activeInHierarchy) return;
            _positionHandlerService.OnLateTick();
        }
    }
}
