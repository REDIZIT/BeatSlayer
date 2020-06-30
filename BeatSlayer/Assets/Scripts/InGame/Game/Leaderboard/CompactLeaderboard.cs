using GameNet;
using InGame.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Leaderboard
{
    /// <summary>
    /// Used on maps end overlays
    /// </summary>
    public class CompactLeaderboard : MonoBehaviour
    {
        public Transform content;
        [SerializeField] private Text leaderboardText;


        public async Task LoadLeaderboard(string trackname, string nick)
        {
            leaderboardText.text = "Loading..";

            var items = await NetCore.ServerActions.Account.GetMapLeaderboard(trackname, nick);

            leaderboardText.text = "";

            FillLeaderboard(items, nick);
        }

        public void SetStatus(string statusText)
        {
            leaderboardText.text = statusText;
        }



        private void FillLeaderboard(List<LeaderboardItem> leaderboardItems, string mapperNick)
        {
            int place = 0;
            HelperUI.FillContent<CompactLeaderboardItem, LeaderboardItem>(content, leaderboardItems, (item, leaderboardItem) =>
            {
                place++;
                CompactLeaderboardItem.PlayerType type = CompactLeaderboardItem.PlayerType.Player;

                if(Payload.CurrentAccount != null && Payload.CurrentAccount.Nick == leaderboardItem.Nick)
                {
                    type = CompactLeaderboardItem.PlayerType.CurrentPlayer;
                }

                if(leaderboardItem.Nick == mapperNick)
                {
                    type = CompactLeaderboardItem.PlayerType.Mapper;
                }

                item.Refresh(leaderboardItem, place, type);
            });
        }
    }
}
