using UnityEngine;

namespace UniLiveViewer.Player
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineSelector : MonoBehaviour
    {
        //ベジェ曲線用
        [Header("＜曲線の設定＞")]
        [SerializeField]
        Transform LineStartAnchor = null;
        public Transform LineEndAnchor = null;
        private Vector3 EndAnchor_KeepEuler = Vector3.zero;
        [SerializeField]
        float distance = 5.0f;
        [SerializeField]
        float high = 1.5f;
        Vector3[] _bezierCurvePoint = new Vector3[3];
        float _bezierCurveTimer = 0;

        //LineRenderer用
        LineRenderer _lineRenderer = null;
        [SerializeField]
        int positionCount = 10;

        //衝突検知
        [Header("＜衝突検知の設定＞")]
        [SerializeField] 
        Transform rayOrigin;
        [SerializeField] 
        Vector3 rayDirection = new Vector3(0, -1, 0);
        public RaycastHit HitActor;
        Transform _keepHitObj;

        //テレポート
        [Header("＜柱の設定＞")]
        [SerializeField]
        Transform teleportPoint;
        Renderer _renderer;
        MaterialPropertyBlock _materialPropertyBlock;
        Color _baseColor;
        [SerializeField] Color hitColor = new Color(255.0f, 0.0f, 0.0f);

        PlayerEnums.HandState _handState = PlayerEnums.HandState.DEFAULT;

        void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            //LineRendererのパラメータ設定
            _lineRenderer.positionCount = positionCount;

            //角度の初期値を取得
            EndAnchor_KeepEuler = LineEndAnchor.localRotation.eulerAngles;
            //レンダーとそのマテリアルプロパティを取得
            _renderer = teleportPoint.GetComponent<Renderer>();
            _materialPropertyBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(_materialPropertyBlock);
            _baseColor = _renderer.material.GetColor("_TintColor");

            //開幕無効化しておく
            gameObject.SetActive(false);
        }

        void Update()
        {
            UpdateBezierCurve();
            ChecFloorCollision();
            CheckActorCollision();

            var isForceReset = _handState != PlayerEnums.HandState.SUMMONCIRCLE;
            SetMaterial(isForceReset);
        }

        void UpdateBezierCurve()
        {
            //ベジェ曲線の開始点、中間点、終了点を算出する
            _bezierCurvePoint[0] = LineStartAnchor.position;
            _bezierCurvePoint[1] = LineStartAnchor.position + (LineStartAnchor.forward * distance / high);
            _bezierCurvePoint[2] = LineStartAnchor.position + (LineStartAnchor.forward * distance);
            _bezierCurvePoint[2].y = transform.position.y;//一旦親の高さに揃える

            var pos = Vector3.zero;
            for (int i = 0; i < _lineRenderer.positionCount; i++)
            {
                //BezierCurveTimer:0～1
                _bezierCurveTimer = (float)i / (_lineRenderer.positionCount - 1);
                //ベジェ曲線の補完座標を取得
                pos = GetLerpPoint(_bezierCurvePoint[0], _bezierCurvePoint[1], _bezierCurvePoint[2], _bezierCurveTimer);
                _lineRenderer.SetPosition(i, pos);
            }
        }

        void ChecFloorCollision()
        {
            //床に向かってrayを飛ばす
            Physics.Raycast(rayOrigin.position, rayDirection, out var hitCollider, 3.0f, Constants.LayerMaskStageFloor);
            //Debug.DrawRay(rayOrigin.position, rayDirection, Color.red);
            //床の高さに合わせる
            if (hitCollider.collider) _bezierCurvePoint[2].y = hitCollider.point.y;

            //地面Anchor用のオブジェクトを移動する
            if (LineEndAnchor) LineEndAnchor.position = _bezierCurvePoint[2];
        }

        void CheckActorCollision()
        {
            //衝突検知(なるべく短くしてる)
            Physics.Raycast(LineEndAnchor.position, Vector3.up, out var hitCollider, 2.0f, Constants.LayerMaskFieldObject);
            //Physics.BoxCast(LineEndAnchor.position, Vector3.one * 0.1f ,Vector3.up, out var hitCollider, transform.rotation,1.5f, Constants.LayerMaskFieldObject);
            HitActor = hitCollider;
            Debug.DrawRay(LineEndAnchor.position, Vector3.up, Color.red);
        }

        public void OnChangeHandState(PlayerEnums.HandState handState)
        {
            _handState = handState;
        }

        /// <summary>
        /// 柱の色を設定
        /// </summary>
        void SetMaterial(bool isForcedReset)
        {
            const string ColorName = "_TintColor";

            //強制初期化
            if (isForcedReset)
            {
                _keepHitObj = null;
                _materialPropertyBlock.SetColor(ColorName, _baseColor);
                _renderer.SetPropertyBlock(_materialPropertyBlock);
            }
            else
            {
                if (_keepHitObj == HitActor.transform) return;
                _keepHitObj = HitActor.transform;

                //hit状態に応じてマテリアルプロパティの色情報を変更
                if (HitActor.transform) _materialPropertyBlock.SetColor(ColorName, hitColor);
                else _materialPropertyBlock.SetColor(ColorName, _baseColor);
                //レンダーにプロパティをセット
                _renderer.SetPropertyBlock(_materialPropertyBlock);
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
        Vector3 GetLerpPoint(Vector3 point0, Vector3 point1, Vector3 point2, float time)
        {
            Vector3 movePointA = Vector3.Lerp(point0, point1, time);
            Vector3 movePointB = Vector3.Lerp(point1, point2, time);
            Vector3 movePointC = Vector3.Lerp(movePointA, movePointB, time);

            return movePointC;
        }

        void OnEnable()
        {
            LineEndAnchor.localRotation = Quaternion.Euler(EndAnchor_KeepEuler);
        }

        void OnDisable()
        {
            LineEndAnchor.localRotation = Quaternion.Euler(EndAnchor_KeepEuler);
        }
    }
}