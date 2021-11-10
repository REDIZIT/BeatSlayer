using InGame.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace InGame.UI.Menu.Winter
{
    public class WordCollectUI : MonoBehaviour
	{
        private WordEvent wordEvent;

        [Inject]
        public void Construct(WordEvent wordEvent, WordContainerUII.Factory factory)
        {
            this.wordEvent = wordEvent;

            foreach (Word word in wordEvent.words)
            {
                factory.Create(word);
            }
        }

        private void Update()
        {
            if (Input.anyKeyDown)
            {
                foreach (WordLetter letter in wordEvent.CurrentWord.letters)
                {
                    if (Input.GetKeyDown(letter.letter.ToString().ToLower()))
                    {
                        wordEvent.TryGiveLetter(letter);
                        break;
                    }
                }
            }
        }
    }


    public class WordEvent
    {
        public List<Word> words = new List<Word>();

        public Word CurrentWord => words.FirstOrDefault(w => w.IsCompleted == false);

        public WordEvent()
        {
            words.Add(new Word("Happy", 1000));
            words.Add(new Word("New", 2500));
            words.Add(new Word("Year", 7000));
        }

        public void TryGiveLetter(WordLetter letter)
        {
            CurrentWord.TryGiveLetter(letter);
        }
    }


	public class Word
    {
		public WordLetter[] letters;
        public int reward;

        public bool IsCompleted => letters.All(l => l.isCollected);

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
	public class WordLetter
    {
		public char letter;
		public bool isCollected;

        public WordLetter() { }
        public WordLetter(char letter)
        {
            this.letter = letter.ToString().ToUpper()[0];
        }
    }
}