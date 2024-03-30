using UnityEngine;

namespace UniLiveViewer.Menu.Config.Common
{
    public class CommonMenuSettings : MonoBehaviour
    {
        public Button_Base VibrationButton => _vibrationButton;
        [SerializeField] Button_Base _vibrationButton;

        public Button_Base PassthroughButton => _passthroughButton;
        [SerializeField] Button_Base _passthroughButton;

        public SliderGrabController FixedFoveatedSlider => _fixedFoveatedSlider;
        [SerializeField] SliderGrabController _fixedFoveatedSlider;

        public TextMesh FixedFoveatedText => _fixedFoveatedText;
        [SerializeField] TextMesh _fixedFoveatedText;
    }
}
