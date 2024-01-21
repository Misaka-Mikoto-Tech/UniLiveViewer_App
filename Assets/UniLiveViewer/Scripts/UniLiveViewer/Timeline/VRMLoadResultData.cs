using UniLiveViewer.Actor;

namespace UniLiveViewer.Timeline
{
    public class VRMLoadResultData
    {
        public IActorService Value => _value;
        IActorService _value;

        public VRMLoadResultData(IActorService value)
        {
            _value = value;
        }
    }
}