using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniLiveViewer.Timeline;
using UniLiveViewer.ValueObject;
using UnityEngine;
using VContainer;

namespace UniLiveViewer.Actor.AttachPoint
{
    /// <summary>
    /// Actorの特定のボーンにデコアイテムを付与できる専用コライダーを生成する
    /// SDや人型以外に対応しきれていない
    /// </summary>
    public class AttachPointService
    {
        IReadOnlyDictionary<HumanBodyBones, Transform> _boneMap;
        readonly List<AttachPoint> _attachPoints = new();

        readonly InstanceId _instanceId;
        readonly PlayableAnimationClipService _playableAnimationClipService;
        readonly AttachPoint _attachPointPrefab;

        [Inject]
        public AttachPointService(
            InstanceId instanceId,
            AttachPoint attachPoint,
            PlayableAnimationClipService playableAnimationClipService)
        {
            _instanceId = instanceId;
            _attachPointPrefab = attachPoint;
            _playableAnimationClipService = playableAnimationClipService;
        }

        public void SetActive(bool isActive)
        {
            foreach (var attachPoint in _attachPoints)
            {
                attachPoint.SetActive(isActive);
            }
        }

        public async UniTask SetupAsync(IReadOnlyDictionary<HumanBodyBones, Transform> boneMap, CancellationToken cancellation)
        {
            _boneMap = boneMap;
            await SetupAsync(HumanBodyBones.Hips, 0.35f, cancellation);
            await SetupAsync(HumanBodyBones.LeftLowerLeg, 0.15f, cancellation);
            await SetupAsync(HumanBodyBones.RightLowerLeg, 0.15f, cancellation);
            await SetupAsync(HumanBodyBones.Spine, 0.2f, cancellation);
            await SetupAsync(HumanBodyBones.Chest, 0.2f, cancellation);//任意
            await SetupAsync(HumanBodyBones.UpperChest, 0.35f, cancellation);//超任意
            await SetupAsync(HumanBodyBones.LeftLowerArm, 0.15f, cancellation);
            await SetupAsync(HumanBodyBones.RightLowerArm, 0.15f, cancellation);
            await SetupAsync(HumanBodyBones.Neck, 0.1f, cancellation);//任意
            await SetupHeadAsync(cancellation);
            await SetupLeftHandAsync(cancellation);
            await SetupRightHandAsync(cancellation);

            SetActive(false);
        }

        async UniTask SetupAsync(HumanBodyBones humanBodyBone, float scale, CancellationToken cancellation)
        {
            var bone = _boneMap[humanBodyBone];
            if (bone == null) return;
            var go = GameObject.Instantiate(_attachPointPrefab.gameObject, bone);
            var ap = go.transform.GetComponent<AttachPoint>();
            ap.Setup(_instanceId, humanBodyBone, bone.position, bone.rotation, scale);

            //go.name = "AP_" + Enum.GetName(typeof(HumanBodyBones), humanBodyBone);
            //go.transform.SetPositionAndRotation(bone.position, bone.rotation);
            //go.transform.localScale *= scale;

            _attachPoints.Add(ap);

            await UniTask.Yield(cancellationToken: cancellation);
        }

        /// <summary>
        /// 中点微妙...サイズも大分ズレる。。
        /// バウンディングボックスもどうかなぁ、単純にoffsetが安定な気がする
        /// TODO: VRMの顔メッシュ取得する奴ためす
        /// </summary>
        /// <returns></returns>
        async UniTask SetupHeadAsync(CancellationToken cancellation)
        {
            var bone = _boneMap[HumanBodyBones.Head];
            if (bone == null) return;
            var childPositions = bone.Cast<Transform>().Select(t => t.position).ToList();

            // 子ボーンの中点があれば適用
            var point = bone.position;
            var maxHeight = bone.position.y;
            if (childPositions.Count > 0)
            {
                var sum = Vector3.zero;
                foreach (var pos in childPositions)
                {
                    sum += pos;
                    if (maxHeight < pos.y) maxHeight = pos.y;
                }
                var average = sum / childPositions.Count;
                point.y = average.y;
            }

            var go = GameObject.Instantiate(_attachPointPrefab.gameObject, bone);
            var ap = go.transform.GetComponent<AttachPoint>();
            var scale = (maxHeight - bone.position.y) * 1.5f;//小さすぎるので微調整
            ap.Setup(_instanceId, HumanBodyBones.Head, point, bone.rotation, scale);

            //go.name = "AP_" + Enum.GetName(typeof(HumanBodyBones), HumanBodyBones.Head);
            //go.transform.SetPositionAndRotation(point, bone.rotation);
            //var scale = (maxHeight - bone.position.y) * 1.5f;//小さすぎるので微調整
            //go.transform.localScale *= scale;

            _attachPoints.Add(ap);

            await UniTask.Yield(cancellationToken: cancellation);
        }

        /// <summary>
        /// 指は全て任意ボーン
        /// </summary>
        /// <returns></returns>
        async UniTask SetupLeftHandAsync(CancellationToken cancellation)
        {
            var handBone = _boneMap[HumanBodyBones.LeftHand];
            var fingerTipBone = _boneMap[HumanBodyBones.LeftMiddleDistal];

            Vector3 fingerDistance;
            float handSize;
            if (fingerTipBone != null)
            {
                fingerDistance = fingerTipBone.position - handBone.position;
                handSize = Vector3.Distance(handBone.position, fingerTipBone.position);
            }
            else
            {
                //しょうがないので謎ロジックで調整
                var upperArm = _boneMap[HumanBodyBones.LeftUpperArm];
                fingerDistance = (handBone.position - upperArm.position) * 0.5f;
                handSize = Vector3.Distance(handBone.position, upperArm.position) * 0.5f;
            }
            var go = GameObject.Instantiate(_attachPointPrefab.gameObject, handBone);
            var ap = go.transform.GetComponent<AttachPoint>();
            var point = handBone.position + fingerDistance * 0.5f;
            ap.Setup(_instanceId, HumanBodyBones.LeftHand, point, handBone.rotation, handSize);

            //go.name = "AP_" + Enum.GetName(typeof(HumanBodyBones), HumanBodyBones.LeftHand);
            //go.transform.SetPositionAndRotation(handBone.position + fingerDistance * 0.5f, handBone.rotation);
            //go.transform.localScale *= handSize;

            _attachPoints.Add(ap);

            await UniTask.Yield(cancellationToken: cancellation);
        }

        /// <summary>
        /// 指は全て任意ボーン
        /// </summary>
        /// <returns></returns>
        async UniTask SetupRightHandAsync(CancellationToken cancellation)
        {
            var handBone = _boneMap[HumanBodyBones.RightHand];
            var fingerTipBone = _boneMap[HumanBodyBones.RightMiddleDistal];
            Vector3 fingerDistance;
            float handSize;

            if (fingerTipBone != null)
            {
                fingerDistance = fingerTipBone.position - handBone.position;
                handSize = Vector3.Distance(handBone.position, fingerTipBone.position);
            }
            else
            {
                //しょうがないので謎ロジックで調整
                var upperArm = _boneMap[HumanBodyBones.RightUpperArm];
                fingerDistance = (handBone.position - upperArm.position) * 0.5f;
                handSize = Vector3.Distance(handBone.position, upperArm.position) * 0.5f;
            }
            var go = GameObject.Instantiate(_attachPointPrefab.gameObject, handBone);

            var ap = go.transform.GetComponent<AttachPoint>();
            var point = handBone.position + fingerDistance * 0.5f;
            ap.Setup(_instanceId, HumanBodyBones.RightHand, point, handBone.rotation, handSize);


            //go.name = "AP_" + Enum.GetName(typeof(HumanBodyBones), HumanBodyBones.RightHand);
            //go.transform.SetPositionAndRotation(handBone.position + fingerDistance * 0.5f, handBone.rotation);
            //go.transform.localScale *= handSize;

            _attachPoints.Add(ap);

            await UniTask.Yield(cancellationToken: cancellation);
        }

        /// <summary>
        /// TODO: 本当は奪い取ったタイミングで手を開きたいが現状厳しいので
        /// timeline再生タイミングで全員開閉チェックしている（どうせ多分インタラクション変える
        /// </summary>
        public void OnPlayTimeline()
        {
            foreach (var ap in _attachPoints)
            {
                if (ap.HumanBodyBones == HumanBodyBones.LeftHand ||
                    ap.HumanBodyBones == HumanBodyBones.RightHand)
                {
                    // LS化しないと解除判定ムズイので強引に常時0チェック
                    if (ap.transform.childCount != 0) continue;
                    _playableAnimationClipService.SetHandAnimation(ap.InstanceId, ap.HumanBodyBones, false);
                }
            }
        }
    }
}