using UnityEngine;

namespace UniLiveViewer
{
    public class LoadAnimation : MonoBehaviour
    {
        private Animator anime;
        private int current = 0;
        private string[] sLoadAnime = new string[2] { "isType01", "isType02" };
        [SerializeField] private TextMesh sceneName;

        private void Awake()
        {
            anime = GetComponent<Animator>();
        }

        private void OnDisable()
        {
            anime.SetBool(sLoadAnime[current], false);
        }

        private void OnEnable()
        {
            if (sceneName)
            {
                sceneName.text = SystemInfo.sceneMode switch
                {
                    SceneMode.CANDY_LIVE => "★CRS Live★",
                    SceneMode.KAGURA_LIVE => "★KAGURA Live★",
                    SceneMode.VIEWER => "★ViewerScene★",
                    SceneMode.GYMNASIUM => "★Gymnasium★",
                    _ => "",
                };
            }

            //ローディングアニメーションをランダム設定
            current = Random.Range(0, 2);

            //ローディングアニメーション開始
            anime.gameObject.SetActive(true);
            anime.SetBool(sLoadAnime[current], true);
        }
    }
}