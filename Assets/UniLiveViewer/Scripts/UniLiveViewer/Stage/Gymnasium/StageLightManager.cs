using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniLiveViewer
{
    public class StageLightManager : MonoBehaviour
    {
        [SerializeField] int currnt = 0;
        [SerializeField] Transform[] _lights = new Transform[5];
        IStageLight[] _stagelights;

        void Start()
        {
            SetStageLight(0, FileReadAndWriteUtility.UserProfile.scene_gym_whitelight, out string str);

            _stagelights = _lights
                .Select(t => t.GetComponent<IStageLight>())
                .Where(i => i != null)
                .ToArray();
        }

        public void OnUpdateSummonedCount(int count)
        {
            if (currnt < _lights.Length) return;
            _stagelights[currnt]?.ChangeCount(count);
        }

        public void SetStageLight(int moveIndex, bool isWhite, out string resultName)
        {
            resultName = "";
            currnt += moveIndex;
            if (_lights.Length <= currnt) currnt = 0;
            else if (currnt < 0) currnt = _lights.Length - 1;

            SetLightColor(isWhite);
        }

        public void SetLightColor(bool isWhite)
        {
            if (currnt < _lights.Length) return;
            _stagelights[currnt]?.ChangeColor(isWhite);
        }

        void Update()
        {
            if (currnt < _lights.Length) return;
            _stagelights[currnt]?.OnUpdate();
        }
    }
}