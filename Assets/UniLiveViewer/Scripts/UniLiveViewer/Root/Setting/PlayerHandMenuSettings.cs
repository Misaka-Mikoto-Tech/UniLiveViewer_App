using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/PlayerHandMenuSettings", fileName = "PlayerHandMenuSettings")]
    public class PlayerHandMenuSettings : ScriptableObject
    {
        public GameObject ActorManipulate => _actorManipulate;
        [SerializeField] GameObject _actorManipulate;

        public GameObject ItemMaterialSelection => _itemMaterialSelection;
        [SerializeField] GameObject _itemMaterialSelection;

        public GameObject CameraHeighte => _cameraHeighte;
        [SerializeField] GameObject _cameraHeighte;
    }

    public class HandMenu
    {
        public GameObject Instance => _instance;
        GameObject _instance;
        TextMesh _textMesh;
        public bool IsShow => _isShow;
        bool _isShow = false;

        public HandMenu(GameObject instance, Transform parentAnchor)
        {
            _instance = instance;
            _instance.transform.parent = parentAnchor;
            _instance.transform.localPosition = Vector3.zero;
            _instance.transform.localRotation = Quaternion.identity;
            _textMesh = _instance.transform.GetChild(0).GetComponent<TextMesh>();
        }

        public void SetText(string text)
        {
            _textMesh.text = text;
        }

        public void SetShow(bool isShow)
        {
            if (_instance.activeSelf != isShow) _instance.SetActive(isShow);
            _isShow = isShow;
        }

        public void UpdateLookat(Transform target)
        {
            if (!_instance.activeSelf) return;
            _instance.transform.LookAt(target);
        }
    }
}