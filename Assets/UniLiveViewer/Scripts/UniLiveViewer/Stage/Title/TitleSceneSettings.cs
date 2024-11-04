using UnityEngine;

namespace UniLiveViewer.Stage.Title
{
    public class TitleSceneSettings : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer => _spriteRenderer;
        [SerializeField] SpriteRenderer _spriteRenderer;

        public TextMesh AppVersionText => _appVersionText;
        [SerializeField] TextMesh _appVersionText;

        public OVRScreenFade OvrScreenFade => _ovrScreenFade;
        [SerializeField] OVRScreenFade _ovrScreenFade;

        public GameObject ScalingEffect => _scalingEffect;
        [SerializeField] GameObject _scalingEffect;
    }
}