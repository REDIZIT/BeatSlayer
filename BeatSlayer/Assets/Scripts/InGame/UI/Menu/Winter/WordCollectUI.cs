using GameNet;
using InGame.Helpers;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Zenject;

namespace InGame.UI.Menu.Winter
{
    public class WordCollectUI : MonoBehaviour
	{
        [SerializeField] private HorizontalScrollSnap scrollSnap;
        [SerializeField] private GameObject takeRewardButton;

        private WordEventManager wordEvent;

        private Word SelectedWord => wordEvent.Event.words[scrollSnap.CurrentPage];

        [Inject]
        public void Construct(WordEventManager wordEvent, WordContainerUII.Factory factory)
        {
            this.wordEvent = wordEvent;

            foreach (Word word in wordEvent.Event.words)
            {
                factory.Create().Refresh(word);
            }
        }

        private void Update()
        {
            takeRewardButton.SetActive(wordEvent.Event.words[scrollSnap.CurrentPage].IsCompleted && SelectedWord.isRewarded == false);

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

        public void ClickTakeReward()
        {
            if (SelectedWord.IsCompleted == false || SelectedWord.isRewarded) return;

            Payload.Account.AddCoins(SelectedWord.reward);
            SelectedWord.isRewarded = true;
        }
    }
}