using UnityEngine;
using UnityEngine.UI;

namespace UniLiveViewer
{
    //�{�^�����
    public enum SWITCHSTATE
    {
        NULL = 0,
        OFF,
        ON,
    }

    public enum DRAWTYPE
    {
        NULL = 0,
        IMAGE,
        SPRITE,
        MESHRENDER,
        TEXTMESH
    }

    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class CollisionChecker : MonoBehaviour
    {
        [SerializeField] private SWITCHSTATE _myState = SWITCHSTATE.ON;
        //private SWITCHSTATE keepState = SWITCHSTATE.NULL;
        public bool Touching() { return isTouch; }
        private bool isTouch = false;
        public bool isTouchL = false;
        public SWITCHSTATE myState
        {
            get
            {
                return _myState;
            }
            set
            {
                //�{�^����Ԃɉ����ĐF���X�V
                _myState = value;
                isTouch = false;
                ColorUpdate();
            }
        }
        //�F�̐ݒ�
        public TargetColorSetting[] colorSetting;

        private void Awake()
        {
            for (int i = 0; i < colorSetting.Length; i++)
            {
                colorSetting[i].Init();
            }
        }

        //Enter�������̃t���[����Exit�����Stay�͌Ă΂�Ȃ��炵��
        //����Exit�����蔲���Ă���C������̂ŁAStay������Ίm����Exit���������邩���H�Ȃ̂�Stay���g��
        private void OnCollisionStay(Collision collision)
        {
            if (isTouch) return;
            isTouch = true;
            ColorUpdate();

            //�q�b�g�Ώ�
            if (collision.transform.name.Contains("Left")) isTouchL = true;
            else isTouchL = false;

            //�U������
            if (isTouchL) PlayerStateManager.ControllerVibration(OVRInput.Controller.LTouch, 1, 0.6f, 0.05f);
            else PlayerStateManager.ControllerVibration(OVRInput.Controller.RTouch, 1, 0.6f, 0.05f);
        }

        /// <summary>
        /// ���ꂽ��
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionExit(Collision collision)
        {
            if (!isTouch) return;
            isTouch = false;
            ColorUpdate();
        }

        /// <summary>
        /// ���̉�������
        /// </summary>
        private void OnEnable()
        {
            isTouch = false;
            ColorUpdate();
        }

        /// <summary>
        /// ��Ԃɉ����ĐF���X�V
        /// </summary>
        private void ColorUpdate()
        {
            if (colorSetting == null) return;

            for (int i = 0; i < colorSetting.Length; i++)
            {
                colorSetting[i].SetColor(_myState, isTouch);
            }
        }
    }

    /// <summary>
    /// �F�����Ǘ�����N���X
    /// </summary>
    [System.Serializable]
    public class TargetColorSetting
    {
        //TODO:�_�T...�ł��������@�m��Ȃ�
        [Header("�ǂꂩ1��ɃA�^�b�`")]
        public Image targetImage;
        public SpriteRenderer targetSprite;
        public MeshRenderer meshRender;
        public TextMesh textMesh;
        [Space(20)]
        [Tooltip("������ԃJ���[")]
        public Color DisableColor = new Color(0.3f, 0.3f, 0.3f);
        [Tooltip("�L����ԃJ���[")]
        public Color EnableColor = new Color(0.7f, 0.7f, 0.7f);
        [Tooltip("�G��Ă����ԃJ���[")]
        public Color TouchColor = new Color(0.7f, 0.7f, 0.3f);
        //[System.NonSerialized]
        private DRAWTYPE drawType = DRAWTYPE.NULL;

        private MaterialPropertyBlock materialPropertyBlock;

        public void Init()
        {
            if (targetImage != null) drawType = DRAWTYPE.IMAGE;
            else if (targetSprite != null) drawType = DRAWTYPE.SPRITE;
            else if (meshRender != null) drawType = DRAWTYPE.MESHRENDER;
            else if (textMesh != null)
            {
                drawType = DRAWTYPE.TEXTMESH;
                EnableColor = GlobalConfig.btnColor_Ena;
                DisableColor = GlobalConfig.btnColor_Dis;
            }

            materialPropertyBlock = new MaterialPropertyBlock();
        }

        public void SetColor(SWITCHSTATE state, bool isTouch)
        {
            Color _color = DisableColor;

            if (isTouch)
            {
                _color = TouchColor;
            }
            else
            {
                switch (state)
                {
                    case SWITCHSTATE.OFF:
                        _color = DisableColor;
                        break;
                    case SWITCHSTATE.ON:
                        _color = EnableColor;
                        break;
                }
            }


            switch (drawType)
            {
                case DRAWTYPE.IMAGE:
                    targetImage.color = _color;
                    break;
                case DRAWTYPE.SPRITE:
                    targetSprite.color = _color;
                    break;
                case DRAWTYPE.MESHRENDER:
                    materialPropertyBlock.SetColor("_Color", _color);
                    meshRender.SetPropertyBlock(materialPropertyBlock);
                    break;
                case DRAWTYPE.TEXTMESH:
                    textMesh.color = _color;
                    break;
            }
        }
    }
}