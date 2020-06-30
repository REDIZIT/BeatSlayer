using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using Assets.SimpleLocalization;
using GameNet;
using UnityEngine;
using InGame.Helpers;
using Newtonsoft.Json;
using ProjectManagement;
using UnityEngine.UI;
using BeatSlayerServer.Multiplayer.Accounts;

namespace Multiplayer.Chat
{
    public class ChatUI : MonoBehaviour
    {
        public ChatAvatarLoader avatarLoader;
        /// <summary>
        /// Count of message player didn't check
        /// </summary>
        public static int UnviewedMessagesCount;

        // Hub methods
        public const string method_leaveGroup = "Chat_LeaveGroup";

        
        List<ChatMessage> chatMessages = new List<ChatMessage>();
        public string selectedGroupName;
        
        
        [Header("UI")]
        public Transform content;
        GameObject prefab;
        public CustomInputField field;
        public Text onlineText, groupText;

        public GameObject newMessagesCount;
        public Text newMessagesCountText;

        public Transform groupContent;

        
        public void Configuration()
        {
            NetCore.Subs.OnSendChatMessage += OnSendChatMessage;
            NetCore.Subs.OnJoinGroup += OnJoinGroup;
            NetCore.Subs.OnGetGroups += OnGetGroups;


            // On connected or logged in
            // If not logged => return and show message "Not logged in"
            // Get groups
            // Set default group if not set
            // Get selected group chat history
            // Set text "Connected"
            // Unlock inputfield
            // Resubscribe if needed


            // On connection lost
            // Set text "connection lost"
            // Lock inputfield



            NetCore.OnConnect += () =>
            {
                OnConnectedOrLoggedIn();
            };
            NetCore.OnLogIn += () =>
            {
                OnConnectedOrLoggedIn();
            };
            NetCore.OnFullReady += () =>
            {
                OnConnectedOrLoggedIn();
            };

            NetCore.Subs.OnOnlineChange += (int online) =>
            {
                if(onlineText != null) onlineText.text = LocalizationManager.Localize("Online") + ": " + online;
            };

            onlineText.text = "";

            avatarLoader.Configure();
        }

        private void Start()
        {
            prefab = HelperUI.ClearContent(content);
            prefab.SetActive(false);
            field.onEndEdit = OnSendBtnClicked;
            avatarLoader = new ChatAvatarLoader();

            SetUnreadMessagesCount(UnviewedMessagesCount);
        }






        private void OnConnectedOrLoggedIn()
        {
            Debug.Log("OnConnectedOrLoggedIn");

            if (Payload.CurrentAccount == null)
            {
                onlineText.text = "You should be logged in";
                return;
            }

            NetCore.ServerActions.Chat.GetGroups();
            onlineText.text = "Connected";
        }
        


        public void OnConnectionLost()
        {
            onlineText.text = "Connection lost";
        }
        
        
        public void OnSendBtnClicked()
        {
            if (field.text.Trim() == "") return;

            NetCore.ServerActions.SendChatMessage(Payload.CurrentAccount.Nick, field.text, Payload.CurrentAccount.Role, selectedGroupName);
            field.text = "";
        }

        public void JoinGroup(ChatGroupData data)
        {
            if (Payload.CurrentAccount == null) return;
            
            NetCore.ServerActions.Chat.LeaveGroup(Payload.CurrentAccount.Nick, selectedGroupName);
            NetCore.ServerActions.Chat.JoinGroup(Payload.CurrentAccount.Nick, data.Name);
            selectedGroupName = data.Name;
            groupText.text = data.Name;
        }
        
        
        
        
        
        
        public void OnSendChatMessage(string json)
        {
            Debug.Log("Got message");

            ChatMessage msg = JsonConvert.DeserializeObject<ChatMessage>(json);

            if(content == null || !content.gameObject.activeInHierarchy)
            {
                UnviewedMessagesCount++;
                SetUnreadMessagesCount(UnviewedMessagesCount);
            }

            if(content != null)
            {
                HelperUI.AddContent<ChatMessageItem>(content, prefab, item =>
                {
                    item.message = msg;
                    item.Refresh();

                    avatarLoader.Request(item.image, item.message.nick);
                });
            }
            
            chatMessages.Add(msg);
        }

        public void OnGetGroups(List<ChatGroupData> groups)
        {
            Debug.Log("Got groups");

            if(selectedGroupName == "")
            {
                selectedGroupName = groups[0].Name;
            }
            groupText.text = selectedGroupName;

            NetCore.ServerActions.Chat.JoinGroup(Payload.CurrentAccount.Nick, selectedGroupName);
            
            HelperUI.FillContent<ChatGroupItemUI, ChatGroupData>(groupContent, groups, (ui, data) =>
            {
                ui.Refresh(data);
            });
        }

        public void OnJoinGroup(string json)
        {
            Debug.Log("OnJoinGroup");

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



        public void OnShowChatBtnClick()
        {
            UnviewedMessagesCount = 0;
            SetUnreadMessagesCount(0);
        }


        private void SetUnreadMessagesCount(int count)
        {
            if (newMessagesCount == null) return;

            newMessagesCount.SetActive(count != 0);
            newMessagesCountText.text = count.ToString();
        }
    }
    
    public class ChatMessage
    {
        public string nick, message;
        public AccountRole role;
        public string groupName;
    }
}
