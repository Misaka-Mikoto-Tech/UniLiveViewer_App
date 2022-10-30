namespace UniLiveViewer
{
    public interface IHeadLookAt
    {
        public void HeadUpdate();
        public void HeadUpdate_OnAnimatorIK();
    }

    public interface IEyeLookAt
    {
        public void EyeUpdate();
    }
    public interface ILookAtVRM 
    {
        public void SetEnable(bool isEnable);
        public void EyeReset();

        public UnityEngine.Transform GetLookAtTarget();
    }
}
