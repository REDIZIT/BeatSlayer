using GameNet;
using InGame.Helpers;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Zenject;

namespace InGame.UI.Menu.Winter
{
    public class WordCollectUI : MonoBehaviour
	{
        [SerializeField] private HorizontalScrollSnap scrollSnap;
        [SerializeField] private Button takeRewardButton;
        [SerializeField] private GameObject rewardedLabel;

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
            takeRewardButton.gameObject.SetActive(SelectedWord.isRewarded == false);
            takeRewardButton.interactable = SelectedWord.IsCompleted;
            rewardedLabel.SetActive(SelectedWord.isRewarded);


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

            wordEvent.Save();
        }
    }
}