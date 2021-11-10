using InGame.Helpers;
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

        [Inject]
        public void Construct(WordEventManager wordEvent, Word word)
        {
            HelperUI.UpdateContent(content, prefab, word.letters, (uii, m) => { uii.Refresh(m); });
            rewardText.text = word.reward.ToString();

            numberText.text = (wordEvent.Event.words.IndexOf(word) + 1) + "/" + wordEvent.Event.words.Count;
        }

        public class Factory : PlaceholderFactory<Word, WordContainerUII>
        {

        }
    }
}