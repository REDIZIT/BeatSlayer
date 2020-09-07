using InGame.Multiplayer.Lobby.Chat.Models;
using Multiplayer.Chat;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Lobby.Chat
{
    public class LobbyChatMessagePresenter : MonoBehaviour
    {
        public LobbyPlayerChatMessage message;

        public Text nickText;
        public Text messageText;

        public RectTransform rect;

        public VerticalLayoutGroup[] verticalLayoutGroups;
        public HorizontalLayoutGroup[] horizontalLayoutGroups;

        private float maxWidth;

        private void Awake()
        {
            maxWidth = rect.sizeDelta.x;
        }
        public void Refresh(LobbyPlayerChatMessage message)
        {
            this.message = message;

            nickText.text = message.SenderNick;
            messageText.text = message.Message;

            RebuildSize();
        }
        public void AppendMessage(LobbyPlayerChatMessage message)
        {
            this.message = message;

            rect.sizeDelta = new Vector2(maxWidth, rect.sizeDelta.y);

            messageText.text += "\n" + message.Message;
            RebuildSize();
        }

        public void RebuildSize()
        {
            float height = nickText.preferredHeight + messageText.preferredHeight + 25;
            float width = 120 + Mathf.Max(nickText.preferredWidth, messageText.preferredWidth) + 50;

            rect.sizeDelta = new Vector2(Mathf.Clamp(width, 0, maxWidth), height);
        }
    }
}
