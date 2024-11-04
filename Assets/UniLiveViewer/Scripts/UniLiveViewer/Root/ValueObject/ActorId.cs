using UniLiveViewer.Actor;

namespace UniLiveViewer.ValueObject
{
    public class ActorId
    {
        /// <summary>
        /// アクター種類
        /// </summary>
        public ActorType Type => _type;
        readonly ActorType _type;

        /// <summary>
        /// 登録順なIndexと同義
        /// NOTE: 同じVRMをロードした場合は異なるIDとする
        /// </summary>
        public int ID => _id;
        readonly int _id;

        ActorId()
        {
        }

        public ActorId(ActorType type, int id)
        {
            _type = type;
            _id = id;
        }

        public static bool operator ==(ActorId left, ActorId right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
            return left.ID == right.ID;
        }

        public static bool operator !=(ActorId left, ActorId right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is ActorId id && this == id;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
