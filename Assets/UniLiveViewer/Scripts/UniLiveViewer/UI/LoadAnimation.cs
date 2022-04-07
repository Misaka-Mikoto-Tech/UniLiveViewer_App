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
            current = Random.Range(0, 2);
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            anime.SetBool(sLoadAnime[current], false);
        }

        private void OnEnable()
        {
            //�V�[�����[�h���݂̂̏���
            SceneLoad();

            //���[�f�B���O�A�j���[�V�����������_���ݒ�
            current = Random.Range(0, 2);

            //���[�f�B���O�A�j���[�V�����J�n
            anime.gameObject.SetActive(true);
            anime.SetBool(sLoadAnime[current], true);
        }

        /// <summary>
        /// �V�[�����[�h���̂�
        /// </summary>
        private void SceneLoad()
        {
            if (sceneName)
            {
                switch (GlobalConfig.sceneMode_static)
                {
                    case GlobalConfig.SceneMode.CANDY_LIVE:
                        sceneName.text = "��CRS Live��";
                        break;
                    case GlobalConfig.SceneMode.KAGURA_LIVE:
                        sceneName.text = "��KAGURA Live��";
                        break;
                    case GlobalConfig.SceneMode.VIEWER:
                        sceneName.text = "��ViewerScene��";
                        break;
                }
            }
        }
    }

}