using UnityEngine;

namespace UniLiveViewer.Actor.LookAt
{
    /// <summary>
    /// 実質プリセットであるFBX専用
    /// </summary>
    public class LookAtSettings : MonoBehaviour
    {
        public Vector2 eyeAmplitude => _eyeAmplitude;
        [SerializeField] protected Vector2 _eyeAmplitude;

        public SkinnedMeshRenderer Face => _face;
        [SerializeField] SkinnedMeshRenderer _face;
    }
}