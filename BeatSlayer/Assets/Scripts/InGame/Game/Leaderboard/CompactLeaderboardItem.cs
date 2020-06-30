using Assets.SimpleLocalization;
using Ranking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace InGame.Leaderboard
{
    /// <summary>
    /// Used on maps end overlays and controlled by <see cref="CompactLeaderboard"/>
    /// </summary>
    public class CompactLeaderboardItem : MonoBehaviour
    {
        public LeaderboardItem leaderboardItem;

        public Text placeText, nickText, accuracyText, missedText, playedTimesText, RPText, totalRPText;
        public Text scoreText;
        public Text gradeText;
        public UICornerCut cornerCut;

        public enum PlayerType { Player, CurrentPlayer, Mapper }
        public PlayerType type;



        [Header("Colors")]
        public Color32 defaultBodyColor;
        public Color32 defaultBorderColor;
        public Color32 defaultTextColor;

        public Color32 selectedBodyColor, selectedBorderColor;
        public Color32 selectedTextColor;

        public void Refresh(LeaderboardItem item, int place, PlayerType type)
        {
            placeText.text = "#" + place;
            nickText.text = (type == PlayerType.Mapper ? $"<color=#888>({LocalizationManager.Localize("Author")})</color> " : "") + item.Nick;

            float roundedAccuray = Mathf.FloorToInt(item.Accuracy * 1000f) / 10f;
            accuracyText.text = roundedAccuray + "%";
            ColorizeText(accuracyText, type);

            missedText.text = item.MissedCount.ToString();
            ColorizeText(missedText, type);

            float RP = Mathf.FloorToInt((float)item.RP * 10f) / 10f;
            RPText.text = RP.ToString();

            scoreText.text = Mathf.FloorToInt((float)item.Score).ToString();

            cornerCut.color = type == PlayerType.CurrentPlayer ? selectedBodyColor : defaultBodyColor;
            cornerCut.ColorDown = type == PlayerType.CurrentPlayer ? selectedBorderColor : defaultBorderColor;

            gradeText.text = item.Grade == Grade.Unknown ? "-" : item.Grade.ToString();
            ColorizeText(gradeText, type);
        }
        private void ColorizeText(Text text, PlayerType type)
        {
            text.color = type == PlayerType.CurrentPlayer ? selectedTextColor : defaultTextColor;
        }
    }
}
