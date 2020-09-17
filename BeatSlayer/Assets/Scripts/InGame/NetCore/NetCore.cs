using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Models.Database;
using BeatSlayerServer.Multiplayer.Accounts;
using InGame.Game.Tutorial;
using InGame.Leaderboard;
using InGame.Multiplayer;
using LobbyDTO = InGame.Multiplayer.Lobby.Lobby;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Notifications;
using Ranking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using InGame.Game.Scoring.Mods;
using InGame.Multiplayer.Lobby.Chat.Models;
using ProjectManagement;
using InGame.Models;

[assembly: Preserve]
namespace GameNet
{
    /// <summary>
    /// This class is controlling SignalR connection and messaging between server and client
    /// </summary>
    [Preserve]
    public static class NetCore
    {
        static HubConnection conn;
        public static HubConnectionState State => conn.State;
        public static Subscriptions Subs { get; private set; }
        public static string Url_Server => Config.ServerUrl;
        public static string Url_Hub
        {
            get { return Url_Server + "/GameHub"; }
        }
        public enum ConnectionType { Production, Local, Development }

        public static ConnectionType ConnType => Config.ServerType;
        public static NetConfigurationModel Config { get; set; } = new NetConfigurationModel();



        public static Action OnConnect, OnDisconnect, OnReconnect, OnFullReady;
        public static Action OnLogIn, OnLogOut;

        
        // There are delegates of NetCore config methods (Instead of Configure(Action config))
        // Subs here your config code. This is invoked when wrapper invoke NetCore.Configure
        public static Action Configurators;

        public static bool TryReconnect { get; set; }
        public static int ReconnectAttempt { get; private set; }


        
        
        
        

        static NetCore()
        {
            SceneManager.activeSceneChanged += (arg0, scene) =>
            {
                //NetCore.Configurators = null;
                //if(Time.realtimeSinceStartup > 5) OnSceneLoad(); 
            };
            Application.quitting += () =>
            {
                //conn?.Stop();
                conn?.StopAsync();
                TryReconnect = false;
            };
            
            TryReconnect = true;
        }

        
        // Use after completing Configurators subs
        // This method apply configuration for current scene
        // (External usage)
        public static void Configure()
        {
            Subs = new Subscriptions();
            OnFullReady = null;
            OnConnect = null;
            OnDisconnect = null;
            OnReconnect = null;

            OnSceneLoad();
            
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Configurators.Invoke();
                
                OnFullReady?.Invoke();
            });
        }
        public static void Configure(Action config)
        {
            Subs = new Subscriptions();
            OnFullReady = null;
            OnConnect = null;
            OnDisconnect = null;
            OnReconnect = null;

            OnSceneLoad();

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                config();

                OnFullReady?.Invoke();
            });
        }

        
        
        #region Internal usage
        
        
        
        // This method is invoked when Scene changed
        // Connect if not already
        // (Internal usage)
        static void OnSceneLoad()
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (conn == null)
                {
                    CreateConnection();
                }
            });
        }

        // This method is invoked on first load
        // (Internal usage)
        static void CreateConnection()
        {
            /*ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls11;*/

            BuildConnection();
            SubcribeOnServerCalls();
            SubcribeOnServerCallsManually();
            //SubcribeOnClientInvokes();
            Connect();
        }
        
        
        
        
        

        // This async method connects to server via existing HubConnection
        // (Internal usage)
        static async void Connect()
        {
            try
            {
                await conn.StartAsync();
                if (conn.State == HubConnectionState.Connected)
                {
                    Log("[ Connected ]");
                    OnConnect?.Invoke();
                    ReconnectAttempt = 0;
                }
                else
                {
                    Log("[ Reconnecting ]");
                    if (Application.isPlaying)
                    {
                        OnReconnect?.Invoke();
                        Reconnect();   
                    }
                }
            }
            catch (Exception err)
            {
                Debug.Log("Connection failed: " + err);
                Log("[ Reconnecting ]");
                if (Application.isPlaying)
                {
                    OnReconnect?.Invoke();
                    Reconnect();
                }
            }
            
        }

        /// <summary>
        /// Debug.Log if has internet access
        /// </summary>
        static void Log(string msg)
        {
            if (Application.internetReachability == NetworkReachability.NotReachable) return;

            Debug.Log(msg);
        }

        static async void Reconnect(bool force = false)
        {
            if (!TryReconnect) return;
            
            if(!force) await Task.Delay(2000);

            ReconnectAttempt++;
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Connect();
            });
        }

        // This method creates new connection
        // (Internal usage)
        static void BuildConnection()
        {
            Debug.Log("> Create HubConnection with " + Url_Hub);

            conn = new HubConnectionBuilder()
                 .WithUrl(Url_Hub, (o) =>
                 {
                     o.SkipNegotiation = false;
                     o.Transports = HttpTransportType.WebSockets;
                 })
                 .ConfigureLogging(logging =>
                 {
                     /*logging.ClearProviders();
                     logging.SetMinimumLevel(LogLevel.Information);
                     logging.AddProvider(new UnityLogger());*/
                 })
                 .Build();
            conn.ServerTimeout = TimeSpan.FromMinutes(10);

            conn.Closed += (err =>
            {
                Debug.Log("Conn closed due to " + err.Message);
                OnDisconnect?.Invoke();
                Reconnect(true);
                return null;
            });
        }

        // This method invoke conn.On<T1,T2,TN> for EACH FIELD in NetCore.Subs
        // This is internal method used only after building connection
        // (Internal usage)
        static void SubcribeOnServerCalls()
        {
            //return;
            Subs = new Subscriptions();
           

            var fields = typeof(Subscriptions).GetFields();
            foreach (var field in fields)
            {
                Type t = field.FieldType;
                //Debug.Log(" << " + field.Name);
                conn.On(field.Name, t.GenericTypeArguments, (objects =>
                {
                    //Debug.Log("[CONNECTION ON] << " + field.Name);
                    FieldInfo info = typeof(Subscriptions).GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);

                    object yourfield = info.GetValue(Subs);

                    MethodInfo method = yourfield.GetType().GetMethod("Invoke");

                    //method.Invoke(yourfield, objects);
                    UnityMainThreadDispatcher.Instance().Enqueue(() => method.Invoke(yourfield, objects));
                    
                    return Task.Delay(0);
                }));
            }
        }
        static void SubcribeOnServerCallsManually()
        {
            return;
            Debug.Log(" > Sub on server calls (manually)");
            //HubConnectionBindExtensions.BindOnInterface<ISubs>(conn, Subs.OnTest, Subs.OnTest)
            conn.On("OnTest2", () =>
            {
                Debug.Log(" << Manually on test2");
                Subs.OnTest();
            });
            conn.On<int>("OnTestPar", (int i) =>
            {
                Debug.Log(" << Manually on OnTestPar with " + i);
                Subs.OnTest();
            });

            conn.On<OperationMessage>(nameof(Subs.Accounts_OnLogIn), (op) =>
            {
                Debug.Log(" << Manualy on log in\n" + JsonConvert.SerializeObject(op, Formatting.Indented));
                Subs.Accounts_OnLogIn(op);
            });
            conn.On<int>("OnOnlineChange", (i) =>
            {
                Debug.Log(" << Manually on online change\n" + i);
                Subs.OnOnlineChange(i);
            });
            conn.On<string>(nameof(Subs.OnJoinGroup), (str) =>
            {
                Debug.Log(" << Manually on join group\n" + str);
                UnityMainThreadDispatcher.Instance().Enqueue(() => Subs.OnJoinGroup(str));
            });
            conn.On<List<ChatGroupData>>(nameof(Subs.OnGetGroups), (ls) =>
            {
                Debug.Log(" << Manually on get groups\n" + ls.Count);
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Subs.OnGetGroups(ls);
                });
                
            });
            conn.On<string>(nameof(Subs.OnSendChatMessage), (str) =>
            {
                Debug.Log(" << Manually OnSendChatMessage\n" + str);
                Subs.OnSendChatMessage(str);
            });
            conn.On(nameof(Subs.Friends_OnGetFriends), (List<AccountData> ls) =>
            {
                Debug.Log(" << Manually Friends_OnGetFriends\n" + ls.Count);
                UnityMainThreadDispatcher.Instance().Enqueue(() => Subs.Friends_OnGetFriends(ls));
            });
            conn.On(nameof(Subs.Accounts_OnGetAvatar), (byte[] bytes) =>
            {
                Debug.Log(" << Manually Accounts_OnGetAvatar\n");
                UnityMainThreadDispatcher.Instance().Enqueue(() => Subs.Accounts_OnGetAvatar(bytes));
            });
        }


        #endregion






        // This class contains implementation of server-side methods
        // (Piece of 90 lines of code ._.) 
        // Блять, вот как убрать этот пиздец?
        // (External usage)
        [Preserve]
        public static class ServerActions
        {
            public static void SendChatMessage(string nick, string msg, AccountRole role, string group)
               =>  conn.InvokeAsync("Chat_SendMessage", nick, msg, role, group);

            public static void UpdateInGameTime()
            {
                if (Payload.Account == null) return;
                if (Payload.PrevInGameTimeUpdate == 0) Payload.PrevInGameTimeUpdate = Time.realtimeSinceStartup;
                int seconds = Mathf.RoundToInt(Time.realtimeSinceStartup - Payload.PrevInGameTimeUpdate);
                if (seconds < 30) return;
                Payload.PrevInGameTimeUpdate = Time.realtimeSinceStartup;
                conn.InvokeAsync("Accounts_UpdateInGameTime", Payload.Account.Nick, seconds);
            }

            public static class Account
            {
                public static void LogIn(string nick, string password)
                {
                    conn.InvokeAsync("Accounts_LogIn", nick, password);
                    UpdateInGameTime();
                }
                public static void SignUp(string nick, string password, string country, string email) => conn.InvokeAsync("Accounts_SignUp", nick, password, country, email);

                public static void Search(string nick) => conn.InvokeAsync("Accounts_Search", nick);
                public static void View(string nick) => conn.InvokeAsync("Accounts_View", nick);
                public static async Task<AccountData> GetAccountByNick(string nick) => await conn.InvokeAsync<AccountData>("GetAccountByNick", nick);



                public static void GetAvatar(string nick) => conn.InvokeAsync("Accounts_GetAvatar", nick);

                public static void ChangePassword(string nick, string oldpass, string newpass) => conn.InvokeAsync("Accounts_ChangePassword", nick, oldpass, newpass);
                public static void Restore(string nick, string password) => conn.InvokeAsync("Accounts_Restore", nick, password);
                public static void ConfirmRestore(string code) => conn.InvokeAsync("Accounts_ConfirmRestore", code);


                public static void ChangeEmptyEmail(string nick, string email) => conn.InvokeAsync("Accounts_ChangeEmptyEmail", nick, email);

                public static void ChangeEmail(string nick, string email) => conn.InvokeAsync("Accounts_ChangeEmail", nick, email);

                public static void SendChangeEmailCode(string nick, string email) => conn.InvokeAsync("Accounts_SendChangeEmailCode", nick, email);

                public static async Task<ReplaySendData> SendReplay(ReplayData dto)
                {
                    return await conn.InvokeAsync<ReplaySendData>("SendReplay", dto);
                }

                public static async Task<ReplayData> GetBestReplay(string nick, string trackname, string creatornick)
                {
                    return await conn.InvokeAsync<ReplayData>("GetBestReplay", nick, trackname, creatornick);
                }

                public static async Task<List<LeaderboardItem>> GetMapLeaderboard(string trackname, string nick) =>
                    await conn.InvokeAsync<List<LeaderboardItem>>("GetMapLeaderboard", trackname, nick);

                public static async Task<List<LeaderboardItem>> GetGlobalLeaderboard() =>
                    await conn.InvokeAsync<List<LeaderboardItem>>("GetGlobalLeaderboard");

                public static void GetBestReplays(string nick, int count) => conn.InvokeAsync("Accounts_GetBestReplays", nick, count);

                public static async Task<bool> IsPassed(string nick, string author, string name) =>
                    await conn.InvokeAsync<bool>("IsPassed", nick, author, name);
            }

            public static class Friends
            {
                public static void GetFriends(string nick) => conn.InvokeAsync("Friends_GetFriends", nick);
                
                public static void InviteFriend(string targetNick, string requesterNick) => conn.InvokeAsync("Friends_InviteFriend",targetNick, requesterNick);
                
                public static void RemoveFriend(string fromNick, string nick) => conn.InvokeAsync("Friends_RemoveFriend",fromNick, nick);
            }

            public static class Notifications
            {
                public static void Accept(string nick, int id) => conn.InvokeAsync("Friends_AcceptInvite", nick, id);
                
                public static void Reject(string nick, int id) => conn.InvokeAsync("Friends_RejectInvite", nick, id);
                public static void Ok(string nick, int id) => conn.InvokeAsync("Notification_Ok", nick, id);
            }
            
            
            public static class Shop
            {
                public static void SendCoins(string nick, int coins) => conn.InvokeAsync("Shop_SendCoins", nick, coins);
                public static void SyncCoins(string nick, int coins) => conn.InvokeAsync("Shop_SyncCoins", nick, coins);

                public static async Task<List<PurchaseModel>> UpgradePurchases(string nick, bool[] boughtSabers, bool[] boughtTails, bool[] boughtMaps) =>
                    await conn.InvokeAsync<List<PurchaseModel>>("Shop_UpgradePurchases", nick, boughtSabers, boughtTails, boughtMaps);
                public static async Task<bool> IsPurchaseBought(string nick, int coins) => await conn.InvokeAsync<bool>("Shop_IsPurchaseBought", nick, coins);
                public static async Task<PurchaseModel> TryBuyPurchase(string nick, int purchaseId) => await conn.InvokeAsync<PurchaseModel>("Shop_TryBuy", nick, purchaseId);
            }
            public static class Chat
            {
                public static void GetGroups() => conn.InvokeAsync("Chat_GetGroups");
                public static void JoinGroup(string nick, string group) => conn.InvokeAsync("Chat_JoinGroup", nick, group);
                public static void LeaveGroup(string nick, string group) => conn.InvokeAsync("Chat_LeaveGroup", nick, group);
            }

            public static class Tutorial
            {
                public static void TutorialPlayed(TutorialResult result) => conn.InvokeAsync("Tutorial_Played", JsonConvert.SerializeObject(result));

                public async static Task<KeyValuePair<string, string>> GetTutorialMap() => await conn.InvokeAsync<KeyValuePair<string, string>>("Tutorial_GetTutorialMap");
                public async static Task<Dictionary<string, string>> GetEasyMaps() => await conn.InvokeAsync<Dictionary<string, string>>("Tutorial_EasyMaps");
                public async static Task<Dictionary<string, string>> GetHardMaps() => await conn.InvokeAsync<Dictionary<string, string>>("Tutorial_HardMaps");
            }


            public static class Lobby
            {
                public static async Task<List<LobbyDTO>> GetLobbies() => await conn.InvokeAsync<List<LobbyDTO>>("GetLobbies");
                public static async Task<LobbyDTO> GetLobby(int lobbyId) => await conn.InvokeAsync<LobbyDTO>("GetLobby", lobbyId);
                public static async Task<LobbyDTO> Create(string creatorNick) => await conn.InvokeAsync<LobbyDTO>("CreateLobby", creatorNick);
                public static async Task<LobbyDTO> Join(string nick, int lobbyId) => await conn.InvokeAsync<LobbyDTO>("JoinLobby", lobbyId, nick);
                public static async Task Leave(string nick, int lobbyId) => await conn.InvokeAsync("LeaveLobby", lobbyId, nick);
                public static async Task Rename(int lobbyId, string lobbyName) => await conn.InvokeAsync("RenameLobby", lobbyId, lobbyName);
                public static async Task ChangePassword(int lobbyId, string password) => await conn.InvokeAsync("ChangeLobbyPassword", lobbyId, password);


                public static void ChangeHost(int lobbyId, string newHostNick) => conn.InvokeAsync("ChangeLobbyHost", lobbyId, newHostNick);
                public static void Kick(int lobbyId, string nick) => conn.InvokeAsync("KickPlayerFromLobby", lobbyId, nick);


                public static void HostStartChangingMap(int lobbyId) => conn.InvokeAsync("HostStartChangingMap", lobbyId);
                public static void HostCancelChangingMap(int lobbyId) => conn.InvokeAsync("HostCancelChangingMap", lobbyId);
                public static async Task ChangeMap(int lobbyId, BasicMapData map, DifficultyInfo difficulty) => await conn.InvokeAsync("ChangeLobbyMap", lobbyId, map, difficulty);
                public static void ChangeMods(int lobbyId, string nick, ModEnum mods) => conn.InvokeAsync("ChangeLobbyMods", lobbyId, nick, mods);
                public static async Task ChangeReadyState(int lobbyId, string nick, LobbyPlayer.ReadyState state)
                    => await conn.InvokeAsync("ChangeReadyState", lobbyId, nick, state);


                public static void OnStartDownloading(int lobbyId, string nick) => conn.InvokeAsync("OnLobbyStartDownloading", lobbyId, nick);
                public static void OnDownloadProgress(int lobbyId, string nick, int percent) => conn.InvokeAsync("OnLobbyDownloadProgress", lobbyId, nick, percent);
                public static void OnDownloaded(int lobbyId, string nick) => conn.InvokeAsync("OnLobbyDownloaded", lobbyId, nick);



                public static void SendChatMessage(int lobbyId, LobbyPlayerChatMessage message) => conn.InvokeAsync("SendLobbyPlayerMessage", lobbyId, message);
                public static void StartTyping(int lobbyId, string nick) => conn.InvokeAsync("OnLobbyPlayerStartTyping", lobbyId, nick);
                public static void StopTyping(int lobbyId, string nick) => conn.InvokeAsync("OnLobbyPlayerStopTyping", lobbyId, nick);
            }



            public static class Multiplayer
            {
                public static void StartGame(int lobbyId) => conn.InvokeAsync("OnMultiplayerStartGame", lobbyId);
                public static void OnLoaded(int lobbyId, string nick) => conn.InvokeAsync("OnMultiplayerPlayerLoaded", lobbyId, nick);
                public static async Task<bool> AreAllLoaded(int lobbyId) => await conn.InvokeAsync<bool>("OnMultiplayerAreAllPlayersLoaded", lobbyId);
                public static void ScoreUpdate(int lobbyId, string nick, float score, int combo) => conn.InvokeAsync("OnMultiplayerScoreUpdate", lobbyId, nick, score, combo);
                public static void AliveChanged(int lobbyId, string nick, bool isAlive) => conn.InvokeAsync("OnMultiplayerPlayerAliveChanged", lobbyId, nick, isAlive);
                public static void OnFinished(int lobbyId, string nick, ReplayData replay) => conn.InvokeAsync("OnMultiplayerPlayerFinished", lobbyId, nick, replay);
                public static void LeftGame(int lobbyId, string nick) => conn.InvokeAsync("OnMultiplayerPlayerLeft", lobbyId, nick);
            }
        }




        // This class contains all methods which can invoke server on client-side
        // How server determine what should invoke? -Field name :D
        // Server: Client.Caller.SendAsync("MethodName", args)
        // There must be Action with name same as MethodName, else server won't be able to find it.

        // Some rules to use SignalR (пиздец, а нормально можно было сделать?! Вот чтоб без ебли в жопу!)
        // 1) If you want to send class USE {GET;SET} !!!!
        // 2) Don't use ctors at all, data won't be sent
        [Preserve]
        public class Subscriptions// : ISubs
        {
            public Action OnTest;
            public Action<OperationMessage> Accounts_OnLogIn;
            public Action<OperationMessage> Accounts_OnSignUp;
            public Action<List<AccountData>> Accounts_OnSearch;
            public Action<AccountData> Accounts_OnView;

            public Action<OperationMessage> Accounts_OnChangeEmail;
            public Action<OperationMessage> Accounts_OnChangePassword;
            public Action<bool> Accounts_OnConfirmRestore;



            public Action<int> OnOnlineChange;
            
            public Action<string> OnJoinGroup;
            public Action<List<ChatGroupData>> OnGetGroups;
            public Action<string> OnSendChatMessage;

            public Action<byte[]> Accounts_OnGetAvatar;
            public Action<string, string, string> Accounts_ChangePassword;
            
            public Action<List<ReplayData>> Accounts_OnGetBestReplays;

            public Action<List<AccountData>> Friends_OnGetFriends;


            public Action<NotificationInfo> Notification_OnSend;


            #region Lobby

            public Action<LobbyPlayer> OnLobbyPlayerJoin;
            public Action<LobbyPlayer> OnLobbyPlayerLeave;
            public Action<LobbyPlayer> OnLobbyHostChange;
            public Action<LobbyPlayer> OnLobbyPlayerKick;
            public Action OnHostStartChangingMap;
            public Action OnHostCancelChangingMap;
            public Action<BasicMapData, DifficultyInfo> OnLobbyMapChange;

            public Action<string, LobbyPlayer.ReadyState> OnRemotePlayerReadyStateChange;
            public Action<string, ModEnum> OnRemotePlayerModsChange;
            public Action<string> OnRemotePlayerStartDownloading;
            public Action<string, int> OnRemotePlayerDownloadProgress;
            public Action<string> OnRemotePlayerDownloaded;
            public Action<string> OnLobbyRename;
            public Action<string> OnLobbyChangePassword;
            public Action<bool> OnLobbyPlayStatusChanged;


            public Action<LobbyPlayerChatMessage> OnLobbyPlayerMessage;
            public Action<LobbySystemChatMessage> OnLobbySystemMessage;
            public Action<string> OnLobbyPlayerStartTyping;
            public Action<string> OnLobbyPlayerStopTyping;

            #endregion

            #region Multiplayer

            public Action OnMultiplayerGameStart;
            public Action OnMultiplayerPlayersLoaded;
            public Action<string, float, int> OnMultiplayerScoreUpdate;
            public Action<string, ReplayData> OnMultiplayerPlayerFinished;
            public Action<string, bool> OnMultiplayerPlayerAliveChanged;
            public Action<string> OnMultiplayerPlayerLeft;

            #endregion
        }
    }
}