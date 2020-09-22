using InGame.Menu;
using InGame.Settings;
using UnityEngine;
using UnityEngine.UI;

public class MenuAudioManager : MonoBehaviour
{
    public AudioSource asource;

    public LineSpectrumVisualizer spectrum;

    public Text audioText;

    

    // == Settings ========
    bool ThemeSongEnabled;
    float Volume;

    private void Start()
    {
        OnSceneLoaded();
        //ispectrum = (ISpectrumVisualizer)ispectrumObject;
    }

    private void Update()
    {
        if(ThemeSongEnabled != SettingsManager.Settings.Sound.MenuMusicEnabled)
        {
            ThemeSongEnabled = SettingsManager.Settings.Sound.MenuMusicEnabled;
            if (SettingsManager.Settings.Sound.MenuMusicEnabled)
            {
                OnSetOn();
            }
            else
            {
                OnSetOff();
            }
        }

        float volumeFromSettings = SettingsManager.Settings.Sound.MenuMusicVolume / 500f;
        if (Volume != volumeFromSettings)
        {
            Volume = volumeFromSettings;
            asource.volume = Volume;
        }

        spectrum.UpdateData();
    }


    public void OnSceneLoaded()
    {
        //ThemeSongEnabled = SSytem.GetBool("MenuMusic");
        ThemeSongEnabled = SettingsManager.Settings.Sound.MenuMusicEnabled;
        Volume = SettingsManager.Settings.Sound.MenuMusicVolume / 100f * 0.4f;
        asource.volume = Volume;

        if (ThemeSongEnabled)
        {
            asource.Play();
            SetAudioText(asource.clip.name);
            spectrum.SetEnabled(true);
        }
    }


    // Invoked when player set checkmark in settings
    public void OnSetOn()
    {
        //spectrum.Init();
        spectrum.SetEnabled(true);
        asource.Play();
        SetAudioText(asource.clip.name);
    }
    public void OnSetOff()
    {
        //spectrum.Stop();
        spectrum.SetEnabled(false);
        asource.Stop();
        SetAudioText("");
    }





    void SetAudioText(string content)
    {
        if (content == "") audioText.text = "";
        else audioText.text = $"♪    {content}    ♪";
    }
}
