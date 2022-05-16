using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer
{
    public class ItemPage : MonoBehaviour
    {
        [System.Serializable]
        public class DecorationItems
        {
            public DecorationItemInfo[] ItemPrefab;
        }
        [SerializeField] private MenuManager menuManager;
        private const int SUBPAGE_ITEMS_ROW = 2;
        private const int SUBPAGE_ITEMS_COL = 3;
        private int SUBPAGE_ITEMS_MAX = SUBPAGE_ITEMS_ROW * SUBPAGE_ITEMS_COL;
        private static readonly Vector2[] itemOffsetPos = { 
            new Vector2(-0.24f, 0.08f), new Vector2(0, 0.08f), new Vector2(0.24f, 0.08f),    
            new Vector2(-0.24f,-0.10f), new Vector2(0,-0.10f),new Vector2(0.24f,-0.10f),
        };
        private static readonly Quaternion reverseQuaternion = Quaternion.Euler(new Vector3(0, 180, 0));
        [SerializeField] private Button_Base[] btn_Item = new Button_Base[2];
        [SerializeField] private TextMesh textMesh;

        [SerializeField] private PageController pageController;
        [SerializeField] private Transform itemMaterialAnchor;
        [SerializeField] private GameObject itemMaterialPrefab;
        [SerializeField] private int[] currentSubPage;

        [Header("＜各ページに相当＞")]
        [SerializeField] private DecorationItems[] decorationItems;

        private PlayerStateManager playerStateManager;
        private CancellationToken cancellation_token;
        private int languageCurrent;

        private void Awake()
        {
            cancellation_token = this.GetCancellationTokenOnDestroy();

            //その他
            for (int i = 0; i < btn_Item.Length; i++)
            {
                btn_Item[i].onTrigger += MoveIndex_Item;
            }

            languageCurrent = (int)SystemInfo.userProfile.LanguageCode - 1;
            currentSubPage = new int[decorationItems.Length];
            playerStateManager = PlayerStateManager.instance;

            EnablePassthrough(playerStateManager.myOVRManager.isInsightPassthroughEnabled);

            pageController.onSwitchPage += Init;
        }
        // Start is called before the first frame update
        void Start()
        {
            
        }

        private void Init()
        {
            EnablePassthrough(playerStateManager.myOVRManager.isInsightPassthroughEnabled);

            //アイテム数に応じてサブページ送りボタンの表示切替
            int itemLength = decorationItems[pageController.current].ItemPrefab.Length;
            bool isOver = itemLength > SUBPAGE_ITEMS_MAX;
            for (int i = 0; i < btn_Item.Length; i++)
            {
                if(btn_Item[i].gameObject.activeSelf != isOver) btn_Item[i].gameObject.SetActive(isOver);
            }

            //アクティブページのアイテムを生成
            GenerateItems();
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
        }
        private void EnablePassthrough(bool isEnable)
        {
            if (pageController.BtnTab[5].gameObject.activeSelf != isEnable)
            {
                pageController.BtnTab[5].gameObject.SetActive(isEnable);
                pageController.BtnTab[5].isEnable = false;
            }
        }

        /// <summary>
        /// アイテム変更ボタン
        /// </summary>
        /// <param name="btn"></param>
        private void MoveIndex_Item(Button_Base btn)
        {
            int nowSubPage = currentSubPage[pageController.current];
            int itemLength = decorationItems[pageController.current].ItemPrefab.Length;
            int maxSubPage = itemLength / SUBPAGE_ITEMS_MAX + (itemLength % SUBPAGE_ITEMS_MAX == 0 ? 0:1) - 1;

            for (int i = 0; i < 2; i++)
            {
                //押されたボタンの判別
                if (btn_Item[i] == btn)
                {
                    int moveIndex = 0;
                    if (i == 0) moveIndex = -1;
                    else if (i == 1) moveIndex = 1;

                    //元サブページのアイテムを削除
                    DeleteItems();

                    //サブページ移動
                    nowSubPage += moveIndex;
                    if (nowSubPage > maxSubPage) nowSubPage = 0;
                    else if (nowSubPage < 0) nowSubPage = maxSubPage;
                    currentSubPage[pageController.current] = nowSubPage;

                    //アクティブページのアイテムを生成
                    GenerateItems();

                    //クリック音
                    menuManager.PlayOneShot(SoundType.BTN_CLICK);
                    break;
                }
            }
        }

        /// <summary>
        /// 現在のページとサブページを基準にアイテムを全削除
        /// </summary>
        private void DeleteItems()
        {
            Transform targetPage = pageController.GetCurrentPage();
            int max = targetPage.childCount;

            for (int i = 0; i< max; i++)
            {
                Destroy(targetPage.GetChild(max - i - 1).gameObject);//常に末尾を削除
            }
        }

        /// <summary>
        /// 現在のページとサブページを基準にアイテムが無ければ生成
        /// </summary>
        private void GenerateItems()
        {
            int nowSubPage = currentSubPage[pageController.current];
            int itemLength = decorationItems[pageController.current].ItemPrefab.Length;
            int maxSubPage = itemLength / SUBPAGE_ITEMS_MAX + (itemLength % SUBPAGE_ITEMS_MAX == 0 ? 0 : 1) - 1;
            int min = nowSubPage * SUBPAGE_ITEMS_MAX;

            textMesh.text = $"{nowSubPage + 1} / {maxSubPage + 1}";

            var currentItems = decorationItems[pageController.current];

            int index = 0;
            string currentName;

            for (int i = 0;i < SUBPAGE_ITEMS_MAX; i++)
            {
                index = min + i;
                if (index >= currentItems.ItemPrefab.Length) return;

                //重複生成しない
                currentName = currentItems.ItemPrefab[index].ItemName[languageCurrent];
                if (CheckGenerated(currentName)) continue;

                var instance = Instantiate(currentItems.ItemPrefab[index]).transform;

                instance.name = currentName;//重複チェックの為
                instance.parent = pageController.GetCurrentPage();
                instance.localPosition = new Vector3(itemOffsetPos[i].x, itemOffsetPos[i].y, 0);
                instance.localRotation = Quaternion.identity * reverseQuaternion;
                instance.localScale *= 0.5f;
            }
        }

        /// <summary>
        /// アイテムが既出か確認
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        private bool CheckGenerated(string targetName)
        {
            foreach (Transform instance in pageController.GetCurrentPage())
            {
                if (instance.name == targetName) return true;
            }
            return false;
        }

        private void DebugInput()
        {
            //if (Input.GetKeyDown(KeyCode.I)) ChangeItem(1);
            //if (Input.GetKeyDown(KeyCode.K)) ChangeItem(-1);
        }
    }
}