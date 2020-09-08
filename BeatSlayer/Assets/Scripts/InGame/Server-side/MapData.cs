using BeatSlayerServer.Dtos.Mapping;
using System.Collections.Generic;

namespace BeatSlayerServer.Multiplayer.Accounts
{
    public class MapData
    {
        public GroupData Group { get; set; }
        public string Trackname { get { return Group.Author + "-" + Group.Name; } }

        public string Nick { get; set; }

        public List<ReplayData> Replays { get; set; }

        public MapData() { }
        public MapData(ProjectManagement.MapInfo map)
        {
            Group = new GroupData()
            {
                Author = map.author,
                Name = map.name
            };
            Nick = map.nick;
        }
    }
}
