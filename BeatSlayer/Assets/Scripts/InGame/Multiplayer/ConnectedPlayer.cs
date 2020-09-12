using InGame.Game.Scoring.Mods;

namespace InGame.Multiplayer
{
    public class ConnectedPlayer
    {
        //public int Id { get; set; }
        public string Nick { get; set; }
    }

    public class LobbyPlayer
    {
        public ConnectedPlayer Player { get; set; }
        public int SlotIndex { get; set; }
        public bool IsHost { get; set; }
        public ReadyState State { get; set; }
        public ModEnum Mods { get; set; }

        public enum ReadyState
        {
            NotReady, Ready, Downloading, Playing
        }
    }
}
