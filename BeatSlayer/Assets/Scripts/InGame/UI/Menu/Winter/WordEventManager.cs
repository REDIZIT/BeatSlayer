using InGame.Shop;
using InGame.UI.Menu.Winter;
using InGame.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace InGame.UI.Menu
{
    public class WordEventManager
    {
        public WordEvent Event { get; private set; }

        private readonly string filepath = Application.persistentDataPath + "/data/winter_event.json";
        private readonly ShopService shop;

        public WordEventManager(ShopService shop)
        {
            this.shop = shop;
            Load();
        }

        public void Load()
        {
            if (File.Exists(filepath) == false)
            {
                Event = new WordEvent(new List<Word>()
                {
                    new Word("Happy", new List<WordReward>() { new WordCoinsReward(5000) }),
                    new Word("New",  new List<WordReward>() { new WordCoinsReward(12000) }),
                    new Word("Year",  new List<WordReward>()
                    {
                        new WordCoinsReward(20000),
                        new WordPurchaseReward(shop, 16)
                    })
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