using GameNet;
using InGame.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Leaderboard
{
    /// <summary>
    /// Global players leaderboard
    /// </summary>
    public class PlayerLeaderboardUI : MonoBehaviour
    {
        public Transform content;
        public Text leaderboardText;


        private void FillLeaderboard(List<LeaderboardItem> leaderboardItems)
        {
            int place = 0;
            HelperUI.FillContent<LeaderboardUIItem, LeaderboardItem>(content, leaderboardItems, (item, leaderboardItem) =>
            {
                place++;
                item.Refresh(leaderboardItem, place, leaderboardItem.Nick == Payload.Account?.Nick);
            });
        }


        public async Task LoadLeaderboard()
        {
            leaderboardText.text = "Loading..";

            var items = await NetCore.ServerActions.Account.GetGlobalLeaderboard();

            leaderboardText.text = "";

            FillLeaderboard(items);
        }

        public async void OnShowBtnClick()
        {
            await LoadLeaderboard();
        }
    }
}