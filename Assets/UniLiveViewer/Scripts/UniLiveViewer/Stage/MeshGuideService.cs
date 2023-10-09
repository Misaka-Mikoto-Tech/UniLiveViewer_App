using UniLiveViewer.Timeline;
using UnityEngine;

namespace UniLiveViewer
{
    //暫定的
    public class MeshGuideService
    {
        GameObject _guidePrefab;
        bool _isShow = false;
        public bool IsShow 
        { 
            get { return _isShow; } 
            set 
            { 
                if(_isShow != value)
                {
                    _isShow = value;

                    for (int i = 0; i < _pair.Length; i++)
                    {
                        if (i == TimelineController.PORTAL_INDEX) continue;
                        if (!_timeline.BindCharaMap[i]) _pair[i].guideMesh.enabled = false;
                        else _pair[i].guideMesh.enabled = _isShow; 
                    }
                }
            } 
        }
        Pair[] _pair;
        Vector3 _distance = Vector3.zero;
        TimelineController _timeline;

        MeshGuideService()
        {
            
        }

        public void OnStart(TimelineController timeline)
        {
            _timeline = timeline;
            _pair = new Pair[_timeline.BindCharaMap.Count];
            _guidePrefab = Resources.Load<GameObject>("Prefabs/GuideBody");
            
            var anchor = new GameObject("GuideMeshs");
            for (int i = 0; i < _pair.Length; i++)
            {
                _pair[i] = new Pair();
                _pair[i].guideMesh = GameObject.Instantiate(_guidePrefab).GetComponent<MeshRenderer>();
                _pair[i].guideMesh.transform.parent = anchor.transform;
                _pair[i].guideMesh.enabled = false;//非表示にしておく
            }
        }

        public void OnFieldCharacterCount()
        {
            for (int i = 0; i < _pair.Length; i++)
            {
                if (i == TimelineController.PORTAL_INDEX) continue;
                var chara = _timeline.BindCharaMap[i];
                if (!chara)
                {
                    _pair[i].charaController = null;
                    _pair[i].head = null;
                    _pair[i].guideMesh.enabled = false;
                }
                else
                {
                    _pair[i].charaController = chara;
                    _pair[i].head = chara.GetAnimator.GetBoneTransform(HumanBodyBones.Head);
                }
            }
        }

        public void OnTick()
        {
            if (!_isShow) return;

            for (int i = 0; i < _pair.Length; i++)
            {
                if (i == TimelineController.PORTAL_INDEX) continue;
                if (_pair[i].charaController)
                {
                    _pair[i].guideMesh.transform.position = _pair[i].charaController.transform.position;
                    _distance = _pair[i].head.position - _pair[i].guideMesh.transform.position;
                    _pair[i].guideMesh.transform.forward = _distance;
                }
            }
        }
    }

    public class Pair
    {
        public CharaController charaController;
        public Transform head;
        public MeshRenderer guideMesh;
    }
}