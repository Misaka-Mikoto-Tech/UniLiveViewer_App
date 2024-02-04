using System.Collections.Generic;
using UniLiveViewer.SceneLoader;
using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/PlayerConfigData", fileName = "PlayerConfigData")]
    public class PlayerConfigData : ScriptableObject
    {
        public IReadOnlyList<LocationMap> Map => _map;
        [SerializeField] List<LocationMap> _map;

        public KeyConfig LeftKeyConfig => _leftKeyConfig;
        [SerializeField] KeyConfig _leftKeyConfig;
        public KeyConfig RightKeyConfig => _rightKeyConfig;
        [SerializeField] KeyConfig _rightKeyConfig;


        [System.Serializable]
        public class LocationMap
        {
            public SceneType SceneType;
            public Vector3 InitializePosition;
            public Vector3 InitializeRotation;
        }


        [System.Serializable]
        public class KeyConfig
        {
            [Header("アナログスティック")]
            public OVRInput.RawAxis2D thumbstick;
            [Header("プレイヤーや魔法陣の回転")]
            public OVRInput.RawButton rotate_L;
            public OVRInput.RawButton rotate_R;
            [Header("キャラのリサイズ")]
            public OVRInput.RawButton resize_D;
            public OVRInput.RawButton resize_U;
            [Header("アクション(ラインセレクターなど)")]
            public OVRInput.RawButton action;
            [Header("メイン・サブUI")]
            public OVRInput.RawButton menuUI;
            [Header("アタッチなど")]
            public OVRInput.RawButton trigger;
        }
    }
}