using MagicaCloth;
using UnityEngine;
using UniVRM10;
using VRM;

namespace UniLiveViewer.Stage
{
    /// <summary>
    /// MagicaやVRMのコライダーがついている部分
    /// </summary>
    [RequireComponent(typeof(ColliderComponent))]
    [RequireComponent(typeof(VRMSpringBoneColliderGroup))]
    [RequireComponent(typeof(VRM10SpringBoneCollider))]
    public class PlayerHandVRMCollidersAnchor : MonoBehaviour
    {
        //目印
    }
}