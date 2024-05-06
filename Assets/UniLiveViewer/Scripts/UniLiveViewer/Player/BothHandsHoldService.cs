using System.Collections.Generic;
using UniLiveViewer.OVRCustom;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Player
{
    /// <summary>
    /// OVRGrabberを両手握りに無理やり対応、古いままなので雑
    /// </summary>
    public class BothHandsHoldService
    {
        List<OVRGrabber_UniLiveViewer> _ovrGrabbers;

        /// <summary>
        /// inspectorで両手確認用
        /// </summary>
        OVRGrabbable_Custom[] _bothHandsCandidate = new OVRGrabbable_Custom[2];
        //両手で掴む
        OVRGrabbable_Custom _bothHandsGrabObj;
        Vector3 _initBothHandsDistance;
        Transform _bothHandsCenterAnchor;

        [Inject]
        public void Construct(List<OVRGrabber_UniLiveViewer> ovrGrabbers)
        {
            _ovrGrabbers = ovrGrabbers;
        }

        public void Setup()
        {
            _bothHandsCenterAnchor = new GameObject("BothHandsCenter").transform;
        }

        public void OnLateTick()
        {
            //両手で掴むオブジェクトがあれば座標を上書きする
            if (!_bothHandsGrabObj) return;
            //両手の中間座標
            var bothHandsDistance = (_ovrGrabbers[1].GetGripPoint - _ovrGrabbers[0].GetGripPoint);
            _bothHandsCenterAnchor.localScale = Vector3.one * bothHandsDistance.sqrMagnitude / _initBothHandsDistance.sqrMagnitude;
            _bothHandsCenterAnchor.position = bothHandsDistance * 0.5f + _ovrGrabbers[0].GetGripPoint;
            _bothHandsCenterAnchor.forward = (_ovrGrabbers[0].transform.forward + _ovrGrabbers[1].transform.forward) * 0.5f;
        }

        /// <summary>
        /// 両手掴み候補として登録
        /// </summary>
        /// <param name="newHand"></param>
        public void BothHandsCandidate(OVRGrabber_UniLiveViewer newHand)
        {
            if (newHand == _ovrGrabbers[0])
            {
                _bothHandsCandidate[0] = newHand.GrabbedObj.Value;

                //直前まで反対の手で掴んでいたオブジェクトなら
                if (_bothHandsCandidate[1] != _bothHandsCandidate[0]) return;
                //両手用オブジェクトとしてセット
                _bothHandsGrabObj = _bothHandsCandidate[0];
                //初期値を記録
                _initBothHandsDistance = (_ovrGrabbers[1].GetGripPoint - _ovrGrabbers[0].GetGripPoint);
                _bothHandsCenterAnchor.position = _initBothHandsDistance * 0.5f + _ovrGrabbers[0].GetGripPoint;
                _bothHandsCenterAnchor.forward = (_ovrGrabbers[0].transform.forward + _ovrGrabbers[1].transform.forward) * 0.5f;
                _bothHandsGrabObj.transform.parent = _bothHandsCenterAnchor;
            }
            else if (newHand == _ovrGrabbers[1])
            {
                _bothHandsCandidate[1] = newHand.GrabbedObj.Value;

                //直前まで反対の手で掴んでいたオブジェクトなら
                if (_bothHandsCandidate[0] != _bothHandsCandidate[1]) return;
                //両手用オブジェクトとしてセット
                _bothHandsGrabObj = _bothHandsCandidate[1];
                //初期値を記録
                _initBothHandsDistance = (_ovrGrabbers[1].GetGripPoint - _ovrGrabbers[0].GetGripPoint);
                _bothHandsCenterAnchor.position = _initBothHandsDistance * 0.5f + _ovrGrabbers[0].GetGripPoint;
                _bothHandsCenterAnchor.forward = (_ovrGrabbers[0].transform.forward + _ovrGrabbers[1].transform.forward) * 0.5f;
                _bothHandsGrabObj.transform.parent = _bothHandsCenterAnchor;
            }
        }

        /// <summary>
        /// 反対の手で持ち直す
        /// </summary>
        /// <param name="releasedHand"></param>
        public void BothHandsGrabEnd(OVRGrabber_UniLiveViewer releasedHand)
        {
            //両手に何もなければ処理しない
            if (!_bothHandsCandidate[0] && !_bothHandsCandidate[1]) return;

            //初期化
            if (releasedHand == _ovrGrabbers[0])
            {
                if (_bothHandsCandidate[0] == _bothHandsCandidate[1])
                {
                    _ovrGrabbers[1].ForceGrabBegin(_bothHandsGrabObj);
                }
                _bothHandsCandidate[0] = null;
            }
            else if (releasedHand == _ovrGrabbers[1])
            {
                if (_bothHandsCandidate[0] == _bothHandsCandidate[1])
                {
                    _ovrGrabbers[0].ForceGrabBegin(_bothHandsGrabObj);
                }
                _bothHandsCandidate[1] = null;
            }
            //両手は終了
            if (_bothHandsGrabObj)
            {
                _bothHandsGrabObj.transform.parent = null;
                _bothHandsCenterAnchor.localScale = Vector3.one;
                _bothHandsGrabObj = null;
            }
        }
    }
}
