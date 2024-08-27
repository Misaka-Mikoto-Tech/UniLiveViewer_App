using UnityEngine;

namespace UniLiveViewer.Stage.Title
{
    public class TitleSceneSettings : MonoBehaviour
    {
        public TextMesh AppVersionText => _appVersionText;
        [SerializeField] TextMesh _appVersionText;

        public OVRScreenFade OvrScreenFade => _ovrScreenFade;
        [SerializeField] OVRScreenFade _ovrScreenFade;
    }
}