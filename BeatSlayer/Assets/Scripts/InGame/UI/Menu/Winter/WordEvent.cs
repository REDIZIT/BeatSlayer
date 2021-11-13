using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.UI.Menu.Winter
{
    public class WordEvent
    {
        public List<Word> words = new List<Word>();

        [JsonIgnore] public Word CurrentWord => words.FirstOrDefault(w => w.IsCompleted == false);

        public WordEvent()
        {

        }
        public WordEvent(List<Word> words)
        {
            this.words = words;
        }
        public void TryGiveLetter(WordLetter letter)
        {
            CurrentWord.TryGiveLetter(letter);
        }
    }
}