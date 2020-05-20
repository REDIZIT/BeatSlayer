namespace BeatSlayerServer.Multiplayer.Accounts
{
    public class MapInfo
    {
        public int Id { get; set; }
        public virtual GroupInfo Group { get; set; }
        public string Nick { get; set; }

        //public virtual ICollection<ReplayInfo> Replays { get; set; } = new List<ReplayInfo>();
    }
}
