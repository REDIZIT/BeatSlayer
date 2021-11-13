using Newtonsoft.Json;
using System.Linq;

namespace InGame.UI.Menu.Winter
{
    public class Word
    {
		public WordLetter[] letters;
        public int reward;
        public bool isRewarded;

        [JsonIgnore] public bool IsCompleted => letters.All(l => l.isCollected);

        public Word() { }
        public Word(string word, int reward)
        {
            letters = new WordLetter[word.Length];
            for (int i = 0; i < letters.Length; i++)
            {
                letters[i] = new WordLetter(word[i]);
            }

            this.reward = reward;
        }

        public void TryGiveLetter(WordLetter letter)
        {
            foreach (WordLetter wordLetter in letters)
            {
                if (wordLetter.isCollected == false && wordLetter.letter == letter.letter)
                {
                    wordLetter.isCollected = true;
                    return;
                }
            }
        }
    }
}