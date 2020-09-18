using InGame.Extensions.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace InGame.Settings
{
    public static class SettingsManager
    {
        public static SettingsModel Settings
        {
            get
            {
                if(_settings == null) Load();

                if (_settings == null) throw new Exception("Settings model is null. Check SettingsManager and settings file");

                return _settings;
            }
            set
            {
                _settings = value;
                Save();
            }
        }
        private static SettingsModel _settings;

        public static SettingsUIItemModel SettingsViewModel;

        private static string SettingsFilePath => Application.persistentDataPath + "/data/settings.json";



        public static SettingsUIItemType GetItemType(Type propertyType)
        {
            if (propertyType.IsEnum)
            {
                return SettingsUIItemType.Dropdown;
            }
            else if (propertyType.IsNumericType())
            {
                return SettingsUIItemType.Slider;
            }
            else if (propertyType.IsClass)
            {
                return SettingsUIItemType.Group;
            }
            else if (propertyType == typeof(bool))
            {
                return SettingsUIItemType.Toggle;
            }

            throw new Exception("Unknown setting property type " + propertyType.Name + $" ( {propertyType} )");
        }

        
        public static SettingsUIItemModel GetSpecificModelForType(PropertyInfo property, object parentClass, SettingsUIItemType type)
        {
            object propertyValue = property.GetValue(parentClass);

            switch (type)
            {
                case SettingsUIItemType.Dropdown:
                    object underlayingValue = Convert.ChangeType(propertyValue, Enum.GetUnderlyingType(propertyValue.GetType()));

                    List<string> values = new List<string>();
                    values.AddRange(Enum.GetNames(propertyValue.GetType()));

                    return new SettingsDropdownValue(values, (int)underlayingValue);


                case SettingsUIItemType.Slider:
                    RangeAttribute range = property.GetCustomAttribute<Settings.RangeAttribute>();

                    float valueFloat = Convert.ToSingle(propertyValue);

                    return new SettingsSliderValue(range.MinValue, range.MaxValue, valueFloat);

                case SettingsUIItemType.Group:
                    return new SettingsGroupValue();

                case SettingsUIItemType.Toggle:

                    bool isOn = Convert.ToBoolean(propertyValue);

                    return new SettingsToggleValue(isOn);

                default: return null;
            }
        }

        /// <summary>Get attached media files names (<see cref="MediaAttribute"/>) or null</summary>
        public static string[] GetMediaOrNull(PropertyInfo prop)
        {
            MediaAttribute mediaAttribute = prop.GetCustomAttribute<MediaAttribute>();
            string[] mediaArray = null;
            if (mediaAttribute != null)
            {
                mediaArray = mediaAttribute.Images;
            }
            return mediaArray;
        }

        /// <summary>Is property can be changed. Affected by <see cref="EnabledAttribute"/> and his dependencies</summary>
        public static bool IsPropertyEnabled(PropertyInfo prop, IEnumerable<PropertyInfo> allProperties, object optionValue)
        {
            EnabledAttribute enabledAttribute = prop.GetCustomAttribute<EnabledAttribute>();

            // If EnabledAttribute isn't set, just return true
            if (enabledAttribute == null) return true;

            // Get based PropertyInfo by EnabledAttribue.Name
            PropertyInfo basedProp = allProperties.FirstOrDefault(c => c.Name == enabledAttribute.BaseName);
            if (basedProp == null)
            {
                Debug.LogError($"BasedProp is null: c ({allProperties.Count()})=> c.Name == {enabledAttribute.BaseName}");
                return false;
            }


            // If option isn't bool
            if (basedProp.PropertyType != typeof(bool))
            {
                Debug.LogError($"SettingsModel has property ({prop.Name}) with EnabledAttribute based on non bool value");
                return false;
            }

            // If is, return this bool value
            return (bool)basedProp.GetValue(optionValue);
        }



        public static void Reset()
        {
            File.Delete(SettingsFilePath);
            _settings = new SettingsModel();
        }


        #region Service code


        public static void Load()
        {
            if (!File.Exists(SettingsFilePath))
            {
                _settings = new SettingsModel();
                return;
            }

            string json = File.ReadAllText(SettingsFilePath);

            _settings = JsonConvert.DeserializeObject<SettingsModel>(json);

            ValidateAndCorrect();
        }
        public static void Save()
        {
            string json = JsonConvert.SerializeObject(_settings, Formatting.Indented);

            File.WriteAllText(SettingsFilePath, json);
        }

        /// <summary>
        /// Validate file data, if value isn't valid correct to valid
        /// </summary>
        public static void ValidateAndCorrect()
        {
            foreach (PropertyInfo prop in _settings.GetType().GetProperties())
            {
                CorrectRanges(prop);
            }
        }






        private static void CorrectRanges(PropertyInfo prop)
        {
            RangeAttribute range = prop.GetCustomAttribute<Settings.RangeAttribute>();
            if (range == null) return;

            object propValue = prop.GetValue(Settings);

            float propFloatValue = Convert.ToSingle(propValue);

            float clampledValue = range.Clamp((float)propFloatValue);

            propValue = Convert.ChangeType(clampledValue, prop.PropertyType);

            prop.SetValue(Settings, propValue);
        }


        #endregion
    }
}
