using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class HandUIController : MonoBehaviour
    {
        public handUI[] handUI_CharaAdjustment;
        public handUI[] handUI_ItemMatSelecter;
        private ItemMaterialSelector[] itemMaterialSelector = new ItemMaterialSelector[2];

        public handUI handUI_PlayerHeight;
        private CharacterCameraConstraint_Custom charaCam;

        private float _playerHeight = 0;
        public float PlayerHeight 
        { 
            get { return _playerHeight; }
            set 
            {
                _playerHeight = Mathf.Clamp(value, 0, 1.5f);
                charaCam.HeightOffset = _playerHeight;
                handUI_PlayerHeight.textMesh.text = $"{_playerHeight:0.00}";
            }
        }

        private TimelineController timeline = null;
        private Transform lookTarget;

        private void Awake()
        {
            timeline = GameObject.FindGameObjectWithTag("TimeLineDirector").gameObject.GetComponent<TimelineController>();
            charaCam = GetComponent<CharacterCameraConstraint_Custom>();
            lookTarget = GameObject.FindGameObjectWithTag("MainCamera").gameObject.transform;
        }

        // Start is called before the first frame update
        void Start()
        {
            handUI_PlayerHeight.Init(Instantiate(handUI_PlayerHeight.prefab));
            PlayerHeight = charaCam.HeightOffset;
            handUI_PlayerHeight.Show = false;

            for (int i = 0;i< handUI_CharaAdjustment.Length;i++)
            {
                handUI_CharaAdjustment[i].Init(Instantiate(handUI_CharaAdjustment[i].prefab));
                handUI_CharaAdjustment[i].textMesh.text = $"{SystemInfo.userProfile.data.InitCharaSize}0.00";
                handUI_CharaAdjustment[i].Show = false;
            }

            for (int i = 0; i < handUI_ItemMatSelecter.Length; i++)
            {
                handUI_ItemMatSelecter[i].Init(Instantiate(handUI_ItemMatSelecter[i].prefab));
                handUI_ItemMatSelecter[i].Show = false;
            }
        }

        private void LateUpdate()
        {
            if(handUI_PlayerHeight.Show)
            {
                handUI_PlayerHeight.instance.transform.LookAt(lookTarget);
            }
            for (int i = 0; i < handUI_CharaAdjustment.Length; i++)
            {
                handUI_CharaAdjustment[i].instance.transform.LookAt(lookTarget);
            }
            for (int i = 0; i < handUI_ItemMatSelecter.Length; i++)
            {
                handUI_ItemMatSelecter[i].instance.transform.LookAt(lookTarget);
            }
        }

        public void SetCurrent_ItemMaterial(int handType, int current)
        {
            itemMaterialSelector[handType].Current = current;
        }
        public Texture GetTexture_ItemMaterial(int handType)
        {
            return itemMaterialSelector[handType].GetCurrentTex();
        }

        public void InitItemMaterialSelector(int handType, DecorationItemInfo decorationItemInfo)
        {
            itemMaterialSelector[handType] = handUI_ItemMatSelecter[handType].instance.GetComponent<ItemMaterialSelector>();
            itemMaterialSelector[handType].Init(decorationItemInfo);
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