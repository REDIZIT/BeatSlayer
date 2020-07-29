using System;
using System.Collections;
using System.Collections.Generic;
using GameNet;
using ProjectManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Chat
{
    public class ChatAvatarLoader
    {
        List<ChatAvatarRequest> ls = new List<ChatAvatarRequest>();
        private ChatAvatarRequest currentRequest;
        
        public Dictionary<string, Texture2D> avatars = new Dictionary<string, Texture2D>();


        public void Configure()
        {
            NetCore.Subs.Accounts_OnGetAvatar += OnGetAvatar;
        }
        
        
        public void Request(RawImage img, string nick)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                ChatAvatarRequest req = new ChatAvatarRequest(img, nick);
                ls.Add(req);

                OnListChange();
            });
            
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
            if(currentRequest == null) Debug.LogError("WTF UNITY!?");
            
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Texture2D tex = ProjectManager.LoadTexture(bytes);
                if(currentRequest == null) Debug.LogError("WTF UNITY THREADS!?");
                if (currentRequest.img == null) return;
                
                try
                {
                    avatars.Add(currentRequest.nick, tex);
                    ls.RemoveAt(0);
                    
                    currentRequest.img.texture = tex;
                    currentRequest = null;
                    
                    OnListChange();
                }
                catch (Exception e)
                {
                    Debug.Log("Is tex null? " + (tex == null));
                    Debug.Log("Is bytes null? " + (bytes == null || bytes.Length == 0));
                    Debug.Log("Is req null? " + (currentRequest == null));
                    Debug.Log("Is img null? " + (currentRequest.img == null));
                    Debug.Log("Is texture null? " + (currentRequest.img.texture == null));
                    Debug.LogError(e);
                }
                
               
            });
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