using InGame.Menu;
using InGame.Settings;
using ProjectManagement;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MenuAudioManager : MonoBehaviour
{
    public AudioSource asource;
    public AdvancedSaveManager saveManager;

    //public SpectrumVisualizer spectrum;
    //public MonoBehaviour ispectrumObject;
    public LineSpectrumVisualizer spectrum;
    //private ISpectrumVisualizer ispectrum;

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
        //ThemeSongEnabled = SSytem.instance.GetBool("MenuMusic");
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





    // Invoked when player select group.
    public void OnMapSelected(GroupInfoExtended group)
    {
        return;
        StartCoroutine(IEOnMapSelected(group));
    }
    IEnumerator IEOnMapSelected(GroupInfoExtended group)
    {
        string trackname = group.author + "-" + group.name;
        string groupFolder = Application.persistentDataPath + "/maps/" + trackname;

        if (!Directory.Exists(groupFolder)) yield break;

        string mapFolder = Directory.GetDirectories(groupFolder)[0];



        string unknownAudioPath = mapFolder + "/" + trackname;
        string audioFilePath = ProjectManager.GetRealPath(unknownAudioPath, ".mp3", ".ogg");

        if (audioFilePath == "") yield break;



        AudioClip clip = null;
        yield return ProjectManager.LoadAudioCoroutine(audioFilePath, (c) => clip = c);

        if (clip == null) yield break;



        asource.clip = clip;
        asource.Play();
        asource.time = 30;
        SetAudioText(trackname.Replace("-", " - "));
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
