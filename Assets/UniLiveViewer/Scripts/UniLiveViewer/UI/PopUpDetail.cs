using UnityEngine;

namespace UniLiveViewer
{
    //TODO：1タブ1?に整理したい
    public class PopUpDetail : MonoBehaviour
    {
        [SerializeField] private Sprite[] sprPrefab = new Sprite[2];
        [SerializeField] private SpriteRenderer sprRender;

        private Animator anime;
        private int hitLayer = 0;
        private bool isPopUp = false;
        private int cnt = 0;

        // Start is called before the first frame update
        void Awake()
        {
            anime = GetComponent<Animator>();
            hitLayer = LayerMask.NameToLayer("Ignore Raycast");

            //言語で差し替える
            if (GlobalConfig.systemData.LanguageCode == (int)SaveData.USE_LANGUAGE.JP)
            {
                sprRender.sprite = sprPrefab[1];
            }
            else
            {
                sprRender.sprite = sprPrefab[0];
            }
        }

        private void OnEnable()
        {
            cnt = 0;
            isPopUp = false;

            anime.enabled = true;
        }

        private void OnDisable()
        {
            anime.enabled = false;
            sprRender.enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (isPopUp)
            {
                cnt++;
                if (cnt > 30)
                {
                    isPopUp = false;
                    anime.SetBool("isDisplay", false);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.layer == hitLayer)
            {
                cnt = 0;
                if (!isPopUp)
                {
                    isPopUp = true;
                    anime.SetBool("isDisplay", true);
                }
            }
        }
    }

}