namespace UniLiveViewer.Kari
{
    /// <summary>
    /// 必要性再検討
    /// </summary>
    public class SummonedCount
    {
        public int Value => _value;
        int _value;

        public SummonedCount(int value)
        {
            _value = value;
        }
    }
}