using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer.Menu.Config.Sound
{
    public class SoundMenuSettings : MonoBehaviour
    {
        public IReadOnlyList<SliderGrabController> SoundSlider => _soundSlider;
        [SerializeField] List<SliderGrabController> _soundSlider;

        public IReadOnlyList<TextMesh> SoundText => _soundText;
        [SerializeField] List<TextMesh> _soundText;

        void Start()
        {
        }
    }
}