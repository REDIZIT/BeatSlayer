using CoversManagement;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Game
{
    public class MpLeaderboardItemPresenter : MonoBehaviour
    {
        public LobbyPlayer player;

        public Animator animator;

        public int currentPlace;
        public float currentScore;
        public int currentCombo;


        [Header("UI")]
        public Text nickText;
        public Text scoreText, comboText, placeText;
        public RawImage avatarImage;
        public GameObject deadLocker;
       

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

        public void UpdatePlace(int place)
        {
            currentPlace = place;
            placeText.text = "#" + (place + 1);
        }

        public void UpdateAlive(bool isAlive)
        {
            deadLocker.SetActive(!isAlive);
        }

        public void PlayAnimation(string clipName)
        {
            animator.Play(clipName);
        }
    }
}
