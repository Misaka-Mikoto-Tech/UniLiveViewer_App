using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class RandomLight : MonoBehaviour, IStageLight
    {
        const float MAX_INTERVAL = 0.25f;
        const string PropertyName = "_TintColor";

        [SerializeField] MeshRenderer[] _lights;
        [SerializeField] AnimationCurve _collarCurveR = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _collarCurveG = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _collarCurveB = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] float _colorSpeed = 1;
        [SerializeField] float _maxLifeTime = 1.00f;

        bool _isWhitelight = true;
        float _colorTimer = 0;
        float _interval;
        float[] _timer;

        void Start()
        {
            _interval = MAX_INTERVAL;
            _timer = new float[_lights.Length];
            for (int i = 0; i < _timer.Length; i++)
            {
                _timer[i] = _maxLifeTime;
            }
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
            _interval -= Time.deltaTime;
            if (_interval < 0)
            {
                _interval = MAX_INTERVAL;

                int index = Random.Range(0, _lights.Length);
                if (!_lights[index].gameObject.activeSelf) _lights[index].gameObject.SetActive(true);
                _lights[index].transform.localRotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(-205, -155)));

                if (!_isWhitelight)
                {
                    _lights[index].sharedMaterial.SetColor
                            (PropertyName,
                            new Color(
                                Random.Range(0, 1.0f),
                                Random.Range(0, 1.0f),
                                Random.Range(0, 1.0f)
                                )
                            );
                }
            }

            for (int i = 0; i < _lights.Length; i++)
            {
                if (_lights[i].gameObject.activeSelf)
                {
                    _timer[i] -= Time.deltaTime;
                    if (_timer[i] < 0)
                    {
                        _timer[i] = _maxLifeTime;
                        _lights[i].gameObject.SetActive(false);
                    }
                }
            }

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
