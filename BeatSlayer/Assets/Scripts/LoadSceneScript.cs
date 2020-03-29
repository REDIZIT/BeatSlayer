using System;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.Advertisements;
using Assets.SimpleLocalization;
using System.Xml.Serialization;
using ProjectManagement;

public class LoadSceneScript : MonoBehaviour {

    AsyncOperation ao;
    public Slider loadingSlider;
    public Text loadingText;
    public Text loadingLabel, loadingLabel2;
    public RectTransform loadingCircle;

    IEnumerator Start()
    {
        loadingCircle.anchoredPosition = new Vector2(loadingLabel.preferredWidth / 2f + 80, 0);
        if (LCData.sceneLoading == "")
        {
            SceneManager.LoadScene("Menu");
            yield return null;
        }

        

        yield return IELoadGame(LCData.loadparams);

        ao = SceneManager.LoadSceneAsync(LCData.sceneLoading, LoadSceneMode.Single);
        ao.allowSceneActivation = false;

        if (!Advertisement.isInitialized)
        {
            Advertisement.Initialize("3202418", false);
        }
        if (LCData.sceneLoading == "Menu")
        {
            if (Advertisement.IsReady())
            {
                Advertisement.Show("video");
            }
        }
        else
        {
            if(Application.internetReachability != NetworkReachability.NotReachable)
            {
                GetComponent<AccountManager>().UpdateSessionTime();
            }
        }

        //LoadGame(LCData.loadparams);

        

        while (ao.progress < 0.9f)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public void CancelLoading()
    {
        if(ao != null) ao.allowSceneActivation = false;
        ao = null;
        LCData.sceneLoading = "Menu";
        SceneManager.LoadScene("Menu");
    }



    IEnumerator InitProjectFile()
    {
        string mapFolder = Application.persistentDataPath + "/maps/" + LCData.track.group.author + "-" + LCData.track.group.name + "/" + LCData.track.nick;
        string mapPath = mapFolder + "/" + LCData.track.group.author + "-" + LCData.track.group.name;

        Debug.Log("projectPath: " + mapPath + ".bsu");

        XmlSerializer xml = new XmlSerializer(typeof(Project));
        Stream loadStream = File.OpenRead(mapPath + ".bsu");
        LCData.project = (Project)xml.Deserialize(loadStream);
        loadStream.Close();

        Debug.Log("Cubes count " + LCData.project.beatCubeList.Count);

        string audioFilePath = mapPath + (LCData.project.audioExtension == Project.AudioExtension.Ogg ? ".ogg" : ".mp3");

        using (WWW www = new WWW("file:///" + audioFilePath))
        {
            yield return www;
            LCData.aclip = www.GetAudioClip();
        }
    }
    IEnumerator InitCustomFile()
    {
        string audioFilePath = LCData.track.group.filepath.Replace(@"\","/");

        

        // Лять, оказывается надо скопировать из папки в которой лежит файл в папку с игрой
        // Вот почему нельзя было выдавать exception?! Ааааааа! Ипался с этим с момента создания игры!!
        // Это прям турбо херня
        string copiedPath = Application.persistentDataPath + "/custom" + Path.GetExtension(audioFilePath);

        Debug.Log("[LOADING GAME] " + System.Net.WebUtility.UrlEncode(audioFilePath) + "\n" + copiedPath);

        File.Copy(audioFilePath, copiedPath, true);

        using (WWW www = new WWW("file:///" + System.Net.WebUtility.UrlEncode(copiedPath)))
        {
            yield return www;
            LCData.aclip = www.GetAudioClip(false, false);
        }

        File.Delete(copiedPath);

        Debug.Log("[CLIP] Is null? " + (LCData.aclip == null));
        Debug.Log("[CLIP] len: " + LCData.aclip.length);
        Debug.Log("[CLIP] isReadyToPlay? " + LCData.aclip.isReadyToPlay);
        Debug.Log("[CLIP] loadState: " + LCData.aclip.loadState.ToString());
        //Debug.Log("[CLIP] loadType: " + LCData.aclip.loadType.ToString());

        yield return new WaitForEndOfFrame();
    }

    IEnumerator InitZippedProjectFile()
    {
        XmlSerializer xml = new XmlSerializer(typeof(Project));
        Stream loadStream = File.OpenRead(LCData.projectFilePath);
        LCData.project = (Project)xml.Deserialize(loadStream);
        loadStream.Close();

        string audioFilePath = Application.persistentDataPath + "/tempaudio" + (LCData.project.audioExtension == Project.AudioExtension.Mp3 ? ".mp3" : ".ogg");
        File.WriteAllBytes(audioFilePath, LCData.project.audioFile);

        using (WWW www = new WWW("file:///" + audioFilePath))
        {
            yield return www;
            LCData.aclip = www.GetAudioClip();
        }
    }

    void Update()
    {
        if (ao != null)
        {
            if (ao.progress >= 0.9f)
            {
                if (!Advertisement.isShowing)
                {
                    ao.allowSceneActivation = true;
                }
                loadingSlider.value = 1;
                loadingText.text = "";
                loadingLabel.text = LocalizationManager.Localize("Loaded");
            }
            else
            {
                loadingSlider.value = ao.progress;
                loadingText.text = ao.progress * 100 + "%";
            }
        }
    }

    public void LoadGame(SceneloadParameters loadparams)
    {
        StartCoroutine(IELoadGame(loadparams));
    }
    IEnumerator IELoadGame(SceneloadParameters loadparams)
    {
        if(loadparams == null)
        {
            if (LCData.loadingType == LCData.LoadingType.Standard)
            {
                yield return InitProjectFile();
            }
            else if (LCData.loadingType == LCData.LoadingType.AudioFile) yield return InitCustomFile();
            else yield return InitZippedProjectFile();
        }
        else
        {
            switch (loadparams.type)
            {
                case SceneloadParameters.LoadType.Standard: yield return InitProjectFile(); break;
                case SceneloadParameters.LoadType.AudioFile: yield return InitCustomFile(); break;
                case SceneloadParameters.LoadType.ProjectFolder: yield return InitProjectFolder(loadparams); break;
            }
        }
    }


    IEnumerator InitProjectFolder(SceneloadParameters loadparams)
    {
        Debug.Log("Init project folder " + loadparams.path);

        string bsuPath = loadparams.path;

        LCData.project = ProjectManager.LoadProject(bsuPath);

        string trackname = LCData.project.author + "-" + LCData.project.name;
        string audioPath = new FileInfo(bsuPath).DirectoryName + "/" + trackname + Project.ToString(LCData.project.audioExtension);

        Debug.Log("Audio path is " + audioPath);

        LCData.aclip = ProjectManager.LoadAudio(audioPath);

        Debug.Log("Is audio clip null? " + (LCData.aclip == null));
        Debug.Log("Audio clip len is " + LCData.aclip.length);

        yield return new WaitForEndOfFrame();
    }

}

// Класс для временного (межстраничного) хранения данных
public static class LCData
{
    public static string sceneLoading = "";
    public enum LoadingType { Standard, AudioFile, ProjectFile, MapFolder };
    public static LoadingType loadingType;

    public static TrackClass track;
    public static Project project;

    public static AudioClip aclip;

    public static int[] hitsIds;

    public static string newVersion,newVersionDescription;

    public static bool isBossLevel = false;


    /// <summary>
    /// Путь до файла проекта (.bsz). Используется при loadingType = ProjectFile
    /// </summary>
    public static string projectFilePath;


    public static SceneloadParameters loadparams;


    public static string author
    {
        get
        {
            if (loadparams.type == SceneloadParameters.LoadType.ProjectFolder) return project.author;
            else return track.group.author;
        }
    }
    public static string name
    {
        get
        {
            if (loadparams.type == SceneloadParameters.LoadType.ProjectFolder) return project.name;
            else return track.group.name;
        }
    }
}

public class SceneloadParameters
{
    /// <summary>
    /// Project loading method
    /// </summary>
    public enum LoadType 
    { 
        /// <summary>
        /// From Author list
        /// </summary>
        Standard,
        /// <summary>
        /// From Own music list (from .mp3 or .ogg file)
        /// </summary>
        AudioFile,
        /// <summary>
        /// From 'From file' list. Select folder with unzipped project (Folder contains bsu, mp3/ogg, png/jpg files)
        /// </summary>
        ProjectFolder
    }

    public LoadType type;
    public string path;


    public SceneloadParameters(LoadType type)
    {
        this.type = type;
    }
    public SceneloadParameters(LoadType type, string path)
    {
        this.type = type;
        this.path = path;
    }
}