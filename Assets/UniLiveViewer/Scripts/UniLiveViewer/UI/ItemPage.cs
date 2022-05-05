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
        private const int SUBPAGE_ITEMS_ROW = 2;
        private const int SUBPAGE_ITEMS_COL = 3;
        private int SUBPAGE_ITEMS_MAX = SUBPAGE_ITEMS_ROW * SUBPAGE_ITEMS_COL;
        private Vector2[] itemOffsetPos = new Vector2[] { 
            new Vector2(-0.25f,0.06f), new Vector2(0,0.06f),new Vector2(0.25f,0.06f),
            new Vector2(-0.25f,-0.12f), new Vector2(0,-0.12f),new Vector2(0.25f,-0.12f),
        };

        private MenuManager menuManager;
        //[SerializeField] private Button_Base[] btn_jumpList;

        [SerializeField] private Button_Base[] btn_Item = new Button_Base[2];
        //[SerializeField] private DecorationItemInfo[] ItemPrefab = new DecorationItemInfo[0];
        [SerializeField] private GameObject itemMaterialPrefab;
        [SerializeField] private int[] currentSubPage;
        [SerializeField] private TextMesh textMesh;
        //[SerializeField] private Transform itemGeneratAnchor;
        [SerializeField] private Transform itemMaterialAnchor;

        [SerializeField] private PageController pageController;

        [Header("＜各ページに相当＞")]
        [SerializeField] private DecorationItems[] decorationItems;

        private CancellationToken cancellation_token;

        private void Awake()
        {
            menuManager = transform.root.GetComponent<MenuManager>();
            cancellation_token = this.GetCancellationTokenOnDestroy();

            currentSubPage = new int[decorationItems.Length];
            

            //ジャンプリスト
            //foreach (var e in btn_jumpList)
            //{
            //    e.onTrigger += OpenJumplist;
            //}
            //menuManager.jumpList.onSelect += (jumpCurrent) =>
            //{
            //    int moveIndex = 0;
            //    switch (menuManager.jumpList.target)
            //    {
            //        case JumpList.TARGET.ITEM:
            //            moveIndex = jumpCurrent - currentItem;
            //            ChangeItem(moveIndex);
            //            break;
            //    }
            //    menuManager.PlayOneShot(SoundType.BTN_CLICK);
            //};
            //その他
            for (int i = 0; i < btn_Item.Length; i++)
            {
                btn_Item[i].onTrigger += MoveIndex_Item;
            }

            pageController.onSwitchPage += () => Init().Forget();
        }

        private void OnEnable()
        {
            //Init().Forget();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        private async UniTaskVoid Init()
        {
            await UniTask.Yield(cancellation_token);

            //アイテム数に応じてサブページ送りボタンの表示切替
            int itemLength = decorationItems[pageController.current].ItemPrefab.Length;
            bool isOver = itemLength > SUBPAGE_ITEMS_MAX;
            for (int i = 0; i < btn_Item.Length; i++)
            {
                if(btn_Item[i].gameObject.activeSelf != isOver) btn_Item[i].gameObject.SetActive(isOver);
            }

            //アクティブページのアイテムを生成
            GenerateItems();

            //アイテムがなければ生成
            //if (itemGeneratAnchor.childCount == 0) ChangeItem(0);
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            DebugInput();
#elif UNITY_ANDROID
#endif
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

                    //ChangeItem(moveIndex);

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
            Debug.Log(targetPage + ":このページには" + max);
            for (int i = 0; i< max; i++)
            {
                Destroy(targetPage.GetChild(max - i - 1).gameObject);//常に先頭を削除
            }
        }

        /// <summary>
        /// 現在のページとサブページを基準にアイテムが無ければ生成
        /// </summary>
        private void GenerateItems()
        {
            int nowSubPage = currentSubPage[pageController.current];
            int min = nowSubPage * SUBPAGE_ITEMS_MAX;
            //int max = min + SUBPAGE_ITEMS_MAX - 1;

            var currentItems = decorationItems[pageController.current];
            //Vector2 btnPos;

            int index = 0;
            int languageCurrent = (int)SystemInfo.userProfile.data.LanguageCode - 1;
            string currentName;

            for (int i = 0;i < SUBPAGE_ITEMS_MAX; i++)
            {
                index = min + i;
                if (index >= currentItems.ItemPrefab.Length) return;

                //重複生成しない
                currentName = currentItems.ItemPrefab[index].itemName[languageCurrent];
                if (CheckGenerated(currentName)) continue;

                var instance = Instantiate(currentItems.ItemPrefab[index]).transform;

                instance.name = currentName;//重複チェックの為
                instance.parent = pageController.GetCurrentPage();
                instance.localPosition = new Vector3(itemOffsetPos[i].x, itemOffsetPos[i].y, 0);
                instance.localRotation = Quaternion.identity;
            }

            
            //for (int i = 0;i < SUBPAGE_ITEMS_ROW; i++)
            //{
            //    for (int j = 0;j < SUBPAGE_ITEMS_COL; j++)
            //    {
            //        index = (i * SUBPAGE_ITEMS_COL) + j;
            //        if (index > currentItems.ItemPrefab.Length) return;
            //        //重複生成しない
            //        currentName = currentItems.ItemPrefab[index].itemName[languageCurrent];

            //        if (CheckGenerated(currentName)) continue;
            //        var instance = Instantiate(currentItems.ItemPrefab[index]).transform;

                    

            //        //座標調整
            //        btnPos.x = -0.25f + (j * 0.25f);
            //        btnPos.y = 0.06f - (i * 0.18f);

            //        instance.name = currentName;//重複チェックの為
            //        instance.parent = pageController.GetCurrentPage();
            //        instance.localPosition = new Vector3(btnPos.x, btnPos.y, 0);
            //        instance.localRotation = Quaternion.identity;
            //    }
            //}

    
        }

        /// <summary>
        /// アイテムが既出か確認
        /// </summary>
        /// <param name="currentItems"></param>
        /// <param name="index"></param>
        /// <param name="languageCurrent"></param>
        /// <returns></returns>
        private bool CheckGenerated(string targetName)
        {
            foreach (Transform instance in pageController.GetCurrentPage())
            {
                if (instance.name == targetName)
                {
                    return true;
                }
            }
            return false;
        }

        //private void OpenJumplist(Button_Base btn)
        //{
        //    if (!menuManager.jumpList.gameObject.activeSelf) menuManager.jumpList.gameObject.SetActive(true);
        //    if (btn == btn_jumpList[0])
        //    {
        //        menuManager.jumpList.SetItemData(ItemPrefab);
        //    }
        //    menuManager.PlayOneShot(SoundType.BTN_CLICK);
        //}

        /// <summary>
        /// アイテムを変更する
        /// TODO:ちゃんと作り直す
        /// </summary>
        /// <param name="btn"></param>
        //private void ChangeItem(int moveIndex)
        //{
        //    currentItem += moveIndex;

        //    if (currentItem < 0) currentItem = ItemPrefab.Length - 1;
        //    else if (currentItem >= ItemPrefab.Length) currentItem = 0;

        //    //アイテムが残っていれば削除
        //    if (itemGeneratAnchor.childCount > 0)
        //    {
        //        int max = itemGeneratAnchor.childCount - 1;
        //        for (int j = 0; j <= max; j++)
        //        {
        //            //常に先頭を削除
        //            Destroy(itemGeneratAnchor.GetChild(max - j).gameObject);
        //        }
        //    }
        //    //アイテム用マテリアルが残っていれば削除
        //    if (itemMaterialAnchor.childCount > 0)
        //    {
        //        int max = itemMaterialAnchor.childCount - 1;
        //        for (int j = 0; j <= max; j++)
        //        {
        //            //後ろから削除
        //            Destroy(itemMaterialAnchor.GetChild(max - j).GetComponent<Renderer>().material);//これよくないなぁ
        //            Destroy(itemMaterialAnchor.GetChild(max - j).gameObject);
        //        }
        //    }

        //    //生成
        //    var newItem = Instantiate(ItemPrefab[currentItem].gameObject, transform.position, Quaternion.identity).transform;
        //    newItem.parent = itemGeneratAnchor;
        //    newItem.localPosition = Vector3.zero;
        //    newItem.localRotation = Quaternion.identity;

        //    var itemInfo = newItem.GetComponent<DecorationItemInfo>();
        //    if (itemInfo.texs.Length > 0)
        //    {
        //        //マテリアルオブジェクトの生成
        //        for (int j = 0; j < itemInfo.texs.Length; j++)
        //        {
        //            var mat = Instantiate(itemMaterialPrefab, transform.position, Quaternion.identity).transform;
        //            mat.GetComponent<MeshRenderer>().materials[0].SetTexture("_BaseMap", itemInfo.texs[j]);
        //            mat.parent = itemMaterialAnchor;
        //            mat.localPosition = new Vector3(-5, 2.5f - j, 0);
        //            mat.localRotation = Quaternion.identity;
        //        }
        //    }

        //    //名前を表示
        //    if (SystemInfo.userProfile.data.LanguageCode == (int)USE_LANGUAGE.JP) textMesh.text = itemInfo.itemName[1];
        //    else textMesh.text = itemInfo.itemName[0];

        //}

        private void DebugInput()
        {
            //if (Input.GetKeyDown(KeyCode.I)) ChangeItem(1);
            //if (Input.GetKeyDown(KeyCode.K)) ChangeItem(-1);
        }
    }
}