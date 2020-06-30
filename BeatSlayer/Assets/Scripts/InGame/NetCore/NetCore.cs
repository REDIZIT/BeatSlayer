using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet.Invokes;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Notifications;
using Ranking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

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
        //public static ConnectionState State => conn.State;
        public static Subscriptions Subs { get; private set; }
        public static Invokes Actions { get; private set; }

        public static string Url_Server
        {
            get
            {
                return ConnType == ConnectionType.Local
                    ? "https://localhost:5011" : ConnType == ConnectionType.Development
                        ? "http://www.bsserver.tk:5010"
                            : "https://bsserver.tk";
            }
        }
        public static string Url_Hub
        {
            get { return Url_Server + "/GameHub"; }
        }
        public enum ConnectionType { Production, Local, Development }

        public static ConnectionType ConnType = ConnectionType.Development;



        public static Action OnConnect, OnDisconnect, OnReconnect, OnFullReady;
        public static Action OnLogIn;

        
        // There are delegates of NetCore config methods (Instead of Configure(Action config))
        // Subs here your config code. This is invoked when wrapper invoke NetCore.Configure
        public static Action Configurators;

        
        
        public static bool TryReconnect { get; set; }
        public static int ReconnectAttempt { get; private set; }


        
        
        
        

        static NetCore()
        {
            SceneManager.activeSceneChanged += (arg0, scene) =>
            {
                Debug.Log("OnSceneChanged");
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
            //OnLogIn = null;
            
            OnSceneLoad();
            
            Debug.Log(" > Configure()");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log(" > Configure() in unity thread");
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

            Debug.Log(" > Configure()");
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
                    Debug.Log("CreateConnection via Dispatcher");
                    CreateConnection();
                }
                else
                {
                     Debug.Log("Dont create connection via Dispatcher (" + conn.State + ")");
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
            Debug.Log("> Connect");
            try
            {
                //await conn.Start();
                await conn.StartAsync();
                if (conn.State == HubConnectionState.Connected)
                {
                    Debug.Log("[ Connected ]");
                    OnConnect?.Invoke();
                    ReconnectAttempt = 0;
                }
                else
                {
                    Debug.Log("[ Reconnecting ]");
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
                Debug.Log("[ Reconnecting ]");
                if (Application.isPlaying)
                {
                    OnReconnect?.Invoke();
                    Reconnect();
                }
            }
            
        }

        static async void Reconnect(bool force = false)
        {
            if (!TryReconnect) return;
            
            if(!force) await Task.Delay(3000);

            ReconnectAttempt++;
            Debug.Log("Try to reconnect. Attempt " + ReconnectAttempt);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Connect();
            });
        }

        // This method creates new connection
        // (Internal usage)
        static void BuildConnection()
        {
            Debug.Log("> Create HubConnection (autoreconnect = true) with " + Url_Hub);
            //conn = new HubConnection(Url_Hub);
            conn = new HubConnectionBuilder()
                .WithUrl(new Uri(Url_Hub), options => {
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                    options.SkipNegotiation = false;
                })
                .WithAutomaticReconnect()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddProvider(new UnityLogger());
                })
                .Build();

            //conn.KeepAliveInterval = new TimeSpan(0, 0, 5);

            conn.Closed += (err =>
            {
                Debug.Log("Conn closed due to " + err.Message);
                OnDisconnect?.Invoke();
                BuildConnection();
                Reconnect(true);
                return null;
            });
        }

        // This method invoke conn.On<T1,T2,TN> for EACH FIELD in NetCore.Subs
        // This is internal method used only after building connection
        // (Internal usage)
        static void SubcribeOnServerCalls()
        {
            Debug.Log("> Sub on server calls");
            Subs = new Subscriptions();
           

            var fields = typeof(Subscriptions).GetFields();
            foreach (var field in fields)
            {
                Type t = field.FieldType;
                Debug.Log(" << " + field.Name);
                conn.On(field.Name, t.GenericTypeArguments, (objects =>
                {
                    Debug.Log("[CONNECTION ON] << " + field.Name);
                    FieldInfo info = typeof(Subscriptions).GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
                    object yourfield = info.GetValue(Subs);
                    MethodInfo method = yourfield.GetType().GetMethod("Invoke");
                    
                    method.Invoke(yourfield, objects);

                    return null;
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
                Subs.OnJoinGroup(str);
            });
            conn.On<List<ChatGroupData>>(nameof(Subs.OnGetGroups), (ls) =>
            {
                Debug.Log(" << Manually on get groups\n" + ls.Count);
                Subs.OnGetGroups(ls);
            });
            conn.On<string>(nameof(Subs.OnSendChatMessage), (str) =>
            {
                Debug.Log(" << Manually OnSendChatMessage\n" + str);
                Subs.OnSendChatMessage(str);
            });
        }

        public class ConnectionMethodsWrapper
        {
            /*private readonly HubConnection _connection;

            public ConnectionMethodsWrapper(HubConnection connection)
                => _connection = connection;

            public Task Test()
                => _connection.InvokeAsync(nameof(IInvokes.Test));

            public IDisposable RegisterOnFoo(Action onTest)
                => _connection.BindOnInterface<ISubs>(x => x.OnTest, onTest);*/

            /*public IDisposable RegisterOnBar(Action<string, BarData> onBar)
                => _connection.BindOnInterface(x => x.OnBar, onBar);*/
        }



        static void SubcribeOnClientInvokes()
        {
            /*Actions = new Invokes();

            
            //MethodInfo mi = Actions.GetType().GetMethod("Invoke");
            //Action del = (Action)Delegate.CreateDelegate(typeof(Action), mi);

            
            //Actions.Test = del;
            //Actions.Test();
            
            
            var fields = typeof(Invokes).GetFields();
            foreach (var field in fields)
            {
                Type t = field.FieldType;
                FieldInfo info = typeof(Invokes).GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);

                var act = info.GetValue(Actions);

                var del = (Action)act;
                
                
                info.SetValue(Actions, act);
                
                conn.On(field.Name, t.GenericTypeArguments, (objects =>
                {
                    FieldInfo info = typeof(Subscriptions).GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
                    object yourfield = info.GetValue(Subs);
                    MethodInfo method = yourfield.GetType().GetMethod("Invoke");
                    
                    method.Invoke(yourfield, objects);

                    return null;
                }));
            }

            Actions.Log("Suka but no blyat");*/
        }
        //static void DoInvoke(MethodInfo)
        static void SuperVoid(string name, params object[] objs)
        {
            Debug.Log("> Invoke " + name);
        }




        #endregion






        // This class contains implementation of server-side methods
        // (Piece of 90 lines of code ._.) 
        // Блять, вот как убрать этот пиздец?
        // (External usage)
        [Preserve]
        public static class ServerActions
        {
            public static void Test() => NetCore.conn.InvokeAsync("Test");
            public static void TestPar(int i) => conn.InvokeAsync("TestPar", i);
            public static void SendChatMessage(string nick, string msg, BeatSlayerServer.Multiplayer.Accounts.AccountRole role, string group)
               =>  NetCore.conn.InvokeAsync("Chat_SendMessage", nick, msg, role, group);

            public static void UpdateInGameTime()
            {
                if (Payload.CurrentAccount == null) return;
                if (Payload.PrevInGameTimeUpdate == 0) Payload.PrevInGameTimeUpdate = Time.realtimeSinceStartup;
                int seconds = Mathf.RoundToInt(Time.realtimeSinceStartup - Payload.PrevInGameTimeUpdate);
                if (seconds < 30) return;
                Payload.PrevInGameTimeUpdate = Time.realtimeSinceStartup;
                NetCore.conn.InvokeAsync("Accounts_UpdateInGameTime", Payload.CurrentAccount.Nick, seconds);
            }

            public static class Account
            {
                public static void LogIn(string nick, string password)
                {
                    NetCore.conn.InvokeAsync("Accounts_LogIn", nick, password);
                    UpdateInGameTime();
                }
                    
                
                
                public static void SignUp(string nick, string password, string country, string email) =>NetCore.conn.InvokeAsync("Accounts_SignUp", nick, password, country, email);

                public static void Search(string nick) =>NetCore.conn.InvokeAsync("Accounts_Search", nick);
                public static void View(string nick) =>NetCore.conn.InvokeAsync("Accounts_View", nick);




                public static void GetAvatar(string nick) => conn.InvokeAsync("Accounts_GetAvatar", nick);

                public static void ChangePassword(string nick, string oldpass, string newpass) => conn.InvokeAsync("Accounts_ChangePassword", nick, oldpass, newpass);
                public static void Restore(string nick, string password) => conn.InvokeAsync("Accounts_Restore", nick, password);
                public static void ConfirmRestore(string code) => conn.InvokeAsync("Accounts_ConfirmRestore", code);


                public static void ChangeEmptyEmail(string nick, string email) => conn.InvokeAsync("Accounts_ChangeEmptyEmail", nick, email);

                public static void ChangeEmail(string nick, string email) => conn.InvokeAsync("Accounts_ChangeEmail", nick, email);

                public static void SendChangeEmailCode(string nick, string email) => conn.InvokeAsync("Accounts_SendChangeEmailCode", nick, email);

                public static void SendReplay(string json) => conn.InvokeAsync("Accounts_SendReplay", json);

                public static void GetBestReplay(string nick, string trackname, string creatornick) => conn.InvokeAsync("Accounts_GetBestReplay", nick, trackname, creatornick);

                public static void GetBestReplays(string nick, int count) => conn.InvokeAsync("Accounts_GetBestReplays", nick, count);
            }

            public static class Friends
            {
                public static void GetFriends(string nick) => conn.InvokeAsync("Friends_GetFriends", nick);
                
                public static void InviteFriend(string addNick, string nick) => conn.InvokeAsync("Friends_InviteFriend",addNick, nick);
                
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
            }
            public static class Chat
            {
                public static void GetGroups() => conn.InvokeAsync("Chat_GetGroups");
                public static void JoinGroup(string nick, string group) => conn.InvokeAsync("Chat_JoinGroup", nick, group);
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
            
            public Action<int> OnOnlineChange;
            
            public Action<string> OnJoinGroup;
            public Action<List<ChatGroupData>> OnGetGroups;
            public Action<string> OnSendChatMessage;

            public Action<byte[]> Accounts_OnGetAvatar;
            public Action<string, string, string> Accounts_ChangePassword;
            
            public Action<ReplaySendData> Accounts_OnSendReplay;
            public Action<List<ReplayData>> Accounts_OnGetBestReplays;
            public Action<ReplayData> Accounts_OnGetBestReplay;

            public Action<List<AccountData>> Friends_OnGetFriends;


            public Action<NotificationInfo> Notification_OnSend;
        }

        public class Invokes
        {
            public Action<string> Log = (str) => SuperVoid("Log", str);

           
            //public void Test(string nick, string password) {}
            //public void Test(string nick, string password) => NetCore.Invoke();
        }

        public abstract class ServerInvokes
        {
            public abstract void Test();
            // conn.Invoke("Test");

            public abstract void Test2(string arg);
            // conn.Invoke("Test2", arg);


            public void Test3() => SuperVoid("123");
        }
    }
}


public class UnityLogger : ILoggerProvider
{
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        return new UnityLog();
    }
    public class UnityLog : Microsoft.Extensions.Logging.ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            var id = Guid.NewGuid();
            Debug.Log($"BeginScope ({id}): {state}");
            return new Scope<TState>(state, id);
        }
        struct Scope<TState> : IDisposable
        {
            public Scope(TState state, Guid id)
            {
                State = state;
                Id = id;
            }

            public TState State { get; }
            public Guid Id { get; }

            public void Dispose() => Debug.Log($"EndScope ({Id}): {State}");
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Information:
                    Debug.Log($"{logLevel}, {eventId}, {state}, {exception}");
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"{logLevel}, {eventId}, {state}, {exception}");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.LogError($"{logLevel}, {eventId}, {state}, {exception}");
                    break;
                case LogLevel.None: break;
            }
        }
    }

    public void Dispose() { }
}