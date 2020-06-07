using BeatSlayerServer.Multiplayer.Accounts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Multiplayer.Chat
{
    public class ChatMessageItem : MonoBehaviour
    {
        public ChatMessage message;

        public Text nickText, messageText;
        public TextMeshProUGUI messageTextPro;
        public RectTransform content;
        public RawImage image;
        
        
        public void Refresh()
        {
            string colorHex = message.role == AccountRole.Moderator ? "f40" : message.role == AccountRole.Developer ? "07f" : "fff";
            string roleStr = $"  <color=#{colorHex}>{message.role.ToString()}</color>";
            
            nickText.text = message.nick + (message.role != AccountRole.Player ? roleStr : "");
            //messageText.text = message.message;
            messageTextPro.text = message.message;

            float height = messageTextPro.preferredHeight + 50;
            content.sizeDelta = new Vector2(0, height < 90 ? 90 : height);
        }
    }

}