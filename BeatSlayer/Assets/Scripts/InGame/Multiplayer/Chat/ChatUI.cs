using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
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
        public MultiplayerCore core;
        public ChatAvatarLoader avatarLoader;

        // Hub methods
        public const string method_getGroups = "Chat_GetGroups"; 
        public const string method_joinGroup = "Chat_JoinGroup";
        public const string method_leaveGroup = "Chat_LeaveGroup";
        public const string method_sendMessage = "Chat_SendMessage";
        
        
        List<ChatMessage> chatMessages = new List<ChatMessage>();
        string selectedGroupName;
        
        
        [Header("UI")]
        public Transform content;
        GameObject prefab;
        public CustomInputField field;
        public Text onlineText, groupText;

        public Transform groupContent;


        
        
        private void Start()
        {
            prefab = HelperUI.ClearContent(content);
            prefab.SetActive(false);
            field.onEndEdit = OnSendBtnClicked;
            avatarLoader = new ChatAvatarLoader(core);
        }
        

        public void OnConnect()
        {
            core.conn.InvokeAsync(method_getGroups);
            onlineText.text = "Connected";
        }

        public void OnConnectionLost()
        {
            onlineText.text = "Connection lost";
        }

        
        
        
        public void OnSendBtnClicked()
        {
            if (field.text.Trim() == "") return;
            if (AccountManager.LegacyAccount == null) return;
            
            Debug.Log("OnSendBtnClicked() Connection state: " + core.conn.State);
            
            core.conn.InvokeAsync(method_sendMessage,AccountManager.LegacyAccount.nick, field.text, AccountManager.LegacyAccount.Role, selectedGroupName);
            field.text = "";
        }
        
        
        
        
        public void OnSendChatMessage(string json)
        {
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

            core.conn.InvokeAsync(method_joinGroup, AccountManager.LegacyAccount.nick, selectedGroupName);
            
            //groupDropdown.ClearOptions();
            //groupDropdown.AddOptions(groups);
            
        }

        public void OnJoinGroup(string json)
        {
            List<ChatMessage> msgs = JsonConvert.DeserializeObject<List<ChatMessage>>(json);
            //List<ChatMessage> msg = JsonConvert.DeserializeObject<List<ChatMessage>>(json);
            HelperUI.FillContent<ChatMessageItem, ChatMessage>(content, msgs, (item, message) =>
            {
                item.message = message;
                item.Refresh();
                
                avatarLoader.Request(item.image, item.message.nick);
            });
        }

        public void JoinGroup(string groupName)
        {
            core.conn.InvokeAsync(method_leaveGroup, AccountManager.LegacyAccount.nick, selectedGroupName);
            selectedGroupName = groupName;
            groupText.text = selectedGroupName;
            core.conn.InvokeAsync(method_joinGroup, AccountManager.LegacyAccount.nick, selectedGroupName);
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
