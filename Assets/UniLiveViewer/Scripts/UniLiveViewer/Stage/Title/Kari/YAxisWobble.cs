using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UniLiveViewer.SceneUI.Title.Kari
{
    public class YAxisWobble : MonoBehaviour
    {
        [SerializeField] int _delayStart = 1000;
        [SerializeField] float _speed = 1.5f;
        [SerializeField] float _amplitude = 0.1f;
        float _initialY;
        float _startTime;

        async void Start()
        {
            _initialY = transform.position.y;

            await UniTask.Delay(_delayStart);
            _startTime = Time.time;
            while (this != null)
            {
                var elapsedTime = Time.time - _startTime;
                var newY = _initialY + Mathf.Sin(elapsedTime * _speed) * _amplitude;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
                await UniTask.Yield();
            }
        }
    }
}