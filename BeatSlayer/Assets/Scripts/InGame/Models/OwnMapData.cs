using ProjectManagement;

namespace InGame.Models
{
    public class OwnMapData : FullMapData
    {
        public string Filepath { get; set; }

        public OwnMapData()
        {
            MapType = GroupType.Own;
        }

        public OwnMapData(OwnMapsData mapsDataParent) : base(mapsDataParent)
        {
            Author = mapsDataParent.Author;
            Name = mapsDataParent.Name;
            Filepath = mapsDataParent.Filepath;
            MapType = GroupType.Own;
        }
    }
}
