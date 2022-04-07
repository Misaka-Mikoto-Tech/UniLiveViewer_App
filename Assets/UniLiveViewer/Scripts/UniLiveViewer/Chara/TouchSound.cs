using UnityEngine;

namespace UniLiveViewer
{
    //����SSU�̂����ې�p
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

            //�e�q�S�ĂɃR���C�_�[��t����
            CreateColliders(parentAnchor);
        }

        // Start is called before the first frame update
        void Start()
        {
            //�����͖��������Ă����A�ݒu��Ԃ̂ݗL����
            this.enabled = false;
        }

        private void CreateColliders(Transform parent)
        {
            //���[�Ŗ�����Ώ�������
            if (parent.childCount != 0)
            {
                //�R���C�_�[��t����
                parent.gameObject.AddComponent(typeof(SphereCollider));
                var col = parent.GetComponent<SphereCollider>();
                col.gameObject.layer = Parameters.layerNo_UI;
                col.radius = colliderRadius;
                col.isTrigger = true;

                foreach (Transform child in parent)
                {
                    //�ċA
                    CreateColliders(child);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!this.enabled) return;
            //�^�b�`���������_���ɂȂ炷
            int i = Random.Range(0, Sound.Length);
            audioSource.PlayOneShot(Sound[i]);
        }
    }
}