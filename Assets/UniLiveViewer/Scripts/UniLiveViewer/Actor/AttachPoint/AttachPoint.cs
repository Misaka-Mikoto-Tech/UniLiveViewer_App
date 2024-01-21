using System;
using UniLiveViewer.ValueObject;
using UnityEngine;

namespace UniLiveViewer.Actor.AttachPoint
{
    /// <summary>
    /// MonoBehaviourけしたい..
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(SphereCollider))]
    public class AttachPoint : MonoBehaviour
    {
        MeshRenderer _meshRenderer;
        SphereCollider _sphereCollider;

        public HumanBodyBones HumanBodyBones => _humanBodyBones;
        HumanBodyBones _humanBodyBones;

        /// <summary>
        /// MyActor特定用
        /// </summary>
        public InstanceId InstanceId => _instanceId;
        InstanceId _instanceId;

        void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _sphereCollider = GetComponent<SphereCollider>();
        }

        public void Setup(InstanceId instanceId, HumanBodyBones humanBodyBones, Vector3 pos, Quaternion rot, float scale)
        {
            _instanceId = instanceId;
            _humanBodyBones = humanBodyBones;
            transform.name = "AP_" + Enum.GetName(typeof(HumanBodyBones), humanBodyBones);
            transform.SetPositionAndRotation(pos, rot);
            transform.localScale *= scale;
        }

        public void SetActive(bool isActive)
        {
            _meshRenderer.enabled = isActive;
            _sphereCollider.enabled = isActive;
        }
    }
}