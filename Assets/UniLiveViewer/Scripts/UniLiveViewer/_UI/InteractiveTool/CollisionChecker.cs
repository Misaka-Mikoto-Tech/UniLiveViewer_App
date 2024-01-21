using UnityEngine;
using UnityEngine.UI;

namespace UniLiveViewer
{
    //ボタン状態
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
                //ボタン状態に応じて色を更新
                _myState = value;
                isTouch = false;
                ColorUpdate();
            }
        }
        //色の設定
        public TargetColorSetting[] colorSetting;

        /// <summary>
        /// 実体化した時
        /// </summary>
        private void OnEnable()
        {
            isTouch = false;
            ColorUpdate();
        }

        public void Init()
        {
            for (int i = 0; i < colorSetting.Length; i++)
            {
                colorSetting[i].Init(Constants.btnColor_Ena_sky);
            }
        }

        /// <summary>
        /// 状態に応じて色を更新
        /// </summary>
        private void ColorUpdate()
        {
            if (colorSetting == null) return;

            for (int i = 0; i < colorSetting.Length; i++)
            {
                colorSetting[i].SetColor(_myState, isTouch);
            }
        }

        //Enterした次のフレームでExitするとStayは呼ばれないらしい
        //だがExitもすり抜けている気がするので、Stayさせれば確実にExitが発生するかも？なのでStayを使う
        private void OnCollisionStay(Collision collision)
        {
            if (isTouch) return;
            isTouch = true;
            ColorUpdate();

            //ヒット対象
            if (collision.transform.name.Contains("Left")) isTouchL = true;
            else isTouchL = false;

            //振動処理
            if (isTouchL) ControllerVibration.Execute(OVRInput.Controller.LTouch, 1, 0.6f, 0.05f);
            else ControllerVibration.Execute(OVRInput.Controller.RTouch, 1, 0.6f, 0.05f);
        }

        /// <summary>
        /// 離れた時
        /// </summary>
        /// <param name="collision"></param>
        private void OnCollisionExit(Collision collision)
        {
            if (!isTouch) return;
            isTouch = false;
            ColorUpdate();
        }
    }

    /// <summary>
    /// 色情報を管理するクラス
    /// </summary>
    [System.Serializable]
    public class TargetColorSetting
    {
        //TODO:ダサ...でもいい方法知らない
        [Header("どれか1種にアタッチ")]
        public Image targetImage;
        public SpriteRenderer targetSprite;
        public MeshRenderer meshRender;
        public TextMesh textMesh;
        [Space(20)]
        [Tooltip("無効状態カラー")]
        public Color DisableColor = new Color(0.3f, 0.3f, 0.3f);
        [Tooltip("有効状態カラー")]
        public Color EnableColor = new Color(0.7f, 0.7f, 0.7f);
        [Tooltip("触れている状態カラー")]
        public Color TouchColor = new Color(0.7f, 0.7f, 0.3f);
        //[System.NonSerialized]
        private DRAWTYPE drawType = DRAWTYPE.NULL;

        private MaterialPropertyBlock materialPropertyBlock;

        public void Init(Color enableColor)
        {
            if (targetImage != null) drawType = DRAWTYPE.IMAGE;
            else if (targetSprite != null) drawType = DRAWTYPE.SPRITE;
            else if (meshRender != null) drawType = DRAWTYPE.MESHRENDER;
            else if (textMesh != null)
            {
                drawType = DRAWTYPE.TEXTMESH;
                EnableColor = enableColor;
                DisableColor = Constants.btnColor_Dis;
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