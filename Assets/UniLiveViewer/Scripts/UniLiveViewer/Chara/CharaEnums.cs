

namespace UniLiveViewer
{
    public class CharaEnums
    {
        public enum STATE
        {
            /// <summary>
            /// 未
            /// </summary>
            NULL = 0,
            /// <summary>
            /// UI上の小サイズ
            /// </summary>
            MINIATURE,
            /// <summary>
            /// Playerにホールドされている
            /// </summary>
            HOLD,
            /// <summary>
            /// 召喚陣上
            /// </summary>
            ON_CIRCLE,
            /// <summary>
            /// ステージ上・召喚済み
            /// </summary>
            FIELD,
        }
        public enum ANIMATION_MODE
        {
            CLIP = 0,
            VMD,
        }

        public enum LIPTYPE
        {
            A = 0,
            I,
            U,
            E,
            O
        }

        public enum FACIALTYPE
        {
            /// <summary>
            /// 瞬き・寝てる時の目
            /// </summary>
            BLINK = 0,
            /// <summary>
            /// 喜び
            /// </summary>
            JOY,
            /// <summary>
            /// 怒り
            /// </summary>
            ANGRY,
            /// <summary>
            /// 悲しみ
            /// </summary>
            SORROW,
            /// <summary>
            /// 驚き
            /// </summary>
            SUP,
            /// <summary>
            /// 楽しい
            /// </summary>
            FUN
        }
    }
}