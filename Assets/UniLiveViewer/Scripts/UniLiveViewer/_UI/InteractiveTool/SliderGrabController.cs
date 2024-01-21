using System;
using UniLiveViewer.OVRCustom;
using UnityEngine;

namespace UniLiveViewer
{
    /// <summary>
    /// 一旦Rxしない
    /// </summary>
    public class SliderGrabController : MonoBehaviour
    {
        readonly Quaternion _handAdjustment = Quaternion.Euler(new Vector3(0, 0, 180));

        /// <summary>
        /// 操作開始
        /// </summary>
        public event Action Controled;
        /// <summary>
        /// 操作終了
        /// </summary>
        public event Action UnControled;
        /// <summary>
        /// スライダー更新
        /// </summary>
        public event Action ValueUpdate;

        [Header("--- 設定 ---")]
        public Transform visibleHandler;
        [SerializeField] private Transform startAnchor;
        [SerializeField] private Transform endAnchor;
        [SerializeField] private Transform[] handMesh = new Transform[2];
        [Tooltip("VisibleHandleの子オブジェクトを指定")]
        [SerializeField] private OVRGrabbable_Custom unVisibleHandler = null;
        
        public float maxValuel = 1.0f;//スライダーの最大値
        public float minStepValuel = 0.1f;//スライダーを動かす間隔
        [SerializeField] private bool SkipMoveMode = false;

        [Header("--- 確認 ---")]
        [SerializeField] float _value = 0;

        /// <summary>
        /// スライダーを握って操作されているか
        /// </summary>
        public bool IsGrabbed => _isGrabbed;
        bool _isGrabbed = false;

        Vector3 _nextHandllocalPos;
        Vector3 _axis = Vector3.zero;
        float _handleMaxRangeX = 0;
        float _coefficient;
        bool _isGrabbedByLeftHand = false;

        /// <summary>
        /// ハンドルに指定したオブジェクトを範囲内で制御する(0～1)
        /// </summary>
        public float Value
        {
            get { return _value; }
            set
            {
                _value = Mathf.Clamp(value, 0, maxValuel);
                _nextHandllocalPos.x = _handleMaxRangeX * _value / maxValuel;
                visibleHandler.localPosition = startAnchor.localPosition + _nextHandllocalPos;
            }
        }

        void Awake()
        {
            _handleMaxRangeX = endAnchor.localPosition.x - startAnchor.localPosition.x;
            Value = 0;//0でスライダーの位置を初期化する

            unVisibleHandler.OnGrab += OnGrab;
            unVisibleHandler.OnRelease += OnRelease;
        }

        void OnEnable()
        {
            InitializationOnRelease();
        }

        void OnDisable()
        {
            InitializationOnRelease();
        }

        void Start()
        {
            //ハンドルの初期化
            InitializationOnRelease();
            //係数決定
            _coefficient = maxValuel / minStepValuel / 2;
        }

        void OnGrab(OVRGrabbable_Custom grabbable_Custom)
        {
            if (_isGrabbed) return;
            InitializationOnGrab();
            Controled?.Invoke();
        }

        void OnRelease(OVRGrabbable_Custom grabbable_Custom)
        {
            if (!_isGrabbed) return;
            InitializationOnRelease();
            UnControled?.Invoke();
        }

        /// <summary>
        /// 離された時の初期化
        /// </summary>
        void InitializationOnRelease()
        {
            unVisibleHandler.transform.parent = visibleHandler;
            unVisibleHandler.transform.localPosition = Vector3.zero;

            handMesh[0].parent.transform.localRotation = Quaternion.identity;

            //UI用handを非表示に
            if (handMesh[0].gameObject.activeSelf) handMesh[0].gameObject.SetActive(false);
            if (handMesh[1].gameObject.activeSelf) handMesh[1].gameObject.SetActive(false);

            _isGrabbed = false;
        }

        /// <summary>
        /// 掴まれたときの初期化
        /// </summary>
        void InitializationOnGrab()
        {
            //捕まれていなかったら異常
            if (!unVisibleHandler.IsGrabbed)
            {
                Debug.LogError("スライダーは掴まれていない");
                return;
            }

            unVisibleHandler.transform.parent = null;//必須

            //実際の手を非表示
            var playerHand = (OVRGrabber_UniLiveViewer)unVisibleHandler.grabbedBy;
            playerHand.handMeshRoot.gameObject.SetActive(false);

            if (playerHand.name.Contains("HandL"))
            {
                if ((playerHand.transform.right).y <= 0)
                {
                    handMesh[0].parent.transform.localRotation *= _handAdjustment;
                }
                _isGrabbedByLeftHand = true;
                handMesh[0].gameObject.SetActive(true);//UI用の手を表示
            }
            else if (playerHand.name.Contains("HandR"))
            {
                if ((-playerHand.transform.right).y <= 0)
                {
                    handMesh[1].parent.transform.localRotation *= _handAdjustment;
                }
                _isGrabbedByLeftHand = false;
                handMesh[1].gameObject.SetActive(true);//UI用の手を表示
            }

            _isGrabbed = true;
        }

        void Update()
        {
            if (_isGrabbed) UpdateByPlayerGrab();
        }

        /// <summary>
        /// Playerに握られているマニュアル更新状態
        /// </summary>
        void UpdateByPlayerGrab()
        {
            var direction = unVisibleHandler.transform.position - visibleHandler.position;
            _axis = Vector3.Cross(visibleHandler.forward, direction);
            var abs = Mathf.Abs(_axis.y);
            //滑らかに動く
            if (SkipMoveMode && abs >= 0.08f)
            {
                Value = _value + (_coefficient * _axis.y * Time.deltaTime);
                ValueUpdate?.Invoke();

                var touch = _isGrabbedByLeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
                ControllerVibration.Execute(touch, 1, 0.2f, 0.05f);
            }
            //Min設定値で刻む
            else if (abs >= 0.02f)
            {
                Value = _value + (Mathf.Sign(_axis.y) * minStepValuel);
                ValueUpdate?.Invoke();

                var touch = _isGrabbedByLeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
                ControllerVibration.Execute(touch, 1, 0.4f, 0.05f);
            }
        }
    }
}
