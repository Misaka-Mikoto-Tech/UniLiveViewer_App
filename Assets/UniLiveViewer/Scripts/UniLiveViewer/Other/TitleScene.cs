using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniLiveViewer 
{
    public class TitleScene : MonoBehaviour
    {
        public OVRScreenFade fade;
        [SerializeField] private Sprite[] sprPrefab = new Sprite[2];
        [SerializeField] private SpriteRenderer sprRender;
        [SerializeField] private TextMesh text_Version;
        [SerializeField] private Button_Base[] btn_Language = new Button_Base[2];

        [SerializeField] private GameObject manualHand;
        [SerializeField] private Transform[] Chara = new Transform[3];

        //クリックSE
        private AudioSource audioSource;
        [SerializeField] private AudioClip[] Sound;//ボタン音

        private void Awake()
        {
            for (int i = 0; i < btn_Language.Length; i++)
            {
                btn_Language[i].onTrigger += onChangeLanguage;
            }

            text_Version.text = "ver." + Application.version;

            manualHand.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = GlobalConfig.soundVolume_SE;

            //randomにキャラが変わる
            int r = Random.Range(0, 3);
            for (int i = 0; i < 3; i++)
            {
                if (r == i) Chara[i].gameObject.SetActive(true);
                else Chara[i].gameObject.SetActive(false);
            }

            if (GlobalConfig.systemData.LanguageCode != (int)SaveData.USE_LANGUAGE.NULL)
            {
                //2回目以降
                sprRender.gameObject.SetActive(false);
                StartCoroutine(SceneChange());
            }
            else
            {
                StartCoroutine(InitHand());
            }
        }

        private IEnumerator InitHand()
        {
            yield return new WaitForSeconds(2.0f);
            transform.GetChild(0).gameObject.SetActive(true);
            yield return new WaitForSeconds(1.0f);
            manualHand.SetActive(true);
        }

        private void onChangeLanguage(Button_Base btn)
        {
            if (btn.name.Contains("_JP"))
            {
                GlobalConfig.systemData.LanguageCode = (int)SaveData.USE_LANGUAGE.JP;
                SaveData.SetJson_SystemData(GlobalConfig.systemData);
                //差し替える
                sprRender.sprite = sprPrefab[1];
            }
            else
            {
                GlobalConfig.systemData.LanguageCode = (int)SaveData.USE_LANGUAGE.EN;
                SaveData.SetJson_SystemData(GlobalConfig.systemData);
                //差し替える
                sprRender.sprite = sprPrefab[0];
            }

            //クリック音
            audioSource.PlayOneShot(Sound[0]);
            //Handを消す
            manualHand.SetActive(false);

            StartCoroutine(SceneChange());
        }

        private IEnumerator SceneChange()
        {
            yield return new WaitForSeconds(0.5f);
            if (transform.GetChild(0).gameObject.activeSelf) transform.GetChild(0).gameObject.SetActive(false);

            yield return new WaitForSeconds(1.0f);
            fade.FadeOut();
            yield return new WaitForSeconds(2.0f);
            SceneManager.LoadSceneAsync("LiveScene");
            yield return null;
        }
    }

}
