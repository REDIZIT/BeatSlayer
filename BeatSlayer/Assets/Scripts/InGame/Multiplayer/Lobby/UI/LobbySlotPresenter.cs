using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Lobby.UI
{
    public class LobbySlotPresenter : MonoBehaviour
    {
        public Text nickText;
        public Image backgroundImage;
        public GameObject hostIcon;
        public Image readyIndicatorImage;
        public GameObject textsParent;

        public Color filledSlotColor, emptySlotColor;
        public Color readyColor, notReadyColor, downloadingColor;

        public LobbyPlayer player;

        public void Refresh(LobbyPlayer player)
        {
            this.player = player;

            textsParent.SetActive(true);
            backgroundImage.color = filledSlotColor;


            hostIcon.SetActive(player.IsHost);

            nickText.text = player.Player.Nick;
            RectTransform nickRect = nickText.GetComponent<RectTransform>();
            nickRect.offsetMin = player.IsHost ? new Vector2(80, nickRect.offsetMin.y) : new Vector2(26, nickRect.offsetMin.y);

            RefreshIndicator();
        }

        public void Clear()
        {
            player = null;
            textsParent.SetActive(false);
            backgroundImage.color = emptySlotColor;
        }


        public void ChangeState(LobbyPlayer.ReadyState state)
        {
            player.State = state;
            RefreshIndicator();
        }


        public void OnMoreButtonClick()
        {
            LobbyActionsLocker.instance.Show(player.Player, LobbyUIManager.instance.AmIHost());
        }



        private void RefreshIndicator()
        {
            readyIndicatorImage.color =
                player.State == LobbyPlayer.ReadyState.Downloading ? downloadingColor :
                player.State == LobbyPlayer.ReadyState.Ready ? readyColor : notReadyColor;
        }
    }
}
