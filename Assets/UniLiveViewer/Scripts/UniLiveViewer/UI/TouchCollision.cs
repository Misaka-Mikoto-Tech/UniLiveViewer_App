using UnityEngine;

namespace UniLiveViewer 
{
    //RollSelectorÇÃUIóp
    public class TouchCollision : MonoBehaviour
    {
        public bool isTouch = false;

        private void OnTriggerStay(Collider other)
        {
            if (isTouch) return;

            isTouch = true;

            //êUìÆèàóù
            if (other.transform.name.Contains("_l_"))
            {
                PlayerStateManager.ControllerVibration(OVRInput.Controller.LTouch, 1, 1, 0.1f);
            }
            else if (other.transform.name.Contains("_r_"))
            {
                PlayerStateManager.ControllerVibration(OVRInput.Controller.RTouch, 1, 1, 0.1f);
            }
        }
    }
}
