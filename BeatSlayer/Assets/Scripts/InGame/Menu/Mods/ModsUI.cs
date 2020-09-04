using InGame.Game.Scoring.Mods;
using InGame.Helpers;
using InGame.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Menu.Mods
{
    public class ModsUI : MonoBehaviour
    {
        public List<ModUIItem> modsButtons;
        public List<ModSO> selectedMods;
        public SODB sodb;
        
        [Header("UI")]
        public GameObject locker;
        public Text scoreBonusText, rpBonusText;

        [Header("Hints UI")]
        public Transform hintsContent;


        private void Start()
        {
            HelperUI.FillContent<ModHintItem, ModSO>(hintsContent, sodb.mods, (item, data) =>
            {
                item.Refresh(data);
            });
        }
        public void Open()
        {
            locker.SetActive(true);

            foreach (ModUIItem item in modsButtons)
            {
                item.Refresh(selectedMods.Contains(item.mod));
            }
        }
        public void Close()
        {
            locker.SetActive(false);
        }

        public void OnModsChanged()
        {
            float scoreMultiplier = 1;
            float rpMultiplier = 1;

            selectedMods.Clear();
            foreach (ModUIItem item in modsButtons)
            {
                if (item.isSelected)
                {
                    selectedMods.Add(item.mod);
                    scoreMultiplier *= item.mod.scoreMultiplier;
                    rpMultiplier *= item.mod.rpMultiplier;
                }
            }

            scoreMultiplier = Mathf.RoundToInt(scoreMultiplier * 100) / 100f;
            rpMultiplier = Mathf.RoundToInt(rpMultiplier * 100) / 100f;

            scoreBonusText.text = $"Score: <b>x{(scoreMultiplier == 1 ? "1.0" : scoreMultiplier.ToString())}</b>";
            rpBonusText.text = $"RP: <b>x{(rpMultiplier == 1 ? "1.0" : rpMultiplier.ToString())}</b>";



            ModEnum selectedModEnum = ModEnum.None;
            foreach (var mod in selectedMods)
            {
                selectedModEnum = mod.ApplyEnum(selectedModEnum);
            }
            Debug.Log("Mods are " + selectedModEnum);
        }
    }
}
