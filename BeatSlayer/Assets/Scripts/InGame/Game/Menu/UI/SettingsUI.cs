using InGame.Settings;
using System.Reflection;
using System;
using InGame.Helpers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Game.Menu
{
    public class SettingsUI : MonoBehaviour
    {
        public Transform content;
        public List<SettingsOptionImage> OptionsImages = new List<SettingsOptionImage>();


        [SerializeField] private GameObject itemPrefab, groupPrefab;
        [SerializeField] private Transform dropdownlocker;

        private SettingsModel Settings => SettingsManager.Settings;

        private List<OptionChangeSubscription> Subscriptions { get; set; } = new List<OptionChangeSubscription>();



        private void Start()
        {
            ShowSettingsPage();
        }

        public void OnResetBtnClick()
        {
            SettingsManager.Reset();
            ShowSettingsPage();
        }

        public void ShowSettingsPage()
        {
            HelperUI.ClearContentAll(content);

            var props = Settings.GetType().GetProperties();

            foreach (var prop in props)
            {
                if (prop.PropertyType.IsClass)
                {
                    ShowItemsGroup(prop, prop.GetValue(Settings), content);
                }
                else
                {
                    AddOptionItem(prop, props, Settings, null);
                }
            }
        }

        public void Subscribe(string optionName, Action callback)
        {
            Subscriptions.Add(new OptionChangeSubscription(optionName, callback));
        }












        public void SaveSetting(PropertyInfo prop, SettingsUIItemModel model, object value)
        {
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
                ShowSettingsPage();
                OnOptionChange(model.NameInFile);
            }
        }

        private void OnOptionChange(string optionName)
        {
            foreach (var sub in Subscriptions.Where(c => c.OptionName == optionName))
            {
                sub.Callback?.Invoke();
            } 
        }




        private void ShowItemsGroup(PropertyInfo parentprop, object parentObject, Transform currentContent)
        {
            SettingsUIGroup group = AddOptionsGroup(parentprop, currentContent);


            var props = parentprop.PropertyType.GetProperties();

            foreach (PropertyInfo prop in props)
            {
                if (prop.PropertyType.IsClass)
                {
                    ShowItemsGroup(prop, prop.GetValue(parentObject), group.content);
                }
                else
                {
                    AddOptionItem(prop, props, parentObject, group);
                }
            }

            group.FitContent();
        }
        private SettingsUIGroup AddOptionsGroup(PropertyInfo prop, Transform content)
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
        private void AddOptionItem(PropertyInfo prop, PropertyInfo[] props, object subValue, SettingsUIGroup group)
        {
            string strName = prop.Name;

            OptionAttribute attribute = prop.GetCustomAttribute<OptionAttribute>();
            if (attribute == null)
            {
                //Debug.LogError($"SettingsModel has property ({prop.Name}) with no display name attribute");
                return;
            }

            EnabledAttribute enabledAttribute = prop.GetCustomAttribute<EnabledAttribute>();
            bool enabled = true;
            if (enabledAttribute != null)
            {
                var basedProp = props.FirstOrDefault(c => c.Name == enabledAttribute.BaseName);
                if (basedProp == null) return;
                if (basedProp.PropertyType != typeof(bool))
                {
                    Debug.LogError($"SettingsModel has property ({prop.Name}) with EnabledAttribute based on non bool value");
                    return;
                }

                bool basedPropValue = (bool)basedProp.GetValue(subValue);
                enabled = basedPropValue;
            }


            MediaAttribute mediaAttribute = prop.GetCustomAttribute<MediaAttribute>();
            string[] mediaArray = null;
            if(mediaAttribute != null)
            {
                mediaArray = mediaAttribute.Images;
            }




            SettingsUIItemType type = SettingsManager.GetItemType(prop.PropertyType);

            SettingsUIItemModel model = GetItemValue(prop, subValue, type);
            model.NameInFile = prop.Name;
            model.NameWithoutLocalization = attribute.DisplayName;
            model.Description = attribute.Description ?? "";
            model.Media = mediaArray;
            model.Type = type;
            model.PropertyInfo = prop;
            model.PropertyTarget = subValue;
            model.Enabled = enabled;

            if(group == null)
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
                });
            }
        }


        private SettingsUIItemModel GetItemValue(PropertyInfo property, object parentClass, SettingsUIItemType type)
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
                    Settings.RangeAttribute range = property.GetCustomAttribute<Settings.RangeAttribute>();

                    //float currentValueInt = (float)property.GetValue(parentClass);
                    float valueFloat = Convert.ToSingle(propertyValue);

                    //currentValueInt = (float)range.Clamp(currentValueInt);

                    return new SettingsSliderValue(range.MinValue, range.MaxValue, valueFloat);

                case SettingsUIItemType.Group:
                    return new SettingsGroupValue();

                case SettingsUIItemType.Toggle:

                    bool isOn = Convert.ToBoolean(propertyValue);

                    return new SettingsToggleValue(isOn);

                default: return null;
            }
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
