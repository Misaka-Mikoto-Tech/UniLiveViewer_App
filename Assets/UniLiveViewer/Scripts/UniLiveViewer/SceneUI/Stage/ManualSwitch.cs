using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniLiveViewer.Menu
{
    public class ManualSwitch : MonoBehaviour
    {
        [Header("＜マニュアル＞")]
        [SerializeField] private Sprite[] sprManualPrefab_A = new Sprite[2];
        [SerializeField] private Sprite[] sprManualPrefab_B = new Sprite[2];
        [SerializeField] private SpriteRenderer[] sprManual = new SpriteRenderer[2];

        public void SetEnable(bool isEnable)
        {
            FileReadAndWriteUtility.UserProfile.scene_crs_manual = isEnable;
            if (sprManual[0].gameObject.activeSelf != isEnable) sprManual[0].gameObject.SetActive(isEnable);
            if (sprManual[1].gameObject.activeSelf != isEnable) sprManual[1].gameObject.SetActive(isEnable);
        }
    }
}