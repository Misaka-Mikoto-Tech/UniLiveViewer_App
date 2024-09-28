using UniLiveViewer.ValueObject;

namespace UniLiveViewer.Menu
{
    public class RegisterData
    {
        public ActorId Id { get; }

        public string FileName { get; }

        public string FullPath { get; }

        public bool LoadVrmAsMode10 { get; }

        public RegisterData(ActorId id, string fileName)
        {
            Id = id;
            FileName = fileName;
            FullPath = "";
            LoadVrmAsMode10 = false;
        }

        public RegisterData(ActorId id, string fileName, bool loadVrmAsMode10)
        {
            Id = id;
            FileName = fileName;
            FullPath = $"{PathsInfo.GetFullPath(FolderType.CHARA)}/{fileName}"; ;
            LoadVrmAsMode10 = loadVrmAsMode10;
        }
    }
}