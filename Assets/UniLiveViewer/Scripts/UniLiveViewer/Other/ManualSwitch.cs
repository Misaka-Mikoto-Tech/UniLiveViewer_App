using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer
{
    public class ManualSwitch : MonoBehaviour
    {
        [Header("＜マニュアル＞")]
        [SerializeField] private Sprite[] sprManualPrefab_A = new Sprite[2];
        [SerializeField] private Sprite[] sprManualPrefab_B = new Sprite[2];
        [SerializeField] private SpriteRenderer[] sprManual = new SpriteRenderer[2];

        // Start is called before the first frame update
        void Start()
        {
            if (FileReadAndWriteUtility.UserProfile.LanguageCode == (int)USE_LANGUAGE.JP)
            {
                sprManual[0].sprite = sprManualPrefab_A[0];
                sprManual[1].sprite = sprManualPrefab_B[0];
            }
            else
            {
                sprManual[0].sprite = sprManualPrefab_A[1];
                sprManual[1].sprite = sprManualPrefab_B[1];
            }

            bool b = FileReadAndWriteUtility.UserProfile.scene_crs_manual;
            if (sprManual[0].gameObject.activeSelf != b) sprManual[0].gameObject.SetActive(b);
            if (sprManual[1].gameObject.activeSelf != b) sprManual[1].gameObject.SetActive(b);
        }

        public void SetEnable(bool isEnable)
        {
            FileReadAndWriteUtility.UserProfile.scene_crs_manual = isEnable;
            if (sprManual[0].gameObject.activeSelf != isEnable) sprManual[0].gameObject.SetActive(isEnable);
            if (sprManual[1].gameObject.activeSelf != isEnable) sprManual[1].gameObject.SetActive(isEnable);
        }
    }
}