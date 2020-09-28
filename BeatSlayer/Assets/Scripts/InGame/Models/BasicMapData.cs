using Newtonsoft.Json;
using ProjectManagement;

namespace InGame.Models
{
    public class BasicMapData : IMapData
    {
        public string Author { get; set; }
        public string Name { get; set; }
        public string MapperNick { get; set; }
        public bool IsApproved { get; set; }
        public GroupType MapType { get; set; } = GroupType.Author;

        [JsonIgnore] public string Trackname { get { return Author + "-" + Name; } }

       
        public BasicMapData() { }

        public BasicMapData(MapsData mapsDataParent)
        {
            Author = mapsDataParent.Author;
            Name = mapsDataParent.Name;
            MapType = mapsDataParent.MapType;
        }
    }

    public interface IMapData
    {
        string Author { get; set; }
        string Name { get; set; }
        string MapperNick { get; set; }
        bool IsApproved { get; set; }
        GroupType MapType { get; set; }

        [JsonIgnore] string Trackname { get; }
    }
    public interface IOwnMapData : IMapData
    {
        string Filepath { get; set; }
    }
}
