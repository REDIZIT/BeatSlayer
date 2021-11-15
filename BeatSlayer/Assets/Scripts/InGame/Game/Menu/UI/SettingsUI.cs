using InGame.Settings;
using System.Reflection;
using System;
using InGame.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace InGame.Game.Menu
{
    public class SettingsUI : MonoBehaviour
    {
        public Transform content;
        public List<SettingsOptionImage> OptionsImages = new List<SettingsOptionImage>();


        [SerializeField] private GameObject itemPrefab, groupPrefab;
        [SerializeField] private Transform dropdownlocker;

        private SettingsModel Settings => SettingsManager.Settings;
        private List<PropertyInfo> SettingsProperties { get; set; } = new List<PropertyInfo>();

        private List<OptionChangeSubscription> Subscriptions { get; set; } = new List<OptionChangeSubscription>();
        private List<SettingsUIItem> Items { get; set; } = new List<SettingsUIItem>();


        private void Start()
        {
            ShowSettingsPage();
        }

        public void OnResetBtnClick()
        {
            SettingsManager.Reset();
            ShowSettingsPage();
        }

        /// <summary>Recreate all groups and items</summary>
        public void ShowSettingsPage()
        {
            //Stopwatch w = Stopwatch.StartNew();

            HelperUI.ClearContentAll(content);

            var props = Settings.GetType().GetProperties().ToList();

            SettingsProperties.Clear();

            foreach (var prop in props)
            {
                var innerProps = prop.PropertyType.GetProperties();
                SettingsProperties.AddRange(innerProps);
            }

            foreach (var prop in props)
            {
                if (prop.PropertyType.IsClass)
                {
                    CreateGroup(prop, prop.GetValue(Settings), content);
                }
                else
                {
                    AddGroupItem(prop, Settings, null);
                }
            }

            //Debug.Log($"ShowSettingsPage elapsed {w.ElapsedMilliseconds}ms");
        }
        /// <summary>Update already crated groups and items</summary>
        public void UpdateSettingsPage()
        {
            foreach (SettingsUIItem item in Items)
            {
                RefreshGroupItem(item);
            }
        }

        public void Subscribe(string optionName, Action callback)
        {
            Subscriptions.Add(new OptionChangeSubscription(optionName, callback));
        }












        public void SaveSetting(PropertyInfo prop, SettingsUIItemModel model, object value)
        {
            Stopwatch w = Stopwatch.StartNew();


            object casted;
            if (prop.PropertyType.IsEnum)
            {
                casted = Enum.Parse(prop.PropertyType, value.ToString());
            }
            else
            {
                casted = Convert.ChangeType(value, prop.PropertyType);
            }


            prop.SetValue(model.PropertyTarget, casted);

            SettingsManager.Save();

            if (model.Type == SettingsUIItemType.Toggle || model.Type == SettingsUIItemType.Dropdown)
            {
                //ShowSettingsPage();
                UpdateSettingsPage();
                OnOptionChange(model.NameInFile);
            }


            Debug.Log($"SaveSetting elapsed {w.ElapsedMilliseconds}ms");
        }

        private void OnOptionChange(string optionName)
        {
            foreach (var sub in Subscriptions.Where(c => c.OptionName == optionName))
            {
                sub.Callback?.Invoke();
            } 
        }




        private void CreateGroup(PropertyInfo parentprop, object parentObject, Transform currentContent)
        {
            SettingsUIGroup group = AddGroup(parentprop, currentContent);


            var props = parentprop.PropertyType.GetProperties();

            foreach (PropertyInfo prop in props)
            {
                if (prop.PropertyType.IsClass)
                {
                    CreateGroup(prop, prop.GetValue(parentObject), group.content);
                }
                else
                {
                    AddGroupItem(prop, parentObject, group);
                }
            }

            group.FitContent();
        }
        private SettingsUIGroup AddGroup(PropertyInfo prop, Transform content)
        {
            SettingsUIGroup group = null;
            OptionAttribute attribute = prop.GetCustomAttribute<OptionAttribute>();
            if (attribute == null)
            {
                Debug.LogError($"SettingsModel has group property ({prop.Name}) with no display name attribute");
                return null;
            }

            HelperUI.AddContent<SettingsUIGroup>(content, groupPrefab, (item) =>
            {
                group = item;
                group.Refresh(new SettingsUIItemModel()
                {
                    NameInFile = prop.Name,
                    NameWithoutLocalization = attribute.DisplayName,
                    Description = attribute.Description ?? ""
                }, this, dropdownlocker);

                SettingsOptionImage img = OptionsImages.FirstOrDefault(c => c.name == prop.Name);
                if (img != null)
                {
                    group.SetImage(img);
                }
            });

            return group;
        }
        private void AddGroupItem(PropertyInfo prop, object subValue, SettingsUIGroup group)
        {
            // Get attribute to detect property as option
            OptionAttribute attribute = prop.GetCustomAttribute<OptionAttribute>();
            if (attribute == null) return;


            SettingsUIItemType type = SettingsManager.GetItemType(prop.PropertyType);

            SettingsUIItemModel model = SettingsManager.GetSpecificModelForType(prop, subValue, type);
            model.NameInFile = prop.Name;
            model.NameWithoutLocalization = attribute.DisplayName;
            model.Description = attribute.Description ?? "";
            model.Media = SettingsManager.GetMediaOrNull(prop);
            model.Type = type;
            model.PropertyInfo = prop;
            model.PropertyTarget = subValue;
            model.Enabled = SettingsManager.IsPropertyEnabled(prop, SettingsProperties, subValue);

            if (group == null)
            {
                HelperUI.AddContent<SettingsUIItem>(content, itemPrefab, (item) =>
                {
                    item.Refresh(model, this, dropdownlocker);
                });
            }
            else
            {
                HelperUI.AddContent<SettingsUIItem>(group.content, itemPrefab, (item) =>
                {
                    item.Refresh(model, this, dropdownlocker);
                    Items.Add(item);
                });
            }
        }
        private void RefreshGroupItem(SettingsUIItem item)
        {
            // Get value
            SettingsUIItemModel model = SettingsManager.GetSpecificModelForType(item.model.PropertyInfo, item.model.PropertyTarget, item.model.Type);

            // Copy values
            model.NameInFile = item.model.NameInFile;
            model.NameWithoutLocalization = item.model.NameWithoutLocalization;
            model.Description = item.model.Description;
            model.Media = item.model.Media;
            model.Type = item.model.Type;
            model.PropertyInfo = item.model.PropertyInfo;
            model.PropertyTarget = item.model.PropertyTarget;

            // Update values
            model.Enabled = SettingsManager.IsPropertyEnabled(item.model.PropertyInfo, SettingsProperties, item.model.PropertyTarget);

            item.Refresh(model, this, dropdownlocker);
        }
    }



    



    public class OptionChangeSubscription
    {
        public string OptionName { get; set; }
        public Action Callback { get; set; }

        public OptionChangeSubscription(string optionName, Action callback)
        {
            OptionName = optionName;
            Callback = callback;
        }
    }
}
