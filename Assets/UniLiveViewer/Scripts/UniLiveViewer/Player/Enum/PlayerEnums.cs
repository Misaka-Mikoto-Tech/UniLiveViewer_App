
namespace UniLiveViewer.Player
{
    public enum PlayerHandType
    {
        LHand = 0,
        RHand = 1
    }

    public enum PlayerHandState
    {
        /// <summary>
        /// 何もしていない、何もつかんでいない
        /// </summary>
        DEFAULT,
        /// <summary>
        /// キャラを掴んでいる
        /// </summary>
        GRABBED_CHARA,
        /// <summary>
        /// アイテムを掴んでいる
        /// </summary>
        GRABBED_ITEM,
        /// <summary>
        /// キャラ以外を掴んでいる
        /// </summary>
        GRABBED_OTHER,
        /// <summary>
        /// 召喚陣ON
        /// </summary>
        SUMMONCIRCLE,
        /// <summary>
        /// キャラを掴んでいるかつ召喚陣ON
        /// </summary>
        CHARA_ONCIRCLE
    }
}