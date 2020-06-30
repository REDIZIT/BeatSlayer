using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Assets.SimpleLocalization;
using BeatSlayerServer.Multiplayer.Accounts;
using CoversManagement;
using InGame.Helpers;
using InGame.Menu;
using InGame.SceneManagement;
using InGame.UI;
using Newtonsoft.Json;
using Pixelplacement;
using ProjectManagement;
using Testing;
using UnityEngine;
using UnityEngine.UI;
using Web;
using GroupInfo = ProjectManagement.GroupInfo;
using MapInfo = ProjectManagement.MapInfo;

public class BeatmapUI : MonoBehaviour
{
    public DatabaseScript database;
    public MenuScript_v2 menu;
    public MenuAudioManager menuAudioManager;
    public PracticeModeUI practiceModeUI;
    //public BeatmapAudio bmAudio;
    
    public GameObject overlay;

    private MapInfo currentMapInfo;
    private DifficultyInfo currentDifficultyInfo;
    private bool isGroupDeleted;

    [Header("UI")] 
    public RawImage coverImage;
    public Text authorText, nameText;
    public GameObject approvedIcon;
    public Transform beatmapsContent, difficultiesContent;
    public GameObject recordPan, modsPan, downloadPan;
    public Button backBtn;
    public GameObject errorPan, alertPan;
    public Text errorText, alertText;

    public LoadingCircle loadingCircle;

    public TestRequest testRequest;

    [Header("Footer UI")] 
    public GameObject footer;
    public GameObject playBtn, locationBtn, deleteBtn;
    

    [Header("Download UI")] 
    public Slider progressBar;
    public Text progressText;
    public GameObject downloadBtn, updateBtn;

    [Header("Record UI")] 
    public Text recordText;
    public Text leaderboardPlaceText;



    public void Open(TrackListItem listItem)
    {
        overlay.SetActive(true);
        ResetUI();
        
        authorText.text = listItem.groupInfo.author;
        nameText.text = listItem.groupInfo.name;
        coverImage.texture = listItem.coverImage.texture;


        // Refresh list of player's maps
        List<MapInfo> mapInfos = null;
        bool async = false;
        isGroupDeleted = false;
        testRequest = null;

        if (listItem.groupInfo.groupType == GroupInfo.GroupType.Own) mapInfos = database.GetCustomMaps(listItem.groupInfo);
        else if (listItem.isLocalItem) mapInfos = database.GetDownloadedMaps(listItem.groupInfo);
        else
        {
            async = true;
            loadingCircle.Play();
            ClearContent(beatmapsContent);
            database.GetMapsByTrackAsync(listItem.groupInfo, RefreshBeatmapsList, ShowError);
        }

        if(!async) RefreshBeatmapsList(mapInfos);


        Debug.Log("Open");
        menuAudioManager.OnMapSelected(listItem.groupInfo);
    }
    public void Open(GroupInfoExtended group)
    {
        overlay.SetActive(true);
        ResetUI();

        authorText.text = group.author;
        nameText.text = group.name;
        CoversManager.AddPackages(new List<CoverRequestPackage>()
        {
            new CoverRequestPackage(coverImage, group.author + "-" + group.name, priority: true)
        });


        // Refresh list of player's maps
        List<MapInfo> mapInfos = null;
        bool async = false;
        isGroupDeleted = false;
        testRequest = null;

        if (group.groupType == GroupInfo.GroupType.Own) mapInfos = database.GetCustomMaps(group);
        else
        {
            async = true;
            loadingCircle.Play();
            ClearContent(beatmapsContent);
            database.GetMapsByTrackAsync(group, RefreshBeatmapsList, ShowError);
        }

        if (!async) RefreshBeatmapsList(mapInfos);
    }

    public void OpenModeration(TestRequest request, Project proj)
    {
        overlay.SetActive(true);
        //GetComponent<StateMachine>().ChangeState("TracksScreen");
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
        List<MapInfo> mapInfos = new List<MapInfo>();
        
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
        
        mapInfos.Add(new MapInfo()
        {
            group = new GroupInfo()
            {
                author = author,
                name = name, 
                mapsCount = 1
            },
            difficulties = dInfos,
            nick = proj.creatorNick
        });

        RefreshBeatmapsList(mapInfos);
    }

    void ResetUI()
    {
        ClearContent(beatmapsContent);
        ClearContent(difficultiesContent);
        
        modsPan.SetActive(false);
        recordPan.SetActive(false);
        footer.SetActive(false);
        
        errorPan.SetActive(false);
        alertPan.SetActive(false);
        
        playBtn.SetActive(true);
        locationBtn.SetActive(true);
        deleteBtn.SetActive(true);
        
        
    }
    
    
    void RefreshBeatmapsList(List<MapInfo> mapInfos)
    {
        ClearContent(difficultiesContent);

        recordPan.SetActive(false);
        modsPan.SetActive(false);
        downloadPan.SetActive(false);
        footer.SetActive(false);

        FillContent<BeatmapUIItem, MapInfo>(beatmapsContent, mapInfos, (BeatmapUIItem ui, MapInfo info) =>
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
        recordPan.SetActive(false);
        modsPan.SetActive(false);
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
        if (item.mapInfo.difficulties.Count == 0)
        {
            difficulties.Add(new DifficultyInfo()
            {
                name = item.mapInfo.difficultyName,
                stars = item.mapInfo.difficultyStars,
                downloads = item.mapInfo.downloads,
                playCount = item.mapInfo.playCount,
                likes = item.mapInfo.likes,
                dislikes = item.mapInfo.dislikes
            });
        }
        else
        {
            difficulties.AddRange(item.mapInfo.difficulties);
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
            recordPan.SetActive(false);
            modsPan.SetActive(false);
            downloadPan.SetActive(false);
            footer.SetActive(false);
            return;
        }

        BeatmapUIItem item = toggleGroup.ActiveToggles().First().GetComponent<BeatmapUIItem>();

        currentDifficultyInfo = item.difficulty;
        Debug.Log("Clicked id " + item.difficulty.id);
        string trackname = currentMapInfo.author + "-" + currentMapInfo.name;



        bool isDownloaded =
            ProjectManager.IsMapDownloaded(currentMapInfo.author, currentMapInfo.name, currentMapInfo.nick) ||
            currentMapInfo.group.groupType == GroupInfo.GroupType.Own;
        bool hasUpdate = currentMapInfo.group.groupType == GroupInfo.GroupType.Own ? false : DatabaseScript.HasUpdateForMap(trackname, currentMapInfo.nick);
        bool isTest = testRequest != null;

        if (testRequest != null)
        {
            isDownloaded = true;
            testRequest.difficultyId = currentDifficultyInfo.id;
        }

        if (isTest)
        {
            downloadPan.SetActive(false);
            recordPan.SetActive(false);
            footer.SetActive(true);

            playBtn.SetActive(true);
            deleteBtn.SetActive(true);
        }
        else
        {
            downloadPan.SetActive(hasUpdate || (!isDownloaded && !isGroupDeleted));
            downloadBtn.SetActive(!hasUpdate || (!isDownloaded && !isGroupDeleted));
            updateBtn.SetActive(isDownloaded && hasUpdate);

            recordPan.SetActive(!isGroupDeleted);
            modsPan.SetActive(false);

            footer.SetActive(isDownloaded);
            playBtn.SetActive(!isGroupDeleted && !hasUpdate);
            locationBtn.SetActive(!isGroupDeleted);
            deleteBtn.SetActive(true);

        }




        recordText.text = "";
        leaderboardPlaceText.text = "";

        bool doServerStuff =
            !currentMapInfo.isMapDeleted &&
            Application.internetReachability != NetworkReachability.NotReachable &&
            testRequest == null &&
            currentMapInfo.group.groupType == GroupInfo.GroupType.Author;

        recordPan.SetActive(false);

        if (doServerStuff && AccountManager.LegacyAccount != null)
        {
            recordPan.SetActive(true);
            leaderboardPlaceText.text = LocalizationManager.Localize("Loading");
            AccountManager.GetBestReplay(AccountManager.LegacyAccount.nick, currentMapInfo.author + "-" + currentMapInfo.name, currentMapInfo.nick, replay =>
            {
                if (replay == null)
                {
                    recordText.text = "";
                    leaderboardPlaceText.text = LocalizationManager.Localize("RecordNotSet");
                }
                else
                {
                    recordText.text =
                        $"{replay.Score}   <color=#f00>{replay.Missed}</color>   <color=#eee>{replay.Accuracy * 100}%</color>   <color=#08f>{replay.RP}</color>";

                    leaderboardPlaceText.text = LocalizationManager.Localize("Loading");

                    /*AccountManager.GetMapLeaderboardPlace(AccountManager.LegacyAccount.nick,
                        currentMapInfo.author + "-" + currentMapInfo.name, currentMapInfo.nick,
                        (place =>
                        {
                            leaderboardPlaceText.text = $"#{place} " + LocalizationManager.Localize("InMapLeaderboard");
                        }));*/
                }
            });
        }
    }


    #endregion

    #region Game section buttons events

    public void StartBtnCicked()
    {
        // !!!!!  Set defaults coz of mods  !!!!!
        SSytem.instance.SetInt("CubesSpeed", 10);
        SSytem.instance.SetInt("MusicSpeed", 10);

        SceneloadParameters parameters = null;
        
        if (testRequest != null)
        {
            Debug.Log("Moderate with id: " + currentDifficultyInfo.id);
            parameters = SceneloadParameters.ModerationPreset(testRequest, currentDifficultyInfo);
        }
        else if (currentMapInfo.group.groupType == GroupInfo.GroupType.Own)
        {
            parameters = SceneloadParameters.OwnMusicPreset(currentMapInfo.filepath, currentMapInfo);
        }
        else if (currentMapInfo.group.groupType == GroupInfo.GroupType.Author)
        {
            parameters = SceneloadParameters.AuthorMusicPreset(currentMapInfo, currentDifficultyInfo);
        }

        SceneController.instance.LoadScene(parameters);
    }
    public void OnPracticeBtnClick()
    {
        practiceModeUI.ShowWindow(currentMapInfo, currentDifficultyInfo);
    }

    public void OnDownloadBtnClicked()
    {
        progressBar.gameObject.SetActive(true);
        progressText.text = LocalizationManager.Localize("Waiting");
        progressBar.value = 0;
        
        downloadBtn.SetActive(false);
        updateBtn.SetActive(false);
        playBtn.SetActive(false);
        
        footer.SetActive(false);
        
        backBtn.interactable = false;

        WebAPI.DownloadMap(currentMapInfo.author + "-" + currentMapInfo.name, currentMapInfo.nick, args =>
        {
            progressBar.value = args.ProgressPercentage;
            progressText.text = args.ProgressPercentage + "%";
        }, args =>
        {
            bool downloaded = true;
            backBtn.interactable = true;
            if (args.Cancelled || args.Error != null)
            {
                downloaded = false;
            }
            downloadBtn.SetActive(!downloaded);
            updateBtn.SetActive(false);
            playBtn.SetActive(downloaded);
            
            progressBar.gameObject.SetActive(false);
            
            recordPan.SetActive(downloaded);
            modsPan.SetActive(false);
            downloadPan.SetActive(!downloaded);
            footer.SetActive(downloaded);
            
            menu.TrackListUI.ReloadDownloadedList();
            RefreshBeatmapsList();
        });
    }

    public void OnCancelBtnClicked()
    {
        WebAPI.CancelMapDownloading();
    }
    
    public void OnDeleteBtnClicked()
    {
        if (testRequest == null)
        {
            ProjectManager.DeleteProject(currentMapInfo.author + "-" + currentMapInfo.name, currentMapInfo.nick);
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
        ProjectManager.DeleteProject(currentMapInfo.author + "-" + currentMapInfo.name, currentMapInfo.nick);
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

        //bmAudio.OnClose();
        
        overlay.SetActive(false);
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
    public void FillContent<T, T2>(Transform content, IEnumerable<T2> list, Action<T, T2> implementation)
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
    
    public void FillContent(Transform content, int count, Action<GameObject, int> implementation)
    {
        GameObject prefab = ClearContent(content);
        prefab.SetActive(true);

        for (int i = 0; i < count; i++)
        {
            GameObject item = Instantiate(prefab, content);
            implementation(item, i);
        }

        prefab.SetActive(false);
    }

    public GameObject ClearContent(Transform content)
    {
        Transform prefabTransform = content.GetChild(0);
        foreach(Transform child in content) if (child != prefabTransform) Destroy(child.gameObject);
        return content.GetChild(0).gameObject;
    }
}