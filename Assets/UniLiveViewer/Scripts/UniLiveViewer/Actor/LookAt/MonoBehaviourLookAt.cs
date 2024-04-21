﻿using UnityEngine;

namespace UniLiveViewer.Actor.LookAt
{
    public class MonoBehaviourLookAt : MonoBehaviour
    {
        Animator _animator;
        Transform _lookTarget;
        float _headWeight = 0.0f;
        float _eyeWeight = 0.0f;

        public void Setup(Animator animator, Transform lookTarget)
        {
            _animator = animator;
            _lookTarget = lookTarget;
        }

        public void ChangeHeadWeight(float weight)
        {
            _headWeight = weight;
        }

        public void ChangeEyeWeight(float weight)
        {
            _eyeWeight = weight;
        }

        void OnAnimatorIK()
        {
            if (_animator == null) return;
            //全体、体、頭、目
            _animator.SetLookAtWeight(1.0f, 0.0f, _headWeight, _eyeWeight);
            _animator.SetLookAtPosition(_lookTarget.position);
            //_animator.SetLookAtPosition(_test.lookTarget_limit.position);// TODO
        }
    }
}