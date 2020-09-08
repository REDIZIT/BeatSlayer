using Assets.SimpleLocalization;
using UnityEngine;

namespace InGame.Multiplayer.Lobby.Chat.Models
{
    public abstract class LobbyChatMessage
    {
        public string PlayerNick { get; set; }
        public abstract string GetMessage();
    }
    public class LobbyPlayerChatMessage : LobbyChatMessage
    {
        public string Message { get; set; }

        public override string GetMessage()
        {
            return Message;
        }
    }
    public class LobbySystemChatMessage : LobbyChatMessage
    {
        public SystemMessageType MessageType { get; set; }
        public override string GetMessage()
        {
            switch (MessageType)
            {
                case SystemMessageType.Join:
                    return LocalizationManager.Localize("ChatMessageJoin" + Random.Range(0, 3), PlayerNick);
                case SystemMessageType.Leave:
                    return LocalizationManager.Localize("ChatMessageLeave" + Random.Range(0, 3), PlayerNick);
                case SystemMessageType.Kick:
                    return LocalizationManager.Localize("ChatMessageKick" + Random.Range(0, 2), PlayerNick);
                default:
                    return "No localiztion for type " + MessageType;
            }
        }

        public enum SystemMessageType
        {
            Join, Leave, Kick
        }
    }
}
