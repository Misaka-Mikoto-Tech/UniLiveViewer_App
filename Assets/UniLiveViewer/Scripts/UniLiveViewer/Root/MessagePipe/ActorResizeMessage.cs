using UniLiveViewer.ValueObject;

namespace UniLiveViewer.MessagePipe
{
    public class ActorResizeMessage
    {
        public InstanceId InstanceId => _instanceId;
        InstanceId _instanceId;

        public float AddScale => _addScale;
        float _addScale;

        public ActorResizeMessage(InstanceId instanceId, float addScale)
        {
            _instanceId = instanceId;
            _addScale = addScale;
        }
    }
}