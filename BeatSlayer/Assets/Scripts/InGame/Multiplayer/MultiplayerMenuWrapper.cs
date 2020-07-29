using GameNet;
using InGame.Multiplayer;
using Microsoft.AspNetCore.SignalR.Client;
using Multiplayer.Accounts;
using Multiplayer.Chat;
using Multiplayer.Notification;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MultiplayerMenuWrapper : MonoBehaviour
{
    public ChatUI chatUI;
    public AccountUI accountUI;
    public FriendsUI friendsUI;
    public NotificationUI notificationUI;
    public MenuScript_v2 menu;

    public NetConfigurationModel config;
   

    [Header("UI")] 
    public Animator serverStateAnim;
    public Animator bottomButtonsAnim;
    public Text serverStateText;
    private float timeUntilClose;
    private bool isNoInternetShowed; // False when message wasn't showed yet, and true if yes
    

    private void Awake()
    {
        try
        {
            string filepath = Application.persistentDataPath + "/netconfig.json";
            if (File.Exists(filepath))
            {
                config = JsonConvert.DeserializeObject<NetConfigurationModel>(File.ReadAllText(filepath));
            }
            else
            {
                config = new NetConfigurationModel();
            }
        }
        catch(Exception err)
        {
            Debug.LogError("Error on reading netconfig\n" + err.Message);
            config = new NetConfigurationModel();
        }

        NetCore.Config = config;
    }

    private void Start()
    {
        //Debug.Log("Using dev NetCore_v2!");
        /*NetCore_v2.Initialize();
        NetCore_v2.Connect();*/
        NetCore.Configure(() =>
        {
            NetCore.OnFullReady += () =>
            {
                //accountUI.ShowMessage("Ready");
                accountUI.OnSceneLoad();
            };

            NetCore.OnConnect += OnConnect;
            NetCore.OnReconnect += OnReconnecting;
            NetCore.OnDisconnect += OnDisconnected;
            NetCore.OnLogIn += OnLogIn;
            NetCore.OnLogOut += OnLogOut;

            // Subscribe/Resubscribe all
            //NetCore.Subs.OnTest += (() => accountUI.ShowMessage("Got test"));

            accountUI.Configuration();
            chatUI.Configuration();
            friendsUI.Configuration();
            notificationUI.Configuration();
            menu.Configuration();
        });


        // Hide bottom buttons if no logged
        if(Payload.Account != null)
        {
            bottomButtonsAnim.Play("Show");
        }
    }

    private void Update()
    {
        if (timeUntilClose > 0) timeUntilClose -= Time.deltaTime;
        else
        {
            timeUntilClose = 0;
            serverStateAnim.Play("Hide");
        }

        NetCore.ServerActions.UpdateInGameTime();
    }

    public void OnConnect()
    {
        if(NetCore.ReconnectAttempt != 0) ShowState("Connected", 3); 
        accountUI.OnConnect(this);
        isNoInternetShowed = false;
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






    void OnDisconnected()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            ShowState("Connection lost", 5);
            chatUI.OnConnectionLost();
            //if (err != null)
            //{
                //Debug.LogError("Disconnected due to " + err);
            //}
        });
    }

    void OnReconnecting()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if(!isNoInternetShowed)
            {
                ShowState("No internet connection", 5);
                isNoInternetShowed = true;
            }
        }
        else
        {
            ShowState("Reconnecting.. attempt " + (NetCore.ReconnectAttempt), 30);
        }
    }
    
    void OnLogIn()
    {
        bottomButtonsAnim.Play("Show");
    }
    void OnLogOut()
    {
        bottomButtonsAnim.Play("Hide");
    }
}