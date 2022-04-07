using UnityEngine;

namespace UniLiveViewer
{
    //現状SSUのしっぽ専用
    [RequireComponent(typeof(AudioSource))]
    public class TouchSound : MonoBehaviour
    {
        [SerializeField] private Transform parentAnchor;
        [SerializeField] private float colliderRadius = 0.07f;

        private AudioSource audioSource;
        [SerializeField] private AudioClip[] Sound;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = GlobalConfig.soundVolume_SE;

            //親子全てにコライダーを付ける
            CreateColliders(parentAnchor);
        }

        // Start is called before the first frame update
        void Start()
        {
            //初期は無効化しておき、設置状態のみ有効化
            this.enabled = false;
        }

        private void CreateColliders(Transform parent)
        {
            //末端で無ければ処理する
            if (parent.childCount != 0)
            {
                //コライダーを付ける
                parent.gameObject.AddComponent(typeof(SphereCollider));
                var col = parent.GetComponent<SphereCollider>();
                col.gameObject.layer = Parameters.layerNo_UI;
                col.radius = colliderRadius;
                col.isTrigger = true;

                foreach (Transform child in parent)
                {
                    //再帰
                    CreateColliders(child);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!this.enabled) return;
            //タッチ音をランダムにならす
            int i = Random.Range(0, Sound.Length);
            audioSource.PlayOneShot(Sound[i]);
        }
    }
}