using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace LeaderboardManagement
{
    public static class LeaderboardManager
    {
        public const string url_leaderboard = "http://www.bsserver.tk/Account/GetGlobalLeaderboard";

        public static void GetLeaderboard(Action<List<LeaderboardItem>> callback)
        {
            WebClient c = new WebClient();
            c.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
            {
                if (!e.Cancelled && e.Error != null) throw e.Error;
                callback(JsonConvert.DeserializeObject<List<LeaderboardItem>>(e.Result));
            };

            c.DownloadStringAsync(new Uri(url_leaderboard));
        }
    }

    public class LeaderboardItem
    {
        public string nick;
        public int place;
        public int playCount;
        public int slicedCount, missedCount;
        public float Accuracy { get { return slicedCount / (float)(slicedCount + missedCount); } }
        public double RP, score;
    }
}