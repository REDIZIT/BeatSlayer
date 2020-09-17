using Assets.SimpleLocalization;
using GameNet;
using InGame.Multiplayer.Lobby;
using System.Collections;
using System.Threading.Tasks;
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

        [Header("UI")]
        public GameObject restartBtn;
        public GameObject leaderboardToDisable;
        public Button restartButton, restartGameOverBtn;

        public bool isGameStarted;


        private void Awake()
        {
            enabled = LobbyManager.lobby != null;

            if (LobbyManager.lobby == null) return;

            // Stop automatic game starting after loading
            // We will start game manually further
            gm.StartGameAuto = false;

            restartBtn.SetActive(false);
            restartButton.interactable = false;
            restartGameOverBtn.interactable = false;
        }
        private void Start()
        {
            if (LobbyManager.lobby == null) return;

            FinishHandler.instance.OnFinishEvent += OnLocalPlayerFinished;
            NetCore.Configure(() =>
            {
                NetCore.Subs.OnMultiplayerPlayersLoaded += OnAllPlayersLoaded;
            });

            // Notify server that we are loaded
            NetCore.ServerActions.Multiplayer.OnLoaded(LobbyManager.lobby.Id, Payload.Account.Nick);

            CheckAllPlayersStatus();
        }

        private void CheckAllPlayersStatus()
        {
            Task.Run(async () =>
            {
                bool areAllLoaded = await NetCore.ServerActions.Multiplayer.AreAllLoaded(LobbyManager.lobby.Id);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    if (areAllLoaded)
                    {
                        Debug.Log("Start game coz all are loaded");
                        isGameStarted = true;
                        gm.StartGame();
                    }
                    else
                    {
                        // Enabling overlay for waiting other players loaded
                        waitingOverlay.SetActive(true);
                        waitingAnimator.Play("FadeOpen");
                        waitingText.text = LocalizationManager.Localize("WaitingForPlayersLoaded");
                    }
                });
            });
        }

        private void OnAllPlayersLoaded()
        {
            StartCoroutine(IEOnAllPlayersLoaded());
        }

        private IEnumerator IEOnAllPlayersLoaded()
        {
            // If someone isn't loaded when we're
            if (!isGameStarted)
            {
                waitingAnimator.Play("FadeClose");

                yield return new WaitForSeconds(2);

                // If someone is still not loaded
                if (!isGameStarted)
                {
                    Debug.Log("Start game coz IEOnAllPlayersLoaded");
                    isGameStarted = true;
                    gm.StartGame();
                }
            }
        }


        private void OnLocalPlayerFinished()
        {
            leaderboardToDisable.SetActive(false);
        }
    }
}
