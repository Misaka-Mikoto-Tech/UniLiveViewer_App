using System.Collections.Generic;
using UniLiveViewer.Timeline;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UniLiveViewer
{
    public class LookAtLight : MonoBehaviour, IStageLight
    {
        const HumanBodyBones TargetHumanBodyBone = HumanBodyBones.Spine;
        const string PropertyName = "_TintColor";

        [SerializeField] MeshRenderer[] _lights;
        [SerializeField] AnimationCurve _collarCurveR = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _collarCurveG = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _collarCurveB = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] float _colorSpeed = 1;

        bool _isWhitelight = true;
        float _colorTimer = 0;
        Vector3 _distance;

        Transform[] _targetBones;
        List<Transform> _targetList;
        TimelineController _timeline;

        void Start()
        {
            _targetBones = new Transform[SystemInfo.MaxFieldChara];
            _targetList = new List<Transform>();

            var container = LifetimeScope.Find<TimelineLifetimeScope>().Container;
            _timeline = container.Resolve<TimelineController>();
        }

        public void ChangeCount(int count)
        {
            for (int i = 0; i < _lights.Length; i++)
            {
                var enable = i < count;
                if (_lights[i].gameObject.activeSelf == enable) continue;
                _lights[i].gameObject.SetActive(enable);
            }


            _targetList.Clear();
            for (int i = 0; i < _targetBones.Length; i++)
            {
                var portalChara = _timeline.BindCharaMap[i + 1];
                if (!portalChara) _targetBones[i] = null;
                else
                {
                    _targetBones[i] = portalChara.GetAnimator.GetBoneTransform(TargetHumanBodyBone);
                    _targetList.Add(_targetBones[i]);
                }
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
            for (int i = 0; i < _targetList.Count; i++)
            {
                _distance = _targetList[i].position - _lights[i].transform.position;
                _lights[i].transform.up = _distance;
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