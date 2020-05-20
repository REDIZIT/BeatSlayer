using System.Collections;
using System.Collections.Generic;
using GameNet;
using Microsoft.AspNetCore.SignalR.Client;
using ProjectManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Chat
{
    public class ChatAvatarLoader
    {
        public List<ChatAvatarRequest> ls = new List<ChatAvatarRequest>();
        private ChatAvatarRequest currentRequest;
        
        public Dictionary<string, Texture2D> avatars = new Dictionary<string, Texture2D>();


        public void Configure()
        {
            NetCore.Subs.Accounts_OnGetAvatar += OnGetAvatar;
        }
        
        
        public void Request(RawImage img, string nick)
        {
            ChatAvatarRequest req = new ChatAvatarRequest(img, nick);
            ls.Add(req);

            OnListChange();
        }


        void OnListChange()
        {
            if (ls.Count == 0) return;
            if (currentRequest != null) return;
            
            currentRequest = ls[0];

            if (avatars.ContainsKey(currentRequest.nick))
            { 
                currentRequest.img.texture = avatars[currentRequest.nick];
                
                ls.RemoveAt(0);
                currentRequest = null;

                OnListChange();
            }
            else
            {
                NetCore.ServerActions.Account.GetAvatar(currentRequest.nick);
            }
        }

        public void OnGetAvatar(byte[] bytes)
        {
            Texture2D tex = ProjectManager.LoadTexture(bytes);
            currentRequest.img.texture = tex;
            
            avatars.Add(currentRequest.nick, tex);

            ls.RemoveAt(0);
            currentRequest = null;

            OnListChange();
        }
    }

    public class ChatAvatarRequest
    {
        public RawImage img;
        public string nick;

        public ChatAvatarRequest(RawImage img, string nick)
        {
            this.img = img;
            this.nick = nick;
        }
    }
}