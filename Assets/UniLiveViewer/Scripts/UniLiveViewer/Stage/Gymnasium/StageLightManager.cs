using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class StageLightManager : MonoBehaviour
    {
        [SerializeField] private Material[] shadowMat = new Material[5];
        [SerializeField] private int currnt = 0;
        private Transform[] targets;

        // Start is called before the first frame update
        void Start()
        {
            targets = new Transform[transform.childCount];
            for (int i = 0;i<targets.Length;i++)
            {
                targets[i] = transform.GetChild(i);
            }

            SetStageLight(0, StageSettingService.UserProfile.scene_gym_whitelight, out string str);
        }

        // Update is called once per frame
        public void SetStageLight(int moveIndex, bool isWhite, out string resultName)
        {
            resultName = "";
            currnt += moveIndex;
            if (targets.Length <= currnt) currnt = 0;
            else if (currnt < 0) currnt = targets.Length - 1;

            bool isEnabel;
            for (int i = 0;i< targets.Length;i++)
            {
                isEnabel = i == currnt;
                if (targets[i].gameObject.activeSelf != isEnabel) targets[i].gameObject.SetActive(isEnabel);
                if(isEnabel) resultName = targets[currnt].name;
            }
            SetLightColor(isWhite);
        }

        public void SetLightColor(bool isWhite)
        {
            var lightBase = targets[currnt].GetComponent<LightBase>();
            if (lightBase) lightBase.SetLightCollar(isWhite);
        }
    }
}