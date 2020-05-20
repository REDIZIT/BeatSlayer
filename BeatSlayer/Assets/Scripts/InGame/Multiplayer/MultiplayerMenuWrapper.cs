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
    public NetCore.ConnectionType connType;
    public bool forceChangeConnType;



    private int reconnectTry;
    private bool tryToReconnect = true;

    public bool doReconnect;

    

    private void Awake()
    {
        if (forceChangeConnType)
        {
            NetCore.ConnType = connType;
        }
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
                accountUI.OnSceneLoad();
            };

            NetCore.OnConnect += OnConnect;
            NetCore.OnDisconnect += OnDisconnected;
            
            // Subscribe/Resubscribe all
            NetCore.Subs.OnTest += (() => accountUI.ShowMessage("Got test"));
            NetCore.Subs.Accounts_OnGetBestReplay += info => Debug.Log("Got best replay with json = " + info);

            accountUI.Configure();
            chatUI.Configure();
        });
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Reloading scene");
            SceneManager.LoadScene("Menu");
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("Send test");
            NetCore.ServerActions.Test();
        }*/
    }

    public void OnConnect()
    {
        accountUI.OnConnect(this);
        NetCore.ServerActions.Account.GetBestReplay("REDIZIT", "kasai harcores-cycle hit", "idxracer");
    }




    void OnDisconnected(/*Exception err*/)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(IOnDisconnected(null));
    }
    IEnumerator IOnDisconnected(Exception err)
    {
        chatUI.OnConnectionLost();
        if (err != null)
        {
            Debug.LogError("Disconnected due to " + err);
            //MultiplayerCore.Reconnect();
        }
        
        yield break;
    }

    
}