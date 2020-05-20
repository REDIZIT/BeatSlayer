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
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameNet
{
    public static class NetCore
    {
        static HubConnection conn;
        public static HubConnectionState State => conn.State;
        public static Subscriptions Subs { get; private set; }
        //public static Invokes Actions { get; private set; }

        public static string Url_Hub
        {
            get
            {
                return ConnType == ConnectionType.Local
                    ? "https://localhost:5001/GameHub"
                    : "http://bsserver.tk/GameHub";
            }
        }
        public enum ConnectionType { Production, Local }

        public static ConnectionType ConnType = ConnectionType.Local;



        public static Action OnConnect, OnDisconnect, OnReconnect, OnFullReady;

        
        
        public static bool TryReconnect { get; set; }
        public static int ReconnectAttempt { get; private set; }


        
        
        
        

        static NetCore()
        {
            SceneManager.activeSceneChanged += (arg0, scene) =>
            {
                OnSceneLoad();
            };
            Application.quitting += () =>
            {
                conn.StopAsync();
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

            UnityMainThreadDispatcher dispatcher = UnityMainThreadDispatcher.Instance();
            dispatcher.Enqueue(() =>
            {
                Debug.Log("NetCore.Configure via Dispatcher");
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
            SubcribeOnClientInvokes();
            Connect();
        }
        
        
        
        
        

        // This async method connects to server via existing HubConnection
        // (Internal usage)
        static async void Connect()
        {
            try
            {
                await conn.StartAsync();
            }
            catch (Exception err)
            {
                Debug.Log("Connection failed: " + err);
            }
            
            if (conn.State == HubConnectionState.Connected)
            {
                ReconnectAttempt = 0;
                OnConnect?.Invoke();
                //OnFullReady?.Invoke();
            }
            else
            {
                Reconnect();
            }
        }

        static void Reconnect()
        {
            if (!TryReconnect) return;
            ReconnectAttempt++;
            Debug.Log("Try to reconnect. Attempt " + ReconnectAttempt);
            Connect();
        }

        // This method creates new connection
        // (Internal usage)
        static void BuildConnection()
        {
            conn = new HubConnectionBuilder()
                .WithUrl(new Uri(Url_Hub))
                .Build();
            
            Debug.Log("Build connecting with " + Url_Hub);
        
            conn.KeepAliveInterval = TimeSpan.FromSeconds(3);
            conn.ServerTimeout = TimeSpan.FromSeconds(3*3);

            conn.Closed += (err =>
            {
                Debug.Log("Conn closed");
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
            /*Actions = new Invokes();

            
            MethodInfo mi = Actions.GetType().GetMethod("Invoke");
            Action del = (Action)Delegate.CreateDelegate(typeof(Action), mi);

            
            Actions.Test = del;
            Actions.Test();*/
        }


        
        
        
        #endregion
        
        
        
        
        
        
        // This class contains implementation of server-side methods
        // (External usage)
        public static class ServerActions
        {
            public static void Test() => NetCore.conn.InvokeAsync("Test");
            public static void SendChatMessage(string nick, string msg, BeatSlayerServer.Multiplayer.Accounts.AccountRole role, string group) 
                => NetCore.conn.InvokeAsync("Chat_SendMessage", nick, msg, role, group);

            public static class Account
            {
                public static void LogIn(string nick, string password) =>
                    NetCore.conn.InvokeAsync("Accounts_LogIn", nick, password);
                
                public static void SignUp(string nick, string password, string country, string email) =>
                    NetCore.conn.InvokeAsync("Accounts_SignUp", nick, password, country, email);
                
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
        public class Subscriptions
        {
            public Action OnTest;
            public Action<OperationMessage> Accounts_OnLogIn;
            public Action<OperationMessage> Accounts_OnSignUp;
            public Action<string> OnJoinGroup;
            public Action<string> OnGetGroups;
            public Action<string> OnSendChatMessage;

            public Action<byte[]> Accounts_OnGetAvatar;
            public Action<string, string, string> Accounts_ChangePassword;
            
            public Action<float> Accounts_OnSendReplay;
            public Action<string> Accounts_OnGetBestReplay;
        }

        /*public class Invokes
        {
            public Action Test { get; set; }
            //public void Test(string nick, string password) {}
            //public void Test(string nick, string password) => NetCore.Invoke();
            
            public void Invoke()
            {
                Debug.Log("ITS AAAALLLIIIIVVEEEE!!!");
            }
        }*/
    }
}