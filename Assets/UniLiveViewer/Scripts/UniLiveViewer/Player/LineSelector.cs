using UnityEngine;

namespace UniLiveViewer
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineSelector : MonoBehaviour
    {
        //ベジェ曲線用
        [Header("＜曲線の設定＞")]
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

        //LineRenderer用
        private LineRenderer lineRenderer = null;
        //[SerializeField]
        //private float LineWidth = 0.01f;
        [SerializeField]
        private int positionCount = 10;


        //衝突検知
        [Header("＜衝突検知の設定＞")]
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private Vector3 rayDirection = new Vector3(0, -1, 0);
        public RaycastHit hitCollider;
        private Transform keepHitObj;

        //テレポート
        [Header("＜柱の設定＞")]
        [SerializeField]
        private Transform teleportPoint;
        private Renderer _renderer;
        private MaterialPropertyBlock materialPropertyBlock;
        private Color baseColor;
        [SerializeField] private Color hitColor = new Color(255.0f, 0.0f, 0.0f);

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            //LineRendererのパラメータ設定
            lineRenderer.positionCount = positionCount;

            //角度の初期値を取得
            EndAnchor_KeepEuler = LineEndAnchor.localRotation.eulerAngles;
            //レンダーとそのマテリアルプロパティを取得
            _renderer = teleportPoint.GetComponent<Renderer>();
            materialPropertyBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(materialPropertyBlock);
            //ベースカラーを取得
            baseColor = _renderer.material.GetColor("_TintColor");

            //開幕無効化しておく
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            //ベジェ曲線の開始点、中間点、終了点を算出する
            BezierCurvePoint[0] = LineStartAnchor.position;
            BezierCurvePoint[1] = LineStartAnchor.position + (LineStartAnchor.forward * distance / high);
            BezierCurvePoint[2] = LineStartAnchor.position + (LineStartAnchor.forward * distance);
            BezierCurvePoint[2].y = transform.position.y;//一旦親の高さに揃える

            //ベジェ曲線を作成
            Vector3 pos = Vector3.zero;
            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                //BezierCurveTimer:0～1
                BezierCurveTimer = (float)i / (lineRenderer.positionCount - 1);
                //ベジェ曲線の補完座標を取得
                pos = GetLerpPoint(BezierCurvePoint[0], BezierCurvePoint[1], BezierCurvePoint[2], BezierCurveTimer);
                //座標をLineRendererにセット
                lineRenderer.SetPosition(i, pos);
            }

            //床に向かってrayを飛ばす
            Physics.Raycast(rayOrigin.position, rayDirection, out hitCollider, 3.0f, Constants.LayerMaskStageFloor);
            //Debug.DrawRay(rayOrigin.position, rayDirection, Color.red);
            //床の高さに合わせる
            if (hitCollider.collider) BezierCurvePoint[2].y = hitCollider.point.y;

            //地面Anchor用のオブジェクトを移動する
            if (LineEndAnchor) LineEndAnchor.position = BezierCurvePoint[2];

            //衝突検知(なるべく短くしてる)
            //Physics.Raycast(LineEndAnchor.position, Vector3.up, out hitCollider, 2.0f, SystemInfo.layerMask_FieldObject);
            Physics.BoxCast(LineEndAnchor.position, Vector3.one * 0.1f ,Vector3.up, out hitCollider, transform.rotation,1.5f, Constants.LayerMaskFieldObject);
            //Debug.DrawRay(LineEndAnchor.position, Vector3.up, Color.red);
        }

        /// <summary>
        /// 柱の色を設定
        /// </summary>
        public void SetMaterial(bool isForcedReset)
        {
            //強制初期化
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

                //hit状態に応じてマテリアルプロパティの色情報を変更
                if (hitCollider.transform) materialPropertyBlock.SetColor("_TintColor", hitColor);
                else materialPropertyBlock.SetColor("_TintColor", baseColor);
                //レンダーにプロパティをセット
                _renderer.SetPropertyBlock(materialPropertyBlock);
            }
        }

        /// <summary>
        /// GroundPointerのオイラー角度を加算する
        /// </summary>
        /// <param 加算する角度="addAngles"></param>
        public void GroundPointer_AddEulerAngles(Vector3 addAngles)
        {
            Vector3 eulerAngles = LineEndAnchor.localRotation.eulerAngles + addAngles;
            LineEndAnchor.localRotation = Quaternion.Euler(eulerAngles);
        }

        /// <summary>
        /// ベジェ曲線上の補間座標を返す
        /// </summary>
        /// <param 開始点="point0"></param>
        /// <param 中間点="point1"></param>
        /// <param 終了点="point2"></param>
        /// <param Lerp係数="time"></param>
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