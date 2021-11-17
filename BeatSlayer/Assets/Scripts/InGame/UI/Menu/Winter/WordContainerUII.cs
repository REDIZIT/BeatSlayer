using InGame.Helpers;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace InGame.UI.Menu.Winter
{
    public class WordContainerUII : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private WordLetterUII prefab;

        [SerializeField] private Text rewardText, numberText;
        [SerializeField] private GameObject saberRewardGroup;

        private WordEventManager wordEvent;

        [Inject]
        public void Construct(WordEventManager wordEvent)
        {
            this.wordEvent = wordEvent;
        }

        public void Refresh(Word word)
        {
            HelperUI.UpdateContent(content, prefab, word.letters, (uii, m) => { uii.Refresh(m); });
            numberText.text = (wordEvent.Event.words.IndexOf(word) + 1) + "/" + wordEvent.Event.words.Count;

            saberRewardGroup.SetActive(false);

            foreach (WordReward reward in word.rewards)
            {
                switch (reward)
                {
                    case WordCoinsReward coins:
                        RefreshCoins(coins);
                        break;
                    case WordPurchaseReward purchase:
                        RefreshSaber(purchase);
                        break;
                    default:
                        throw new System.Exception($"Can't define type of word reward ({reward.GetType()})");
                }
            }
        }

        private void RefreshCoins(WordCoinsReward reward)
        {
            rewardText.text = reward.ToString();
        }
        private void RefreshSaber(WordPurchaseReward reward)
        {
            saberRewardGroup.SetActive(true);
        }

        public class Factory : PlaceholderFactory<WordContainerUII>
        {

        }
    }
}