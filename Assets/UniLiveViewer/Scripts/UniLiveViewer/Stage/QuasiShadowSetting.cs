using System;
using UnityEngine;

namespace UniLiveViewer
{
    public enum SHADOWTYPE
    {
        NONE,
        CIRCLE,
        CROSS,
        NONE_CIRCLE,
        NONE_CROSS,
        CIRCLE_CIRCLE,
        CROSS_CROSS,
        CIRCLE_CROSS,
        CROSS_CIRCLE,
    }

    // SerializeField沢山
    public class QuasiShadowSetting : MonoBehaviour
    {
        public SHADOWTYPE ShadowType
        {
            get { return _shadowType; }
            set
            {
                _shadowType = value;
                if ((int)_shadowType >= _typeLength) _shadowType = 0;
                else if ((int)_shadowType < 0) _shadowType = (SHADOWTYPE)(_typeLength - 1);
            }
        }
        [Header("確認用露出(readonly)")]
        [SerializeField] SHADOWTYPE _shadowType = SHADOWTYPE.NONE;
        int _typeLength = Enum.GetNames(typeof(SHADOWTYPE)).Length;

        public MeshRenderer MeshRendererPrefab => _meshRendererPrefab;
        [SerializeField] MeshRenderer _meshRendererPrefab;

        public float ShadowScale => _shadowScale;
        [SerializeField] float _shadowScale = 1.0f;

        public Preset[] Presets => _preset;
        [SerializeField] Preset[] _preset;

        public float FootRay => _footRay;
        [SerializeField] float _footRay = 0.05f;

        public bool IsStepSE => _isStepSE;
        [SerializeField] bool _isStepSE = false;

        public AudioClip[] Sounds => _sound;
        [Space(10), Header("サウンド")]
        [SerializeField] AudioClip[] _sound;//UI開く,UI閉じる

        public void OnStart()
        {
            _isStepSE = StageSettingService.UserProfile.StepSE;
            _shadowScale = StageSettingService.UserProfile.CharaShadow;
        }

        [Serializable]
        public class Preset
        {
            public SHADOWTYPE shadowType;
            public Texture2D texture_Body;
            public Texture2D texture_Foot;
            public float scala_Body;
            public float scala_Foot;
        }

        public void SetStepSE(bool value)
        {
            _isStepSE = value;
        }

        public void SetShadowScale(float value)
        {
            _shadowScale = value;
        }
    }
}
