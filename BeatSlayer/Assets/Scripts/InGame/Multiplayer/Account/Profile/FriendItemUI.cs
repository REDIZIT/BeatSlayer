using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using Multiplayer.Chat;
using UnityEngine;
using UnityEngine.UI;
using Web;

public class FriendItemUI : MonoBehaviour
{
    public FriendsUI ui;
    public AccountData data;
    
    public RawImage avatarImage;
    public Text nickText, activeText;

    public GameObject removeBtn, addBtn, writeBtn;

    public void Refresh(AccountData data)
    {
        this.data = data;
        //avatarImage =
        nickText.text = data.Nick;
        activeText.text = data.IsOnline ? "Online" : "Was online " + data.LastActiveTimeUtc.ToLocalTime().ToShortTimeString();

        WebAPI.GetAvatar(data.Nick, (Texture2D tex) =>
        {
            avatarImage.texture = tex;
        });
        
        RefreshButtons();
    }

    void RefreshButtons()
    {
        bool isFriend = NetCorePayload.CurrentAccount != null && NetCorePayload.CurrentAccount.Friends.Any(c => c.Nick == data.Nick);
        bool isMe = NetCorePayload.CurrentAccount != null && NetCorePayload.CurrentAccount.Nick == data.Nick;
        
        removeBtn.SetActive(!isMe && isFriend);
        addBtn.SetActive(!isMe && !isFriend);
        writeBtn.SetActive(!isMe);
    }

    public void OnRemoveBtnClick()
    {
        ui.RemoveFriend(data.Nick);
        NetCorePayload.CurrentAccount.Friends.RemoveAll(c => c.Nick == data.Nick);
        RefreshButtons();
        //Destroy(gameObject);
    }

    public void OnAddBtnClick()
    {
        NetCorePayload.CurrentAccount.Friends.Add(data);
        ui.AddFriend(data.Nick);
        RefreshButtons();
    }

    public void OnViewBtnClick()
    {
        ui.ViewAccount(data);
    }
}
