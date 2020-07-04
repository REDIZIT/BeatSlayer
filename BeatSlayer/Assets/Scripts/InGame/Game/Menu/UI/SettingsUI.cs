using InGame.Settings;
using System.Reflection;
using System;
using InGame.Helpers;
using System.ComponentModel;
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



        private void Start()
        {
            ShowSettingsPage();
        }
        //private void DescribeClass(/*Type classType*/PropertyInfo subField, object target)
        //{
        //    object subValue = subField.GetValue(target);
        //    foreach (PropertyInfo property in subField.PropertyType.GetProperties())
        //    {
        //        string strName = property.Name;
        //        Debug.Log(strName + " = " + property.GetValue(subValue, null).ToString());
        //    }
        //}
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
        public void ShowItemsGroup(PropertyInfo parentprop, object parentObject, Transform currentContent)
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





            SettingsUIItemType type = SettingsManager.GetItemType(prop.PropertyType);

            SettingsUIItemModel model = GetItemValue(prop, subValue, type);
            model.NameInFile = prop.Name;
            model.NameWithoutLocalization = attribute.DisplayName;
            model.Description = attribute.Description ?? "";
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


        public SettingsUIItemModel GetItemValue(PropertyInfo property, object parentClass, SettingsUIItemType type)
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


        public void SaveSetting(PropertyInfo prop, SettingsUIItemModel model, object value)
        {
            //PropertyInfo prop = modelType.GetProperties().FirstOrDefault(c => c.Name == name);
            //if(prop == null)
            //{
            //    throw new Exception($"Can't save setting with name '{name}' due to can't find it in model class");
            //}



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

            if(model.Type == SettingsUIItemType.Toggle || model.Type == SettingsUIItemType.Dropdown)
            {
                ShowSettingsPage();
            }
        }
    }
}
