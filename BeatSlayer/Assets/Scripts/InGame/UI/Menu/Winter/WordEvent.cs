using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.UI.Menu.Winter
{
    public class WordEvent
    {
        public List<Word> words = new List<Word>();

        public Word CurrentWord => words.FirstOrDefault(w => w.IsCompleted == false);

        public WordEvent()
        {
            Debug.Log("WordEvent ctor");

            words.Add(new Word("Happy", 1000));
            words.Add(new Word("New", 2500));
            words.Add(new Word("Year", 7000));
        }

        public void TryGiveLetter(WordLetter letter)
        {
            CurrentWord.TryGiveLetter(letter);
        }
    }
}