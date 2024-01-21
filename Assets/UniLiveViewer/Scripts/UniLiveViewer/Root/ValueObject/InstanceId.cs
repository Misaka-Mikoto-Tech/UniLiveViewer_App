namespace UniLiveViewer.ValueObject
{
    /// <summary>
    /// アクター種に関係なく完全ユニークなID
    /// ステージ生成順、常時インクリメントとする
    /// </summary>
    public class InstanceId
    {
        public int Id => _id;

        readonly int _id;

        InstanceId()
        {
        }

        public InstanceId(int id)
        {
            _id = id;
        }

        public static bool operator ==(InstanceId left, InstanceId right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null)) return false;
            return left.Id == right.Id;
        }

        public static bool operator !=(InstanceId left, InstanceId right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is InstanceId id && this == id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
