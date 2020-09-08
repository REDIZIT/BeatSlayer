using Newtonsoft.Json;
using ProjectManagement;

namespace InGame.Models
{
    public class BasicMapData
    {
        public string Author { get; set; }
        public string Name { get; set; }
        public string Nick { get; set; }
        public bool IsApproved { get; set; }
        public GroupType MapType { get; set; } = GroupType.Author;

        [JsonIgnore] public string Trackname { get { return Author + "-" + Name; } }


        public BasicMapData() { }
        public BasicMapData(ProjectMapInfo map)
        {
            Author = map.Author;
            Name = map.Name;
            Nick = map.nick;
            IsApproved = map.approved;
        }
    }

    public class OwnMapData : BasicMapData
    {
        public string Filepath { get; set; }

        public OwnMapData()
        {
            MapType = GroupType.Own;
        }
    }
}
