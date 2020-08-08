using InGame.Game.Scoring.Mods;
using Ranking;

namespace InGame.Leaderboard
{
    public class LeaderboardItem
    {
        public string Nick { get; set; }
        public int Place { get; set; }
        public int PlayCount { get; set; }
        public int SlicedCount { get; set; }
        public int MissedCount { get; set; }

        /// <summary>
        /// Range from 0 to 1
        /// </summary>
        public float Accuracy { get { return SlicedCount / (float)(SlicedCount + MissedCount); } }
        public double RP { get; set; }
        public double Score { get; set; }
        public Grade Grade { get; set; }
        public ModEnum Mods { get; set; }
    }
}