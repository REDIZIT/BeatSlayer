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
            playersCountText.text = lobby.Players.Count + "/" + LobbyUIManager.MAX_LOBBY_PLAYERS;

            CoversManager.AddPackage(new CoverRequestPackage(mapCoverImage, lobby.SelectedMap.Trackname, lobby.SelectedMap.Nick, true));
            mapNameText.text = lobby.SelectedMap.Group.Name;
            mapAuthorText.text = lobby.SelectedMap.Group.Author;
        }

        public void OnJoinButtonClick()
        {
            LobbyUIManager.instance.JoinLobby(lobby);
        }
    }
}
