using System.Linq;
using Assets.SimpleLocalization;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
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
        //activeText.text = data.IsOnline ? LocalizationManager.Localize("Online") : string.Format(LocalizationManager.Localize("WasOnline"), data.LastActiveTimeUtc.ToLocalTime().ToShortTimeString());
        activeText.text = data.IsOnline ? LocalizationManager.Localize("Online") : 
            string.Format(LocalizationManager.Localize("WasOnline"), data.LastActiveTimeUtc.ToLocalTime().ToString("d/M/yyyy HH:mm"));

        WebAPI.GetAvatar(data.Nick, (Texture2D tex) =>
        {
            avatarImage.texture = tex;
        });
        
        RefreshButtons();
    }

    void RefreshButtons()
    {
        bool isFriend = Payload.CurrentAccount != null && Payload.CurrentAccount.Friends.Any(c => c.Nick == data.Nick);
        bool isMe = Payload.CurrentAccount != null && Payload.CurrentAccount.Nick == data.Nick;
        
        removeBtn.SetActive(!isMe && isFriend);
        addBtn.SetActive(!isMe && !isFriend);
        writeBtn.SetActive(!isMe);
    }

    public void OnRemoveBtnClick()
    {
        ui.RemoveFriend(data.Nick);
        Payload.CurrentAccount.Friends.RemoveAll(c => c.Nick == data.Nick);
        RefreshButtons();
        //Destroy(gameObject);
    }

    public void OnAddBtnClick()
    {
        Payload.CurrentAccount.Friends.Add(data);
        ui.AddFriend(data.Nick);
        RefreshButtons();
    }

    public void OnViewBtnClick()
    {
        ui.ViewAccount(data);
    }
}
