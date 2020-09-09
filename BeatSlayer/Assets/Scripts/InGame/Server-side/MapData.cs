using BeatSlayerServer.Dtos.Mapping;
using InGame.Models;
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
        public MapData(ProjectManagement.ProjectMapInfo map)
        {
            Group = new GroupData()
            {
                Author = map.Author,
                Name = map.Name
            };
            Nick = map.nick;
        }
    }
}
