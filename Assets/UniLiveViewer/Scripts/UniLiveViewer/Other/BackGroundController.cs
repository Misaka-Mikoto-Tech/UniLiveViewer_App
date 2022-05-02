using TunnelEffect;
using UnityEngine;

namespace UniLiveViewer
{
    public class BackGroundController : MonoBehaviour
    {
        [SerializeField] private int currntMaster = 0;
        [SerializeField] private int currntCubemap = 0;
        [SerializeField] private int currntHole = 0;
        [SerializeField] private int currntParticle = 0;

        [Header("＜キューブマップ＞")]
        [SerializeField] private Material cubemap_Mat = null;
        public Material GetCubemapMat() { return cubemap_Mat; }
        [SerializeField] private Cubemap[] cubemapList = null;

        [Header("＜トンネル＞")]
        [SerializeField] private TunnelFX2 tunnelAnchor;
        [SerializeField] private int[] tunnelList = new int[0];

        [Header("＜パーティクル＞")]
        [SerializeField] private Transform[] particleAnchors = new Transform[0];

        // Start is called before the first frame update
        void Start()
        {
            string str;
            SetCubemap(0, out str);
            SetWormHole(0, out str);
            SetParticle(0, out str);
        }

        public void SetInit()
        {
            currntMaster = 0;
            currntCubemap = 0;
            currntHole = 0;
            currntParticle = 0;

            //キューブマップを初期化
            cubemap_Mat.SetTexture("_Tex", cubemapList[currntCubemap]);

            //トンネルは無効化
            if (tunnelAnchor.gameObject.activeSelf) tunnelAnchor.gameObject.SetActive(false);

            //パーティクル無効化

        }

        /// <summary>
        /// passthrough用
        /// </summary>
        public void Clear_CubemapTex()
        {
            string str;
            //cubemap_Mat.SetTexture("_Tex", null);
            //ワームホールを無効化しておく
            currntHole = 0;
            SetWormHole(0, out str);
        }

        /// <summary>
        /// キューブマップを変更
        /// </summary>
        /// <param name="moveIndex"></param>
        public void SetCubemap(int moveIndex, out string resultCurrent)
        {
            currntCubemap += moveIndex;
            if (cubemapList.Length <= currntCubemap) currntCubemap = 0;
            else if (currntCubemap < 0) currntCubemap = cubemapList.Length - 1;

            cubemap_Mat.SetTexture("_Tex", cubemapList[currntCubemap]);
            resultCurrent = $"{currntCubemap}";
        }

        /// <summary>
        /// ワームホールを変更
        /// </summary>
        /// <param name="moveIndex"></param>
        public void SetWormHole(int moveIndex, out string resultCurrent)
        {
            currntHole += moveIndex;
            if (tunnelList.Length <= currntHole) currntHole = 0;
            else if (currntHole < 0) currntHole = tunnelList.Length - 1;

            if (currntHole == 0)
            {
                if (tunnelAnchor.gameObject.activeSelf) tunnelAnchor.gameObject.SetActive(false);
                resultCurrent = "None";
            }
            else
            {
                if (!tunnelAnchor.gameObject.activeSelf) tunnelAnchor.gameObject.SetActive(true);
                TunnelFX2.instance.preset = (TUNNEL_PRESET)tunnelList[currntHole];

                resultCurrent = $"{currntHole}";
            }
        }

        /// <summary>
        /// パーティクルを変更
        /// </summary>
        /// <param name="moveIndex"></param>
        public void SetParticle(int moveIndex, out string resultCurrent)
        {
            currntParticle += moveIndex;
            if (particleAnchors.Length <= currntParticle) currntParticle = 0;
            else if (currntParticle < 0) currntParticle = particleAnchors.Length - 1;

            bool setFlag = false;
            for (int i = 0; i < particleAnchors.Length; i++)
            {
                setFlag = (i == currntParticle);
                if (particleAnchors[i].gameObject.activeSelf != setFlag) particleAnchors[i].gameObject.SetActive(setFlag);
            }

            if (currntParticle == 0) resultCurrent = "None";
            else resultCurrent = particleAnchors[currntParticle].name;
        }
    }
}