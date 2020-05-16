using System;
using System.Collections;
using System.Collections.Generic;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using Multiplayer.Accounts;
using Multiplayer.Chat;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerMenuWrapper : MonoBehaviour
{
    public ChatUI chatUI;
    public AccountUI accountUI;

    [Header("Server connection override")]
    public ConnectionType connType;
    public bool forceChangeConnType;



    private int reconnectTry;
    private bool tryToReconnect = true;

    public bool doReconnect;

    

    private void Awake()
    {
        if (forceChangeConnType)
        {
            MultiplayerCore.ConnType = connType;
        }

        MultiplayerCore.onConnected = OnConnect;
        MultiplayerCore.onDisconnected = OnDisconnected;

        
    }

    private void Start()
    {
        Application.quitting += () =>
        {
            //MultiplayerCore.conn.StopAsync();
            tryToReconnect = false;
        };
        
        NetCore.Configure(() =>
        {
            NetCore.OnFullReady += () =>
            {
                accountUI.ShowMessage("Ready");
            };

            NetCore.OnConnect += OnConnect;
            
            // Subscribe/Resubscribe all
            NetCore.Subs.OnTest += (() => accountUI.ShowMessage("Got test"));
            NetCore.Subs.OnSendChatMessage += chatUI.OnSendChatMessage;

            accountUI.Configure();
            chatUI.Configure();
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Reloading scene");
            SceneManager.LoadScene("Menu");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Send test");
            NetCore.ServerActions.Test();
        }
    }

    public void OnConnect()
    {
        Debug.Log("Wrapper.OnConnect");
        chatUI.OnConnect();
        accountUI.OnConnect(this);
    }




    void OnDisconnected(Exception err)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(IOnDisconnected(err));
    }
    IEnumerator IOnDisconnected(Exception err)
    {
        chatUI.OnConnectionLost();
        if (err != null)
        {
            Debug.LogError("Disconnected due to " + err);
            MultiplayerCore.Reconnect();
        }
        
        yield break;
    }

    
}