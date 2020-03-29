using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class CustomSlider : MonoBehaviour
{
    SettingsManager manager;
    Slider slider;
    public Text valueText;
    public float textMultiplier = 10f;

    private void Awake()
    {
        manager = Camera.main.GetComponent<SettingsManager>();
        slider = GetComponent<Slider>();

        slider.value = manager.RequestSettingFloat(slider.name);
        OnChange(slider.value);

        slider.onValueChanged.AddListener(OnChange);
    }

    void OnChange(float value)
    {
        manager.SetSetting(slider.name, slider.value);

        if (valueText != null) valueText.text = Mathf.CeilToInt(value / textMultiplier * 10f) / 10f + "";
    }
}
