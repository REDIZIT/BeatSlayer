using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Menu.Winter
{
    public class WordLetterUII : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private Shadow underline;
        [SerializeField] private Image background;

        [SerializeField] private Color lockedColor, unlockedColor;
        [SerializeField] private Color lockedBackground, unlockedBackground;

        private WordLetter letter;

		public void Refresh(WordLetter letter)
        {
            this.letter = letter;
        }

        private void Update()
        {
            text.text = letter.letter.ToString();

            Color color = letter.isCollected ? unlockedColor : lockedColor;

            text.color = color;
            underline.effectColor = color;

            background.color = letter.isCollected ? unlockedBackground : lockedBackground;
        }
    }
}