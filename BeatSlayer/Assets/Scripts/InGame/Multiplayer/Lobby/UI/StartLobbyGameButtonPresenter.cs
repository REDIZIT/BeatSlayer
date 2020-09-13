using Assets.SimpleLocalization;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Lobby.UI
{
    public class StartLobbyGameButtonPresenter : MonoBehaviour
    {
        public Text mainText;
        public Text subText;
        public Button btn;

        public void RefreshAsStart()
        {
            mainText.text = LocalizationManager.Localize("Start");
            subText.text = LocalizationManager.Localize("AllPlayersReady");
            btn.interactable = true;
        }
        public void RefreshAsForce(int notReadyPlayersCount)
        {
            mainText.text = LocalizationManager.Localize("ForceStart");
            subText.text = LocalizationManager.Localize("NotReadyPlayers", notReadyPlayersCount);
            btn.interactable = true;
        }
        public void RefreshAsMapNotSet()
        {
            mainText.text = LocalizationManager.Localize("Start");
            subText.text = LocalizationManager.Localize("NoMapSetYet");
            btn.interactable = false;
        }
        public void RefreshAsPlaying()
        {
            mainText.text = LocalizationManager.Localize("Start");
            subText.text = LocalizationManager.Localize("GameIsNotFinished");
            btn.interactable = false;
        }
    }
}
