using UnityEngine;

namespace UniLiveViewer
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineSelector : MonoBehaviour
    {
        //�x�W�F�Ȑ��p
        [Header("���Ȑ��̐ݒ聄")]
        [SerializeField]
        private Transform LineStartAnchor = null;
        public Transform LineEndAnchor = null;
        private Vector3 EndAnchor_KeepEuler = Vector3.zero;
        [SerializeField]
        private float distance = 5.0f;
        [SerializeField]
        private float high = 1.5f;
        private Vector3[] BezierCurvePoint = new Vector3[3];
        private float BezierCurveTimer = 0;

        //LineRenderer�p
        private LineRenderer lineRenderer = null;
        //[SerializeField]
        //private float LineWidth = 0.01f;
        [SerializeField]
        private int positionCount = 10;


        //�Փˌ��m
        [Header("���Փˌ��m�̐ݒ聄")]
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private Vector3 rayDirection = new Vector3(0, -1, 0);
        public RaycastHit hitCollider;
        private Transform keepHitObj;

        //�e���|�[�g
        [Header("�����̐ݒ聄")]
        [SerializeField]
        private Transform teleportPoint;
        private Renderer _renderer;
        private MaterialPropertyBlock materialPropertyBlock;
        private Color baseColor;
        [SerializeField] private Color hitColor = new Color(255.0f, 0.0f, 0.0f);

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            //LineRenderer�̃p�����[�^�ݒ�
            lineRenderer.positionCount = positionCount;

            //�p�x�̏����l���擾
            EndAnchor_KeepEuler = LineEndAnchor.localRotation.eulerAngles;
            //�����_�[�Ƃ��̃}�e���A���v���p�e�B���擾
            _renderer = teleportPoint.GetComponent<Renderer>();
            materialPropertyBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(materialPropertyBlock);
            //�x�[�X�J���[���擾
            baseColor = _renderer.material.GetColor("_TintColor");

            //�J�����������Ă���
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            //�x�W�F�Ȑ��̊J�n�_�A���ԓ_�A�I���_���Z�o����
            BezierCurvePoint[0] = LineStartAnchor.position;
            BezierCurvePoint[1] = LineStartAnchor.position + (LineStartAnchor.forward * distance / high);
            BezierCurvePoint[2] = LineStartAnchor.position + (LineStartAnchor.forward * distance);
            BezierCurvePoint[2].y = transform.position.y;//��U�e�̍����ɑ�����

            //�x�W�F�Ȑ����쐬
            Vector3 pos = Vector3.zero;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                //BezierCurveTimer:0�`1
                BezierCurveTimer = (float)i / (lineRenderer.positionCount - 1);
                //�x�W�F�Ȑ��̕⊮���W���擾
                pos = GetLerpPoint(BezierCurvePoint[0], BezierCurvePoint[1], BezierCurvePoint[2], BezierCurveTimer);
                //���W��LineRenderer�ɃZ�b�g
                lineRenderer.SetPosition(i, pos);
            }

            //���Ɍ�������ray���΂�
            Physics.Raycast(rayOrigin.position, rayDirection, out hitCollider, 1.0f, Parameters.layerMask_StageFloor);
            Debug.DrawRay(rayOrigin.position, rayDirection, Color.red);
            //���̍����ɍ��킹��
            if (hitCollider.collider) BezierCurvePoint[2].y = hitCollider.point.y;

            //�n��Anchor�p�̃I�u�W�F�N�g���ړ�����
            if (LineEndAnchor) LineEndAnchor.position = BezierCurvePoint[2];

            //�Փˌ��m(�Ȃ�ׂ��Z�����Ă�)
            Physics.Raycast(LineEndAnchor.position, Vector3.up, out hitCollider, 1.0f, Parameters.layerMask_FieldObject);

            //Debug.DrawRay(LineEndAnchor.position, Vector3.up, Color.red);
        }

        /// <summary>
        /// ���̐F��ݒ�
        /// </summary>
        public void SetMaterial(bool isForcedReset)
        {
            //����������
            if (isForcedReset)
            {
                keepHitObj = null;
                materialPropertyBlock.SetColor("_TintColor", baseColor);
                _renderer.SetPropertyBlock(materialPropertyBlock);
            }
            else
            {
                if (keepHitObj == hitCollider.transform) return;
                keepHitObj = hitCollider.transform;

                //hit��Ԃɉ����ă}�e���A���v���p�e�B�̐F����ύX
                if (hitCollider.transform) materialPropertyBlock.SetColor("_TintColor", hitColor);
                else materialPropertyBlock.SetColor("_TintColor", baseColor);
                //�����_�[�Ƀv���p�e�B���Z�b�g
                _renderer.SetPropertyBlock(materialPropertyBlock);
            }
        }

        /// <summary>
        /// GroundPointer�̃I�C���[�p�x�����Z����
        /// </summary>
        /// <param ���Z����p�x="addAngles"></param>
        public void GroundPointer_AddEulerAngles(Vector3 addAngles)
        {
            Vector3 eulerAngles = LineEndAnchor.localRotation.eulerAngles + addAngles;
            LineEndAnchor.localRotation = Quaternion.Euler(eulerAngles);
        }

        /// <summary>
        /// �x�W�F�Ȑ���̕�ԍ��W��Ԃ�
        /// </summary>
        /// <param �J�n�_="point0"></param>
        /// <param ���ԓ_="point1"></param>
        /// <param �I���_="point2"></param>
        /// <param Lerp�W��="time"></param>
        private Vector3 GetLerpPoint(Vector3 point0, Vector3 point1, Vector3 point2, float time)
        {
            Vector3 movePointA = Vector3.Lerp(point0, point1, time);
            Vector3 movePointB = Vector3.Lerp(point1, point2, time);
            Vector3 movePointC = Vector3.Lerp(movePointA, movePointB, time);

            return movePointC;
        }

        private void OnEnable()
        {
            LineEndAnchor.localRotation = Quaternion.Euler(EndAnchor_KeepEuler);
        }

        private void OnDisable()
        {
            LineEndAnchor.localRotation = Quaternion.Euler(EndAnchor_KeepEuler);
        }
    }
}