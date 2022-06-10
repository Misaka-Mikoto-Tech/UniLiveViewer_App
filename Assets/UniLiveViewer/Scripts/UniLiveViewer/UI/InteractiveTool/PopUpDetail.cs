using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace UniLiveViewer
{
    public class PopUpDetail : MonoBehaviour
    {
        public event Action OnHit;
        private int hitLayer = 0;
        private bool touchable = true;

        // Start is called before the first frame update
        void Awake()
        {
            hitLayer = SystemInfo.layerNo_IgnoreRaycats;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (touchable && other.gameObject.layer == hitLayer)
            {
                Interval().Forget();
                OnHit?.Invoke();
            }
        }

        private async UniTaskVoid Interval()
        {
            touchable = false;
            await UniTask.Delay(1000);
            touchable = true;
        }
    }
}