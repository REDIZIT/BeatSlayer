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
        }

        public void ClickTakeReward()
        {
            if (SelectedWord.IsCompleted == false || SelectedWord.isRewarded) return;

            SelectedWord.ApplyRewards();

            wordEvent.Save();
        }
    }
}