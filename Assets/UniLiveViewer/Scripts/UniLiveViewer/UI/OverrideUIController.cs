using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniLiveViewer
{
    public class OverrideUIController : MonoBehaviour
    {
        [SerializeField] private PlayerStateManager player;
        [SerializeField] private FileAccessManager fileManager;
        [SerializeField] private SwitchController switchController;
        private BackGroundController backGroundCon;

        [Header("UI")]
        [SerializeField] private LoadAnimation anime_Loading;
        [SerializeField] private Camera overlayCamera;
        [SerializeField] private Renderer _rendererFade;
        [SerializeField] private Renderer _rendererClosing;

        private MaterialPropertyBlock materialPropertyBlock;
        private Color baseColor;

        private void Awake()
        {
            if (GlobalConfig.sceneMode_static == GlobalConfig.SceneMode.VIEWER)
            {
                backGroundCon = GameObject.FindGameObjectWithTag("BackGroundController").GetComponent<BackGroundController>();
            }

            _rendererClosing.sharedMaterial.SetFloat("_Scala", 0);
            _rendererClosing.enabled = false;

            materialPropertyBlock = new MaterialPropertyBlock();
            _rendererFade.GetPropertyBlock(materialPropertyBlock);

            baseColor = _rendererFade.material.GetColor("_BaseColor");

            //暗転する
            baseColor.a = 1;//不透明に
            materialPropertyBlock.SetColor("_BaseColor", baseColor);
            _rendererFade.SetPropertyBlock(materialPropertyBlock);

            fileManager.onThumbnailCompleted += LoadEnd;
            switchController.onSceneSwitch += SceneEnd;
        }

        private void Start()
        {
            anime_Loading.gameObject.SetActive(true);
        }

        private void LoadEnd()
        {
            StartCoroutine(EndUpdate());
        }

        IEnumerator EndUpdate()
        {
            yield return null;

            //まずloadingアニメーションを消す
            anime_Loading.gameObject.SetActive(false);

            yield return null;

            //暗転する
            baseColor = materialPropertyBlock.GetColor("_BaseColor");
            baseColor.a = 1;//不透明に

            while (baseColor.a >= 0.0f)
            {
                baseColor.a -= Time.deltaTime;

                materialPropertyBlock.SetColor("_BaseColor", baseColor);
                _rendererFade.SetPropertyBlock(materialPropertyBlock);
                yield return null;
            }

            overlayCamera.enabled = false;
            player.enabled = true;//操作可能に
        }

        private void SceneEnd(string sceneName)
        {
            StartCoroutine(SceneEndUpdate(sceneName));
        }

        IEnumerator SceneEndUpdate(string sceneName)
        {
            overlayCamera.enabled = true;
            player.enabled = false;//操作不可に
            yield return null;

            //閉幕演出
            _rendererClosing.enabled = true;
            float t = 0;

            for (int i = 0; i < 50; i++)
            {
                t += Time.deltaTime * (50 - i) * 0.1f;
                _rendererClosing.sharedMaterial.SetFloat("_Scala", t);
                yield return null;
            }

            while (t < 150)
            {
                t += Time.deltaTime * 150;
                _rendererClosing.sharedMaterial.SetFloat("_Scala", t);
                yield return null;
            }

            //skyboxの初期化
            if (backGroundCon) backGroundCon.SetInit();

            anime_Loading.gameObject.SetActive(true);

            //ロードが早すぎても最低演出分は待機する
            var async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;
            yield return new WaitForSeconds(1.0f);
            async.allowSceneActivation = true;
        }
    }

}