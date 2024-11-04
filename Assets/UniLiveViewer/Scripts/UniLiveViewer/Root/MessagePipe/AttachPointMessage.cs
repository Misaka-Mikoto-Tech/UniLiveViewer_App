namespace UniLiveViewer.MessagePipe
{
    /// <summary>
    /// ポイントの可視化のみ
    /// </summary>
    public class AttachPointMessage
    {
        public bool IsActive => _isActive;
        bool _isActive;

        public AttachPointMessage(bool isActive)
        {
            _isActive = isActive;
        }
    }
}