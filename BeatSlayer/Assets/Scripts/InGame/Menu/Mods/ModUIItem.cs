using UnityEngine;
using UnityEngine.UI;

namespace InGame.Menu.Mods
{
    public class ModUIItem : MonoBehaviour
    {
        public ModsUI ui;
        public ModUIItem incompatibleModItem;
        public Button button;

        [Header("UI")]
        public Image iconImage;
        public GameObject checkmark;
        public Text modNameText, labelText, subLabelText;

        public ModSO mod;

        public bool isSelected;


        public void Refresh(bool isSelected, bool isInteractable)
        {
            button.interactable = isInteractable;

            this.isSelected = isSelected;
            RefreshUI();
        }
        private void RefreshUI()
        {
            checkmark.SetActive(isSelected);
            iconImage.transform.localScale = isSelected ? Vector3.one * 1.15f : Vector3.one;
            iconImage.transform.eulerAngles = isSelected ? new Vector3(0, 0, -3.5f) : Vector3.zero;

            iconImage.color = mod.modPillowColor;
            modNameText.text = mod.shortname;
            labelText.text = mod.name;
            subLabelText.text = mod.sublabel;
        }

        public void OnClick()
        {
            isSelected = !isSelected;
            if (incompatibleModItem != null && isSelected)
            {
                incompatibleModItem.isSelected = !isSelected;
                incompatibleModItem.RefreshUI();
            }

            RefreshUI();

            ui.OnModsChanged();
        }
    }
}
