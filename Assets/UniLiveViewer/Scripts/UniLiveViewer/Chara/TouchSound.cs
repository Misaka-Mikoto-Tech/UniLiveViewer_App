using UnityEngine;

namespace UniLiveViewer
{
    //現状SSUのしっぽ専用
    [RequireComponent(typeof(AudioSource))]
    public class TouchSound : MonoBehaviour
    {
        [SerializeField] Transform parentAnchor;
        [SerializeField] float colliderRadius = 0.07f;

        AudioSource _audioSource;
        [SerializeField] AudioClip[] Sound;

        void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = SystemInfo.soundVolume_SE;

            //親子全てにコライダーを付ける
            CreateColliders(parentAnchor);
        }

        // Start is called before the first frame update
        void Start()
        {
            //初期は無効化しておき、設置状態のみ有効化
            this.enabled = false;
        }

        void CreateColliders(Transform parent)
        {
            //末端で無ければ処理する
            if (parent.childCount != 0)
            {
                //コライダーを付ける
                parent.gameObject.AddComponent(typeof(SphereCollider));
                var col = parent.GetComponent<SphereCollider>();
                col.gameObject.layer = SystemInfo.layerNo_UI;
                col.radius = colliderRadius;
                col.isTrigger = true;

                foreach (Transform child in parent)
                {
                    //再帰
                    CreateColliders(child);
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!this.enabled || other.gameObject.layer != SystemInfo.layerNo_IgnoreRaycats) return;
            //タッチ音をランダムにならす
            int i = Random.Range(0, Sound.Length);
            _audioSource.PlayOneShot(Sound[i]);
        }
    }
}