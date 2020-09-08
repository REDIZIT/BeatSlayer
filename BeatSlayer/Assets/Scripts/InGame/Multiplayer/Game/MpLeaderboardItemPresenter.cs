using CoversManagement;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Game
{
    public class MpLeaderboardItemPresenter : MonoBehaviour
    {
        public LobbyPlayer player;

        public int currentPlace;
        public float currentScore;
        public int currentCombo;


        [Header("UI")]
        public Text nickText;
        public Text scoreText, comboText;
        public RawImage avatarImage;

       

        public void Refresh(LobbyPlayer player)
        {
            this.player = player;

            nickText.text = player.Player.Nick;
            UpdateScore(0, 0);

            CoversManager.AddAvatarPackage(avatarImage, player.Player.Nick, true);
        }

        public void UpdateScore(float score, int combo)
        {
            scoreText.text = Mathf.RoundToInt(score).ToString();
            comboText.text = "x" + combo.ToString();

            currentScore = score;
            currentCombo = combo;
        }
    }
}
