using InGame.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace InGame.UI.Menu.Winter
{
    public class WordEventManager
    {
        public WordEvent Event { get; private set; }

        private readonly string filepath = Application.persistentDataPath + "/data/winter_event.json";

        public WordEventManager()
        {
            Load();
        }

        public void Load()
        {
            if (File.Exists(filepath) == false)
            {
                Event = new WordEvent(new List<Word>()
                {
                    new Word("Happy", 5000),
                    new Word("New", 12000),
                    new Word("Year", 20000)
                });
            }
            else
            {
                Event = FileLoader.LoadJson<WordEvent>(filepath);
            }
        }

        public void Save()
        {
            FileLoader.SaveJson(filepath, Event);
        }

        public void TryGiveLetter(WordLetter letter)
        {
            Event.TryGiveLetter(letter);
            Save();
        }
    }
}