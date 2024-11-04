namespace UniLiveViewer.Actor.LookAt
{
    public interface IHeadLookAt
    {
        /// <summary>
        /// Clip時の頭IK用、VMD時は無効化する
        /// </summary>
        void SetEnable(bool isEnable);

        void SetWeight(float weight);

        void OnLateTick();
    }
}
