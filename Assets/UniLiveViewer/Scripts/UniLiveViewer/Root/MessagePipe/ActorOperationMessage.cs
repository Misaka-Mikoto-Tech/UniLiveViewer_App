using UniLiveViewer.Actor;
using UniLiveViewer.ValueObject;

namespace UniLiveViewer.MessagePipe
{
    /// <summary>
    /// 任意アクター向けに通知
    /// (ActorIdだと同じactorが全員消えてしまう)
    /// </summary>
    public class ActorOperationMessage
    {
        public InstanceId InstanceId => _instanceId;
        InstanceId _instanceId;

        public ActorCommand ActorCommand => _command;
        ActorCommand _command;


        public ActorOperationMessage(InstanceId instanceId, ActorCommand command)
        {
            _instanceId = instanceId;
            _command = command;
        }
    }
}