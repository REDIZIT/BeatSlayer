using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Game.Winter
{
    public class WordSpinnerLotUII : MonoBehaviour
    {
        [SerializeField] private Transform rewardGroup;
        [SerializeField] private Text letterText, coinsText;
        [SerializeField] private GameObject coinsGroup;
        [SerializeField] private Image outline;

        [SerializeField] private Color defaultColor, selectedColor, wonColor;

        private int index;
        private Image image;
        private RectTransform rect;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            image = GetComponent<Image>();
        }

        public void Init(int index, ISpinnerLot lot)
        {
            this.index = index;
            float angle = -index * 60 - 30;

            rect.eulerAngles = new Vector3(0, 0, angle);
            rect.anchoredPosition = new Vector2(-Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad)) * 40;



            letterText.transform.eulerAngles = Vector3.zero;
            coinsGroup.transform.eulerAngles = Vector3.zero;

            coinsGroup.SetActive(false);
            letterText.text = "";

            switch (lot)
            {
                case SpinnerWordLot wordLot:
                    letterText.text = wordLot.letter.letter.ToString();
                    break;
                case SpinnerCoinsLot coinsLot:
                    coinsText.text = coinsLot.coins.ToString();
                    coinsGroup.SetActive(true);
                    break;
            }
        }
        public void Refresh(float currentIndex)
        {
            float distance = Mathf.Min(currentIndex - index, 6 - (currentIndex - index));

            bool isSelected = Mathf.Abs(distance) <= .5f;

            rect.localScale = Vector3.one * (isSelected ? 1.1f : 1);

            image.color = isSelected ? selectedColor : defaultColor;
            outline.color = Color.clear;
        }
        public void OnWon()
        {
            image.color = wonColor;
            transform.SetAsLastSibling();

            StartCoroutine(IEWonAnimation());
        }
        public void Reset()
        {
            image.color = defaultColor;
        }

        private IEnumerator IEWonAnimation()
        {
            GameObject clone = Instantiate(rewardGroup.gameObject, rewardGroup.transform.parent);
            CanvasGroup group = clone.GetComponent<CanvasGroup>();

            float targetTime = 0.8f;
            float time = targetTime;

            while(time > 0)
            {
                time -= Time.unscaledDeltaTime;

                group.alpha = time / targetTime;
                clone.transform.localScale = 1.1f * Mathf.Lerp(1, 1.5f, 1 - group.alpha) * Vector3.one;

                outline.color = new Color(1, 1, 1, group.alpha);
                outline.transform.localScale = 1.1f * Mathf.Lerp(1, 2.2f, 1 - group.alpha) * Vector3.one;

                yield return null;
            }

            Destroy(clone);
        }
    }
}