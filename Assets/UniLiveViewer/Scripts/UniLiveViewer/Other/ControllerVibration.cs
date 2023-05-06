using Cysharp.Threading.Tasks;
using NanaCiel;
using UnityEngine;

namespace UniLiveViewer
{
    public class ControllerVibration
    {
        /// <summary>
        /// Playerインスタンスにコントローラー振動を指示
        /// </summary>
        /// <param name="touch">RTouch or LTouch</param>
        /// <param name="frequency">周波数0~1(1の方が繊細な気がする)</param>
        /// <param name="amplitude">振れ幅0~1(0で停止)</param>
        /// <param name="time">振動時間、上限2秒らしい</param>
        public static void Execute(OVRInput.Controller touch, float frequency, float amplitude, float time)
        {
            if (!SystemInfo.userProfile.TouchVibration) return;

            ExecuteInternal(touch, frequency, amplitude, time);
        }

        /// <summary>
        /// 振動開始から終了までのタスクを実行する
        /// </summary>
        /// <param name="touch">RTouch or LTouch</param>
        /// <param name="frequency">周波数0~1(1の方が繊細な気がする)</param>
        /// <param name="amplitude">振れ幅0~1(0で停止)</param>
        /// <param name="time">振動時間、上限2秒らしい</param>
        static void ExecuteInternal(OVRInput.Controller touch, float frequency, float amplitude, float time)
        {
            int milliseconds = (int)(Mathf.Clamp(time, 0, 2) * 1000);

            UniTask.Void(async () =>
            {
                try
                {
                    //振動開始
                    OVRInput.SetControllerVibration(frequency, amplitude, touch);
                    await UniTask.Delay(milliseconds).OnError();
                }
                catch (System.OperationCanceledException)
                {
                    Debug.Log("振動中にPlayerが削除");
                }
                finally
                {
                    //振動停止
                    OVRInput.SetControllerVibration(frequency, 0, touch);
                }
            });
        }
    }
}
