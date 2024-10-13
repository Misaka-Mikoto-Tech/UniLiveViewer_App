using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer.Stage.Title.Actor
{
    public class TitleActorAnimatorService : MonoBehaviour
    {
        [SerializeField] float _ikWeight = 1.0f;

        bool _isIK = false;
        float _timer;
        Transform _target;
        Animator _animator;

        void Start()
        {
            _target = Camera.main.transform;
            _animator = GetComponent<Animator>();
        }

        public async UniTask OnSceneTransitionAsync(CancellationToken cancellation)
        {
            _animator.SetTrigger("SceneTransitionTrigger");

            await UniTask.Delay(2000, cancellationToken: cancellation);
            _isIK = false;
        }

        void OnAnimatorIK(int layerIndex)
        {
            if (_target == null || _animator == null) return;

            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("IdleLoop"))
            {
                _isIK = true;
            }

            var weight = Mathf.Lerp(0f, _ikWeight, _timer);
            if (_isIK)
            {
                _timer = Mathf.Clamp01(_timer + Time.deltaTime);
                _animator.SetLookAtWeight(weight);
                _animator.SetLookAtPosition(_target.position);
            }
            else
            {
                _timer = Mathf.Clamp01(_timer - Time.deltaTime * 2);
                _animator.SetLookAtWeight(weight);
                _animator.SetLookAtPosition(_target.position);
            }
        }
    }
}