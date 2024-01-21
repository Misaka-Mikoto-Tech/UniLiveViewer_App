using UnityEngine;

namespace UniLiveViewer.Actor.LookAt
{
    public class LookatService
    {
        IHeadLookAt _headLookAt;
        IEyeLookAt _eyeLookAt;

        public void Setup(IHeadLookAt headLookAt, IEyeLookAt eyeLookAt)
        {
            _headLookAt = headLookAt;
            _eyeLookAt = eyeLookAt;
        }

        public void OnChangeHeadLookAt(bool isEnable)
        {
            _headLookAt.SetEnable(isEnable);
        }

        public void OnChangeEyeLookAt(bool isEnable)
        {
            _eyeLookAt.SetEnable(isEnable);
        }

        public void OnLateTick()
        {
            if (Time.timeScale == 0) return;//これ個別に書くのやだな

            if (_headLookAt != null) _headLookAt.OnLateTick();
            if (_eyeLookAt != null) _eyeLookAt.OnLateTick();
        }
    }
}
