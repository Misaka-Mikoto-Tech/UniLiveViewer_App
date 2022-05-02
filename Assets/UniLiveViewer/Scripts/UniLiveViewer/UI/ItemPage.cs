using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UniLiveViewer
{
    public class ItemPage : MonoBehaviour
    {
        private MenuManager menuManager;
        [SerializeField] private Button_Base[] btn_jumpList;

        [SerializeField] private Button_Base[] btn_Item = new Button_Base[2];
        [SerializeField] private DecorationItemInfo[] ItemPrefab = new DecorationItemInfo[0];
        [SerializeField] private GameObject itemMaterialPrefab;
        [SerializeField] private int currentItem = 0;
        [SerializeField] private TextMesh textMesh;
        [SerializeField] private Transform itemGeneratAnchor;
        [SerializeField] private Transform itemMaterialAnchor;

        private CancellationToken cancellation_token;

        private void Awake()
        {
            menuManager = transform.root.GetComponent<MenuManager>();
            cancellation_token = this.GetCancellationTokenOnDestroy();

            //ジャンプリスト
            foreach (var e in btn_jumpList)
            {
                e.onTrigger += OpenJumplist;
            }
            menuManager.jumpList.onSelect += (jumpCurrent) =>
            {
                int moveIndex = 0;
                switch (menuManager.jumpList.target)
                {
                    case JumpList.TARGET.ITEM:
                        moveIndex = jumpCurrent - currentItem;
                        ChangeItem(moveIndex);
                        break;
                }
                menuManager.PlayOneShot(SoundType.BTN_CLICK);
            };
            //その他
            for (int i = 0; i < btn_Item.Length; i++)
            {
                btn_Item[i].onTrigger += MoveIndex_Item;
            }
        }

        private void OnEnable()
        {
            Init().Forget();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        private async UniTaskVoid Init()
        {
            await UniTask.Yield(cancellation_token);

            //アイテムがなければ生成
            if (itemGeneratAnchor.childCount == 0) ChangeItem(0);
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
            if (!btn) return;
            for (int i = 0; i < 2; i++)
            {
                //押されたボタンの判別
                if (btn_Item[i] == btn)
                {
                    int moveIndex = 0;
                    if (i == 0) moveIndex = -1;
                    else if (i == 1) moveIndex = 1;

                    ChangeItem(moveIndex);

                    //クリック音
                    menuManager.PlayOneShot(SoundType.BTN_CLICK);
                    break;
                }
            }
        }

        private void OpenJumplist(Button_Base btn)
        {
            if (!menuManager.jumpList.gameObject.activeSelf) menuManager.jumpList.gameObject.SetActive(true);
            if (btn == btn_jumpList[0])
            {
                menuManager.jumpList.SetItemData(ItemPrefab);
            }
            menuManager.PlayOneShot(SoundType.BTN_CLICK);
        }

        /// <summary>
        /// アイテムを変更する
        /// TODO:ちゃんと作り直す
        /// </summary>
        /// <param name="btn"></param>
        private void ChangeItem(int moveIndex)
        {
            currentItem += moveIndex;

            if (currentItem < 0) currentItem = ItemPrefab.Length - 1;
            else if (currentItem >= ItemPrefab.Length) currentItem = 0;

            //アイテムが残っていれば削除
            if (itemGeneratAnchor.childCount > 0)
            {
                int max = itemGeneratAnchor.childCount - 1;
                for (int j = 0; j <= max; j++)
                {
                    //常に先頭を削除
                    Destroy(itemGeneratAnchor.GetChild(max - j).gameObject);
                }
            }
            //アイテム用マテリアルが残っていれば削除
            if (itemMaterialAnchor.childCount > 0)
            {
                int max = itemMaterialAnchor.childCount - 1;
                for (int j = 0; j <= max; j++)
                {
                    //後ろから削除
                    Destroy(itemMaterialAnchor.GetChild(max - j).GetComponent<Renderer>().material);//これよくないなぁ
                    Destroy(itemMaterialAnchor.GetChild(max - j).gameObject);
                }
            }

            //生成
            var newItem = Instantiate(ItemPrefab[currentItem].gameObject, transform.position, Quaternion.identity).transform;
            newItem.parent = itemGeneratAnchor;
            newItem.localPosition = Vector3.zero;
            newItem.localRotation = Quaternion.identity;

            var itemInfo = newItem.GetComponent<DecorationItemInfo>();
            if (itemInfo.texs.Length > 0)
            {
                //マテリアルオブジェクトの生成
                for (int j = 0; j < itemInfo.texs.Length; j++)
                {
                    var mat = Instantiate(itemMaterialPrefab, transform.position, Quaternion.identity).transform;
                    mat.GetComponent<MeshRenderer>().materials[0].SetTexture("_BaseMap", itemInfo.texs[j]);
                    mat.parent = itemMaterialAnchor;
                    mat.localPosition = new Vector3(-5, 2.5f - j, 0);
                    mat.localRotation = Quaternion.identity;
                }
            }

            //名前を表示
            if (SystemInfo.userProfile.data.LanguageCode == (int)USE_LANGUAGE.JP) textMesh.text = itemInfo.itemName[1];
            else textMesh.text = itemInfo.itemName[0];

        }

        private void DebugInput()
        {
            if (Input.GetKeyDown(KeyCode.I)) ChangeItem(1);
            if (Input.GetKeyDown(KeyCode.K)) ChangeItem(-1);
        }
    }
}