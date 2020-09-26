using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.SimpleLocalization;
using DatabaseManagement;
using InGame.Helpers;
using InGame.Menu;
using InGame.Menu.Maps;
using InGame.Menu.Mods;
using InGame.Models;
using InGame.Multiplayer.Lobby;
using InGame.Multiplayer.Lobby.UI;
using InGame.SceneManagement;
using InGame.UI;
using ProjectManagement;
using Testing;
using UnityEngine;
using UnityEngine.UI;

public class BeatmapUI : MonoBehaviour
{
    public MenuScript_v2 menu;
    public MenuAudioManager menuAudioManager;
    public PracticeModeUI practiceModeUI;
    public MapsDownloadQueuer mapsDownloadQueuer;
    public ModsUI modsUI;
    public LobbyUIManager lobbyUI;

    [Header("Map Info")]
    private BasicMapData currentMapInfo;
    private DifficultyInfo currentDifficultyInfo;
    private bool isGroupDeleted;
    private bool isDownloaded, hasUpdate;

    [Header("UI")]
    public GameObject overlay;
    public RawImage coverImage;
    public Text authorText, nameText;
    public GameObject approvedIcon;
    public Transform beatmapsContent, difficultiesContent;
    public Button backBtn;
    public GameObject errorPan, alertPan;
    public Text errorText, alertText;

    public LoadingCircle loadingCircle;

    [Header("Pans UI")]
    public GameObject downloadPan;
    public GameObject progressPan, footer;

    [Header("Download UI")] 
    public Slider progressBar;
    public Text progressText;
    public GameObject downloadBtn, updateBtn;


    public TestRequest testRequest;



    private void Start()
    {
        mapsDownloadQueuer.onDownloaded = OnDifficultyItemClicked;
    }
    void Update()
    {
        RefreshControls();
    }
    public void RefreshControls()
    {
        if (currentDifficultyInfo != null)
        {
            var task = MapsDownloadQueuerBackground.queue.FirstOrDefault(c => c.Trackname == currentMapInfo.Author + "-" + currentMapInfo.Name && c.Mapper == currentMapInfo.MapperNick);
            if (task != null)
            {
                if (task.TaskState == MapDownloadTask.State.Downloading || task.TaskState == MapDownloadTask.State.Waiting)
                {
                    RefreshAsProgress(task.ProgressPercentage);
                }
                else
                {
                    bool downloaded = true;

                    if (task.TaskState == MapDownloadTask.State.Error || task.TaskState == MapDownloadTask.State.Canceled)
                    {
                        downloaded = false;
                    }

                    isDownloaded = downloaded;

                    RefreshBeatmapsList();

                    if (downloaded) RefreshAsDownloaded();
                    else RefreshAsUndownloaded();
                }

            }
            else
            {
                bool isTest = testRequest != null;

                if (isTest)
                {
                    downloadPan.SetActive(false);
                    footer.SetActive(true);
                }
                else
                {
                    if (currentDifficultyInfo == null) RefreshAsNotSelected();
                    else if (isDownloaded) RefreshAsDownloaded();
                    else RefreshAsUndownloaded();
                }

            }
        }
    }

    private void RefreshAsNotSelected()
    {
        downloadPan.SetActive(false);

        progressPan.SetActive(false);

        footer.SetActive(false);
    }
    private void RefreshAsUndownloaded()
    {
        // Download stuff enabled
        downloadPan.SetActive(true);
        downloadBtn.SetActive(true);
        updateBtn.SetActive(false);

        progressPan.SetActive(false);

        footer.SetActive(false);
    }
    private void RefreshAsProgress(int progress)
    {
        downloadPan.SetActive(false);

        // Downloading stuff enabled
        progressPan.SetActive(true);
        progressBar.value = progress;
        progressText.text = progress + "%";

        footer.SetActive(false);
    }

    private void RefreshAsDownloaded()
    {
        downloadPan.SetActive(false);

        progressPan.SetActive(false);

        // Game stuff enabled
        footer.SetActive(true);
    }





    public void Open(GroupPresenter listItem)
    {
        overlay.SetActive(true);
        ResetUI();
        
        authorText.text = listItem.groupInfo.Author;
        nameText.text = listItem.groupInfo.Name;
        coverImage.texture = listItem.coverImage.texture;


        // Refresh list of player's maps
        List<FullMapData> mapInfos = null;
        bool async = false;
        isGroupDeleted = false;
        testRequest = null;

        if (listItem.groupInfo.MapType == GroupType.Own) mapInfos = new List<FullMapData>() { new FullMapData(listItem.groupInfo) };
        else if (listItem.isLocalItem) mapInfos = DatabaseManager.GetDownloadedMaps(listItem.groupInfo);
        else
        {
            async = true;
            loadingCircle.Play();
            ClearContent(beatmapsContent);
            DatabaseManager.GetMapsByTrackAsync(listItem.groupInfo, RefreshBeatmapsList, ShowError);
        }

        if(!async) RefreshBeatmapsList(mapInfos);
    }
    public void OpenModeration(TestRequest request, Project proj)
    {
        overlay.SetActive(true);
        ResetUI();
        ShowAlert("Moderation");

        string author = request.trackname.Split('-')[0];
        string name = request.trackname.Split('-')[1];
        
        authorText.text = author;
        nameText.text = name;

        string coverPath = request.filepath.Replace(".bsu", Project.ToString(proj.imageExtension));
        if (File.Exists(coverPath)) coverImage.texture = ProjectManager.LoadTexture(coverPath);
        else coverImage.texture = ProjectManager.defaultTrackTexture;
        
        testRequest = request;
        isGroupDeleted = false;


        // Refresh list of player's maps
        List<FullMapData> mapInfos = new List<FullMapData>();
        
        List<DifficultyInfo> dInfos = new List<DifficultyInfo>();
        foreach (var d in proj.difficulties)
        {
            dInfos.Add(new DifficultyInfo()
            {
                name = d.name,
                stars = d.stars,
                id = d.id
            });
        }
        
        mapInfos.Add(new FullMapData()
        {
            Author = author,
            Name = name,
            Difficulties = dInfos,
            MapperNick = proj.creatorNick
        });

        RefreshBeatmapsList(mapInfos);
    }

    void ResetUI()
    {
        ClearContent(beatmapsContent);
        ClearContent(difficultiesContent);
        
        footer.SetActive(false);
        
        errorPan.SetActive(false);
        alertPan.SetActive(false);
    }
    
    
    void RefreshBeatmapsList(List<FullMapData> mapInfos)
    {
        HelperUI.FillContent(beatmapsContent, mapInfos, (BeatmapUIItem ui, FullMapData info) =>
        {
            ui.Setup(info, mapInfos.Count == 1);
        });

        loadingCircle.Stop();
    }

    void RefreshBeatmapsList()
    {
        foreach (Transform child in beatmapsContent)
        {
            if(child.name == "Item") continue;
            BeatmapUIItem item = child.GetComponent<BeatmapUIItem>();
            item.Refresh();
        }
    }

    
    #region Buttons events

    #region List toggles change
    
    public void OnBeatmapItemClicked()
    {
        downloadPan.SetActive(false);
        footer.SetActive(false);
        
        ToggleGroup toggleGroup = beatmapsContent.GetComponent<ToggleGroup>();
        if (toggleGroup.ActiveToggles().Count() == 0)
        {
            ClearContent(difficultiesContent);
            currentMapInfo = null;
            return;
        }

        BeatmapUIItem item = toggleGroup.ActiveToggles().First().GetComponent<BeatmapUIItem>();
        OnBeatmapItemClicked(item);
    }
    public void OnBeatmapItemClicked(BeatmapUIItem item)
    {
        currentMapInfo = item.mapInfo;

        List<DifficultyInfo> difficulties = new List<DifficultyInfo>();
        if (item.mapInfo.Difficulties.Count == 0)
        {
            throw new NotImplementedException("Difficulties count is 0");
            //difficulties.Add(new DifficultyInfo()
            //{
            //    name = item.mapInfo.Difficulties,
            //    stars = item.mapInfo.difficultyStars,
            //    downloads = item.mapInfo.downloads,
            //    playCount = item.mapInfo.PlayCount,
            //    likes = item.mapInfo.Likes,
            //    dislikes = item.mapInfo.Dislikes
            //});
        }
        else
        {
            difficulties.AddRange(item.mapInfo.Difficulties);
        }


        int i = 0;
        FillContent<BeatmapUIItem, DifficultyInfo>(difficultiesContent, difficulties, (ui, info) =>
        {
            ui.Setup(info, i);
            i++;
        });
    }

    public void OnDifficultyItemClicked()
    {
        ToggleGroup toggleGroup = difficultiesContent.GetComponent<ToggleGroup>();
        if (toggleGroup.ActiveToggles().Count() == 0)
        {
            downloadPan.SetActive(false);
            footer.SetActive(false);
            return;
        }

        BeatmapUIItem item = toggleGroup.ActiveToggles().First().GetComponent<BeatmapUIItem>();

        currentDifficultyInfo = item.difficulty;

        string trackname = currentMapInfo.Author + "-" + currentMapInfo.Name;



        isDownloaded =
            ProjectManager.IsMapDownloaded(currentMapInfo.Author, currentMapInfo.Name, currentMapInfo.MapperNick) ||
            //currentMapInfo.group.groupType == GroupInfo.GroupType.Own;
            currentMapInfo.MapType == GroupType.Own;

        hasUpdate = currentMapInfo.MapType == GroupType.Own ? false : DatabaseManager.HasUpdateForMap(trackname, currentMapInfo.MapperNick);
        bool isTest = testRequest != null;

        if (testRequest != null)
        {
            isDownloaded = true;
            testRequest.difficultyId = currentDifficultyInfo.id;
        }

        if (isTest)
        {
            downloadPan.SetActive(false);
            footer.SetActive(true);
        }
        else
        {
            downloadPan.SetActive(hasUpdate || (!isDownloaded && !isGroupDeleted));
            downloadBtn.SetActive(!hasUpdate || (!isDownloaded && !isGroupDeleted));
            updateBtn.SetActive(isDownloaded && hasUpdate);

            footer.SetActive(isDownloaded);
        }

        //bool doServerStuff =
        //    !currentMapInfo.isMapDeleted &&
        //    Application.internetReachability != NetworkReachability.NotReachable &&
        //    testRequest == null &&
        //    currentMapInfo.MapType == GroupType.Author;
    }


    #endregion

    #region Game section buttons events

    public void StartBtnCicked()
    {
        if (LobbyManager.isPickingMap)
        {
            lobbyUI.OnMapPicked(currentMapInfo, currentDifficultyInfo);
            OnCloseBtnClicked();
            return;
        }


        SceneloadParameters parameters = null;
        
        if (testRequest != null)
        {
            parameters = SceneloadParameters.ModerationPreset(testRequest, currentDifficultyInfo);
        }
        else if (currentMapInfo.MapType == GroupType.Own)
        {
            parameters = SceneloadParameters.OwnMusicPreset(currentMapInfo as OwnMapData, modsUI.selectedMods);
        }
        else if (currentMapInfo.MapType == GroupType.Author)
        {
            if (currentMapInfo.Trackname == "Beat Slayer-Tutorial")
            {
                parameters = SceneloadParameters.TutorialPreset(currentMapInfo, currentDifficultyInfo);
            }
            else
            {
                parameters = SceneloadParameters.AuthorMusicPreset(currentMapInfo, currentDifficultyInfo, modsUI.selectedMods);
            }
        }
        //else if (currentMapInfo.MapType == GroupType.Tutorial)
        //{
            
        //}

        DatabaseManager.SendStatistics(currentMapInfo.Trackname, currentMapInfo.MapperNick, currentDifficultyInfo.id, StatisticsKeyType.Launch);

        SceneController.instance.LoadScene(parameters);
    }
    public void OnPracticeBtnClick()
    {
        practiceModeUI.ShowWindow(currentMapInfo, currentDifficultyInfo);
    }

    public void OnDownloadBtnClicked()
    {
        progressBar.value = 0;
        mapsDownloadQueuer.AddTask(currentMapInfo.Trackname, currentMapInfo.MapperNick);
    }

    public void OnCancelBtnClicked()
    {
        mapsDownloadQueuer.RemoveTask(MapsDownloadQueuerBackground.ActiveItem);
    }
    
    public void OnDeleteBtnClicked()
    {
        if (testRequest == null)
        {
            ProjectManager.DeleteProject(currentMapInfo.Trackname, currentMapInfo.MapperNick);
            RefreshBeatmapsList();
            OnBeatmapItemClicked();
        
            menu.TrackListUI.ReloadDownloadedList();
        }
        else
        {
            TestManager.DeleteRequest(true);
            overlay.SetActive(false);
        }
    }

    public void OnUpdateBtnClicked()
    {
        // Delete old project
        ProjectManager.DeleteProject(currentMapInfo.Trackname, currentMapInfo.MapperNick);
        RefreshBeatmapsList();
        
        // Download update
        OnDownloadBtnClicked();
    }
    
    public void OnCloseBtnClicked()
    {
        if (testRequest != null)
        {
            TestManager.DeleteRequest(true);
        }
        
        overlay.SetActive(false);

        currentDifficultyInfo = null;
        currentMapInfo = null;
    }
    
    
    #endregion
    
    #endregion

    void ShowError(string message)
    {
        errorPan.SetActive(true);

        if (message == "Group has been deleted")
        {
            errorText.text = LocalizationManager.Localize("GroupDeleted");
        }
        else
        {
            errorText.text = message;
        }
        
        loadingCircle.Stop();

        if (message == "Group has been deleted")
        {
            isGroupDeleted = true;
        }
    }

    void ShowAlert(string message)
    {
        alertPan.SetActive(true);
        alertText.text = message;
    }
    

    
    /// <summary>
    /// Fill content with list and make some implementation
    /// </summary>
    /// <param name="content">Content transform</param>
    /// <param name="list">List of classes</param>
    /// <param name="implementation"></param>
    /// <typeparam name="T">Content child UI class</typeparam>
    /// <typeparam name="T2">Info class which needs to implement into UI</typeparam>
    private void FillContent<T, T2>(Transform content, IEnumerable<T2> list, Action<T, T2> implementation)
    {
        GameObject prefab = ClearContent(content);
        prefab.SetActive(true);

        foreach (T2 infoClass in list)
        {
            T itemUI = Instantiate(prefab, content).GetComponent<T>();
            implementation(itemUI, infoClass);
        }
        
        prefab.SetActive(false);
    }

    private GameObject ClearContent(Transform content)
    {
        Transform prefabTransform = content.GetChild(0);
        foreach(Transform child in content) if (child != prefabTransform) Destroy(child.gameObject);
        return content.GetChild(0).gameObject;
    }
}