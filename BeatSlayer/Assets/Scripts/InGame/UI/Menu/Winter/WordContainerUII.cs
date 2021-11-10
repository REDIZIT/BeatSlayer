using InGame.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Menu.Winter
{
    public class WordContainerUII : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private WordLetterUII prefab;

        [SerializeField] private Text rewardText, numberText;

        public void Refresh(Word word)
        {
            HelperUI.UpdateContent(content, prefab, word.letters, (uii, m) => { uii.Refresh(m); });
            rewardText.text = word.reward.ToString();
        }
    }
}