using InGame.Helpers;
using UnityEngine;
using Zenject;

namespace InGame.UI.Menu.Winter
{
    public class WordCollectUI : MonoBehaviour
	{
        private WordEventManager wordEvent;

        [Inject]
        public void Construct(WordEventManager wordEvent, WordContainerUII.Factory factory)
        {
            this.wordEvent = wordEvent;

            foreach (Word word in wordEvent.Event.words)
            {
                factory.Create(word);
            }
        }

        private void Update()
        {
            if (Input.anyKeyDown)
            {
                foreach (WordLetter letter in wordEvent.Event.CurrentWord.letters)
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
}