namespace UniLiveViewer.Menu
{
    public struct VRMMenuShowMessage
    {
        /// <summary>
        /// -1をClose扱い
        /// NOTE: Messagepipeは過剰
        /// </summary>
        public int PageIndex => _pageIndex;
        int _pageIndex;
        
        /// <param name="pageIndex">-1をClose扱い</param>
        public VRMMenuShowMessage(int pageIndex)
        {
            _pageIndex = pageIndex;
        }
    }
}