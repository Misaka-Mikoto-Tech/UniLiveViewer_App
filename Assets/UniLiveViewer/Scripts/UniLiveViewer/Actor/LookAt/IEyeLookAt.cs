namespace UniLiveViewer.Actor.LookAt
{
    public interface IEyeLookAt
    {
        void SetEnable(bool isEnable);

        void SetWeight(float weight);

        void OnLateTick();

        /// <summary>
        /// 用意だけした
        /// </summary>
        void Reset();
    }
}
