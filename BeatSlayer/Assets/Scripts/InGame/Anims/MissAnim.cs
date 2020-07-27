using Assets.SimpleLocalization;
using InGame.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Animations
{
    public class MissAnim : MonoBehaviour
    {
        public Text missText;
        public Animator animator;

        /// <summary>
        /// Is animation ends and misses counting from zero
        /// </summary>
        public bool isClear;

        public int consistentMisses;

        public string localizedText;


        private void Start()
        {
            localizedText = LocalizationManager.Localize("Miss");
        }
        public void OnMiss()
        {
            if (SettingsManager.Settings.Gameplay.HideMissedText) return;

            animator.Play("Missed", -1, 0f);

            consistentMisses++;

            if(consistentMisses == 1)
            {
                missText.text = localizedText + "!";
            }
            else
            {
                missText.text = localizedText + " x" + consistentMisses + "!";
            }
        }

        public void OnAnimationEnd()
        {
            isClear = true;
            consistentMisses = 0;
            missText.text = "";
        }
    }
}
