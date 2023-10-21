using System.Linq;
using UnityEngine;

namespace UniLiveViewer.Stage.Gymnasium
{
    public class StageLightChangeService : MonoBehaviour
    {
        [SerializeField] Transform[] _lights = new Transform[5];
        int _currnt = StageEnums.StageLightDefaultIndex;
        IStageLight[] _stagelights;
        bool _isWhite = true;
        int _charaCount;

        void Awake()
        {
            _isWhite = FileReadAndWriteUtility.UserProfile.scene_gym_whitelight;
            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].gameObject.SetActive(i == _currnt);
            }

            _stagelights = _lights
                .Select(t => t.GetComponent<IStageLight>())
                .Where(stageLight => stageLight != null)
                .ToArray();
        }

        public void OnChangeSummonedCount(int count)
        {
            _charaCount = count;
            if (_lights.Length <= _currnt) return;
            _stagelights[_currnt]?.ChangeCount(_charaCount);
        }

        public void OnChangeStageLight(int index)
        {
            _currnt = index;
            for (int i = 0; i < _lights.Length; i++)
            {
                _lights[i].gameObject.SetActive(i == _currnt);
            }

            //各要素反映
            OnChangeSummonedCount(_charaCount);
            OnChangeLightColor(_isWhite);
        }

        public void OnChangeLightColor(bool isWhite)
        {
            _isWhite = isWhite;
            if (_lights.Length <= _currnt) return;
            _stagelights[_currnt].ChangeColor(_isWhite);
        }

        public void OnTick()
        {
            if (_lights.Length <= _currnt) return;
            _stagelights[_currnt].OnUpdate();
        }
    }
}