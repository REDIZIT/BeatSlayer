using GameNet;
using InGame.Menu.Mods;
using ProjectManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Testing;
using UnityEngine;
using UnityEngine.SceneManagement;
using InGame.Models;

namespace InGame.SceneManagement
{
    public class SceneController : MonoBehaviour
    {

        public static SceneControllerUI ui;
        public static SceneController instance;

        public void Init(SceneControllerUI ui)
        {
            SceneController.ui = ui;
            instance = this;
        }



        public void LoadScene(SceneloadParameters loadpamars)
        {
            StartCoroutine(IELoadScene(loadpamars));
        }
        IEnumerator IELoadScene(SceneloadParameters loadpamars)
        {
            LoadingData.loadparams = loadpamars;

            IProjectLoader loader = new ProjectLoaderMenu();

            switch (loadpamars.Type)
            {
                case SceneloadParameters.LoadType.Tutorial:
                case SceneloadParameters.LoadType.Author:
                    loader = new ProjectLoaderMap();
                    break;
                case SceneloadParameters.LoadType.AudioFile:
                    loader = new ProjectLoaderAudioFile();
                    break;
                case SceneloadParameters.LoadType.ProjectFolder:
                    loader = new ProjectLoaderEditorTest();
                    break;
                case SceneloadParameters.LoadType.Moderation:
                    loader = new ProjectLoaderModeration();
                    break;
                case SceneloadParameters.LoadType.EditorTest:
                    loader = new ProjectLoaderEditorTest();
                    break;
            }

            LoadHitSounds();

            ui.Load();
            
            yield return loader.LoadProject(loadpamars);
        }

        public static void LoadHitSounds()
        {
            if (Payload.HitSoundIds != null && Payload.HitSoundIds.Count > 0)
            {
                foreach (int id in Payload.HitSoundIds)
                {
                    AndroidNativeAudio.unload(id);
                }
                AndroidNativeAudio.releasePool();
            }

            int streamsCount = 20;
            if (File.Exists(Application.persistentDataPath + "/streamcount.txt"))
            {
                streamsCount = int.Parse(File.ReadAllText(Application.persistentDataPath + "/streamcount.txt"));
            }

            AndroidNativeAudio.makePool(streamsCount);
            Payload.HitSoundIds.Clear();
            for (int i = 0; i < 10; i++)
            {
                Payload.HitSoundIds.Add(AndroidNativeAudio.load("LastHit" + (i + 1) + ".ogg"));
                Payload.HitSoundIds.Add(AndroidNativeAudio.load("ShortHits/HitShort" + (i + 1) + ".ogg"));
            }
        }
    }
}



public static class LoadingData
{
    public static int sceneLoadCount;

    public static SceneloadParameters loadparams;

    public static Project project;
    public static AudioClip aclip;
    public static Sprite cover;
}






public class SceneloadParameters
{
    /// <summary>
    /// Project loading method
    /// </summary>
    public enum LoadType
    {
        /// <summary>
        /// Load menu scene
        /// </summary>
        Menu = 0,
        /// <summary>
        /// From Author list
        /// </summary>
        Author = 1,
        /// <summary>
        /// From Own music list (from .mp3 or .ogg file)
        /// </summary>
        AudioFile = 2,
        /// <summary>
        /// From 'From file' list. Select folder with unzipped project (Folder contains bsu, mp3/ogg, png/jpg files)
        /// </summary>
        ProjectFolder = 3,
        /// <summary>
        /// Moderate map (no awards, no anything, just add GoToEditor button at the end)
        /// </summary>
        Moderation = 4,
        /// <summary>
        /// Executed from editor when player want to test map in game
        /// </summary>
        EditorTest = 5,
        /// <summary>
        /// If loading map is tutorial
        /// </summary>
        Tutorial = 6
    }

    public LoadType Type { get; private set; }
    public List<ModSO> Mods { get; private set; } = new List<ModSO>();

    public BasicMapData Map { get; private set; } = new BasicMapData();
    public DifficultyInfo difficultyInfo { get; private set; }
    public string Trackname { get { return Map.Author + "-" + Map.Name; } }

    public string AudioFilePath { get; private set; }
    public string ProjectFolderPath { get; private set; }

    // Practice mode
    public bool IsPracticeMode { get; set; }
    public float StartTime { get; set; }
    public float MusicSpeed { get; set; }
    public float CubesSpeed { get; set; }








    private SceneloadParameters() { }

    public static SceneloadParameters AuthorMusicPreset(BasicMapData map, DifficultyInfo difficultyInfo, List<ModSO> mods)
    {
        map.MapType = GroupType.Tutorial;
        var parameters = new SceneloadParameters()
        {
            Type = LoadType.Author,
            Map = map,
            difficultyInfo = difficultyInfo,
            Mods = mods
        };
        return parameters;
    }



    public static SceneloadParameters TutorialPreset(BasicMapData map, DifficultyInfo difficultyInfo)
    {
        var parameters = new SceneloadParameters()
        {
            Type = LoadType.Tutorial,
            Map = map,
            difficultyInfo = difficultyInfo
        };
        return parameters;
    }

    /// <summary>
    /// Start author music with practice mode
    /// </summary>
    public static SceneloadParameters AuthorMusicPreset(BasicMapData map, DifficultyInfo difficultyInfo, float startTime, float musicSpeed, float cubesSpeed, List<ModSO> mods)
    {
        var parameters = new SceneloadParameters()
        {
            Type = LoadType.Author,
            Map = map,
            difficultyInfo = difficultyInfo,
            IsPracticeMode = true,
            StartTime = startTime,
            MusicSpeed = musicSpeed,
            CubesSpeed = cubesSpeed,
            Mods = mods
        };
        return parameters;
    }
    public static SceneloadParameters OwnMusicPreset(OwnMapData map, List<ModSO> mods)
    {
        var parameters = new SceneloadParameters()
        {
            Type = LoadType.AudioFile,
            AudioFilePath = map.Filepath,
            Map = map,
            difficultyInfo = new DifficultyInfo()
            {
                name = "Standard",
                stars = 4
            },
            Mods = mods
        };
        return parameters;
    }
    public static SceneloadParameters ModerationPreset(TestRequest request, DifficultyInfo difficultyInfo)
    {
        string mapFolder = Application.persistentDataPath + "/data/moderation/map";

        BasicMapData map = new BasicMapData()
        {
            Author = request.trackname.Split('-')[0],
            Name = request.trackname.Split('-')[1],
            MapperNick = "[MODERATION *]"
        };

        var parameters = new SceneloadParameters()
        {
            Type = LoadType.Moderation,
            Map = map, 
            ProjectFolderPath = mapFolder,
            difficultyInfo = difficultyInfo
        };
        return parameters;
    }
    public static SceneloadParameters EditorTestPreset(TestRequest request)
    {
        string trackname = Path.GetFileNameWithoutExtension(request.filepath);
        BasicMapData map = new BasicMapData()
        {
            Author = trackname.Split('-')[0],
            Name = trackname.Split('-')[1],
            MapperNick = "[EDITOR TEST *]",
        };

        var parameters = new SceneloadParameters()
        {
            Type = LoadType.ProjectFolder,
            Map = map,
            ProjectFolderPath = new FileInfo(request.filepath).DirectoryName,
            difficultyInfo = new DifficultyInfo()
            {
                id = request.difficultyId
            }
        };
        return parameters;
    }
    public static SceneloadParameters GoToMenuPreset()
    {
        var parameters = new SceneloadParameters()
        {
            Type = LoadType.Menu
        };
        return parameters;
    }
}








public interface IProjectLoader
{
    IEnumerator LoadProject(SceneloadParameters parameters);
}
public class ProjectLoaderMap : IProjectLoader
{
    public IEnumerator LoadProject(SceneloadParameters parameters)
    {
        string projectFolderPath;

        projectFolderPath = Application.persistentDataPath + "/Maps/" + parameters.Trackname + "/" + parameters.Map.MapperNick;


        string bsuPath = projectFolderPath + "/" + parameters.Trackname + ".bsu";
        LoadingData.project = ProjectManager.LoadProject(bsuPath);

        string audioFilePath = projectFolderPath + "/" + parameters.Trackname + Project.ToString(LoadingData.project.audioExtension);

        yield return ProjectManager.LoadAudioCoroutine(audioFilePath);

        if (LoadingData.project.hasImage)
        {
            string coverFilePath = projectFolderPath + "/" + parameters.Trackname + Project.ToString(LoadingData.project.imageExtension);
            LoadingData.cover = ProjectManager.LoadSprite(coverFilePath);
        }
    }
}
public class ProjectLoaderAudioFile : IProjectLoader
{
    public IEnumerator LoadProject(SceneloadParameters parameters)
    {
        string audioFilePath = parameters.AudioFilePath;
        string filename = Path.GetFileNameWithoutExtension(audioFilePath);
        string author = filename.Split('-')[0];
        string name = filename.Contains("-") ? filename.Split('-')[1] : "Unknown";

        LoadingData.project = new Project()
        {
            author = author,
            name = name,
            creatorNick = "[LOCAL STORAGE]"
        };

        yield return ProjectManager.LoadAudioCoroutine(audioFilePath);
    }
}
public class ProjectLoaderMenu : IProjectLoader
{
    public IEnumerator LoadProject(SceneloadParameters parameters)
    {
        yield return new WaitForEndOfFrame();
    }
}
public class ProjectLoaderModeration : IProjectLoader
{
    public IEnumerator LoadProject(SceneloadParameters parameters)
    {
        string mapFolder = parameters.ProjectFolderPath;
        string bsuPath = mapFolder + "/" + parameters.Trackname + ".bsu";

        Project proj = ProjectManager.LoadProject(bsuPath);
        LoadingData.project = proj;

        string audioPath = mapFolder + "/" + parameters.Trackname + Project.ToString(proj.audioExtension);

        yield return ProjectManager.LoadAudioCoroutine(audioPath);
    }
}
public class ProjectLoaderEditorTest : IProjectLoader
{
    public IEnumerator LoadProject(SceneloadParameters parameters)
    {
        string folderPath = parameters.ProjectFolderPath;
        string trackname = parameters.Trackname;
        string bsuPath = folderPath + "/" + trackname + ".bsu";

        Project proj = ProjectManager.LoadProject(bsuPath);
        LoadingData.project = proj;

        string audioPath = folderPath + "/" + trackname + Project.ToString(proj.audioExtension);

        yield return ProjectManager.LoadAudioCoroutine(audioPath);
    }
}