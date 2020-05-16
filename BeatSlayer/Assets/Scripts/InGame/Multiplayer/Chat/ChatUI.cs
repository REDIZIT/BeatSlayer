using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using UnityEngine;
using InGame.Helpers;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using ProjectManagement;
using UnityEngine.UI;

namespace Multiplayer.Chat
{
    public class ChatUI : MonoBehaviour
    {
        public ChatAvatarLoader avatarLoader;

        // Hub methods
        public const string method_leaveGroup = "Chat_LeaveGroup";

        
        List<ChatMessage> chatMessages = new List<ChatMessage>();
        string selectedGroupName;
        
        
        [Header("UI")]
        public Transform content;
        GameObject prefab;
        public CustomInputField field;
        public Text onlineText, groupText;

        public Transform groupContent;




        public void Configure()
        {
            NetCore.Subs.OnJoinGroup += OnJoinGroup;
            NetCore.Subs.OnGetGroups += OnGetGroups;
        }
        
        private void Start()
        {
            prefab = HelperUI.ClearContent(content);
            prefab.SetActive(false);
            field.onEndEdit = OnSendBtnClicked;
            avatarLoader = new ChatAvatarLoader();
        }
        

        public void OnConnect()
        {
            //MultiplayerCore.conn.InvokeAsync(method_getGroups);
            NetCore.ServerActions.Chat.GetGroups();
            //onlineText.text = "Connected";
        }

        public void OnConnectionLost()
        {
            onlineText.text = "Connection lost";
        }

        
        
        
        public void OnSendBtnClicked()
        {
            if (field.text.Trim() == "") return;
            //if (MultiplayerCore.account == null) return;

            //NetCore.ServerActions.SendChatMessage(MultiplayerCore.account.Nick, field.text, MultiplayerCore.account.Role, selectedGroupName);
            NetCore.ServerActions.SendChatMessage("REDIZIT", field.text, BeatSlayerServer.Multiplayer.Accounts.AccountRole.Developer, "Global");
            field.text = "";
        }
        
        
        
        
        public void OnSendChatMessage(string json)
        {
            Debug.Log(json);
            ChatMessage msg = JsonConvert.DeserializeObject<ChatMessage>(json);
            
            HelperUI.AddContent<ChatMessageItem>(content, prefab, item =>
            {
                item.message = msg;
                item.Refresh();
 
                avatarLoader.Request(item.image, item.message.nick);
            });
            
            chatMessages.Add(msg);
        }

        public void OnGetGroups(string json)
        {
            Debug.Log(json);
            List<string> groups = JsonConvert.DeserializeObject<List<string>>(json);
            selectedGroupName = groups[0];
            groupText.text = selectedGroupName;

            Debug.Log("Try to join " + selectedGroupName + " group");
            NetCore.ServerActions.Chat.JoinGroup(NetCorePayload.CurrentAccount.Nick, selectedGroupName);

            //groupDropdown.ClearOptions();
            //groupDropdown.AddOptions(groups);

        }

        public void OnJoinGroup(string json)
        {
            Debug.Log("OnJoinGroup " + json);
            List<ChatMessage> msgs = JsonConvert.DeserializeObject<List<ChatMessage>>(json);
            
            HelperUI.FillContent<ChatMessageItem, ChatMessage>(content, msgs, (item, message) =>
            {
                item.message = message;
                item.Refresh();
                
                avatarLoader.Request(item.image, item.message.nick);
            });
        }
        
        
        public void OnGetAvatar(byte[] bytes)
        {
            avatarLoader.OnGetAvatar(bytes);
        }

        public void OnOnlineChange(int count)
        {
            onlineText.text = "Online: " + count;
        }
    }
    
    public class ChatMessage
    {
        public string nick, message;
        public AccountRole role;
        public string groupName;
    }
}
