using GameNet;
using InGame.Multiplayer.Lobby.Chat.Models;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Lobby.Chat
{
    public class LobbyChatUIManager : MonoBehaviour
    {
        public ContentSizeFitter contentSizeFitter;

        public Transform chatContent;
        public GameObject playerMessageItemPrefab;

        public InputField inputField;

        private LobbyChatMessagePresenter prevMessage;


        private void Start()
        {
            NetCore.Configure(() =>
            {
                NetCore.Subs.OnLobbyPlayerMessage += RemotePlayerMessage;
            });
        }


        public void OnSendButtonClick()
        {
            NetCore.ServerActions.Lobby.SendChatMessage(LobbyManager.lobby.Id, new LobbyPlayerChatMessage()
            {
                SenderNick = Payload.Account.Nick,
                Message = inputField.text
            });
            inputField.text = "";
        }


        public void RemotePlayerMessage(LobbyPlayerChatMessage msg)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (prevMessage != null && prevMessage.message.SenderNick == msg.SenderNick)
                {
                    prevMessage.AppendMessage(msg);
                    contentSizeFitter.enabled = false;
                    contentSizeFitter.enabled = true;
                }
                else
                {
                    GameObject obj = Instantiate(playerMessageItemPrefab, chatContent);
                    LobbyChatMessagePresenter presenter = obj.GetComponent<LobbyChatMessagePresenter>();
                    presenter.Refresh(msg);
                    prevMessage = presenter;
                }
            });
            
        }
    }
}
