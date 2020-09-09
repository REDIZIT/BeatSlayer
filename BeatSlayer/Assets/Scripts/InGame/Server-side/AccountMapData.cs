using BeatSlayerServer.Dtos.Mapping;
using System.Collections.Generic;

namespace BeatSlayerServer.Multiplayer.Accounts
{
    public class AccountMapData
    {
        public GroupData Group { get; set; }
        public string Trackname { get { return Group.Author + "-" + Group.Name; } }

        public string Nick { get; set; }

        public List<ReplayData> Replays { get; set; }

        public AccountMapData() { }
    }
}
