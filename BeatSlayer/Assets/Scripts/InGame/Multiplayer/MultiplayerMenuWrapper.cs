﻿using System;
using System.Collections;
using System.Collections.Generic;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using Multiplayer.Accounts;
using Multiplayer.Chat;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerMenuWrapper : MonoBehaviour
{
    public ChatUI chatUI;
    public AccountUI accountUI;
    public FriendsUI friendsUI;
<<<<<<< HEAD
    public NotificationUI notificationUI;
    public MenuScript_v2 menu;
    
    
=======

>>>>>>> parent of ae2d14c... Before redesign
    [Header("Server connection override")]
    public NetCore.ConnectionType connType;
    public bool forceChangeConnType;

    [Header("UI")] 
    public Animator serverStateAnim;
    public Text serverStateText;
    private float timeUntilClose;
    

    private void Awake()
    {
        if (Application.isEditor && forceChangeConnType)
        {
            NetCore.ConnType = connType;
        }
    }

    private void Start()
    {
        NetCore.Configurators += () =>
        {
            NetCore.OnFullReady += () =>
            {
                //accountUI.ShowMessage("Ready");
                accountUI.OnSceneLoad();
            };

            NetCore.OnConnect += OnConnect;
            NetCore.OnReconnect += OnReconnecting;
            NetCore.OnDisconnect += OnDisconnected;

            // Subscribe/Resubscribe all
            NetCore.Subs.OnTest += (() => accountUI.ShowMessage("Got test"));
<<<<<<< HEAD
        };
        NetCore.Configure();
        /*NetCore.Configure(() =>
        {
            

            //accountUI.Configure();
            //chatUI.Configure();
            //friendsUI.Configure();
            //notificationUI.Configure();
            //menu.Configure();
        });*/
=======

            accountUI.Configure();
            chatUI.Configure();
            friendsUI.Configure();
        });
>>>>>>> parent of ae2d14c... Before redesign


        Debug.Log("Url is " + NetCore.Url_Hub);
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
        if (timeUntilClose > 0) timeUntilClose -= Time.deltaTime;
        else
        {
            timeUntilClose = 0;
            serverStateAnim.Play("Hide");
        }
    }

    public void OnConnect()
    {
        if(NetCore.ReconnectAttempt != 0) ShowState("Connected", 3); 
        accountUI.OnConnect(this);
    }




    void OnDisconnected()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            ShowState("Connection lost", 120);
            chatUI.OnConnectionLost();
            //if (err != null)
            //{
                //Debug.LogError("Disconnected due to " + err);
            //}
        });
    }

    void OnReconnecting()
    {
        ShowState("Reconnecting.. attempt " + (NetCore.ReconnectAttempt), 30);
    }
    
    
    public void ShowState(string state, float time)
    {
        timeUntilClose = time;
        serverStateAnim.Play("Show");
        serverStateText.text = state;
    }

    public void HideState()
    {
        serverStateAnim.Play("Hide");
    }
}