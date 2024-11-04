using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/DecorationItemSettings", fileName = "DecorationItemSettings")]
    public class DecorationItemSettings : ScriptableObject
    {
        [Header("＜各ページに相当＞")]
        [SerializeField] DecorationItems[] _itemPrefab;
        public DecorationItems[] ItemPrefab => _itemPrefab;

        [System.Serializable]
        public class DecorationItems
        {
            public DecorationItemInfo[] ItemPrefab;
        }
    }
}