using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using InGame.Game.Scoring.Mods;
using ProjectManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InGame.Multiplayer.Lobby
{
    public static class LobbyManager
    {
        public static Lobby lobby;
        public static LobbyPlayer lobbyPlayer;
        public static List<Lobby> lobbies;

        public static bool isPickingMap;

        public const int MAX_LOBBY_PLAYERS = 10;



        #region Lobby View/Create/Join/Leave

        public static async Task<List<Lobby>> GetLobbies()
        {
            lobbies = await NetCore.ServerActions.Lobby.GetLobbies();
            return lobbies;
        }
        public static async Task CreateAndJoinLobby()
        {
            Lobby createdLobby = await NetCore.ServerActions.Lobby.Create(Payload.Account.Nick);

            lobby = await NetCore.ServerActions.Lobby.Join(Payload.Account.Nick, createdLobby.Id);
            lobbyPlayer = lobby.Players.First(c => c.Player.Nick == Payload.Account.Nick);
        }
        public static async Task JoinLobby(Lobby lobbyToJoin)
        {
            lobby = await NetCore.ServerActions.Lobby.Join(Payload.Account.Nick, lobbyToJoin.Id);
            lobbyPlayer = lobby.Players.First(c => c.Player.Nick == Payload.Account.Nick);
        }
        public static async Task LeaveLobby()
        {
            await NetCore.ServerActions.Lobby.Leave(Payload.Account.Nick, lobby.Id);
            lobby = null;
            lobbyPlayer = null;
        }

        #endregion



        #region Downloading

        public static void PingStartDownloading()
        {
            lobbyPlayer.State = LobbyPlayer.ReadyState.Downloading;
            NetCore.ServerActions.Lobby.OnStartDownloading(lobby.Id, Payload.Account.Nick);
        }
        public static void PingDownloadProgress(int percent)
        {
            NetCore.ServerActions.Lobby.OnDownloadProgress(lobby.Id, Payload.Account.Nick, percent);
        }
        public static void PingDownloadCompleted()
        {
            lobbyPlayer.State = LobbyPlayer.ReadyState.NotReady;
            NetCore.ServerActions.Lobby.OnDownloaded(lobby.Id, Payload.Account.Nick);
        }

        #endregion


        #region Host

        public static void GiveHostRights(string targetNick)
        {
            NetCore.ServerActions.Lobby.ChangeHost(lobby.Id, targetNick);
        }
        public static void RemoteHostChanged(string newHostNick)
        {
            lobby.IsHostChangingMap = false;
            foreach (LobbyPlayer lobbyPlayer in lobby.Players)
            {
                lobbyPlayer.IsHost = lobbyPlayer.Player.Nick == newHostNick;
            }
        }

        #endregion

        #region Map

        public static async Task ChangeMap(ProjectManagement.MapInfo map, DifficultyInfo difficulty)
        {
            lobby.SelectedMap = new MapData(map);
            lobby.SelectedDifficulty = new DifficultyData(difficulty);

            await NetCore.ServerActions.Lobby.ChangeMap(lobby.Id, lobby.SelectedMap, lobby.SelectedDifficulty);
        }
        public static void StartMapPicking()
        {
            NetCore.ServerActions.Lobby.HostStartChangingMap(lobby.Id);
        }
        public static void CancelMapPicking()
        {
            NetCore.ServerActions.Lobby.HostCancelChangingMap(lobby.Id);
        }

        #endregion

        #region Kick

        public static void Kick(string nick)
        {
            NetCore.ServerActions.Lobby.Kick(lobby.Id, nick);
        }
        public static void RemoteKick(string nick)
        {
            lobby.Players.RemoveAll(c => c.Player.Nick == nick);
        }
        public static void RemoteKickMe()
        {
            lobby = null;
            lobbyPlayer = null;
        }

        #endregion




        public static void ChangeMods(ModEnum mods)
        {
            NetCore.ServerActions.Lobby.ChangeMods(lobby.Id, lobbyPlayer.Player.Nick, mods);
        }

        public static void ChangeReadyState(LobbyPlayer.ReadyState state)
        {
            lobbyPlayer.State = state;
            Task.Run(async () =>
                await NetCore.ServerActions.Lobby.ChangeReadyState(lobby.Id, Payload.Account.Nick, state));
        }
    }
}
