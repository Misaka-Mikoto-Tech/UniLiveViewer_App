using UniLiveViewer.Actor;

namespace UniLiveViewer.Timeline
{
    public class VRMLoadResultData
    {
        public IActorEntity Value => _value;
        IActorEntity _value;

        public VRMLoadResultData(IActorEntity value)
        {
            _value = value;
        }
    }
}