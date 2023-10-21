using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer.Stage.Gymnasium
{
    public class AutoLight : MonoBehaviour, IStageLight
    {
        const string PropertyName = "_TintColor";

        [SerializeField] MeshRenderer[] _lights;
        [SerializeField] AnimationCurve _colorCurveR = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _colorCurveG = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _colorCurveB = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] float _colorSpeed = 1;

        bool _isWhitelight = true;
        float _colorTimer = 0;

        void Start()
        {
        }

        void IStageLight.ChangeCount(int count)
        {
            //for (int i = 0; i < _lights.Length; i++)
            //{
            //    var enable = i < count;
            //    if (_lights[i].gameObject.activeSelf == enable) continue;
            //    _lights[i].gameObject.SetActive(enable);
            //}
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
                        _colorCurveR.Evaluate(_colorTimer),
                        _colorCurveG.Evaluate(_colorTimer),
                        _colorCurveB.Evaluate(_colorTimer)
                        )
                    );
            }
            _colorTimer += Time.deltaTime * _colorSpeed;
            if (_colorTimer > 1.05f) _colorTimer = 0;
        }
    }
}