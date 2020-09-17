using InGame.Models;
using ProjectManagement;
using System.Collections.Generic;
using System.Linq;

namespace InGame.Multiplayer.Lobby
{
    public class Lobby
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Password { get; set; }
        public bool HasPassword => !string.IsNullOrWhiteSpace(Password);

        public BasicMapData SelectedMap { get; set; }
        public DifficultyInfo SelectedDifficulty { get; set; }
        //public int MapDuration { get; set; }
        //public float CurrentSecond { get; set; }


        public bool IsHostChangingMap { get; set; }
        public bool IsPlaying { get; set; }

        public List<LobbyPlayer> Players { get; set; }

        public void UpdateValues(Lobby lobby)
        {
            Name = lobby.Name;
            Id = lobby.Id;
            SelectedMap = lobby.SelectedMap;
            SelectedDifficulty = lobby.SelectedDifficulty;

            IsHostChangingMap = lobby.IsHostChangingMap;
            IsPlaying = lobby.IsPlaying;

            foreach (LobbyPlayer actualPlayer in lobby.Players)
            {
                var player = Players.FirstOrDefault(p => p.Player.Nick == actualPlayer.Player.Nick);

                // If no player in current lobby but is on server
                if (player == null)
                {
                    Players.Add(actualPlayer);
                    continue;
                }
                // If there is player both in lobby and on server
                else
                {
                    player.UpdateValues(actualPlayer);
                }
            }

            // Remove all players don't exists on server
            Players.RemoveAll(c => lobby.Players.All(p => p.Player.Nick != c.Player.Nick));
        }
    }
}
