using UniLiveViewer.Menu;
using UniLiveViewer.ValueObject;

namespace UniLiveViewer.MessagePipe
{
    public class ActorAnimationMessage
    {
        public InstanceId InstanceId => _instanceId;
        InstanceId _instanceId;

        public CurrentMode Mode => _mode;
        CurrentMode _mode;
        public int AnimationIndex => _animationIndex;
        int _animationIndex;

        public bool IsReverse => _isReverse;
        bool _isReverse;

        public ActorAnimationMessage(InstanceId instanceId, CurrentMode mode, bool isReverse, int animationIndex)
        {
            _mode = mode;
            _animationIndex = animationIndex;
            _instanceId = instanceId;
            _isReverse = isReverse;
        }
    }
}