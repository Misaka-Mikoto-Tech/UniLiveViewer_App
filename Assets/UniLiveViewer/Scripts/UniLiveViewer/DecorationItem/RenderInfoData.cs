using UnityEngine;

namespace UniLiveViewer
{
    [CreateAssetMenu(menuName = "MyGame/Create ParameterTable/RenderInfoData", fileName = "RenderInfoData")]
    public class RenderInfoData : ScriptableObject
    {
        public int materialIndex = 0;
        public string[] targetShaderName = new string[2] { "_BaseMap", "_1stShadeMap" };
        public string[] partsName = new string[2] { "全体","All" };
        public Texture[] chooseableTexture;
        public int textureCurrent = 0;
    }
}