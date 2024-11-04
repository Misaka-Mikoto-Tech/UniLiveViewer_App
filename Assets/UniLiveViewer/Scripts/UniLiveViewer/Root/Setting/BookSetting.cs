using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/BookSetting", fileName = "BookSetting")]
    public class BookSetting : ScriptableObject
    {
        public GameObject PrefabJP => _prefabJP;
        [SerializeField] GameObject _prefabJP;

        public GameObject PrefabEN => _prefabEN;
        [SerializeField] GameObject _prefabEN;
    }
}