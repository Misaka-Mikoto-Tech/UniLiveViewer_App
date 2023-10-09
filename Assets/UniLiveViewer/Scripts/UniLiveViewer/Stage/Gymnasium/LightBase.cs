using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class LightBase : MonoBehaviour, IStageLight
    {
        const string PropertyName = "_TintColor";

        [SerializeField] MeshRenderer[] _lights;
        [SerializeField] AnimationCurve _collarCurveR = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _collarCurveG = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _collarCurveB = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] float _colorSpeed = 1;

        bool _isWhitelight = true;
        float _colorTimer = 0;

        void Start()
        {
        }

        public void ChangeCount(int count)
        {
            for (int i = 0; i < _lights.Length; i++)
            {
                var enable = i < count;
                if (_lights[i].gameObject.activeSelf == enable) continue;
                _lights[i].gameObject.SetActive(enable);
            }
        }

        public void ChangeColor(bool isWhite)
        {
            _isWhitelight = isWhite;
            if (!_isWhitelight) return;

            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].sharedMaterial.SetColor(PropertyName, Color.white);
                _lights[i].sharedMaterial.color = Color.white;
            }
        }

        public void OnUpdate()
        {
            UpdateColor();
        }

        void UpdateColor()
        {
            if (_isWhitelight) return;

            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].sharedMaterial.SetColor
                    (PropertyName,
                    new Color(
                        _collarCurveR.Evaluate(_colorTimer),
                        _collarCurveG.Evaluate(_colorTimer),
                        _collarCurveB.Evaluate(_colorTimer)
                        )
                    );
            }
            _colorTimer += Time.deltaTime * _colorSpeed;
            if (_colorTimer > 1.05f) _colorTimer = 0;
        }
    }
}