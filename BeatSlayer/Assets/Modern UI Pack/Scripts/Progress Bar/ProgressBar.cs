using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Michsky.UI.ModernUIPack
{
    public class ProgressBar : MonoBehaviour
    {
        [Header("OBJECTS")]
        public Transform loadingBar;
        public Transform textPercent;

        [Header("VARIABLES (IN-GAME)")]
        public bool isOn;
        public bool restart;

        public float CurrentPercent
        {
            get { return currentPercent; }
            set { currentPercent = value; Update(); }
        }
        [Range(0, 100)] public float currentPercent;

        [Range(0, 100)] public int speed;

        [Header("SPECIFIED PERCENT")]
        public bool enableSpecified;
        public bool enableLoop;
        [Range(0, 100)] public float specifiedValue;

        void Update()
        {
            if (CurrentPercent <= 100 && isOn == true && enableSpecified == false)
                CurrentPercent += speed * Time.deltaTime;

            if (CurrentPercent <= 100 && isOn == true && enableSpecified == true)
            {
                if (CurrentPercent <= specifiedValue)
                    CurrentPercent += speed * Time.deltaTime;

                if (enableLoop == true && CurrentPercent >= specifiedValue)
                    CurrentPercent = 0;
            }

            if (CurrentPercent > 100 || CurrentPercent >= 100 && restart == true)
                CurrentPercent = 0;

            if (enableSpecified == true && specifiedValue == 0)
                CurrentPercent = 0;

            loadingBar.GetComponent<Image>().fillAmount = CurrentPercent / 100;
            textPercent.GetComponent<TextMeshProUGUI>().text = ((int)CurrentPercent).ToString("F0") + "%";
        }
    }
}