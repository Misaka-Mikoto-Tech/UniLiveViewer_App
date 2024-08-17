using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UniLiveViewer.Menu.Config.Graphics
{
    public class GraphicsMenuSettings : MonoBehaviour
    {
        public IReadOnlyList<Button_Base> GraphicButton => _graphicButton;
        [SerializeField] List<Button_Base> _graphicButton;

        public IReadOnlyList<SliderGrabController> GraphicSlider => _graphicSlider;
        [SerializeField] List<SliderGrabController> _graphicSlider;

        public SliderGrabController OutlineSlider => _outlineSlider;
        [SerializeField] SliderGrabController _outlineSlider;

        [SerializeField] UniversalRendererData _frd;

        public Material OutlineMat => _outlineMat;
        [SerializeField] Material _outlineMat;
        public ScriptableRendererFeature OutlineRender => _outlineRender;
        [SerializeField] ScriptableRendererFeature _outlineRender;

        void Awake()
        {
            //レンダーパイプラインからoutlineオブジェクトを取得    
            foreach (var renderObj in _frd.rendererFeatures)
            {
                if (renderObj.name == "Outline")
                {
                    _outlineRender = renderObj;
                    break;
                }
            }
            _outlineRender.SetActive(false);
        }
    }
}