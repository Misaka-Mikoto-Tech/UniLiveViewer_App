using UnityEngine;

namespace UniLiveViewer
{
    public class HandUIController : MonoBehaviour
    {
        public handUI[] handUI_CharaAdjustment;//キャラ拡縮用
        public handUI[] handUI_ItemMatSelecter;//アイテム着色用
        ItemMaterialSelector[] _itemMaterialSelector = new ItemMaterialSelector[2];

        public handUI handUI_PlayerHeight;
        CharacterCameraConstraint_Custom _characterCameraConstraintCustom;

        float _playerHeight = 0;
        public float PlayerHeight 
        { 
            get { return _playerHeight; }
            set 
            {
                _playerHeight = Mathf.Clamp(value, 0, 2.0f);
                _characterCameraConstraintCustom.HeightOffset = _playerHeight;
                handUI_PlayerHeight.textMesh.text = $"{_playerHeight:0.00}";
            }
        }

        Transform _lookTarget;

        public void Initialize(CharacterCameraConstraint_Custom characterCameraConstraintCustom, Camera camera)
        {
            _lookTarget = camera.transform;
            _characterCameraConstraintCustom = characterCameraConstraintCustom;

            handUI_PlayerHeight.Init(Instantiate(handUI_PlayerHeight.prefab));
            PlayerHeight = _characterCameraConstraintCustom.HeightOffset;
            handUI_PlayerHeight.Show = false;

            for (int i = 0; i < handUI_CharaAdjustment.Length; i++)
            {
                handUI_CharaAdjustment[i].Init(Instantiate(handUI_CharaAdjustment[i].prefab));
                handUI_CharaAdjustment[i].textMesh.text = $"{StageSettingService.UserProfile.InitCharaSize}0.00";
                handUI_CharaAdjustment[i].Show = false;
            }

            for (int i = 0; i < handUI_ItemMatSelecter.Length; i++)
            {
                handUI_ItemMatSelecter[i].Init(Instantiate(handUI_ItemMatSelecter[i].prefab));
                handUI_ItemMatSelecter[i].Show = false;
            }
        }

        void LateUpdate()
        {
            //表示中は全てカメラに向く
            if(handUI_PlayerHeight.Show)
            {
                handUI_PlayerHeight.instance.transform.LookAt(_lookTarget);
            }
            for (int i = 0; i < handUI_CharaAdjustment.Length; i++)
            {
                if (!handUI_CharaAdjustment[i].Show) continue;
                handUI_CharaAdjustment[i].instance.transform.LookAt(_lookTarget);
            }
            for (int i = 0; i < handUI_ItemMatSelecter.Length; i++)
            {
                if (!handUI_ItemMatSelecter[i].Show) continue;
                handUI_ItemMatSelecter[i].instance.transform.LookAt(_lookTarget);
            }
        }

        public void InitItemMaterialSelector(int handType, DecorationItemInfo decorationItemInfo)
        {
            _itemMaterialSelector[handType] = handUI_ItemMatSelecter[handType].instance.GetComponent<ItemMaterialSelector>();
            _itemMaterialSelector[handType].Init(decorationItemInfo);
        }

        /// <summary>
        /// 指定Currentからテクスチャを取得
        /// </summary>
        /// <param name="handType"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public bool TrySetItemTexture(int handType, int current)
        {
            return _itemMaterialSelector[handType].TrySetTexture(current);
        }

        public bool IsShow_HandUI()
        {
            if (handUI_PlayerHeight.Show) return true;
            for (int i = 0; i < handUI_CharaAdjustment.Length; i++)
            {
                if(handUI_CharaAdjustment[i].Show) return true;
            }
            for (int i = 0; i < handUI_ItemMatSelecter.Length; i++)
            {
                if (handUI_ItemMatSelecter[i].Show) return true;
            }
            return false;
        }

        /// <summary>
        /// カメラの高さUI
        /// </summary>
        public void SwitchPlayerHeightUI()
        {
            //UI表示の切り替え
            handUI_PlayerHeight.Show = !handUI_PlayerHeight.Show;
        }
    }

    [System.Serializable]
    public class handUI
    {
        public GameObject prefab;
        public Transform parentAnchor;
        [Header("自動")]
        public GameObject instance;
        public TextMesh textMesh;
        private bool isShow = false;

        public void Init(GameObject _Instance)
        {
            instance = _Instance;
            instance.transform.parent = parentAnchor;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            textMesh = instance.transform.GetChild(0).GetComponent<TextMesh>();
        }

        public bool Show
        {
            get { return isShow; }
            set 
            {
                isShow = value;
                if(instance.activeSelf != isShow) instance.SetActive(isShow);
            }
        }
    }
}