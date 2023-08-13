using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer
{
    public class MaterialSimpleGear : MonoBehaviour
    {
        public int materialIndex;
        public string targetName = "_Amplitude";
        public AnimationCurve floatCurve = AnimationCurve.Linear(0, 0, 1, 2);
        Material _material;

        void Awake()
        {
            _material = GetComponent<Renderer>().materials[materialIndex];
            UpdateMaterial(this.GetCancellationTokenOnDestroy()).Forget();
        }

        async UniTask UpdateMaterial(CancellationToken cancellationToken)
        {
            var timer = 0.0f;

            while (gameObject)
            {
                _material.SetFloat(targetName, floatCurve.Evaluate(timer));
                timer += 0.1f;
                await UniTask.Delay(100, cancellationToken: cancellationToken);
            }
        }
    }
}