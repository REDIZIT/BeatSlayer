using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Menu.Winter
{
    public class WordLetterUII : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private Shadow underline;

        [SerializeField] private Color lockedColor, unlockedColor;

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
        }
    }
}