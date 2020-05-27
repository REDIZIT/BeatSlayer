using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
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
        public string selectedGroupName;
        
        
        [Header("UI")]
        public Transform content;
        GameObject prefab;
        public CustomInputField field;
        public Text onlineText, groupText;

        public Transform groupContent;

        
        private void Awake()
        {
            NetCore.Configurators += () =>
            {
                NetCore.Subs.OnSendChatMessage += OnSendChatMessage;
                NetCore.Subs.OnJoinGroup += OnJoinGroup;
                NetCore.Subs.OnGetGroups += OnGetGroups;
                NetCore.OnFullReady += () =>
                {
                    if (NetCorePayload.CurrentAccount != null) NetCore.ServerActions.Chat.GetGroups();
                };
                NetCore.OnLogIn += () =>
                {
                    NetCore.ServerActions.Chat.GetGroups();
                };
                NetCore.Subs.OnOnlineChange += (int online) =>
                {
                    onlineText.text = "Онлайн: " + online;
                };
                
                avatarLoader.Configure();
            };
        }

        private void Start()
        {
            prefab = HelperUI.ClearContent(content);
            prefab.SetActive(false);
            field.onEndEdit = OnSendBtnClicked;
            avatarLoader = new ChatAvatarLoader();
        }
        
        public void OnConnectionLost()
        {
            onlineText.text = "Connection lost";
        }
        
        
        public void OnSendBtnClicked()
        {
            if (field.text.Trim() == "") return;

            NetCore.ServerActions.SendChatMessage(NetCorePayload.CurrentAccount.Nick, field.text, NetCorePayload.CurrentAccount.Role, selectedGroupName);
            field.text = "";
        }

        public void JoinGroup(ChatGroupData data)
        {
            if (NetCorePayload.CurrentAccount == null) return;
            
            NetCore.ServerActions.Chat.JoinGroup(NetCorePayload.CurrentAccount.Nick, data.Name);
            selectedGroupName = data.Name;
            groupText.text = data.Name;
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

        public void OnGetGroups(List<ChatGroupData> groups)
        {
            selectedGroupName = groups[0].Name;
            groupText.text = selectedGroupName;

            NetCore.ServerActions.Chat.JoinGroup(NetCorePayload.CurrentAccount.Nick, selectedGroupName);
            
            HelperUI.FillContent<ChatGroupItemUI, ChatGroupData>(groupContent, groups, (ui, data) =>
            {
                ui.Refresh(data);
            });
        }

        public void OnJoinGroup(string json)
        {
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
