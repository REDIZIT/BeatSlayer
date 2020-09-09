using System.Collections.Generic;

namespace BeatSlayerServer.Multiplayer.Accounts
{
    public class GroupInfo
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public string Name { get; set; }

        //public virtual ICollection<MapInfo> Maps { get; set; }
    }
    public class GroupData
    {
        public string Author { get; set; }
        public string Name { get; set; }

        public List<AccountMapData> Maps { get; set; }
    }
}
