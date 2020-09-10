using Assets.SimpleLocalization;
using GameNet;
using InGame.Multiplayer.Lobby;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Game
{
    public class GameLobbyUIManager : MonoBehaviour
    {
        public GameManager gm;

        [Header("Waiting")]
        public Animator waitingAnimator;
        public GameObject waitingOverlay;
        public Text waitingText;


        private void Awake()
        {
            enabled = LobbyManager.lobby != null;

            if (LobbyManager.lobby == null) return;

            // Stop automatic game starting after loading
            // We will start game manually further
            gm.StartGameAuto = false;
        }
        private void Start()
        {
            if (LobbyManager.lobby == null) return;

            // Enabling overlay for waiting other players loaded
            waitingOverlay.SetActive(true);
            waitingAnimator.Play("FadeOpen");
            waitingText.text = LocalizationManager.Localize("WaitingForPlayersLoaded");

            NetCore.Configure(() =>
            {
                NetCore.Subs.OnMultiplayerPlayersLoaded += OnAllPlayersLoaded;
            });

            // Notify server that we are loaded
            NetCore.ServerActions.Multiplayer.OnLoaded(LobbyManager.lobby.Id, Payload.Account.Nick);
        }


        private void OnAllPlayersLoaded()
        {
            StartCoroutine(IEOnAllPlayersLoaded());
        }

        private IEnumerator IEOnAllPlayersLoaded()
        {
            waitingAnimator.Play("FadeClose");

            yield return new WaitForSeconds(2);

            gm.StartGame();
        }
    }
}
