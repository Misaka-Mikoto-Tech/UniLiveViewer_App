using UnityEngine;

namespace UniLiveViewer.Menu
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererSwitcher : MonoBehaviour
    {
        [SerializeField] Sprite[] _prefab;
        SpriteRenderer _render;

        void Start()
        {
            _render = GetComponent<SpriteRenderer>();
        }

        public void SetSprite(int index)
        {
            _render.sprite = _prefab[index];
        }
    }
}
