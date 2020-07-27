using Assets.SimpleLocalization;
using InGame.Game.Menu;
using InGame.Menu.Settings;
using Michsky.UI.ModernUIPack;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Settings
{
    public class SettingsUIItem : MonoBehaviour
    {
        protected SettingsUI ui;
        public SettingsUIItemModel model;

        public GameObject questionBtn;
        public Text nameText;
        public Image backgroundImage;

        [Header("Value controllers")]
        public CustomDropdown dropdown;
        public Slider slider;
        public SwitchManager toggle;

        [Header("Colors")]
        public Color disabledTextColor;
        public Color enabledBackgroundColor, disabledBackgroundColor;


        public virtual void Refresh(SettingsUIItemModel model, SettingsUI ui, Transform dropdownlocker)
        {
            this.model = model;
            this.ui = ui;

            dropdown.listParent = dropdownlocker;

            Refresh();

            if (dropdown != null) dropdown.GetComponent<Button>().interactable = model.Enabled;
            if (slider != null) slider.interactable = model.Enabled;
            if (toggle != null) toggle.GetComponent<Button>().interactable = model.Enabled;

            nameText.color = model.Enabled ? Color.white : disabledTextColor;
            backgroundImage.color = model.Enabled ? enabledBackgroundColor : disabledBackgroundColor;
        }

        public void Refresh()
        {
            nameText.text = LocalizationManager.Localize(model.NameWithoutLocalization);
            if (!string.IsNullOrWhiteSpace(model.Description))
            {
                nameText.text += $" <color=#777><size=26>({LocalizationManager.Localize(model.Description)})</size></color>";
            }


            bool hasMedia = model.Media != null && model.Media.Length > 0;
            questionBtn.SetActive(hasMedia);
            nameText.GetComponent<RectTransform>().offsetMin = new Vector2(hasMedia ? 120 : 45, 0);


            ResetValueContainers();
            switch (model.Type)
            {
                case SettingsUIItemType.Dropdown: RefreshDropdown(); break;
                case SettingsUIItemType.Slider: RefreshSlider(); break;
                case SettingsUIItemType.Toggle: RefreshSwitch(); break;
            }
        }

        public void OnHintBtnClick()
        {
            SettingsMediaViewer.instance.Show(model.Media);
        }


        private void ResetValueContainers()
        {
            dropdown.gameObject.SetActive(false);
            slider.gameObject.SetActive(false);
            toggle.gameObject.SetActive(false);
        }
        private void RefreshDropdown()
        {
            dropdown.gameObject.SetActive(true);
            dropdown.OnValueChanged += (i) => OnValueChange(i);

            FillDropdown((SettingsDropdownValue)model);
        }
        private void RefreshSlider()
        {
            slider.gameObject.SetActive(true);

            SettingsSliderValue val = (SettingsSliderValue)model;

            slider.minValue = val.minValue;
            slider.maxValue = val.maxValue;
            slider.SetValueWithoutNotify(val.currentValue);

            slider.onValueChanged.AddListener((f) => OnValueChange(f));
        }
        private void RefreshSwitch()
        {
            toggle.gameObject.SetActive(true);

            SettingsToggleValue val = (SettingsToggleValue)model;

            toggle.SetValueWithoutNotify(val.isOn, true);
            toggle.OnValueChange += (b) => OnValueChange(b);
        }




        private void OnValueChange(object value)
        {
            ui.SaveSetting(model.PropertyInfo, model, value);
        }



        private void FillDropdown(SettingsDropdownValue settingValue)
        {
            dropdown.dropdownItems.Clear();


            foreach (var value in settingValue.values)
            {
                dropdown.dropdownItems.Add(new CustomDropdown.Item()
                {
                    itemName = LocalizationManager.Localize(value)
                });
            }

            dropdown.selectedItemIndex = settingValue.currentValueIndex;

            dropdown.Refresh();
        }



        public virtual float GetHeight()
        {
            return GetComponent<RectTransform>().sizeDelta.y;
        }
    }
}
