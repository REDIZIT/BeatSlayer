using System.Collections.Generic;


namespace LeaderboardManagement
{
    public static class LeaderboardManager
    {
        public static List<LeaderboardRecord> GetLeaderboard()
        {
            return new List<LeaderboardRecord>()
            {
                new LeaderboardRecord() { nick = "Tester", accuracy = 50, place = 1, playedTimes = 20, RP = 270, totalRP = 570}
            };
        }
    }

    public class LeaderboardRecord
    {
        public int place;
        public string nick;
        public int sliced, missed;
        public float accuracy, playedTimes, RP, totalRP;
    }
}