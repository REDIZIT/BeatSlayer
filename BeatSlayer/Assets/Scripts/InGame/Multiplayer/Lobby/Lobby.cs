using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Multiplayer.Accounts;
using System.Collections.Generic;

namespace InGame.Multiplayer.Lobby
{
    public class Lobby
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public MapData SelectedMap { get; set; }
        public DifficultyData SelectedDifficulty { get; set; }

        public List<LobbyPlayer> Players { get; set; }
    }
}
