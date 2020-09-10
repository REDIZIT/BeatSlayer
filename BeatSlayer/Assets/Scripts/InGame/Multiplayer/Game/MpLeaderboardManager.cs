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
        public GameObject itemPrefab, myItemPrefab;

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
            if (Mathf.RoundToInt(Time.time) % 2 == 0)
            {
                if (Time.timeSinceLevelLoad < 5)
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
    }
}
