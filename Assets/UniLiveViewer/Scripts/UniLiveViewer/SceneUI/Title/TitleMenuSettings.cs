using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniLiveViewer.SceneUI.Title
{
    public class TitleMenuSettings : MonoBehaviour
    {
        public Transform UiRoot => _uiRoot;
        [SerializeField] Transform _uiRoot;

        public Transform MainMenuCanvas => _mainMenuCanvas;
        [SerializeField] Transform _mainMenuCanvas;

        public List<Button> MainMenuButton => _mainMenuButton;
        [SerializeField] List<Button> _mainMenuButton;

        public Transform CustomLiveCanvas => _customLiveCanvas;
        [SerializeField] Transform _customLiveCanvas;

        public List<Button> CustomLiveButton => _customLiveButton;
        [SerializeField] List<Button> _customLiveButton;

        public Transform LicenseCanvas => _licenseCanvas;
        [SerializeField] Transform _licenseCanvas;

        public List<Button> LicenseButton => _licenseButton;
        [SerializeField] List<Button> _licenseButton;

        public AudioSourceService AudioSourceService => _audioSourceService;
        [SerializeField] AudioSourceService _audioSourceService;
    }
}