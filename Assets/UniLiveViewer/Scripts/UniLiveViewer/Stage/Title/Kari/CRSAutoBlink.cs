using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UniLiveViewer.Stage.Title.Kari
{
    public class CRSAutoBlink : MonoBehaviour
    {
        [SerializeField] SkinnedMeshRenderer _faceSkinnedMesh;
        [SerializeField] int _faceIndex;

        [SerializeField] SkinnedMeshRenderer _transSkinnedMesh;
        [SerializeField] int _transIndex;

        float _timer;

        async void Start()
        {
            _timer = 12;

            while (this != null)
            {
                if (_timer <= 0)
                {
                    await LerpValue(0, 1, 0.1f);
                    await LerpValue(1, 0, 0.1f);
                    _timer = Random.Range(3, 10);
                }
                _timer -= Time.deltaTime;
                await UniTask.Yield();
            }
        }

        async UniTask LerpValue(float startValue, float endValue, float duration)
        {
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                var t = elapsedTime / duration;
                var value = Mathf.Lerp(startValue, endValue, t);

                _faceSkinnedMesh.SetBlendShapeWeight(_faceIndex, value * 100);
                _transSkinnedMesh.SetBlendShapeWeight(_transIndex, value * 100);

                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }
        }
    }
}