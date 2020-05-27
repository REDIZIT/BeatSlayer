using System;
using System.Collections;
using System.Collections.Generic;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using InGame.Helpers;
using Multiplayer.Accounts;
using Multiplayer.Notification;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class FriendsUI : MonoBehaviour
{
    public GameObject window;
    public Transform content;
    
    public AccountUI accountUI;
    public NotificationUI notificationUI;

    public Text label;
    
    public RectTransform scrollview;
    public InputField searchField;
    public GameObject noFriends, noMatches;


    private void Awake()
    {
        NetCore.Configurators += () =>
        {
            NetCore.Subs.Friends_OnGetFriends += list =>
            {
                NetCorePayload.CurrentAccount.Friends = list;
                ShowList(list, true);
            };
            NetCore.Subs.Accounts_OnSearch += list =>
            {
                ShowList(list, false);
            };
        
            NetCore.OnLogIn += () =>
            {
                if (NetCorePayload.CurrentAccount != null)
                {
                    NetCore.ServerActions.Friends.GetFriends(NetCorePayload.CurrentAccount.Nick);
                }
            };

            NetCore.Subs.Notification_OnSend += info =>
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.Log(JsonConvert.SerializeObject(info));
                    notificationUI.ShowNotification(info);  
                });
            };
        };
    }

    public void RemoveFriend(string fromNick)
    {
        NetCore.ServerActions.Friends.RemoveFriend(fromNick, NetCorePayload.CurrentAccount.Nick);
    }
    public void AddFriend(string addNick)
    {
        NetCore.ServerActions.Friends.InviteFriend(addNick, NetCorePayload.CurrentAccount.Nick);
    }


    void ShowList(List<AccountData> list, bool isFriendList)
    {
        noFriends.SetActive(isFriendList && list.Count == 0);
        noMatches.SetActive(!isFriendList && list.Count == 0);
        
        HelperUI.FillContent<FriendItemUI, AccountData>(content, list, (ui, item) =>
        {
            ui.Refresh(item);
        });
    }

    public void ViewAccount(AccountData data)
    {
        window.SetActive(false);
        NetCore.ServerActions.Account.View(data.Nick);
    }
    
    
    
    
    public void OnSearchBtnClick()
    {
        HelperUI.ClearContent(content);
        if (searchField.gameObject.activeSelf)
        {
            label.text = "Friends";
            searchField.gameObject.SetActive(false);
            scrollview.offsetMax = new Vector2(scrollview.offsetMax.x, 0);
            NetCore.ServerActions.Friends.GetFriends(NetCorePayload.CurrentAccount.Nick);
        }
        else
        {
            label.text = "Search";
            searchField.gameObject.SetActive(true);
            scrollview.offsetMax = new Vector2(scrollview.offsetMax.x, -100);
        }
    }

    public void OnSearchTypeEnd()
    {
        Search(searchField.text);
    }

    void Search(string str)
    {
        NetCore.ServerActions.Account.Search(str);
    }
}
