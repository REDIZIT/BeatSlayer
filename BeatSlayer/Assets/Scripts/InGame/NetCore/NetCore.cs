using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using BeatSlayerServer.Multiplayer.Accounts;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Notifications;
using Ranking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameNet
{
    public static class NetCore
    {
        static HubConnection conn;
        public static HubConnectionState State => conn.State;
        public static Subscriptions Subs { get; private set; }
        public static Invokes Actions { get; private set; }

        public static string Url_Hub
        {
            get
            {
                return ConnType == ConnectionType.Local
                    ? "https://localhost:5001/GameHub" : ConnType == ConnectionType.Development 
                        ? "http://www.bsserver.tk:8888/GameHub"
                            : "http://bsserver.tk/GameHub";
            }
        }
        public enum ConnectionType { Production, Local, Development }

        public static ConnectionType ConnType;



        public static Action OnConnect, OnDisconnect, OnReconnect, OnFullReady;
        public static Action OnLogIn;

        
        
        public static bool TryReconnect { get; set; }
        public static int ReconnectAttempt { get; private set; }


        
        
        
        

        static NetCore()
        {
            SceneManager.activeSceneChanged += (arg0, scene) =>
            {
                //Debug.Log("activeSceneChanged");
                //if(Time.realtimeSinceStartup > 5) OnSceneLoad();
            };
            Application.quitting += () =>
            {
                conn?.StopAsync();
                TryReconnect = false;
            };
            
            TryReconnect = true;
            
            //OnSceneLoad();
            
        }


        // This methods is invoked by Wrapper
        // In config you should set up all subs, evetns and etc
        // There is automatic send OnFullReady on configuration end
        // (External usage)
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
            Debug.Log("NetCore.OnSceneLoad()");
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
            BuildConnection();
            SubcribeOnServerCalls();
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
                    Debug.Log("[ Connected ]");
                    OnConnect?.Invoke();
                    ReconnectAttempt = 0;
                    //OnFullReady?.Invoke();
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

        static async void Reconnect()
        {
            if (!TryReconnect) return;
            await Task.Delay(3000);
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
            conn = new HubConnectionBuilder()
                .WithUrl(new Uri(Url_Hub))
                .Build();
            
            Debug.Log("Build connecting with " + Url_Hub);

            conn.Closed += (err =>
            {
                Debug.Log("Conn closed due to " + err.Message);
                OnDisconnect?.Invoke();
                Reconnect();
                return null;
            });
        }

        // This method invoke conn.On<T1,T2,TN> for EACH FIELD in NetCore.Subs
        // This is internal method used only after building connection
        // (Internal usage)
        static void SubcribeOnServerCalls()
        {
            Subs = new Subscriptions();
           

            var fields = typeof(Subscriptions).GetFields();
            foreach (var field in fields)
            {
                Type t = field.FieldType;
                conn.On(field.Name, t.GenericTypeArguments, (objects =>
                {
                    FieldInfo info = typeof(Subscriptions).GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
                    object yourfield = info.GetValue(Subs);
                    MethodInfo method = yourfield.GetType().GetMethod("Invoke");
                    
                    method.Invoke(yourfield, objects);

                    return null;
                }));
            }
        }

        static void SubcribeOnClientInvokes()
        {
            Actions = new Invokes();

            
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
                
                /*conn.On(field.Name, t.GenericTypeArguments, (objects =>
                {
                    FieldInfo info = typeof(Subscriptions).GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
                    object yourfield = info.GetValue(Subs);
                    MethodInfo method = yourfield.GetType().GetMethod("Invoke");
                    
                    method.Invoke(yourfield, objects);

                    return null;
                }));*/
            }

            Actions.Log("Suka but no blyat");
        }
        //static void DoInvoke(MethodInfo)
        static void SuperVoid(string name, params object[] objs)
        {
            Debug.Log("> Invoke " + name);
        }

        
        
        
        #endregion
        
        
        
        
        
        
        // This class contains implementation of server-side methods
        // (Piece of 90 lines of code ._.) 
        // (External usage)
        public static class ServerActions
        {
            public static void Test() => NetCore.conn.InvokeAsync("Test");
            public static void SendChatMessage(string nick, string msg, BeatSlayerServer.Multiplayer.Accounts.AccountRole role, string group) 
                => NetCore.conn.InvokeAsync("Chat_SendMessage", nick, msg, role, group);

            public static void UpdateInGameTime()
            {
                if (NetCorePayload.CurrentAccount == null) return;
                if (NetCorePayload.PrevInGameTimeUpdate == 0) NetCorePayload.PrevInGameTimeUpdate = Time.realtimeSinceStartup;
                int seconds = Mathf.RoundToInt(Time.realtimeSinceStartup - NetCorePayload.PrevInGameTimeUpdate);
                if (seconds < 30) return;
                NetCorePayload.PrevInGameTimeUpdate = Time.realtimeSinceStartup;
                NetCore.conn.InvokeAsync("Accounts_UpdateInGameTime", NetCorePayload.CurrentAccount.Nick, seconds);
            }

            public static class Account
            {
                public static void LogIn(string nick, string password)
                {
                    NetCore.conn.InvokeAsync("Accounts_LogIn", nick, password);
                    UpdateInGameTime();
                }
                    
                
                
                public static void SignUp(string nick, string password, string country, string email) =>
                    NetCore.conn.InvokeAsync("Accounts_SignUp", nick, password, country, email);
                
                public static void Search(string nick) =>
                    NetCore.conn.InvokeAsync("Accounts_Search", nick);
                public static void View(string nick) =>
                    NetCore.conn.InvokeAsync("Accounts_View", nick);




                public static void GetAvatar(string nick) => conn.InvokeAsync("Accounts_GetAvatar", nick);

                public static void ChangePassword(string nick, string oldpass, string newpass) =>
                    conn.InvokeAsync("Accounts_ChangePassword", nick, oldpass, newpass);
                public static void Restore(string nick, string password) =>
                    conn.InvokeAsync("Accounts_Restore", nick, password);
                public static void ConfirmRestore(string code) => 
                    conn.InvokeAsync("Accounts_ConfirmRestore", code);
                
                
                public static void ChangeEmptyEmail(string nick, string email) =>
                    conn.InvokeAsync("Accounts_ChangeEmptyEmail", nick, email);
                
                public static void ChangeEmail(string nick, string email) =>
                    conn.InvokeAsync("Accounts_ChangeEmail", nick, email);

                public static void SendChangeEmailCode(string nick, string email) =>
                    conn.InvokeAsync("Accounts_SendChangeEmailCode", nick, email);
                
                public static void SendReplay(string json) =>
                    conn.InvokeAsync("Accounts_SendReplay", json);

                public static void GetBestReplay(string nick, string trackname, string creatornick) =>
                    conn.InvokeAsync("Accounts_GetBestReplay", nick, trackname, creatornick);

                public static void GetBestReplays(string nick, int count) =>
                    conn.InvokeAsync("Accounts_GetBestReplays", nick, count);
            }

            public static class Friends
            {
                public static void GetFriends(string nick) =>
                    conn.InvokeAsync("Friends_GetFriends", nick);
                
                public static void InviteFriend(string addNick, string nick) =>
                    conn.InvokeAsync("Friends_InviteFriend",addNick, nick);
                
                public static void RemoveFriend(string fromNick, string nick) =>
                    conn.InvokeAsync("Friends_RemoveFriend",fromNick, nick);
            }

            public static class Notifications
            {
                public static void Accept(string nick, int id) =>
                    conn.InvokeAsync("Friends_AcceptInvite", nick, id);
                
                public static void Reject(string nick, int id) =>
                    conn.InvokeAsync("Friends_RejectInvite", nick, id);
            }
            
            
            public static class Shop
            {
                public static void SendCoins(string nick, int coins) =>
                    conn.InvokeAsync("Shop_SendCoins", nick, coins);
            }
            public static class Chat
            {
                public static void GetGroups() => NetCore.conn.InvokeAsync("Chat_GetGroups");
                public static void JoinGroup(string nick, string group) => NetCore.conn.InvokeAsync("Chat_JoinGroup", nick, group);
            }
        }


        
        
        // This class contains all methods which can invoke server on client-side
        // How server determine what should invoke? -Field name :D
        // Server: Client.Caller.SendAsync("MethodName", args)
        // There must be Action with name same as MethodName, else server won't be able to find it.
        
        // Some rules to use SignalR (пиздец, а нормально можно было сделать?! Вот чтоб без ебли в жопу!)
        // 1) If you want to send class USE {GET;SET} !!!!
        // 2) Don't use ctors at all, data won't be sent
        public class Subscriptions
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
            public Action Test;
            public Action<string> Log = (str) => SuperVoid("Log", str);
            //public void Test(string nick, string password) {}
            //public void Test(string nick, string password) => NetCore.Invoke();
        }
    }
}