using GameNet;
using InGame.Helpers;
using InGame.Multiplayer.Lobby;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Multiplayer.Game
{
    public class MpLeaderboardManager : MonoBehaviour
    {
        public Transform container;
        public GameObject itemPrefab;

        public Animator gradientAnimator;


        private List<MpLeaderboardItemPresenter> slots = new List<MpLeaderboardItemPresenter>();
        private MpLeaderboardItemPresenter mySlot;

        private void Start()
        {
            if (LobbyManager.lobby == null) return;

            NetCore.Configure(() =>
            {
                //NetCore.Subs.OnMultiplayerScoreUpdate += OnScoreUpdate;
                // TODO: Configute netcore
            });

            CreateItems();
        }
        private void Update()
        {
            if (Time.timeSinceLevelLoad % 1 == 0)
            {
                if(Time.timeSinceLevelLoad < 5)
                {
                    OnScoreUpdate("REDIZIT", Time.timeSinceLevelLoad, 1);
                }
                else
                {
                    OnScoreUpdate("Tester", Time.timeSinceLevelLoad * 2, 1);
                }
            }
        }

        private void CreateItems()
        {
            HelperUI.RefreshContent(container, LobbyManager.lobby.Players, (MpLeaderboardItemPresenter presenter, LobbyPlayer player) =>
            {
                presenter.Refresh(player);
                slots.Add(presenter);

                if (player == LobbyManager.lobbyPlayer)
                {
                    mySlot = presenter;
                }
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
                slot.currentPlace = i;
            }


            if (mySlot.currentPlace != prevPlace)
            {
                ShowAlertGradient(mySlot.currentPlace, prevPlace);
            }
        }
        private void ShowAlertGradient(int currentPlace, int prevPlace)
        {
            if (currentPlace == prevPlace) return;

            gradientAnimator.transform.position = new Vector3(gradientAnimator.transform.position.x, mySlot.transform.position.y);

            if (currentPlace == 0) gradientAnimator.Play("Top");
            else
            {
                if (currentPlace > prevPlace) gradientAnimator.Play("Down");
                else gradientAnimator.Play("Up");
            }
        }


        private void OnScoreUpdate(string nick, float score, int combo)
        {
            var slot = slots.First(c => c.player.Player.Nick == nick);
            slot.UpdateScore(score, combo);

            ResortItems();
        }
    }
}
