using Assets.SimpleLocalization;
using CoversManagement;
using InGame.Helpers;
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
        public GameObject lockerImage;

        public Transform playersAvatarsContainer;

        private Lobby lobby;

        public void Refresh(Lobby lobby)
        {
            this.lobby = lobby;

            nameText.text = lobby.Name;
            playersCountText.text = lobby.Players.Count + "/" + LobbyManager.MAX_LOBBY_PLAYERS;
            lockerImage.SetActive(lobby.HasPassword);

            RefreshAvatarsContainer();


            if (lobby.IsHostChangingMap)
            {
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

            CoversManager.AddPackage(new CoverRequestPackage(mapCoverImage, lobby.SelectedMap.Trackname, lobby.SelectedMap.MapperNick, true));
            mapNameText.text = lobby.SelectedMap.Name;
            mapAuthorText.text = lobby.SelectedMap.Author;
        }

        public void OnJoinButtonClick()
        {
            if (lobby.HasPassword)
            {
                LobbyPasswordUIManager.instance.ShowPasswordLocker(lobby, () => LobbyUIManager.instance.JoinLobby(lobby));
            }
            else
            {
                LobbyUIManager.instance.JoinLobby(lobby);
            }
        }


        private void RefreshAvatarsContainer()
        {
            GameObject prefab = HelperUI.ClearContent(playersAvatarsContainer);

            HelperUI.FillContent<RawImage, LobbyPlayer>(playersAvatarsContainer, prefab, lobby.Players, (rawImage, player) =>
            {
                CoversManager.AddAvatarPackage(rawImage, player.Player.Nick, true);
            });
        }
    }
}
