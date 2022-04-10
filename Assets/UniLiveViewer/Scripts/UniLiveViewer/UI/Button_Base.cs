using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UniLiveViewer
{
    public class Button_Base : MonoBehaviour
    {
        [SerializeField] protected bool _isEnable = true;
        public bool isEnable
        {
            get
            {
                return _isEnable;
            }
            set
            {
                _isEnable = value;
                if (collisionChecker)
                {
                    //強制的に親の状態に揃える
                    if (_isEnable) collisionChecker.myState = SWITCHSTATE.ON;
                    else collisionChecker.myState = SWITCHSTATE.OFF;
                }
            }
        }
        //好きな方使う
        public event Action<Button_Base> onTrigger;
        public UnityEvent onTrigger_Event;//GUI版(パフォーマンスとトレードオフ)

        //public bool isTrigger { get;  protected set; }
        [SerializeField] protected float delayTime = 1.0f;

        //Parameter
        public CollisionChecker collisionChecker = null;
        [SerializeField] private Transform neutralAnchor = null;

        protected Rigidbody myRb;
        protected BoxCollider myCol;

        // Start is called before the first frame update
        protected virtual void Awake()
        {
            myRb = collisionChecker.GetComponent<Rigidbody>();
            myCol = collisionChecker.GetComponent<BoxCollider>();
        }

        private void FixedUpdate()
        {
            //バネ運動処理
            AddSpringForce(1500);
            AddSpringForceExtra();
        }

        private void LateUpdate()
        {
            if (myRb.isKinematic) return;
            //ボタンの移動範囲制限
            if (collisionChecker.transform.localPosition.z >= 0.0f)
            {
                //ボタンに触れていれば
                if (collisionChecker.Touching())
                {
                    ClickAction();
                }
            }
        }

        /// <summary>
        /// クリック処理
        /// </summary>
        public void ClickAction()
        {
            //isTrigger = true;
            if (!gameObject.activeSelf) return;

            StartCoroutine(ClickDirecting());
            //押したと判定する
            onTrigger?.Invoke(this);
            onTrigger_Event?.Invoke();
        }

        /// <summary>
        /// クリック演出
        /// </summary>
        protected virtual IEnumerator ClickDirecting()
        {
            //何度も押せないように物理判定を消す
            myRb.isKinematic = true;
            myCol.enabled = false;

            //フラグ初期化する
            isEnable = true;

            //座標初期化
            collisionChecker.transform.localPosition = Vector3.zero;

            //振動処理
            if (collisionChecker.isTouchL) PlayerStateManager.ControllerVibration(OVRInput.Controller.LTouch, 1, 1, 0.1f);
            else PlayerStateManager.ControllerVibration(OVRInput.Controller.RTouch, 1, 1f, 0.1f);

            //押した後のインターバル
            yield return new WaitForSeconds(delayTime);

            //張り付かないように少し前進
            collisionChecker.transform.position -= collisionChecker.transform.forward * 0.001f;

            //物理演算を再開し、触れられるように戻す 
            myRb.isKinematic = false;
            myCol.enabled = true;
        }

        /// <summary>
        /// テキストメッシュに指定文字列を設定
        /// </summary>
        /// <param name="str"></param>
        public void SetTextMesh(string str)
        {
            if (collisionChecker.colorSetting == null || !collisionChecker.colorSetting[0].textMesh) return;
            collisionChecker.colorSetting[0].textMesh.text = str;
        }

        /// <summary>
        /// バネの力を加える
        /// </summary>
        /// <param バネ係数="r"></param>
        private void AddSpringForce(float r)
        {
            var diff = neutralAnchor.position - collisionChecker.transform.position; //バネの伸び
            var force = diff * r;
            myRb.AddForce(force);
        }

        /// <summary>
        /// オーバーシュートしないバネの力を加える
        /// </summary>
        private void AddSpringForceExtra()
        {
            var r = myRb.mass * myRb.drag * myRb.drag / 4f;//バネ係数
            var diff = neutralAnchor.position - collisionChecker.transform.position; //バネの伸び
            var force = diff * r;
            myRb.AddForce(force);
        }

        private void OnEnable()
        {
            StartCoroutine(InitDirecting());
        }

        private IEnumerator InitDirecting()
        {
            //押せないように物理判定を消す
            myRb.isKinematic = true;
            myCol.enabled = false;

            //座標初期化
            collisionChecker.transform.localPosition = Vector3.zero;

            //インターバル
            yield return new WaitForSeconds(delayTime);

            //張り付かないように少し前進
            collisionChecker.transform.position -= collisionChecker.transform.forward * 0.001f;

            //物理演算を再開し、触れられるように戻す 
            myRb.isKinematic = false;
            myCol.enabled = true;
        }
    }
}