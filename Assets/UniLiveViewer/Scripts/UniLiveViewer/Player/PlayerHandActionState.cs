using static UniLiveViewer.Player.PlayerEnums;

namespace UniLiveViewer.Player
{
    /// <summary>
    /// 握り放しの対象情報
    /// </summary>
    public enum HandTargetType
    {
        Actor,
        Item
    }

    public enum HandActionState
    {
        Grab,
        Release
    }

    
    public class PlayerHandActionState
    {
        public HandType HandType { get; }
        public HandTargetType Target { get; }
        public HandActionState Action { get; }

        public PlayerHandActionState(HandType hand, HandTargetType target, HandActionState action)
        {
            HandType = hand;
            Target = target;
            Action = action;
        }
    }
}