using CoversManagement;
using GameNet;
using InGame.Multiplayer.Lobby.Chat.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Lobby.Chat
{
    public class LobbyChatMessagePresenter : MonoBehaviour
    {
        public LobbyChatMessage message;

        public Text nickText;
        public Text messageText;
        public TextMeshProUGUI messageMeshText;
        public RawImage avatar;

        public RectTransform rect;
        private RectTransform ownRect;

        public Vector2 padding;


        public Image backgroundImage;

        [Header("Colors")]
        public Color defaultColor;
        public Color pingedColor;

        private bool hasMention;
        private float maxWidth;

        private void Awake()
        {
            maxWidth = rect.sizeDelta.x;
            ownRect = GetComponent<RectTransform>();
        }
        public void Refresh(LobbyChatMessage message)
        {
            this.message = message;


            if (message is LobbyPlayerChatMessage)
            {
                nickText.text = message.PlayerNick;
                CoversManager.AddAvatarPackage(avatar, message.PlayerNick, true);
            }


            string resultText = HightlightHyperlinks(HighlightMentions(message.GetMessage()));

            if (messageMeshText == null) messageText.text = resultText;
            else messageMeshText.text = resultText;



            hasMention = GetMentionIndex(message.GetMessage(), Payload.Account.Nick, 0) != -1;
            RefreshPingedColor(hasMention);

            RebuildSize();
        }
        public void AppendMessage(LobbyPlayerChatMessage message)
        {
            this.message = message;

            rect.sizeDelta = new Vector2(maxWidth, rect.sizeDelta.y);


            string resultText = "\n" + HightlightHyperlinks(HighlightMentions(message.Message)); ;
            if (messageMeshText == null) messageText.text += resultText;
            else messageMeshText.text += resultText;


            if (!hasMention)
            {
                hasMention = GetMentionIndex(message.Message, Payload.Account.Nick, 0) != -1;
            }
            RefreshPingedColor(hasMention);

            RebuildSize();
        }

        public void RebuildSize()
        {
            float nickHeight = nickText == null ? 0 : nickText.preferredHeight;
            float nickWidth = nickText == null ? 0 : nickText.preferredWidth;

            Vector2 messageSize = messageMeshText == null ?
                new Vector2(messageText.preferredWidth, messageText.preferredHeight) :
                new Vector2(messageMeshText.preferredWidth, messageMeshText.preferredHeight);

            float height = nickHeight + messageSize.y + padding.y;
            float width = Mathf.Max(nickWidth, messageSize.x) + padding.x;

            rect.sizeDelta = new Vector2(Mathf.Clamp(width, 0, maxWidth), height);

            ownRect.sizeDelta = new Vector2(ownRect.sizeDelta.x, height);
        }







        private string HightlightHyperlinks(string source)
        {
            string result = source;

            Regex regx = new Regex("((http://|https://|www\\.)([A-Z0-9.-:]{1,})\\.[0-9A-Z?;~&#=\\-_\\./]{2,})", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection matches = regx.Matches(source);
            foreach (Match match in matches)
                result = result.Replace(match.Value, ShortLink(match.Value));

            return result;
        }
        private string ShortLink(string link)
        {
            string text = link;
            int left = 30;
            int right = 10;
            string cut = "...";
            if (link.Length > (left + right + cut.Length))
                text = string.Format("{0}{1}{2}", link.Substring(0, left), cut, link.Substring(link.Length - right, right));
            return string.Format("<#7f7fe5><u><link=\"{0}\">{1}</link></u></color>", link, text);
        }


        private string HighlightMentions(string source)
        {
            List<string> nicks = LobbyManager.lobby.Players.Select(c => c.Player.Nick).ToList();

            string result = source;
            foreach (string nick in nicks)
            {
                result = HightlightMention(result, nick, nick == Payload.Account.Nick);
            }

            return result;
        }
        private string HightlightMention(string source, string nick, bool useBold)
        {
            string result = source;
            string stringToInsert = $"<color=#FFA600>{nick}</color>";
            if (useBold) stringToInsert = "<b>" + stringToInsert + "</b>";


            int currentIndex = 0;
            while (true)
            {
                // Get index of nick start
                int indexOfMyName = GetMentionIndex(result, nick, currentIndex);
                // If no indexes -> return result
                if (indexOfMyName == -1) return result;


                // Cut source text (with any case)
                result = result.Remove(indexOfMyName, nick.Length);

                // Insert colored text in original nick case
                result = result.Insert(indexOfMyName, stringToInsert);


                // Make an offset to skip already highlighted text
                currentIndex = indexOfMyName + stringToInsert.Length;
            }
        }
        private int GetMentionIndex(string source, string nick, int startIndex)
        {
            return source.IndexOf(nick, startIndex, System.StringComparison.OrdinalIgnoreCase);
        }



        private void RefreshPingedColor(bool hasMention)
        {
            backgroundImage.color = hasMention ? pingedColor : defaultColor;
        }
    }
}
