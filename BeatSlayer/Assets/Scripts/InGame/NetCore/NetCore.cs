using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameNet
{
    public static class NetCore
    {
        public static HubConnection conn;
        public static Subscribtions Subs { get; private set; }
        
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
            };
            
            TryReconnect = true;
            OnSceneLoad();
        }


        // This methods is invoked by Wrapper
        // In config you should set up all subs, evetns and etc
        // There is automatic send OnFullReady on configuration end
        // (External usage)
        public static void Configure(Action config)
        {
            Subs = new Subscribtions();
            
            config();
            
            OnFullReady?.Invoke();
        }

        
        
        #region Internal usage
        
        
        
        // This method is invoked when Scene changed
        // Connect if not already
        // (Internal usage)
        static void OnSceneLoad()
        {
            if (conn == null)
            {
                Debug.Log("CreateConnection");
                CreateConnection();
            }
        }

        // This method is invoked on first load
        // (Internal usage)
        static void CreateConnection()
        {
            BuildConnection();
            Subscribe();
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
                OnFullReady?.Invoke();
            }
            else if(TryReconnect)
            {
                ReconnectAttempt++;
                Debug.Log("Try to reconnect. Attempt " + ReconnectAttempt);
                Connect();
            }
        }

        // This method creates new connection
        // (Internal usage)
        static void BuildConnection()
        {
            conn = new HubConnectionBuilder()
                .WithUrl(new Uri(MultiplayerCore.url_hub))
                .Build();
        
            conn.KeepAliveInterval = TimeSpan.FromSeconds(3);
            conn.ServerTimeout = TimeSpan.FromSeconds(3*3);

            conn.Closed += (err =>
            {
                Debug.Log("Conn closed");
                return null;
            });
        }

        // This method invoke conn.On<T1,T2,TN> for EACH FIELD in NetCore.Subs
        // This is internal method used only after building connection
        // (Internal usage)
        static void Subscribe()
        {
            Subs = new Subscribtions();

            var fields = typeof(Subscribtions).GetFields();
            foreach (var field in fields)
            {
                Type t = field.FieldType;
                conn.On(field.Name, t.GenericTypeArguments, (objects =>
                {
                    FieldInfo info = typeof(Subscribtions).GetField(field.Name, BindingFlags.Public | BindingFlags.Instance);
                    object yourfield = info.GetValue(Subs);
                    MethodInfo method = yourfield.GetType().GetMethod("Invoke");
                    
                    method.Invoke(yourfield, objects);
                    
                    return Task.Delay(0); 
                }));
            }
        }
        
        
        #endregion
        
        
        
        // Client want to get OnTest()
        // NetCore.Subs.OnTest += (...)
        //
        // Client want to invoke Test() on server
        // NetCore.ServerActions.Test()
        
        // How to add new feature
        // NetCube.Sub.NewFeature()


        // This class contains implementation of server-side methods
        // (External usage)
        public static class ServerActions
        {
            public static void Test() => NetCore.conn.InvokeAsync("Test");
            public static void Accounts_LogIn(string nick, string password) => NetCore.conn.InvokeAsync("Accounts_LogIn", nick, password);
            public static void SendChatMessage(string nick, string msg, BeatSlayerServer.Multiplayer.Accounts.AccountRole role, string group) => NetCore.conn.InvokeAsync("Chat_SendMessage", nick, msg, role, group);

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
        public class Subscribtions
        {
            public Action OnTest;
            public Action<OperationMessage> Accounts_OnLogIn;
            public Action<string> OnJoinGroup;
            public Action<string> OnGetGroups;
            public Action<string> OnSendChatMessage;
        }
    }
}