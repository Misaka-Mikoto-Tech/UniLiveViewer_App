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

        public Transform LicenseCanvas => _licenseCanvas;
        [SerializeField] Transform _licenseCanvas;

        public AudioSourceService AudioSourceService => _audioSourceService;
        [SerializeField] AudioSourceService _audioSourceService;
    }
}