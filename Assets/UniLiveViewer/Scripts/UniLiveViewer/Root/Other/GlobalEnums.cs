
namespace UniLiveViewer
{
    public enum FolderType
    {
        Actor,
        Motion,
        BGM,
        Settings
    }

    public static class FolderTypeExtension
    {
        public static string AsString(this FolderType folderType)
        {
            return folderType switch
            {
                FolderType.Actor => "Actor",
                FolderType.Motion => "Motion",
                FolderType.BGM => "BGM",
                FolderType.Settings => "Setting",
                _ => ""
            };
        }
    }
}
