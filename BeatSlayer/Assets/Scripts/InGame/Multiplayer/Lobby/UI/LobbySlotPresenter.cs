using InGame.Game.Mods;
using InGame.Game.Scoring.Mods;
using InGame.Helpers;
using InGame.Menu.Mods;
using InGame.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;
using Web;

namespace InGame.Multiplayer.Lobby.UI
{
    public class LobbySlotPresenter : MonoBehaviour
    {
        public Text nickText;
        public Image backgroundImage;
        public GameObject hostIcon;
        public Image readyIndicatorImage;
        public GameObject textsParent;
        public RawImage avatarImage;

        [Header("Downloading")]
        public Text progressText;
        public Slider progressBar;

        [Header("Colors")]
        public Color filledSlotColor, emptySlotColor;
        public Color readyColor, notReadyColor, downloadingColor, waitingColor;

        public Transform modParent;
        public GameObject modItemPrefab;
        public SODB sodb;

        public LobbyPlayer player;

        public void Refresh(LobbyPlayer player)
        {
            this.player = player;

            textsParent.SetActive(true);
            backgroundImage.color = filledSlotColor;


            hostIcon.transform.parent.gameObject.SetActive(player.IsHost);
            WebAPI.GetAvatar(player.Player.Nick, (tex) => avatarImage.texture = tex);
            

            nickText.text = player.Player.Nick;

            progressBar.gameObject.SetActive(false);

            RefreshIndicator();
            RefreshMods();
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
        public void ChangeMods(ModEnum mods)
        {
            player.Mods = mods;
            RefreshMods();
        }
        public void RefreshMods()
        {
            HelperUI.ClearContentAll(modParent);

            foreach (ModSO modSO in sodb.mods)
            {
                if (player.Mods.HasFlag(modSO.modEnum))
                {
                    GameObject obj = Instantiate(modItemPrefab, modParent);
                    obj.GetComponent<ModsBarItem>().Refresh(modSO);
                }
            }
        }
        public void OnStartDownloading()
        {
            progressBar.gameObject.SetActive(true);
            progressBar.value = 0;
            progressText.text = "0%";

            player.State = LobbyPlayer.ReadyState.Downloading;
            RefreshIndicator();
        }
        public void OnDownloadProgress(int percent)
        {
            progressBar.value = percent;
            progressText.text = percent + "%";
        }
        public void OnDownloadComplete()
        {
            progressBar.gameObject.SetActive(false);

            player.State = LobbyPlayer.ReadyState.NotReady;
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
                player.State == LobbyPlayer.ReadyState.WaitingForDownloading ? waitingColor :
                player.State == LobbyPlayer.ReadyState.Ready ? readyColor : notReadyColor;
        }
    }
}
