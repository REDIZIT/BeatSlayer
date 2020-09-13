using BeatSlayerServer.Dtos.Mapping;
using GameNet;
using InGame.Game;
using InGame.Helpers;
using InGame.Multiplayer.Game.Leaderboard;
using InGame.Multiplayer.Lobby;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Multiplayer.Game
{
    public class MpLeaderboardManager : MonoBehaviour
    {
        public ScoringManager sm;

        [Header("Finish leaderboard")]
        public GameObject globalMapLeaderboard;
        public GameObject multiplayerMapLeaderboard;

        [Header("Runtime leaderboard")]
        public Transform container;
        public GameObject itemPrefab, myItemPrefab;

        [Header("Result leaderboard")]
        public Transform resultContainer;
        public GameObject resultItemPrefab;

        public Animator gradientAnimator;


        private List<MpLeaderboardItemPresenter> slots = new List<MpLeaderboardItemPresenter>();
        private List<MpResultLeaderboardPresenter> resultSlots = new List<MpResultLeaderboardPresenter>();
        private MpLeaderboardItemPresenter mySlot;
        private float scoreUpdateTimer;

        private void Start()
        {
            globalMapLeaderboard.SetActive(LobbyManager.lobby == null);
            multiplayerMapLeaderboard.SetActive(LobbyManager.lobby != null);

            enabled = LobbyManager.lobby != null;
            if (LobbyManager.lobby == null) return;

            NetCore.Configure(() =>
            {
                NetCore.Subs.OnMultiplayerScoreUpdate += OnScoreUpdate;
                NetCore.Subs.OnMultiplayerPlayerFinished += RemotePlayerFinished;
                NetCore.Subs.OnMultiplayerPlayerAliveChanged += RemotePlayerAliveChanged;
                NetCore.Subs.OnMultiplayerPlayerLeft += RemotePlayerLeft;
            });

            CreateItems();
            CreateResultItems();
        }
        private void Update()
        {
            if (scoreUpdateTimer > 0)
            {
                scoreUpdateTimer -= Time.deltaTime;
            }
            else
            {
                scoreUpdateTimer = 1;
                OnScoreUpdate(Payload.Account.Nick, sm.Replay.Score, Mathf.FloorToInt(sm.comboMultiplier));
                NetCore.ServerActions.Multiplayer.ScoreUpdate(LobbyManager.lobby.Id, Payload.Account.Nick, sm.Replay.Score, Mathf.FloorToInt(sm.comboMultiplier));
            }
        }



        private void CreateItems()
        {
            HelperUI.AddContent(container, myItemPrefab, (MpLeaderboardItemPresenter item) =>
            {
                item.Refresh(LobbyManager.lobbyPlayer);
                slots.Add(item);
                mySlot = item;
                item.UpdatePlace(0);
            });
            int i = 0;
            HelperUI.FillContent(container, itemPrefab, LobbyManager.lobby.Players.Where(c => c != LobbyManager.lobbyPlayer), (MpLeaderboardItemPresenter presenter, LobbyPlayer player) =>
            {
                i++;
                presenter.Refresh(player);
                slots.Add(presenter);
                presenter.UpdatePlace(i);

                if (player == LobbyManager.lobbyPlayer)
                {
                    mySlot = presenter;
                }
            });
        }
        private void CreateResultItems()
        {
            HelperUI.FillContent(resultContainer, resultItemPrefab, LobbyManager.lobby.Players, (MpResultLeaderboardPresenter presenter, LobbyPlayer player) =>
            {
                presenter.RefreshAndWaitForReplay(player);
                resultSlots.Add(presenter);
            });
        }





        private void ResortItems()
        {
            int prevPlace = mySlot.currentPlace;

            IEnumerable<MpLeaderboardItemPresenter> sortedSlots = slots.OrderByDescending(c => c.currentScore);

            int i = -1;
            foreach (var slot in sortedSlots)
            {
                i++;
                // Move slot in the container end
                // As result, all slots will be sorted as currentScore is
                slot.transform.SetAsLastSibling();
                slot.UpdatePlace(i);
            }


            if (mySlot.currentPlace != prevPlace)
            {
                ShowAlertGradient(mySlot.currentPlace, prevPlace);
            }
        }
        private void ShowAlertGradient(int currentPlace, int prevPlace)
        {
            if (currentPlace == prevPlace) return;

            //gradientAnimator.transform.position = new Vector3(gradientAnimator.transform.position.x, mySlot.transform.position.y);
            gradientAnimator.transform.position = mySlot.transform.position;
            //RectTransformUtility.WorldToScreenPoint(null, gradientAnimator.transform.position);

            if (currentPlace == 0)
            {
                //Debug.Log("Top");
                mySlot.PlayAnimation("Top");
                gradientAnimator.Play("Top");
            }
            else
            {
                if (currentPlace > prevPlace)
                {
                    //Debug.Log("Down");
                    mySlot.PlayAnimation("Down");
                    gradientAnimator.Play("Down");
                }
                else
                {
                    //Debug.Log("Up");
                    mySlot.PlayAnimation("Up");
                    gradientAnimator.Play("Up");
                }
            }
        }






        private void OnScoreUpdate(string nick, float score, int combo)
        {
            var slot = slots.First(c => c.player.Player.Nick == nick);
            slot.UpdateScore(score, combo);

            ResortItems();
        }

        private void RemotePlayerAliveChanged(string nick, bool isAlive)
        {
            slots.First(c => c.player.Player.Nick == nick).UpdateAlive(isAlive);
        }
        private void RemotePlayerLeft(string nick)
        {
            slots.First(c => c.player.Player.Nick == nick).OnLeft();
        }
        private void RemotePlayerFinished(string nick, ReplayData replay)
        {
            resultSlots.Find(c => c.player.Player.Nick == nick).RefreshReplay(replay);
        }
    }
}
