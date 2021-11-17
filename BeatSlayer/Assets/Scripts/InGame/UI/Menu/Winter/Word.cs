using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace InGame.UI.Menu.Winter
{
    public class Word
    {
		public WordLetter[] letters;
        public List<WordReward> rewards;
        public bool isRewarded;

        [JsonIgnore] public bool IsCompleted => letters.All(l => l.isCollected);

        public Word() { }
        public Word(string word, List<WordReward> rewards)
        {
            letters = new WordLetter[word.Length];
            for (int i = 0; i < letters.Length; i++)
            {
                letters[i] = new WordLetter(word[i]);
            }

            this.rewards = rewards;
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
        public void ApplyRewards()
        {
            if (isRewarded || IsCompleted == false) return;

            foreach (WordReward reward in rewards)
            {
                reward.Apply();
            }

            isRewarded = true;
        }
    }
}