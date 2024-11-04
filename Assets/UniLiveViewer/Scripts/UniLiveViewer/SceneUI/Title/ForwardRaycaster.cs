using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// NOTE: SDK引っ越すまでの繋ぎ
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class ForwardRaycaster : MonoBehaviour
{
    [SerializeField] OVRInput.RawButton _triggerButton = OVRInput.RawButton.LIndexTrigger;
    [SerializeField] Transform _rayOriginObject;
    [SerializeField] LayerMask _uiLayerMask;
    [SerializeField] float _rayLength = 10f;
    [SerializeField] float _scrollSpeed = 1000f;

    LineRenderer _lineRenderer;
    ScrollRect _scrollRect;
    Vector3 _preHitPoint;
    bool _isScrolling;
    Button _lastHoveredButton = null;

    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (_lastHoveredButton)
        {
            ExecuteEvents.Execute(_lastHoveredButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerExitHandler);
            _lastHoveredButton = null;
        }

        var ray = new Ray(_rayOriginObject.position, _rayOriginObject.forward);
        if (Physics.Raycast(ray, out var hit, _rayLength, _uiLayerMask))
        {
            var localDir = _lineRenderer.transform.InverseTransformPoint(hit.point) - _rayOriginObject.localPosition;

            _lineRenderer.SetPosition(0, Vector3.zero);
            _lineRenderer.SetPosition(1, localDir);

            HandleUIInteraction(hit);
        }
        else
        {
            _isScrolling = false;

            _lineRenderer.SetPosition(0, Vector3.zero);
            _lineRenderer.SetPosition(1, Vector3.forward * 0.3f);
        }
    }



    /// <summary>
    /// UI要素に対してインタラクションを処理
    /// </summary>
    void HandleUIInteraction(RaycastHit hit)
    {
        // ボタンのクリック処理
        var button = hit.collider.GetComponent<Button>();
        if (button != null)
        {
            _lastHoveredButton = button;
            ExecuteEvents.Execute(button.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerEnterHandler);
            if (OVRInput.GetDown(_triggerButton))
            {
                button.onClick.Invoke();
            }
        }

        // スクロールビューの操作
        var nextScrollRect = hit.collider.GetComponent<ScrollRect>();
        if (nextScrollRect != null)
        {
            if (!_isScrolling && OVRInput.GetDown(_triggerButton))
            {
                // スクロールを開始する
                _scrollRect = nextScrollRect;
                _preHitPoint = hit.point;
                _isScrolling = true;
            }
            else if (_isScrolling)
            {
                var currentHitPoint = hit.point;
                var delta = currentHitPoint - _preHitPoint;

                // 移動量に基づいてScrollRectのvelocityを調整
                _scrollRect.velocity = new Vector2(0, delta.y * _scrollSpeed);

                // スクロール量をRayの移動量に基づいて調整（縦スクロールの場合）
                //_scrollRect.verticalNormalizedPosition -= delta.y * 0.1f; // スクロール速度を調整

                _preHitPoint = currentHitPoint;

                if (OVRInput.GetUp(_triggerButton))
                {
                    _isScrolling = false;
                }
            }
        }
    }
}

