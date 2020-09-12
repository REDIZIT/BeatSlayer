using Assets.SimpleLocalization;
using GameNet;
using InGame.Helpers;
using InGame.Multiplayer.Lobby.Chat.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Lobby.Chat
{
    public class LobbyChatUIManager : MonoBehaviour
    {
        public ContentSizeFitter contentSizeFitter;

        public Transform chatContent;
        public GameObject playerMessageItemPrefab, systemMessageItemPrefab;

        public InputField inputField;

        [Header("Typing status")]
        public GameObject typingBar;
        public Text typingText;
        public Animator typingAnimator;



        private LobbyChatMessagePresenter prevMessage;
        private List<string> typingPlayersNicks = new List<string>();
        private bool isTyping;
        private float typingCancelTimer;

        private static List<LobbyChatMessage> chatHistory = new List<LobbyChatMessage>();


        private void Start()
        {
            if (LobbyManager.lobby != null)
            {
                RestoreChat();
            }

            NetCore.Configure(() =>
            {
                NetCore.Subs.OnLobbyPlayerMessage += RemotePlayerMessage;
                NetCore.Subs.OnLobbySystemMessage += RemotePlayerMessage;
                NetCore.Subs.OnLobbyPlayerStartTyping += RemotePlayerStartTyping;
                NetCore.Subs.OnLobbyPlayerStopTyping += RemotePlayerStopTyping;
            });
        }
        private void Update()
        {
            if (LobbyManager.lobby == null) return;

            if (typingCancelTimer > 0)
            {
                typingCancelTimer -= Time.deltaTime;
            }
            else
            {
                isTyping = false;
                NetCore.ServerActions.Lobby.StopTyping(LobbyManager.lobby.Id, Payload.Account.Nick);
            }
        }



        public void RefreshTypingStatus()
        {
            if (typingBar == null) return;

            if(typingPlayersNicks.Count > 0)
            {
                typingBar.SetActive(true);
                typingAnimator.Play(0);
                typingText.text = GetTypingText();
            }
            else
            {
                typingBar.SetActive(false);
            }
        }


        public void ClearChat()
        {
            HelperUI.ClearContentAll(chatContent);
        }

        #region Send button and Input field events

        public void OnSendButtonClick()
        {
            NetCore.ServerActions.Lobby.SendChatMessage(LobbyManager.lobby.Id, new LobbyPlayerChatMessage()
            {
                PlayerNick = Payload.Account.Nick,
                Message = inputField.text
            });
            inputField.text = "";
        }

        public void OnInputFieldTextChange()
        {
            if (LobbyManager.lobby == null) return;

            if(inputField.text == "")
            {
                if (isTyping)
                {
                    isTyping = false;
                    NetCore.ServerActions.Lobby.StopTyping(LobbyManager.lobby.Id, Payload.Account.Nick);
                }
            }
            else
            {
                typingCancelTimer = 5;
                if (!isTyping)
                {
                    isTyping = true;
                    NetCore.ServerActions.Lobby.StartTyping(LobbyManager.lobby.Id, Payload.Account.Nick);
                }
            }
        }

        #endregion





        private void RemotePlayerMessage(LobbyChatMessage msg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                RemotePlayerStopTyping(msg.PlayerNick);
                AppendChatMessage(msg);
                chatHistory.Add(msg);
            });
        }
        private void RestoreChat()
        {
            foreach (var message in chatHistory.Take(30))
            {
                AppendChatMessage(message);
            }
        }

        private void RemotePlayerStartTyping(string nick)
        {
            typingPlayersNicks.Add(nick);
            RefreshTypingStatus();
        }
        private void RemotePlayerStopTyping(string nick)
        {
            typingPlayersNicks.Remove(nick);
            RefreshTypingStatus();
        }


        private void AppendChatMessage(LobbyChatMessage msg)
        {
            bool isPlayerMessage = msg is LobbyPlayerChatMessage;
            bool isPrevPlayerMessage = prevMessage != null && prevMessage.message is LobbyPlayerChatMessage;

            if (prevMessage != null && isPlayerMessage && isPrevPlayerMessage && prevMessage.message.PlayerNick == msg.PlayerNick)
            {
                prevMessage.AppendMessage(msg as LobbyPlayerChatMessage);
                contentSizeFitter.enabled = false;
                contentSizeFitter.enabled = true;
            }
            else
            {
                GameObject obj = Instantiate(isPlayerMessage ? playerMessageItemPrefab : systemMessageItemPrefab, chatContent);
                LobbyChatMessagePresenter presenter = obj.GetComponent<LobbyChatMessagePresenter>();
                presenter.Refresh(msg);
                prevMessage = presenter;
            }
        }




        private string GetTypingText()
        {
            if (typingPlayersNicks.Count == 0) return "";

            if (typingPlayersNicks.Count == 1)
            {
                return LocalizationManager.Localize("TypingOne", typingPlayersNicks[0]);
            }
            if (typingPlayersNicks.Count == 2)
            {
                return LocalizationManager.Localize("TypingTwo", typingPlayersNicks[0], typingPlayersNicks[1]);
            }
            if (typingPlayersNicks.Count == 3)
            {
                return LocalizationManager.Localize("TypingThree", typingPlayersNicks[0], typingPlayersNicks[1], typingPlayersNicks[2]);
            }

            return LocalizationManager.Localize("TypingMore", typingPlayersNicks[0], typingPlayersNicks[1], typingPlayersNicks[2], typingPlayersNicks.Count - 3);
        }
    }
}
