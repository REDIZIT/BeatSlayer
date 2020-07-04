using InGame.Settings.Old;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.CustomUI
{
    [RequireComponent(typeof(Dropdown))]
    public class CustomDropdown : MonoBehaviour
    {
        SettingsManager manager;
        Dropdown dropdown;

        private void Awake()
        {
            manager = Camera.main.GetComponent<SettingsManager>();
            dropdown = GetComponent<Dropdown>();

            dropdown.value = manager.RequestSettingInt(dropdown.name);

            dropdown.onValueChanged.AddListener(OnChange);
        }

        public void OnChange(int value)
        {
            manager.SetSetting(dropdown.name, dropdown.value);
        }
    }
}