using BeatSlayerServer.Dtos.Mapping;
using System.Collections.Generic;

namespace BeatSlayerServer.Multiplayer.Accounts
{
    public class MapInfo
    {
        public int Id { get; set; }
        public virtual GroupInfo Group { get; set; }
        public string Nick { get; set; }

        //public virtual ICollection<ReplayInfo> Replays { get; set; } = new List<ReplayInfo>();
    }
    
    public class MapData
    {
        public GroupData Group { get; set; }
        public string Trackname { get { return Group.Author + "-" + Group.Name; } }

        public string Nick { get; set; }

        public List<ReplayData> Replays { get; set; }
    }
}
