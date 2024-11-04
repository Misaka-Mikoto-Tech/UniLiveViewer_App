using UniLiveViewer.Actor;

namespace UniLiveViewer.MessagePipe
{
    /// <summary>
    /// ActorIdに依存せずtimeline側とmenu側の全員に通知
    /// </summary>
    public class AllActorOptionMessage
    {
        /// <summary>
        /// NULLの場合は全員
        /// </summary>
        public ActorState ActorState => _actorState;
        ActorState _actorState;

        public ActorOptionCommand ActorCommand => _command;
        ActorOptionCommand _command;

        /// <summary>
        /// ActorState.NULLは全員
        /// </summary>
        /// <param name="actorState"></param>
        /// <param name="command"></param>
        public AllActorOptionMessage(ActorState actorState, ActorOptionCommand command)
        {
            _actorState = actorState;
            _command = command;
        }
    }
}