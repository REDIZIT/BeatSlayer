using InGame.Models;
using ProjectManagement;
using System;
using System.Collections.Generic;

namespace InGame.Multiplayer.Lobby
{
    public class Lobby
    {
        public string Name { get; set; }
        public int Id { get; set; }

        public BasicMapData SelectedMap { get; set; }
        public DifficultyInfo SelectedDifficulty { get; set; }
        //public int MapDuration { get; set; }
        //public float CurrentSecond { get; set; }


        public bool IsHostChangingMap { get; set; }
        public bool IsPlaying { get; set; }

        public List<LobbyPlayer> Players { get; set; }
    }
}
