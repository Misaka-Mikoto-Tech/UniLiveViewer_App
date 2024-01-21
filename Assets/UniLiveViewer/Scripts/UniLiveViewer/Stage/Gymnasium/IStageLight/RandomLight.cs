using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer.Stage.Gymnasium
{
    /// <summary>
    /// sharedMaterial事故るから微妙かも..
    /// </summary>
    public class RandomLight : MonoBehaviour, IStageLight
    {
        const string PropertyName = "_TintColor";

        [SerializeField] MeshRenderer[] _lights;

        /// <summary>
        /// ms
        /// </summary>
        [SerializeField] int _intervalTime = 100;
        /// <summary>
        /// ms
        /// </summary>
        [SerializeField] int _maxLifeTime = 500;

        bool _isWhitelight = true;

        CancellationToken _cancellationToken;


        void Awake()
        {
            _cancellationToken = this.GetCancellationTokenOnDestroy();
            for (int i = 0; i < _lights.Length; i++)
            {
                if (_lights[i].gameObject.activeSelf) _lights[i].gameObject.SetActive(false);
            }
        }

        void OnEnable()
        {
            UpdateAsyc(_cancellationToken).Forget();
        }

        void IStageLight.ChangeCount(int count)
        {
            // 何もしない
        }

        void IStageLight.ChangeColor(bool isWhite)
        {
            _isWhitelight = isWhite;
            if (!_isWhitelight) return;

            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].sharedMaterial.SetColor(PropertyName, Color.white);
            }
        }

        void IStageLight.OnUpdate()
        {
            // 何もしない
        }

        async UniTask UpdateAsyc(CancellationToken token)
        {
            while (gameObject.activeSelf)
            {
                await UniTask.Delay(_intervalTime, cancellationToken: token);
                var index = Random.Range(0, _lights.Length);
                var targetRenderer = _lights[index];
                ChangeColorAsync(targetRenderer, token).Forget();
            }
        }

        /// <summary>
        /// ランダムな角度と色で一定時間発光後消す
        /// </summary>
        /// <param name="targetRenderer"></param>
        /// <returns></returns>
        async UniTask ChangeColorAsync(MeshRenderer targetRenderer, CancellationToken token)
        {
            if (!targetRenderer.gameObject.activeSelf) targetRenderer.gameObject.SetActive(true);
            targetRenderer.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-205, -155)));
            if (!_isWhitelight)
            {
                targetRenderer.sharedMaterial.SetColor
                        (PropertyName,
                        new Color(
                            Random.Range(0, 1.0f),
                            Random.Range(0, 1.0f),
                            Random.Range(0, 1.0f)
                            )
                        );
            }

            await UniTask.Delay(_maxLifeTime, cancellationToken: token);
            targetRenderer.gameObject.SetActive(false);
        }
    }
}
