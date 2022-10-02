using System;
using UnityEngine;

namespace UniLiveViewer
{
    public class FacialSyncBase : MonoBehaviour
    {
        [Serializable]
        public class SkinBindInfo
        {
            public SkinnedMeshRenderer skinMesh;//FBXだけ
            public BindInfo[] bindInfo;

            [Serializable]
            public class BindInfo
            {
                public FACIALTYPE facialType;
                public Transform node;
                public KeyPair[] keyPair;

                [Serializable]
                public class KeyPair
                {
                    public string name;
                    [HideInInspector] public int index;
                }
            }
        }

        [SerializeField] AnimationCurve _weightCurve;

        [Header("<VRMはkeyName不要>")]
        public SkinBindInfo[] _skinBindInfo;

        // Start is called before the first frame update
        protected virtual void Start()
        {

        }

        /// <summary>
        /// シェイプキーを全て初期化する
        /// </summary>
        public virtual void MorphReset()
        {

        }

        // Update is called once per frame
        protected virtual void Update()
        {

        }
        protected float GetWeight(Transform tr)
        {
            return _weightCurve.Evaluate(tr.localPosition.z);
        }
    }
}
