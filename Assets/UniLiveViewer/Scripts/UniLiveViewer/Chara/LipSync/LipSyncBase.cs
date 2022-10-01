using System;
using UnityEngine;

namespace UniLiveViewer 
{
    public class LipSyncBase : MonoBehaviour
    {
        [Serializable]
        public class BindInfo
        {
            public LIPTYPE lipType;
            public Transform node;
            public string keyName;
            [HideInInspector] public SkinnedMeshRenderer skinMesh;
            [HideInInspector] public int keyIndex;
        }

        [SerializeField] AnimationCurve _weightCurve;

        [Header("<VRMはkeyName不要>")]
        public BindInfo[] _bindInfo;

        // Start is called before the first frame update
        protected virtual void Start()
        {
            
        }

        public virtual void MorphReset()
        {

        }

        protected virtual void LateUpdate()
        {

        }

        protected float GetWeight(Transform tr)
        {
            return _weightCurve.Evaluate(tr.localPosition.z);
        }
    }

}