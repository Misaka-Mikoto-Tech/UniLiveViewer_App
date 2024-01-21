namespace UniLiveViewer.Timeline
{
    public class VRMLoadData
    {
        public string FileName => _fileName;
        string _fileName;

        public string FullPath => $"{_folderPath}/{_fileName}";
        string _folderPath;

        public VRMLoadData(string fileName)
        {
            _fileName = fileName;
            _folderPath = PathsInfo.GetFullPath(FolderType.CHARA);
        }
    }
}