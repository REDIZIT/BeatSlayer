using BeatSlayerServer.Dtos.Mapping;
using Ranking;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Game.Leaderboard
{
    public class MpResultLeaderboardPresenter : MonoBehaviour
    {
        public ReplayData replay;
        public LobbyPlayer player;

        public Text nickText;
        public Text rankText, scoreText;

        public void RefreshAndWaitForReplay(LobbyPlayer player)
        {
            this.player = player;

            nickText.text = player.Player.Nick;
            rankText.text = "-";
            scoreText.text = "Playing";
        }
        public void RefreshReplay(ReplayData replay) 
        {
            this.replay = replay;

            scoreText.text = GetScoreString(replay.Score, replay.Accuracy);

            if(replay.Grade == Grade.Unknown)
            {
                rankText.text = "-";
                rankText.color = Color.gray;
            }
            else
            {
                rankText.text = replay.Grade.ToString();
                rankText.color = replay.Grade == Grade.Unknown ? FinishHandler.instance.gradeColors[0] : FinishHandler.instance.gradeColors[(int)replay.Grade];
            }
        }

        private string GetScoreString(float score, float accuracy)
        {
            float roundScore = Mathf.FloorToInt(score);
            float roundAccuracy = Mathf.FloorToInt(accuracy * 100 * 10) / 10f;

            return $"{roundScore}   {roundAccuracy}%";
        }
    }
}
