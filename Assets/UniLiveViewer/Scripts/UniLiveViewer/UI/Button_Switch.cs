using System.Collections;
using UnityEngine;

namespace UniLiveViewer
{
    public class Button_Switch : Button_Base
    {
        /// <summary>
        /// �N���b�N���o
        /// </summary>
        protected override IEnumerator ClickDirecting()
        {
            //���x�������Ȃ��悤�ɕ������������
            myRb.isKinematic = true;
            myCol.enabled = false;

            //���]
            isEnable = !isEnable;

            //���W������
            collisionChecker.transform.localPosition = Vector3.zero;

            //�U������
            if (collisionChecker.isTouchL) PlayerStateManager.ControllerVibration(OVRInput.Controller.LTouch, 1, 1, 0.1f);
            else PlayerStateManager.ControllerVibration(OVRInput.Controller.RTouch, 1, 1f, 0.1f);

            yield return new WaitForSeconds(delayTime);

            //����t���Ȃ��悤�ɏ����O�i
            collisionChecker.transform.position -= collisionChecker.transform.forward * 0.001f;
            //�������Z���ĊJ
            myRb.isKinematic = false;

            //��ԃ��Z�b�g
            //collisionChecker.StateReset();

            //�A���ŐG��Ȃ��悤�Ɉ�莞�Ԍ�ɐG�����悤�ɂ���
            yield return new WaitForSeconds(0.25f);
            myCol.enabled = true;
        }
    }

}