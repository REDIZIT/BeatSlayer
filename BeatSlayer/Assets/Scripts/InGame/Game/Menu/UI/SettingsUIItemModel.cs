using System.Collections.Generic;
using System.Reflection;

namespace InGame.Settings
{
    public class SettingsUIItemModel
    {
        /// <summary>
        /// Name which user will see
        /// </summary>
        public string NameWithoutLocalization { get; set; }
        public string NameInFile { get; set; }
        public string Description { get; set; }
        public string[] Media { get; set; }

        /// <summary>
        /// Item type
        /// </summary>
        public SettingsUIItemType Type { get; set; }

        /// <summary>
        /// Can be this option modified? Controlled by <see cref="EnabledAttribute"/>
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Property, which this item presents and which should change
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
        /// <summary>
        /// Object, which contain property to change
        /// </summary>
        public object PropertyTarget { get; set; }
    }

    public enum SettingsUIItemType
    {
        Dropdown, Slider, Group, Toggle
    }





    public class SettingsDropdownValue : SettingsUIItemModel
    {
        public int currentValueIndex;
        public List<string> values;

        public SettingsDropdownValue(List<string> values, int currentValueIndex)
        {
            this.values = values;
            this.currentValueIndex = currentValueIndex;
        }
    }

    public class SettingsSliderValue : SettingsUIItemModel
    {
        public float minValue;
        public float maxValue;
        public float currentValue;

        public SettingsSliderValue(float minValue, float maxValue, float currentValue)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.currentValue = currentValue;
        }
    }

    public class SettingsGroupValue : SettingsUIItemModel
    {

    }

    public class SettingsToggleValue : SettingsUIItemModel
    {
        public bool isOn;

        public SettingsToggleValue(bool isOn)
        {
            this.isOn = isOn;
        }
    }
}
