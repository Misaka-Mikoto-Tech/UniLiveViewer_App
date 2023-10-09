using UnityEngine;
using System;
using UniLiveViewer.OVRCustom;

namespace UniLiveViewer 
{
    public class SliderGrabController : MonoBehaviour
    {
        public Transform visibleHandler;
        [SerializeField] private Transform startAnchor;
        [SerializeField] private Transform endAnchor;
        [SerializeField] private Transform[] handMesh = new Transform[2];

        [Header("VisibleHandleの子オブジェクトを指定")]
        [SerializeField] private OVRGrabbable_Custom unVisibleHandler = null;
        private Vector3 nextHandllocalPos;
        private float handleMaxRangeX = 0;
        public float maxValuel = 1.0f;//スライダーの最大値
        public float minStepValuel = 0.1f;//スライダーを動かす間隔
        [HideInInspector] public bool isControl = false;//操作中フラグ
        private Vector3 axis = Vector3.zero;

        [SerializeField] private bool SkipMoveMode = false;
        private float coefficient;
        private bool isLHandGrabbed = false;

        public event Action Controled;
        public event Action UnControled;
        public event Action ValueUpdate;

        [Header("確認用(readonly)")]
        [SerializeField] private float _value = 0;

        /// <summary>
        /// ハンドルに指定したオブジェクトを範囲内で制御する(0～1)
        /// </summary>
        public float Value
        {
            get { return _value; }
            set
            {
                _value = Mathf.Clamp(value, 0, maxValuel);
                nextHandllocalPos.x = handleMaxRangeX * _value / maxValuel;
                visibleHandler.localPosition = startAnchor.localPosition + nextHandllocalPos;
            }
        }

        private void Awake()
        {
            handleMaxRangeX = endAnchor.localPosition.x - startAnchor.localPosition.x;
            //0でスライダーの位置を初期化する
            Value = 0;
        }

        private void Start()
        {
            //ハンドルの初期化
            initGrabHand();
            //係数決定
            coefficient = maxValuel / minStepValuel / 2;
        }

        /// <summary>
        /// ハンドルを掴んでいない状態に戻す
        /// </summary>
        private void initGrabHand()
        {
            unVisibleHandler.transform.parent = visibleHandler;
            unVisibleHandler.transform.localPosition = Vector3.zero;

            handMesh[0].parent.transform.localRotation = Quaternion.identity;

            //UI用handを非表示に
            if (handMesh[0].gameObject.activeSelf) handMesh[0].gameObject.SetActive(false);
            if (handMesh[1].gameObject.activeSelf) handMesh[1].gameObject.SetActive(false);
        }

        void Update()
        {
            //スライダー非制御中
            if (!isControl)
            {
                //ハンドルが掴まれたら制御中へ移行
                if (unVisibleHandler.isGrabbed)
                {
                    unVisibleHandler.transform.parent = null;//必須
                    isControl = true;

                    //実際の手を非表示
                    var realHand = (OVRGrabber_UniLiveViewer)unVisibleHandler.grabbedBy;
                    realHand.handMeshRoot.gameObject.SetActive(false);

                    //UI用の手を表示
                    if (unVisibleHandler.grabbedBy.name.Contains("HandL"))
                    {
                        if ((realHand.transform.right).y <= 0)
                        {
                            handMesh[0].parent.transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, 180));
                        }

                        isLHandGrabbed = true;
                        handMesh[0].gameObject.SetActive(true);
                    }
                    else if (unVisibleHandler.grabbedBy.name.Contains("HandR"))
                    {
                        if ((-realHand.transform.right).y <= 0)
                        {
                            handMesh[1].parent.transform.localRotation *= Quaternion.Euler(new Vector3(0, 0, 180));
                        }


                        isLHandGrabbed = false;
                        handMesh[1].gameObject.SetActive(true);
                    }

                    //操作開始
                    Controled?.Invoke();
                }
            }
            //スライダー制御中
            else
            {
                //距離を算出
                Vector3 dt = unVisibleHandler.transform.position - visibleHandler.position;
                //外積
                axis = Vector3.Cross(visibleHandler.forward, dt);
                var abs = Mathf.Abs(axis.y);
                //滑らかに動く
                if (SkipMoveMode && abs >= 0.08f)
                {
                    Value = _value + (coefficient * axis.y * Time.deltaTime);
                    ValueUpdate?.Invoke();
                    //コントローラーの振動
                    if (isLHandGrabbed) ControllerVibration.Execute(OVRInput.Controller.LTouch, 1, 0.2f, 0.05f);
                    else ControllerVibration.Execute(OVRInput.Controller.RTouch, 1, 0.2f, 0.05f);
                }
                //minの設定値で刻む
                else if (abs >= 0.02f)
                {
                    Value = _value + (Mathf.Sign(axis.y) * minStepValuel);
                    ValueUpdate?.Invoke();
                    //コントローラーの振動
                    if (isLHandGrabbed) ControllerVibration.Execute(OVRInput.Controller.LTouch, 1, 0.4f, 0.05f);
                    else ControllerVibration.Execute(OVRInput.Controller.RTouch, 1, 0.4f, 0.05f);
                }

                //handleを離したら
                if (!unVisibleHandler.isGrabbed)
                {
                    initGrabHand();
                    isControl = false;
                    //操作終了
                    UnControled?.Invoke();
                }
            }
        }

        private void OnEnable()
        {
            initGrabHand();
            isControl = false;
        }

        private void OnDisable()
        {
            //掴まれていたら解放する
            if (unVisibleHandler.isGrabbed)
            {
                unVisibleHandler.grabbedBy.ForceRelease(unVisibleHandler);
            }

            initGrabHand();
            isControl = false;
        }
    }
}
