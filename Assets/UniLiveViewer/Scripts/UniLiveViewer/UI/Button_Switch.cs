using System.Collections;
using UnityEngine;

namespace UniLiveViewer
{
    public class Button_Switch : Button_Base
    {
        /// <summary>
        /// クリック演出
        /// </summary>
        protected override IEnumerator ClickDirecting()
        {
            //何度も押せないように物理判定を消す
            myRb.isKinematic = true;
            myCol.enabled = false;

            //反転
            isEnable = !isEnable;

            //座標初期化
            collisionChecker.transform.localPosition = Vector3.zero;

            //振動処理
            if (collisionChecker.isTouchL) PlayerStateManager.ControllerVibration(OVRInput.Controller.LTouch, 1, 1, 0.1f);
            else PlayerStateManager.ControllerVibration(OVRInput.Controller.RTouch, 1, 1f, 0.1f);

            yield return new WaitForSeconds(delayTime);

            //張り付かないように少し前進
            collisionChecker.transform.position -= collisionChecker.transform.forward * 0.001f;
            //物理演算を再開
            myRb.isKinematic = false;

            //状態リセット
            //collisionChecker.StateReset();

            //連続で触れないように一定時間後に触れられるようにする
            yield return new WaitForSeconds(0.25f);
            myCol.enabled = true;
        }
    }

}