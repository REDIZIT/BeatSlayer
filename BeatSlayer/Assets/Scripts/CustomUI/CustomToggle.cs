using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class CustomToggle : MonoBehaviour
{
    MenuScript_v2 menu;
    SettingsManager manager;
    Toggle toggle;

    private void Awake()
    {
        menu = Camera.main.GetComponent<MenuScript_v2>();
        manager = Camera.main.GetComponent<SettingsManager>();
        toggle = GetComponent<Toggle>();

        toggle.isOn = manager.RequestSettingBool(toggle.name);
        toggle.onValueChanged.AddListener(OnChange);
    }

    void OnChange(bool value)
    {
        manager.SetSetting(toggle.name, toggle.isOn);
    }
}
