using UnityEngine;
using VContainer;

namespace UniLiveViewer.General
{
    public class PositionHandlerService
    {
        const float LerpSpeed = 20.0f;

        readonly PositionHandlerSrcAnchor _srcAnchor;
        readonly PositionHandlerDestAnchor _destAnchor;

        [Inject]
        public PositionHandlerService(
            PositionHandlerDestAnchor destAnchor,
            PositionHandlerSrcAnchor srcAnchor)
        {
            _destAnchor = destAnchor;
            _srcAnchor = srcAnchor;
        }

        public void OnLateTick()
        {
            var moveStep = LerpSpeed * Time.deltaTime;
            _srcAnchor.transform.SetPositionAndRotation(
                Vector3.Lerp(_srcAnchor.transform.position, _destAnchor.transform.position, moveStep),
                Quaternion.Lerp(_srcAnchor.transform.rotation, _destAnchor.transform.rotation, moveStep));
        }
    }
}