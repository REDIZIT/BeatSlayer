using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace InGame.UI.Menu.Winter
{
    public class WordCollectButton : MonoBehaviour
    {
        [SerializeField] private Text wordText;

        private WordEvent wordEvent;
        private Word prevWord;

        [Inject]
        private void Construct(WordEventManager wordEventManager) 
        {
            wordEvent = wordEventManager.Event;
        }

        private void Update()
        {
            if (wordEvent.CurrentWord != prevWord)
            {
                prevWord = wordEvent.CurrentWord;
                wordText.text = string.Concat(wordEvent.CurrentWord.letters.Select(l => l.letter));
            }
        }
    }
}