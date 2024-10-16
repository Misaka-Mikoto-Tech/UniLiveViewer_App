namespace UniLiveViewer.Timeline
{
    public static class TimelineConstants
    {
        public static readonly int PortalIndex = 0;

        /// <summary>
        /// トラックのユニークな識別名
        /// (残念ながらIDなど便利なものはなくIndexも捜査が面倒)
        /// </summary>
        public static readonly string[] TrackNames = new string[]
        {
            "Animator Track_Portal",
            "Animator Track1",
            "Animator Track2",
            "Animator Track3",
            "Animator Track4",
            "Animator Track5",
        };

        public static readonly string NoCustomDanceMessage = "[ None ]";
        public static readonly string NoCustomFacialSyncMessage = "[ None ]";
        public static readonly string NoCustomBGMMessage = "[ None ]";
    }
}