using InGame.Extensions.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
