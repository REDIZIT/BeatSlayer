using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Multiplayer.Accounts;
using ProjectManagement;
using System.Collections.Generic;

namespace InGame.Multiplayer.Lobby
{
    public class Lobby
    {
        public string Name { get; set; }
        public int Id { get; set; }

        //public MapData SelectedMap { get; set; }
        //public DifficultyData SelectedDifficulty { get; set; }
        public ProjectManagement.MapInfo SelectedMap { get; set; }
        public DifficultyInfo SelectedDifficulty { get; set; }

        public bool IsHostChangingMap { get; set; }

        public List<LobbyPlayer> Players { get; set; }
    }
}
