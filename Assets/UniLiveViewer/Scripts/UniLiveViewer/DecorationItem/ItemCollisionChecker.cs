using System;
using UnityEngine;

namespace UniLiveViewer
{
    [RequireComponent(typeof(BoxCollider))]
    public class ItemCollisionChecker : MonoBehaviour
    {
        public event Action<ItemCollisionChecker, Collider> OnTrigger;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(SystemInfo.tag_ItemMaterial))
            {
                OnTrigger?.Invoke(this, other);
            }
        }
    }
}