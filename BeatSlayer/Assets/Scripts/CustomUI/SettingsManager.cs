using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Settings.Old
{
    public class SettingsManager : MonoBehaviour
    {
        public CustomSlider cubesSpeedSlider, musicSpeedSlider;
        public CustomToggle noArrowsToggle, noLinesToggle;

        public MenuAudioManager menuAudioManager;

        public AudioSource asource;

        public bool RequestSettingBool(string key)
        {
            return SSytem.GetBool(key);
        }
        public float RequestSettingFloat(string key)
        {
            return SSytem.GetFloat(key);
        }
        public int RequestSettingInt(string key)
        {
            return SSytem.GetInt(key);
        }



        public void SetSetting(string key, bool value)
        {
            SSytem.SetBool(key, value);
        }
        public void SetSetting(string key, float value)
        {
            SSytem.SetFloat(key, value);
        }

        public float CalculateScoreMultiplier()
        {
            float cubesSpeed = cubesSpeedSlider.GetComponent<Slider>().value / 10f;
            float musicSpeed = musicSpeedSlider.GetComponent<Slider>().value / 10f;
            bool noArrows = noArrowsToggle.GetComponent<Toggle>().isOn;
            bool noLines = noLinesToggle.GetComponent<Toggle>().isOn;

            return cubesSpeed * musicSpeed * (noArrows ? 0.4f : 1) * (noLines ? 0.8f : 1);
        }

        public static float GetScoreMultiplier()
        {
            float cubesSpeed = SSytem.GetFloat("CubesSpeed") / 10f;
            float musicSpeed = SSytem.GetFloat("MusicSpeed") / 10f;
            bool noArrows = SSytem.GetBool("NoArrows");
            bool noLines = SSytem.GetBool("NoLines");

            return cubesSpeed * musicSpeed * (noArrows ? 0.4f : 1) * (noLines ? 0.8f : 1);
        }


        public void OnMusicSwitch(Toggle toggle)
        {
            if (toggle.isOn) menuAudioManager.OnSetOn();
            else menuAudioManager.OnSetOff();
        }
        public void OnMusicVolumeChange(Slider slider)
        {
            asource.volume = slider.value * 0.2f;
        }
    }
}