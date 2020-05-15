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

public class MultiplayerCore : MonoBehaviour
{
    public ChatUI chatUI;
    public AccountUI accountUI;
    public Account account;
    
    
    private const string url_hub = "http://www.bsserver.tk/GameHub";
    //private const string url_hub = "https://localhost:5001/GameHub";
    private int reconnectTry;
    private bool tryToReconnect = true;

    public bool doReconnect;

    public HubConnection conn;
    
    private void Start()
    {
        BuildConnection();
        Subscribe();
        Connect();
        
        
        Application.quitting += () =>
        {
            conn.StopAsync();
            tryToReconnect = false;
        };
    }


    IEnumerator IOnDisconnect(Exception err)
    {
        chatUI.OnConnectionLost();
        if (err != null)
        {
            Debug.LogError("Disconnected due to " + err);
            Reconnect();
        }
        
        yield break;
    }

    async void Reconnect()
    {
        Debug.Log("Reconnecting");

        BuildConnection();
        Subscribe();
        Connect();
    }
    
    public void BuildConnection()
    {
        conn = new HubConnectionBuilder()
            .WithUrl(new Uri(url_hub))
            .Build();
        
        //conn.KeepAliveInterval = TimeSpan.FromSeconds(3);
        //conn.ServerTimeout = TimeSpan.FromSeconds(3*3);

        conn.Closed += (err =>
        {
            UnityMainThreadDispatcher.Instance().Enqueue(IOnDisconnect(err));
            return Task.Delay(0);
        });
        //conn.Reconnecting += OnReconnecting;
        //conn.Reconnected += OnReconnected;

    }
    public async void Connect()
    {
        Debug.Log("Connect()");
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
                chatUI.OnConnect();
                accountUI.OnConnect(this);
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
    
    public void Subscribe()
    {
        conn.On<string>("OnSendChatMessage", chatUI.OnSendChatMessage);
        
        conn.On<string>("OnGetGroups", chatUI.OnGetGroups);
        conn.On<string>("OnJoinGroup", chatUI.OnJoinGroup);
        
        
        conn.On<byte[]>("OnGetAvatar", chatUI.OnGetAvatar);
        conn.On<int>("OnOnlineChange", chatUI.OnOnlineChange);

        conn.On<OperationMessage>("Accounts_OnLogIn", accountUI.OnLogIn);
        conn.On<OperationMessage>("Accounts_OnSignUp", accountUI.OnSignUp);
        conn.On<OperationMessage>("Accounts_OnRestore", accountUI.OnRestore);
        conn.On<OperationMessage>("Accounts_OnChangePassword", accountUI.OnChangePassword);
        conn.On<OperationMessage>("Accounts_OnChangeEmail", accountUI.OnChangeEmail);
        conn.On<bool>("Accounts_OnConfirmRestore", accountUI.OnConfirmRestore);
    }
}
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