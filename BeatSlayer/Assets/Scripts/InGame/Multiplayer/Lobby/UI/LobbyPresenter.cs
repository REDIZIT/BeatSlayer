using Assets.SimpleLocalization;
using CoversManagement;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Lobby.UI
{
    public class LobbyPresenter : MonoBehaviour
    {
        public Text nameText;
        public Text playersCountText;

        public RawImage mapCoverImage;
        public Text mapNameText, mapAuthorText;

        private Lobby lobby;

        public void Refresh(Lobby lobby)
        {
            this.lobby = lobby;

            nameText.text = lobby.Name;
            playersCountText.text = lobby.Players.Count + "/" + LobbyManager.MAX_LOBBY_PLAYERS;

            
            if (lobby.IsHostChangingMap)
            {
                Debug.Log("Changing map");
                mapNameText.text = LocalizationManager.Localize("HostChangingMap");
                mapAuthorText.text = "";
                return;
            }
            if (lobby.SelectedMap == null)
            {
                mapNameText.text = LocalizationManager.Localize("NoMapSetYet");
                mapAuthorText.text = "";
                return;
            }

            CoversManager.AddPackage(new CoverRequestPackage(mapCoverImage, lobby.SelectedMap.author + "-" + lobby.SelectedMap.name, lobby.SelectedMap.nick, true));
            mapNameText.text = lobby.SelectedMap.name;
            mapAuthorText.text = lobby.SelectedMap.author;
        }

        public void OnJoinButtonClick()
        {
            LobbyUIManager.instance.JoinLobby(lobby);
        }
    }
}
