namespace UniLiveViewer
{
    public interface IStageLight
    {
        void ChangeCount(int count);
        void ChangeColor(bool isWhite);
        void OnUpdate();
    }
}