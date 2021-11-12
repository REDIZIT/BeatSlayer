using GameNet;
using InGame.UI.Menu.Winter;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace InGame.UI.Game.Winter
{
    public class WordSpinner : MonoBehaviour
	{
		[SerializeField] private RectTransform arrow;
        [SerializeField] private WordContainerUII wordContainer;
        [SerializeField] private float speed = 10;
        [SerializeField] private Transform backgroundContent;

        [SerializeField] private AudioClip audioClip;

        private WordSpinnerLotUII[] lotsUII;

        private bool isSpinning;
        private float currentSpin, targetSpin;
        private ISpinnerLot[] lots;

        private float DegreesPerPart => 360 / (float)PARTS_COUNT;

        private const float MAX_SPEED = 0.4f;
        private const int PARTS_COUNT = 6;

        private WordEventManager wordEvent;
        private SpinnerWordLot.Factory spinnerLotFactory;
        private bool isInited;


        [Inject]
        private void Construct(WordEventManager wordEvent, SpinnerWordLot.Factory spinnerLotFactory)
        {
            this.wordEvent = wordEvent;
            this.spinnerLotFactory = spinnerLotFactory;
        }

        private void Start()
        {
            Init();
        }
        private void Update()
        {
            if (isSpinning == false)
            {
                //if (Input.GetKeyDown(KeyCode.Space))
                //{
                //    Spin();
                //}
                return;
            }

            currentSpin += Mathf.Min(MAX_SPEED, (targetSpin - currentSpin) / speed);
            arrow.eulerAngles = new Vector3(0, 0, -Mathf.Repeat(DegreesPerPart / 2f + currentSpin * DegreesPerPart, 360));

            for (int i = 0; i < PARTS_COUNT; i++)
            {
                lotsUII[i].Refresh(currentSpin % PARTS_COUNT);
            }


            if (targetSpin - currentSpin <= 0.02f)
            {
                isSpinning = false;
                int index = (int)Mathf.Repeat(Mathf.RoundToInt(targetSpin), lots.Length);

                ISpinnerLot lot = lots[index];
                lotsUII[index].OnWon();

                lot.ApplyAward();
            }

            wordContainer.Refresh(wordEvent.Event.CurrentWord);
        }

        private void Init()
        {
            if (isInited) return;

            SetupSet();
            InitContent();
            isInited = true;
        }
        private void SetupSet()
        {
            lots = new ISpinnerLot[PARTS_COUNT];

            WordLetter[] letters = GetRandomLetters(wordEvent.Event.CurrentWord);
            int randomLettersIndex = 0;

            int coinsRewardIndex = 0;


            for (int i = 0; i < PARTS_COUNT; i++)
            {
                if (i % 2 == 0)
                {
                    WordLetter letter = letters[randomLettersIndex];
                    randomLettersIndex++;

                    lots[i] = spinnerLotFactory.Create(letter);
                }
                else
                {
                    lots[i] = new SpinnerCoinsLot(200 + coinsRewardIndex * coinsRewardIndex * 350);
                    coinsRewardIndex++;
                }
            }
        }
        public void Spin()
        {
            Init();
            if (isSpinning) return;
            isSpinning = true;

            currentSpin = 0;

            float letterChance = 0.75f;
            if (Random.value <= letterChance)
            {
                float noise = Random.value - 0.5f;
                targetSpin = Random.Range(0, 6) * 2 + noise + PARTS_COUNT * 5;
            }
            else
            {
                float noise = Random.value - 0.5f;
                targetSpin = 1 + Random.Range(0, 6) * 2 + noise + PARTS_COUNT * 5;
            }


            foreach (WordSpinnerLotUII item in lotsUII)
            {
                item.Reset();
            }
        }

        private void InitContent()
        {
            lotsUII = new WordSpinnerLotUII[PARTS_COUNT];
            GameObject prefab = backgroundContent.GetChild(0).gameObject;
            prefab.SetActive(true);

            for (int i = 0; i < PARTS_COUNT; i++)
            {
                WordSpinnerLotUII child = Instantiate(prefab, backgroundContent).GetComponent<WordSpinnerLotUII>();
                child.Init(i, lots[i]);
                lotsUII[i] = child;
            }

            prefab.SetActive(false);
        }
        private WordLetter[] GetRandomLetters(Word word)
        {
            WordLetter[] letters = new WordLetter[3];

            // Take letters, which player didn't collect yet
            IEnumerable<WordLetter> notHaveLetters = word.letters.Where(l => l.isCollected == false);

            for (int i = 0; i < 3; i++)
            {
                // Take letters, which didn't used in letters array
                IEnumerable<WordLetter> otherLetters = notHaveLetters.Where(l => letters.Contains(l) == false);

                // If no other letters (when notHaveLetters.Count() < letters.Length)
                if (otherLetters.Count() == 0)
                {
                    // Return any letter which player don't have yet (will repeat)
                    letters[i] = notHaveLetters.Random();
                }
                else
                {
                    // Return unique letter
                    letters[i] = otherLetters.Random();
                }
            }

            return letters;
        }
    }

    public interface ISpinnerLot
    {
        void ApplyAward();
    }

    public class SpinnerCoinsLot : ISpinnerLot
    {
        public int coins;

        public SpinnerCoinsLot(int coins)
        {
            this.coins = coins;
        }

        public void ApplyAward()
        {
            Payload.Account.Coins += coins;
            NetCore.ServerActions.Shop.SendCoins(Payload.Account.Nick, coins);
        }
    }
    public class SpinnerWordLot : ISpinnerLot
    {
        public WordLetter letter;

        private WordEventManager wordEvent;

        [Inject]
        public SpinnerWordLot(WordEventManager wordEvent, WordLetter letter)
        {
            this.wordEvent = wordEvent;
            this.letter = letter;
        }

        public void ApplyAward()
        {
            wordEvent.TryGiveLetter(letter);
        }

        public class Factory : PlaceholderFactory<WordLetter, SpinnerWordLot> { }
    }
}