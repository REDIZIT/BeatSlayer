using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatSlayerServer.Multiplayer.Accounts;
using Microsoft.AspNetCore.SignalR.Client;
using Multiplayer.Accounts;
using Multiplayer.Chat;
using UnityEngine;
using UnityEngine.Serialization;
using Web;


public static class MultiplayerCore
{
    private const string url_prod_hub = "http://www.bsserver.tk/GameHub";
    private const string url_local_hub = "https://localhost:5001/GameHub";
    public static string url_hub
    {
        get
        {
            return ConnType == ConnectionType.Local ? url_local_hub : url_prod_hub;
        }
    }
    
    

    private static ConnectionType _connType;
    public static ConnectionType ConnType
    {
        get
        {
            if (Application.isEditor)
            {
                return _connType;
            }
            else
            {
                return ConnectionType.Production;
            }
        }
        set
        {
            _connType = value;
        }
    }
    
    
    public static HubConnection conn;
    public static Action onConnected;
    public static Action<Exception> onDisconnected;
    

    
    public static async void Reconnect()
    {
        Debug.Log("Reconnecting");

        Connect();
    }
    
    public static void BuildConnection()
    {
        conn = new HubConnectionBuilder()
            .WithUrl(new Uri(MultiplayerCore.url_hub))
            .Build();
        
        conn.KeepAliveInterval = TimeSpan.FromSeconds(3);
        conn.ServerTimeout = TimeSpan.FromSeconds(3*3);

        conn.Closed += (err =>
        {
            onDisconnected(err);
            return Task.Delay(0);
        });
        
        //conn.Reconnecting += OnReconnecting;
        //conn.Reconnected += OnReconnected;
    }
    public static async void Connect()
    {
        BuildConnection();
        SubscribeOnConnection();
        bool doReconnect = false;
        
        try
        {
            await conn.StartAsync();
            Debug.Log("Connect() " + conn.State);
            if (conn.State != HubConnectionState.Connected)
            {
                Debug.Log("Can't connect");
                await Task.Delay(3000);
                doReconnect = true;
            }
            else
            {
                //AcceptSubscribers();
                onConnected();
                //chatUI.OnConnect();
                //accountUI.OnConnect(this);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Can't connect due to " + e);
            await Task.Delay(5000);
            doReconnect = true;
        }
        
        if(doReconnect) UnityMainThreadDispatcher.Instance().Enqueue(Reconnect);
    }
    
    
    // This class contains all info about events which can invoke server
    public static Subscribtions subscribtions;

    // This class contains all info about events which can invoke client
    //public static Requests requests;
    
    // Used to invoke events on server (Needed due to conn.On overwrite delegates)
    private static SubInvoker subInvoker;
    
    // List of methods need to call to handle all subscribes
    static Action subscribers;

    
    
    
   /* public static void ClearSubscribtions()
    {
        Debug.Log("ClearSubscribtions");
        subscribers = null;
        subscribtions = new Subscribtions();
    }
    public static void Subscribe(Action acceptMethod)
    {
        Debug.Log("Subscribe");
        subscribers += acceptMethod;
    }

    public static void AcceptSubscribers()
    {
        Debug.Log("AcceptSubscribers. Is Null? " + (subscribers == null));
        subscribers?.Invoke();
    }*/
    
    static void SubscribeOnConnection()
    {
        subscribtions = new Subscribtions();
        subInvoker = new SubInvoker(subscribtions);
        
        conn.On<string>("OnSendChatMessage", subInvoker.OnSendChatMessage);
        conn.On<string>("OnGetGroups", (s =>
        {
            Debug.Log("Core.OnGetGroups: " + s);
            subInvoker.OnGetGroups(s);
        }));
        conn.On<string>("OnJoinGroup", (s =>
        {
            Debug.Log("Core.OnJoinGroup: " + s);
            subInvoker.OnJoinGroup(s);
        }));
        
        conn.On<byte[]>("OnGetAvatar", subInvoker.OnGetAvatar);
        conn.On<int>("OnOnlineChange", subInvoker.OnOnlineChange);

        conn.On<OperationMessage>("Accounts_OnLogIn", subInvoker.Accounts_OnLogIn);
        conn.On<OperationMessage>("Accounts_OnSignUp", subInvoker.Accounts_OnSignUp);
        conn.On<OperationMessage>("Accounts_OnRestore", subInvoker.Accounts_OnRestore);
        conn.On<OperationMessage>("Accounts_OnChangePassword", subInvoker.Accounts_OnChangePassword);
        conn.On<OperationMessage>("Accounts_OnChangeEmail", subInvoker.Accounts_OnChangeEmail);
        conn.On<bool>("Accounts_OnConfirmRestore", subInvoker.Accounts_OnConfirmRestore);
    }
    
    public static class Requests
    {
        public static void Chat_GetGroups() => MultiplayerCore.conn.InvokeAsync("Chat_GetGroups");
        public static void Chat_JoinGroup(string nick, string groupName)
        {
            throw new Exception("Rabotay blyat");
            Debug.Log("Request: Chat_JoinGroup groupname: " + groupName);
            MultiplayerCore.conn.InvokeAsync("Chat_JoinGroup", nick, groupName);
        }
    }
}

public class SubInvoker
{
    private Subscribtions subs;

    public SubInvoker(Subscribtions subs)
    {
        this.subs = subs;
    }

    
    public void Accounts_OnLogIn(OperationMessage msg) => subs.Accounts_OnLogIn(msg);
    public void Accounts_OnSignUp(OperationMessage msg) => subs.Accounts_OnSignUp(msg);
    public void Accounts_OnRestore(OperationMessage msg) => subs.Accounts_OnRestore(msg);
    public void Accounts_OnChangePassword(OperationMessage msg) => subs.Accounts_OnChangePassword(msg);
    public void Accounts_OnChangeEmail(OperationMessage msg) => subs.Accounts_OnChangeEmail(msg);
    public void Accounts_OnConfirmRestore(bool msg) => subs.Accounts_OnConfirmRestore(msg);
    

    public void OnOnlineChange(int onlineNumber) => subs.OnOnlineChange(onlineNumber);
    public void OnSendChatMessage(string message)
    {
        Debug.Log("Core.OnSendChatMessage: " + message);
        subs.OnSendChatMessage(message);
    }

    public void OnGetGroups(string groups) => subs.OnGetGroups(groups);
    public void OnJoinGroup(string groupHistory) => subs.OnJoinGroup(groupHistory);
    public void OnGetAvatar(byte[] picture) => subs.OnGetAvatar(picture);
}
public class Subscribtions
{
    public Action<OperationMessage> Accounts_OnLogIn;
    public Action<OperationMessage> Accounts_OnSignUp;
    public Action<OperationMessage> Accounts_OnRestore;
    public Action<OperationMessage> Accounts_OnChangePassword;
    public Action<OperationMessage> Accounts_OnChangeEmail;
    public Action<bool> Accounts_OnConfirmRestore;
    
    
    public Action<string> OnSendChatMessage;
    public Action<string> OnGetGroups;
    public Action<string> OnJoinGroup;
    
    public Action<byte[]> OnGetAvatar;
    public Action<int> OnOnlineChange;
}







public enum ConnectionType { Production, Local }

public class OperationMessage
{
    public enum OperationType
    {
        Fail, Warning, Success
    }
    public OperationType Type { get; set; }
    public string Message { get; set; }

    public OperationMessage() {}
    public OperationMessage(OperationType type)
    {
        Type = type;
    }
    public OperationMessage(OperationType type, string message)
    {
        Type = type;
        Message = message;
    }
}