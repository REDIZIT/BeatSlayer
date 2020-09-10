using Newtonsoft.Json;
using ProjectManagement;

namespace InGame.Models
{
    public class BasicMapData
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
}
