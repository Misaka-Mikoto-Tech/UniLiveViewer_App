namespace UniLiveViewer.Actor.LookAt
{
    public interface IHeadLookAt
    {
        /// <summary>
        /// Clip時の頭IK用、VMD時は無効化する
        /// </summary>
        /// <param name="isEnable"></param>
        void SetEnable(bool isEnable);

        void OnLateTick();
    }

    public interface IEyeLookAt
    {
        void SetEnable(bool isEnable);
        void OnLateTick();

        void Reset();
    }
}
