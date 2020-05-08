using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using System.ComponentModel;
using System.Diagnostics;
using UnityEngine.UI;
using Pixelplacement;
//using UnityEngine.SceneManagement;
using UnityEngine.UI.Extensions;
using Assets.SimpleLocalization;
//using GooglePlayGames;
//using GooglePlayGames.BasicApi;
using SimpleFileBrowser;
using UnityEngine.Video;
using System.Xml.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Reflection;
using CoversManagement;
//using System.Runtime.Serialization.Formatters.Binary;
using InGame.SceneManagement;
//using SaveManagement;
using DatabaseManagement;
using InGame.Helpers;
using ProjectManagement;
using Testing;
using Debug = UnityEngine.Debug;

public class MenuScript_v2 : MonoBehaviour
{
    public DatabaseScript database;
    public DailyRewarder dailyRewarder;
    public BeatmapUI beatmapUI;
    
    public DownloadHelper downloadHelper { get { return GetComponent<DownloadHelper>(); } }
    public TrackListUI TrackListUI { get { return GetComponent<TrackListUI>(); } }
    public AdvancedSaveManager prefsManager { get { return GetComponent<AdvancedSaveManager>(); } }
    public AccountManager accountManager;

    public SpectrumVisualizer spectrumVisualizer;
    public AudioSource secondAudioSource;


    public GameObject debugConsole;
    public GameObject[] screens;

    public Transform[] btnsToRepos;

    public GameObject playCustomBtn;

    public Text[] coinsTexts;
    public GameObject[] mapLockers;
    public Button selectMapBtn;

    [Header("Track UI")]
    public Text musicLoadingText;
    public GameObject authorSplitter;
    public GameObject noInternetAuthors;

    [Header("UI")]
    public GameObject rankingTipLocker;
    [HideInInspector] public List<CustomUI> resizableUI = new List<CustomUI>();
    public GameObject editorAvailableWindow;
    public RectTransform bgVideo;
    public VideoPlayer videoPlayer;
    public GameObject newVersionLocker;
    public Text versionText, newVersionText;
    public Text scoreMultiplyText;

    [Header("Misc")]
    public GameObject newTracksImg;


    public Transform tutorialLocker;
    public AudioSource aSource;













    private void Awake()
    {
        Application.targetFrameRate = 60;
        if(Time.timeScale != 1)
        {
            Debug.LogWarning("Time scale in menu is not 1!");
            Time.timeScale = 1;
        }
        debugConsole.SetActive(true);
        
        /*#if UNITY_EDITOR
        UrlsChecker.IsGameWorkingWithLocalhost();
        #endif*/

        m_currentOrientation = Screen.orientation;
        nextOrientationCheckTime = Time.realtimeSinceStartup + 1f;

        GetComponent<SceneController>().Init(GetComponent<SceneControllerUI>());

        Database.Init();
        ProjectManager.defaultTrackTexture = defaultTrackTexture;


        if (!Directory.Exists(Application.persistentDataPath + "/maps")) Directory.CreateDirectory(Application.persistentDataPath + "/maps");

        if (!Directory.Exists(Application.persistentDataPath + "/data")) Directory.CreateDirectory(Application.persistentDataPath + "/data");
        if (!Directory.Exists(Application.persistentDataPath + "/data/account")) Directory.CreateDirectory(Application.persistentDataPath + "/data/account");
    }

    private void Start()
    {
        versionText.text = Application.version.ToString() + " by " + Application.installerName + " in " + Application.installMode.ToString() + " mode";

        // Google Play Services Auth
        /*PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.Activate();
        Social.localUser.Authenticate(LoadGPSUser);*/


        TranslateStart();
        UnlockMapsTranslate();

        HandleDev();
        HandleSettings();

        TestManager.Setup(this);

        TrackListUI.RefreshDownloadedList();

        #region Loading prefs

        if (File.Exists(Application.persistentDataPath + "/Money.txt"))
        {
            prefsManager.prefs.coins = 999999;
            prefsManager.Save();
        }
        if (File.Exists(Application.persistentDataPath + "/nomoney.txt"))
        {
            prefsManager.prefs.coins = 0;
            prefsManager.Save();
        }

        mapHss.StartingScreen = prefsManager.prefs.selectedMapId;
        mapLockers[0].SetActive(!prefsManager.prefs.mapUnlocked0);
        mapLockers[1].SetActive(!prefsManager.prefs.mapUnlocked1);
        mapLockers[2].SetActive(!prefsManager.prefs.mapUnlocked2);
        mapLockers[3].SetActive(!prefsManager.prefs.mapUnlocked3);
        coinsTexts[0].text = prefsManager.prefs.coins.ToString();
        coinsTexts[1].text = prefsManager.prefs.coins.ToString();
        coinsTexts[2].text = prefsManager.prefs.coins.ToString();

        #endregion

        // Actions with server
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (!File.Exists(Application.persistentDataPath + "/DontUpdate.txt") && Time.realtimeSinceStartup <= 30)
            {
                StartCoroutine(CheckForUpdatesAsync());
            }

            CheckServerMessage();
        }

        dailyRewarder.Calculate();
        CheckAchievement();

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += delegate { videoPlayer.Play(); };
        canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>().rect.size;
        
        // Та нахуй воно мэне нада xD
        /*if (Screen.height > Screen.width) canvasRect = new Vector2(canvasRect.y, canvasRect.x);
        UpdateOrientationHanlder(true);*/
    }

    
    private void Update()
    {

        UpdateTutorial();

        WebHandlers_Handle();

        //UpdateOrientationHanlder();

        if (Input.GetKeyDown(KeyCode.Escape)) OnExitSwipe();

        TestManager.CheckUpdates();
        TestManager.CheckModerationUpdates();
    }


    public void HandleSettings()
    {
        if(debugConsole != null)
        {
            debugConsole.SetActive(SSytem.instance.GetInt("EnableConsole") == 1);
            debugConsole.transform.GetChild(3).gameObject.SetActive(SSytem.instance.GetInt("EnableFps") == 1);
        }
        

        playCustomBtn.SetActive(SSytem.instance.GetInt("EnableFileLoad") == 1);

        GetComponent<AudioSource>().volume = SSytem.instance.GetFloat("MenuMusicVolume") * 0.2f;

        //GetComponent<MenuSpectrum>().useMenuMusic = SSytem.instance.GetBool("MenuMusic");
        //GetComponent<MenuSpectrum>().useKickVideo = SSytem.instance.GetBool("KickVideo");
        //spectrumVisualizer.Start();
    }






    private const float ORIENTATION_CHECK_INTERVAL = 0.2f;

    private float nextOrientationCheckTime;

    private static ScreenOrientation m_currentOrientation;
    public static ScreenOrientation CurrentOrientation
    {
        get
        {
            return m_currentOrientation;
        }
        private set
        {
            if (m_currentOrientation != value)
            {
                m_currentOrientation = value;
                Screen.orientation = value;

                if (OnScreenOrientationChanged != null)
                    OnScreenOrientationChanged(value);
            }
        }
    }
    public static bool AutoRotateScreen = true;
    public static event System.Action<ScreenOrientation> OnScreenOrientationChanged = null;

    bool isVertical;
    Vector2 canvasRect;
    void UpdateOrientationHanlder(bool force = false)
    {
        //if (Screen.height > Screen.width) isPortrait = true;\
        bool onChange = false;

        if (Screen.height > Screen.width)
        {
            if (!isVertical)
            {
                isVertical = true;
                foreach (CustomUI item in resizableUI) item.OnOrientationChange(isVertical);
                onChange = true;
            }
        }
        else
        {
            if (isVertical)
            {
                isVertical = false;
                foreach (CustomUI item in resizableUI) item.OnOrientationChange(isVertical);
                onChange = true;
            }
        }

        if (!onChange && !force) return;

        SettingsRescale(isVertical);
        //ListRescale(isVertical);


        // Background video

        float videoNativeHeight = 1440;
        float videoNativeWidth = 2960;

        var canvas = GameObject.Find("Canvas").GetComponent<RectTransform>();

        float canvasHeight = isVertical ? canvasRect.x : canvasRect.y;

        float heightDifference = canvasHeight - videoNativeHeight;
        float widthRatio = videoNativeWidth / videoNativeHeight;

        float rectHeight = canvasHeight;
        float rectWidth = canvasHeight * widthRatio;

        bgVideo.sizeDelta = new Vector2(rectWidth, rectHeight);
    }
    public static void ForceOrientation(ScreenOrientation orientation)
    {
        if (orientation == ScreenOrientation.AutoRotation)
            AutoRotateScreen = true;
        else if (orientation != ScreenOrientation.Unknown)
        {
            AutoRotateScreen = false;
            CurrentOrientation = orientation;
        }
    }

    void UpdateTutorial()
    {
        bool isPortait = false; // Screen.height > Screen.width

        foreach (Transform page in tutorialLocker.GetChild(1).GetChild(0))
        {
            foreach (Transform item in page)
            {
                if (item.name == "LandVer")
                {
                    item.gameObject.SetActive(!isPortait);
                }
                else if (item.name == "PortVer")
                {
                    item.gameObject.SetActive(isPortait);
                }
            }
        }
    }

    void TranslateStart()
    {
        if(Application.isEditor)
        {
            LocalizationManager.Language = "Russian";
            return;
        }
        if (Application.systemLanguage == SystemLanguage.Russian || Application.systemLanguage == SystemLanguage.Ukrainian) LocalizationManager.Language = "Russian";
        else if (Application.systemLanguage == SystemLanguage.French) LocalizationManager.Language = "French";
        else LocalizationManager.Language = "English";
        
        LocalizationManager.Read();
    }

    public void ShowAchivements()
    {
        Social.ShowAchievementsUI();
    }
    public void ShowLeader()
    {
        if (prefsManager.prefs.showLeaderboardTip)
        {
            rankingTipLocker.SetActive(true);
            prefsManager.prefs.showLeaderboardTip = false;
        }
        else Social.ShowLeaderboardUI();
    }

    

    void HandleDev()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/saved"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/saved");
            tutorialLocker.gameObject.SetActive(true);
            prefsManager.Save();
        }
        if (!Directory.Exists(Application.persistentDataPath + "/custom"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/custom");
            tutorialLocker.gameObject.SetActive(true);
        }
    }

    

    #region Music lists

    #region Music lists: Refresh

    public void OnRefreshAuthorBtnClick()
    {
        //StartCoroutine(database.LoadTracksDataBaseAsyncLegacy(true));
        StartCoroutine(database.LoadDatabaseAsync(true));
    }
    

    #endregion

    

    //public void OnSearchOwnMusic(InputField field)
    //{
    //    string search = field.text.ToLower();

    //    //foreach (Transform child in listController.ownMusicList) child.gameObject.SetActive(false);

    //    List<UserTrackClass> sorted = listController.ownMusicArray.Where(c => (c.author + "-" + c.name).ToLower().Contains(search)).ToList();

    //    float contentSize = 0;
    //    for (int i = 0; i < listController.ownMusicList.childCount; i++)
    //    {
    //        GameObject item = listController.ownMusicList.GetChild(i).gameObject;
    //        TrackListItem track = item.GetComponent<TrackListItem>();
    //        //item.SetActive(sorted.Exists(c => c.author == track.group.author && c.name == track.group.name));
    //    }
        
    //    listController.authorMusicList.GetComponent<RectTransform>().sizeDelta = new Vector2(0, contentSize + 15);
    //}


    public State configScreen;
    public Text trackName, trackAuthor, configMapCreatorText;
    public Image trackImg;
    public Sprite defaultTrackSprite;
    public Texture2D defaultTrackTexture;
    public void OnTrackItemClicked(MenuTrackButton btn)
    {
        selectedTrack = btn;
        configScreen.ChangeState(configScreen.gameObject);

        trackName.text = btn.name;
        trackAuthor.text = btn.author;


        trackRecordText.text = LocalizationManager.Localize("Your record is") + " " + (prefsManager.prefs.GetRecord(selectedTrack.fullname) != 0 ? prefsManager.prefs.GetRecord(selectedTrack.fullname).ToString() : LocalizationManager.Localize("NotSetYet"));

        sourceText.text = selectedTrack.source != "" ? "Source" + ": " + selectedTrack.source : "";

        configMapCreatorText.gameObject.SetActive(selectedTrack.item.mapCreator != "");
        configMapCreatorText.text = LocalizationManager.Localize("MapCreator") + ": " + selectedTrack.item.mapCreator;

        trackImg.sprite = selectedTrack.img.sprite;

        TrackPlayChange(!btn.needDownloading);
        if(btn.path != "")
        {
            trackDeleteBtn.SetActive(false);
        }
    }

    [Header("Track info")]
    public GameObject trackInfoLocker;
    public Text trackInfoAuthorText, trackInfoNameText, trackInfoMapsCountText;
    public GameObject trackInfoMapPrefab;
    public Transform trackInfoMapContent;
    public RawImage trackInfoCoverImage;

    public void OnTrackItemClicked(TrackListItem listItem)
    {
        beatmapUI.Open(listItem);
        return;
        trackInfoLocker.GetComponent<Animator>().Play("TrackWindow-Open");
        trackInfoAuthorText.text = listItem.groupInfo.author;
        trackInfoNameText.text = listItem.groupInfo.name;
        trackInfoMapsCountText.text = LocalizationManager.Localize("MapsCount") + ": " + listItem.groupInfo.mapsCount;
        trackInfoCoverImage.texture = listItem.coverImage.texture;

        foreach (Transform child in trackInfoMapContent) Destroy(child.gameObject);

        // Refresh list of player's maps
        List<MapInfo> mapInfos;
//        TrackRecordGroup records = TheGreat.GetRecords();

        if (listItem.isCustomMusic) mapInfos = database.GetCustomMaps(listItem.groupInfo);
        else if (listItem.isLocalItem) mapInfos = database.GetDownloadedMaps(listItem.groupInfo);
        else mapInfos = database.GetMapsByTrack(listItem.groupInfo);

        float contentHeight = -10;
        for (int i = 0; i < mapInfos.Count; i++)
        {
            MapListItem mapItem = Instantiate(trackInfoMapPrefab, trackInfoMapContent).GetComponent<MapListItem>();

            bool isPassed = accountManager.IsPassed(mapInfos[i].group.author, mapInfos[i].group.name, mapInfos[i].nick);

            if (listItem.isCustomMusic)
            {
                mapItem.SetupForLocalFile(this, mapInfos[i]);
            }
            else mapItem.Setup(this, isPassed, mapInfos[i]);

            //TrackRecord record = TheGreat.GetRecord(records, mapInfos[i].group.author, mapInfos[i].group.name, mapInfos[i].nick);
            //mapItem.recordText.text = record == null ? "" : LocalizationManager.Localize("Record") + ": " + record.score;

            contentHeight += 191.3f + 10;
        }
        trackInfoMapContent.GetComponent<RectTransform>().sizeDelta = new Vector2(trackInfoMapContent.GetComponent<RectTransform>().sizeDelta.x, contentHeight);
    }
   
    public void OnExtendSearchClicked(Animator anim)
    {
        bool isExtendSearchShowed = anim.transform.GetChild(1).GetComponent<CanvasGroup>().alpha == 1f;
        anim.Play(isExtendSearchShowed ? "HideExtend" : "ShowExtend");
    }

    #endregion

    #region UI (Base)

    public void OnDifficultChange()
    {
        scoreMultiplyText.text = LocalizationManager.Localize("Score") + ": <color=#F80>x" + GetComponent<SettingsManager>().CalculateScoreMultiplier() + "</color>";
    }

    #endregion


    // == Rescale
    // ============================================================================================================================
    public Transform[] settingsRescalingObjs;
    public float settingsRescaleOffset;
    bool settingsShowedPortrait;
    public void SettingsRescale(bool isPortrait)
    {
        return;
        if (isPortrait)
        {
            if (!settingsShowedPortrait)
            {
                settingsShowedPortrait = true;
                for (int i = 0; i < settingsRescalingObjs.Length; i++)
                {
                    settingsRescalingObjs[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(settingsRescalingObjs[i].GetComponent<RectTransform>().anchoredPosition.x, settingsRescalingObjs[i].GetComponent<RectTransform>().anchoredPosition.y - settingsRescaleOffset);
                }

                foreach (RectTransform btn in btnsToRepos)
                {
                    Vector2 min = btn.offsetMin;
                    Vector2 max = btn.offsetMax;

                    Vector2 pos = btn.anchoredPosition;

                    btn.anchorMin = new Vector2(btn.anchorMin.x, 0);
                    btn.anchorMax = new Vector2(btn.anchorMax.x, 0);

                    btn.offsetMin = min;
                    btn.offsetMax = max;
                    btn.anchoredPosition = new Vector2(pos.x, -pos.y);
                }
            }
        }
        else
        {
            if (settingsShowedPortrait)
            {
                settingsShowedPortrait = false;
                for (int i = 0; i < settingsRescalingObjs.Length; i++)
                {
                    //settingsRescalingObjs[i].GetComponent<RectTransform>().anchoredPosition = settingsRescalingDefaultPoses[i];
                    settingsRescalingObjs[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(settingsRescalingObjs[i].GetComponent<RectTransform>().anchoredPosition.x, settingsRescalingObjs[i].GetComponent<RectTransform>().anchoredPosition.y + settingsRescaleOffset);
                }

                foreach (RectTransform btn in btnsToRepos)
                {
                    Vector2 min = btn.offsetMin;
                    Vector2 max = btn.offsetMax;

                    Vector2 pos = btn.anchoredPosition;

                    btn.anchorMin = new Vector2(btn.anchorMin.x, 1);
                    btn.anchorMax = new Vector2(btn.anchorMax.x, 1);

                    btn.offsetMin = min;
                    btn.offsetMax = max;
                    btn.anchoredPosition = new Vector2(pos.x, -pos.y);
                }
            }
        }
    }

    bool isPortraitHandled;
    public StateMachine Main_UI;
    public RectTransform selectMapRect;
    public GameObject achievementBtn, leaderboardBtn;
    //public void ListRescale(bool isPortrait)
    //{
    //    if (isPortrait)
    //    {
    //        if (!isPortraitHandled)
    //        {
    //            isPortraitHandled = true;
    //            foreach (Transform child in listController.authorMusicList)
    //            {
    //                if (child.name == "MusicLoadingText") continue;
    //                else if (child.GetComponent<MenuTrackButton>() != null) child.GetComponent<MenuTrackButton>().Rescale(true);

    //            }
    //            foreach (Transform child in listController.ownMusicList)
    //            {
    //                //child.GetComponent<MenuTrackButton>().Rescale(true);
    //            }

    //            selectMapRect.offsetMin = new Vector2(0, selectMapRect.offsetMin.y);
    //            selectMapRect.offsetMax = new Vector2(0, selectMapRect.offsetMax.y);

    //            RectTransform mapScreen = selectMapRect.GetChild(0).GetChild(0).GetComponent<RectTransform>();
    //            mapScreen.offsetMin = new Vector2(0, mapScreen.offsetMin.y);
    //            mapScreen.offsetMax = new Vector2(0, mapScreen.offsetMax.y);

    //            achievementBtn.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300, -35);
    //            leaderboardBtn.GetComponent<RectTransform>().anchoredPosition = new Vector3(300, -35);

    //            // Refresh Main_UI. IMPORTANT! Allow Reentry must be TRUE!!
    //            selectMapRect.GetComponent<StateMachine>().ChangeState(0);
    //            Main_UI.ChangeState(Main_UI.currentState);
    //        }
    //    }
    //    else
    //    {
    //        if (isPortraitHandled)
    //        {
    //            isPortraitHandled = false;
    //            foreach (Transform child in listController.ownMusicList)
    //            {
    //                //child.GetComponent<MenuTrackButton>().Rescale(false);
    //            }
    //            foreach (Transform child in listController.authorMusicList)
    //            {
    //                if (child.GetComponent<MenuTrackButton>() != null) child.GetComponent<MenuTrackButton>().Rescale(false);
    //            }

    //            selectMapRect.offsetMin = new Vector2(250, selectMapRect.offsetMin.y);
    //            selectMapRect.offsetMax = new Vector2(-250, selectMapRect.offsetMax.y);
    //            //Debug.Log("Landscape");

    //            RectTransform mapScreen = selectMapRect.GetChild(0).GetChild(0).GetComponent<RectTransform>();
    //            mapScreen.offsetMin = new Vector2(162.8f, mapScreen.offsetMin.y);
    //            mapScreen.offsetMax = new Vector2(-162.8f, mapScreen.offsetMax.y);

    //            achievementBtn.GetComponent<RectTransform>().anchoredPosition = new Vector3(-600, 150);
    //            leaderboardBtn.GetComponent<RectTransform>().anchoredPosition = new Vector3(600, 150);

    //            // Refresh Main_UI. IMPORTANT! Allow Reentry must be TRUE!!
    //            selectMapRect.GetComponent<StateMachine>().ChangeState(selectMapRect.GetComponent<StateMachine>().currentState);
    //            Main_UI.ChangeState(Main_UI.currentState);
    //        }
    //    }
    //}


    public HorizontalScrollSnap mapHss;
    public void SelectMap()
    {
        prefsManager.prefs.selectedMapId = mapHss._currentPage;
        prefsManager.Save();
    }
    public void ScrollMap(int dir)
    {
        if (mapHss._currentPage + dir != 0)
        {
            int mapIndex = mapHss._currentPage + dir;
            selectMapBtn.interactable = mapIndex == 0 ? true : mapIndex == 1 ? prefsManager.prefs.mapUnlocked0 : mapIndex == 2 ? prefsManager.prefs.mapUnlocked1 : mapIndex == 3 ? prefsManager.prefs.mapUnlocked2 : mapIndex == 4 ? prefsManager.prefs.mapUnlocked3 : false;
        }
        else
        {
            selectMapBtn.interactable = true;
        }
    }


    bool doCancel;
    WebClient downloadClient;

    public void WebHandlers_Handle()
    {
        // Обрабатываем обработчиков
        if (handler_downloadProgress)
        {
            handler_downloadProgress = false;
            trackPlayPanelrogressHandler();
        }

        if (handler_downloadCompleted)
        {
            handler_downloadCompleted = false;
            if (handler_downloadCancelled)
            {
                handler_downloadCancelled = false;
                DownloadTrackCancelHandler();
            }
            else
            {
                DownloadTrackCompletedHandler();
            }
        }
        if (downloadClient != null)
        {
            if (downloadingTimeout == -1)
            {

            }
            else if (downloadingTimeout > 0)
            {
                downloadingTimeout -= Time.deltaTime;
            }
            if (downloadingTimeout <= 0 && downloadingTimeout != -1)
            {
                downloadClient.CancelAsync();
                downloadingTimeout = 10;
                errorDownloadText.text = "Timeout. Check the internet connection";
                errorDownloadText.gameObject.SetActive(true);
                //trackPlayPanel.SetActive(true);
                //downloadTrackCancelButton.SetActive(false);
                trackDownloadBtn.SetActive(true);
            }
        }
    }
    public void TrackPlayChange(bool playable)
    {
        trackChangeMapBtn.SetActive(playable);
        trackPlayBtn.SetActive(playable);
        trackDeleteBtn.SetActive(playable);
        trackDownloadBtn.SetActive(!playable);
        //trackPitchSlider.gameObject.SetActive(playable);
        difficultPanel.SetActive(playable);
        trackRecordText.gameObject.SetActive(playable);
    }
    

    public MenuTrackButton selectedTrack;
    public GameObject trackDownloadBtn, trackDownloadCancelBtn, trackChangeMapBtn, trackPlayBtn, trackDeleteBtn, difficultPanel;
    public Button homeBtn;
    public float downloadingTimeout = 15;
    [HideInInspector] public float handler_downloadProgressPercentage;
    bool handler_downloadProgress, handler_downloadCompleted, handler_downloadCancelled;
    public Text trackRecordText, errorDownloadText, downloadTrackSliderText, sourceText;
    public Slider downloadTrackSlider;

    public void DownloadCancel()
    {
        doCancel = true;
    }

    #region Second Thread Handlers

    public void trackPlayPanelrogress_HandlerSecondThread(object sender, DownloadProgressChangedEventArgs e)
    {
        handler_downloadProgress = true;
        handler_downloadProgressPercentage = e.ProgressPercentage;
    }
    public void DownloadTrackCompleted_HandlerSecondThread(object sender, AsyncCompletedEventArgs e)
    {
        File.AppendAllText(Application.persistentDataPath + "/order.ls", "Downloaded:" + selectedTrack.fullname + "\n");

        handler_downloadCompleted = true;
        if (e.Cancelled)
        {
            doCancel = false; handler_downloadCancelled = true;
            File.Delete(Application.persistentDataPath + "/temp/" + selectedTrack.fullname + ".bsz");
        }
    }
    #endregion

    #region Handlers

    public void DownloadTrackCompletedHandler()
    {
        downloadClient = null;
        try
        {
            // Moving downloaded file into right folder
            string tempPath = Application.persistentDataPath + "/temp/" + selectedTrack.fullname + ".bsz";

            //Project project = ProjectManager.UnpackBspFile(tempPath);
            Project project = null;
            // Deprecated

            string trackFolderPath = Application.persistentDataPath + "/maps/" + selectedTrack.fullname; // Here are all maps with same music
            string mapFolderPath = trackFolderPath + "/" + project.creatorNick;


            downloadTrackSlider.gameObject.SetActive(false);
            trackDownloadCancelBtn.gameObject.SetActive(false);
            homeBtn.interactable = true;
            trackDownloadBtn.SetActive(false);
            TrackPlayChange(true);

            if (project.hasImage)
            {
                selectedTrack.SetImage(mapFolderPath + "/" + selectedTrack.fullname + Project.ToString(project.imageExtension));
                trackImg.sprite = selectedTrack.img.sprite;
            }

            //listController.RefreshAuthorList();
            TrackListUI.RefreshAllMusicList();
        }
        catch (Exception err)
        {
            Debug.LogError(err);

            //return;
            TrackPlayChange(false);
            errorDownloadText.text = err.Message;
            errorDownloadText.gameObject.SetActive(true);
            homeBtn.interactable = true;
            downloadTrackSlider.gameObject.SetActive(false);
            trackDownloadCancelBtn.SetActive(false);

            File.Delete(Application.persistentDataPath + "/saved/" + selectedTrack.fullname + ".bsp");
            File.Delete(Application.persistentDataPath + "/saved/" + selectedTrack.fullname + ".ogg");
            File.Delete(Application.persistentDataPath + "/saved/" + selectedTrack.fullname + ".jpg");
        }
    }

    public void trackPlayPanelrogressHandler()
    {
        downloadTrackSlider.value = handler_downloadProgressPercentage / 100f;
        downloadTrackSliderText.text = handler_downloadProgressPercentage + "%";
        if (doCancel)
        {
            doCancel = false;
            downloadClient.CancelAsync();
            downloadTrackSliderText.text = LocalizationManager.Localize("Cancel");
        }
        downloadingTimeout = 10;
    }

    public void DownloadTrackCancelHandler()
    {
        downloadTrackSlider.value = 0;
        downloadTrackSliderText.text = "";
        downloadTrackSlider.gameObject.SetActive(false);
        homeBtn.interactable = true;
        TrackPlayChange(false);
        trackDownloadCancelBtn.gameObject.SetActive(false);
    }
    #endregion



    public static IEnumerable<string> GetFileList(string fileSearchPattern, string rootFolderPath)
    {
        Queue<string> pending = new Queue<string>();
        pending.Enqueue(rootFolderPath);
        string[] tmp;
        while (pending.Count > 0)
        {
            rootFolderPath = pending.Dequeue();
            try
            {
                tmp = Directory.GetFiles(rootFolderPath, fileSearchPattern);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            for (int i = 0; i < tmp.Length; i++)
            {
                yield return tmp[i];
            }
            tmp = Directory.GetDirectories(rootFolderPath);
            for (int i = 0; i < tmp.Length; i++)
            {
                pending.Enqueue(tmp[i]);
            }
        }
    }



    IEnumerator CheckForUpdatesAsync()
    {
        TimeSpan t = DateTime.Now.TimeOfDay;
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            WWW www = new WWW("http://176.107.160.146/Builds/GetGameVersion");

            yield return www;

            string version = www.text.Split(' ')[0];

            if (isNewVer(Application.version, version))
            {
                newVersionLocker.SetActive(true);
                newVersionText.text = LocalizationManager.Localize("UpdateAvailable") + " <color=#06F>" + version + @"</color>
" + LocalizationManager.Localize("YourVersion") + " <size=34><color=#F60>" + Application.version + "</color></size>";
            }
        }
    }
    public bool isNewVer(string str, string str2)
    {
        string[] strChars = str.Split('.');
        int[] strInts = new int[strChars.Length];
        for (int i = 0; i < strChars.Length; i++) strInts[i] = int.Parse(strChars[i], System.Globalization.NumberStyles.AllowDecimalPoint);

        strChars = str2.Split('.');
        int[] strInts2 = new int[strChars.Length];
        for (int i = 0; i < strChars.Length; i++) strInts2[i] = int.Parse(strChars[i], System.Globalization.NumberStyles.AllowDecimalPoint);

        if (strInts.Length != strInts.Length) return true;
        for (int i = 0; i < strInts.Length; i++)
        {
            if (strInts2[i] > strInts[i]) return true;
            else if (strInts2[i] < strInts[i]) return false;
        }
        return false;
    }

    public void OpenPrivatePolicy()
    {
        //Application.OpenURL("https://tooproprogramms.000webhostapp.com/BeatSlayer/Policy.html");
        Application.OpenURL("https://beats-slayer-tracks.herokuapp.com/Policy.html");
    }

    public void OpenPlayMarket()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.REDIZIT.BeatSlayer");
    }

    public Text[] mapsLockers;
    public int[] mapsCosts;
    void UnlockMapsTranslate()
    {
        for (int i = 0; i < mapsLockers.Length; i++)
        {
            mapsLockers[i].text = LocalizationManager.Localize("UnlockFor") + " <color=#FF7700><b>" + mapsCosts[i] + " " + LocalizationManager.Localize("Coins") + "</b></color>";
        }
    }
    public void UnlockMap(Button btn)
    {
        int coins = prefsManager.prefs.coins;
        int mapIndex = int.Parse(btn.name.Replace("Locker", ""));
        int cost = mapsCosts[mapIndex];
        if (coins >= cost)
        {
            prefsManager.prefs.coins = coins - cost;
            if (mapIndex == 0) prefsManager.prefs.mapUnlocked0 = true;
            else if (mapIndex == 1) prefsManager.prefs.mapUnlocked1 = true;
            else if (mapIndex == 2) prefsManager.prefs.mapUnlocked2 = true;
            else if (mapIndex == 3) prefsManager.prefs.mapUnlocked3 = true;
            //btn.gameObject.SetActive(false);
            coinsTexts[0].text = coins - cost + "";
            coinsTexts[1].text = coins - cost + "";
            coinsTexts[2].text = coins - cost + "";
            selectMapBtn.interactable = true;

            //if (!prefsManager.prefs.hasAchiv_NewMapNewLife)
            //{
                
            //}
            Social.ReportProgress(GPGamesManager.achievement_NewMap, 100, (bool success) =>
            {
                if (!success) Debug.LogError("Achiv error");
                if (success)
                {
                    prefsManager.prefs.hasAchiv_NewMapNewLife = true;
                }
            });

            CheckAchievement();

            // Animation
            StartCoroutine(UnlockMapAnimator(btn.gameObject, cost));
            prefsManager.Save();
        }
    }
    IEnumerator UnlockMapAnimator(GameObject locker, float cost)
    {
        Text txt = locker.transform.GetChild(0).GetChild(0).GetComponent<Text>();
        txt.text = LocalizationManager.Localize("UnlockFor") + " <color=#FF7700><b>" + cost + " " + LocalizationManager.Localize("Coins") + "</b></color>";


        float speed = cost / 2;
        float time = 2;

        yield return new WaitForSeconds(0.5f);
        while (time > 0)
        {
            time -= Time.deltaTime;
            cost -= speed * Time.deltaTime;
            txt.text = LocalizationManager.Localize("UnlockFor") + " <color=#FF7700><b>" + Mathf.RoundToInt(cost) + " " + LocalizationManager.Localize("Coins") + "</b></color>";
            yield return new WaitForSeconds(Time.deltaTime);
        }

        txt.text = LocalizationManager.Localize("Unlocked");

        locker.GetComponent<Animator>().Play("MapUnlock");
    }

    public HorizontalScrollSnap[] allHss;
    public GameObject trackConfigScreen;
    void OnExitSwipe()
    {
        if (trackConfigScreen.activeInHierarchy)
        {
            trackConfigScreen.SetActive(false);
            allHss[1].gameObject.GetComponent<State>().ChangeState(allHss[1].gameObject);
            return;
        }
        foreach (HorizontalScrollSnap hss in allHss)
        {
            if (hss.gameObject.activeInHierarchy)
            {
                int defaultPage = hss.StartingScreen;
                int currentPage = hss._currentPage;
                if (currentPage > defaultPage) hss.PreviousScreen();
                else if (currentPage < defaultPage) hss.NextScreen();
            }
        }
    }


    public string gpsId;
    void LoadGPSUser(bool auth)
    {
        /*//Debug.Log("PlayGames auth result is " + auth);
        //return;
        //string username = PlayGamesPlatform.Instance.RealTime.GetSelf().Player.userName;
        string username = PlayGamesPlatform.Instance.GetUserDisplayName();
        string id = PlayGamesPlatform.Instance.GetUserId();
        gpsId = id;
        if (Application.isEditor) gpsId = "g123";
        //bool isFiened = PlayGamesPlatform.Instance.RealTime.GetSelf().Player.isFriend;
        bool isFiened = false;
        //string state = PlayGamesPlatform.Instance.RealTime.GetSelf().Player.state.ToString();
        string state = PlayGamesPlatform.Instance.GetUserEmail();
        //string displayName = PlayGamesPlatform.Instance.RealTime.GetSelf().DisplayName;
        string displayName = PlayGamesPlatform.Instance.GetIdToken();

        Debug.LogWarning(string.Format("GPS Info: {0}\n{1}\n{2}\n{3}\n{4}", username, id, isFiened, state, displayName));*/
    }

    public void OpenCustomList()
    {
        FileBrowser.Filter filter = new FileBrowser.Filter("Project", ".bsu");
        FileBrowser.SetFilters(false, filter);
        FileBrowser.ShowLoadDialog(OnCustomTrackSelected, delegate { }, false, Application.persistentDataPath);

    }
    // 'From file' button
    void OnCustomTrackSelected(string path)
    {
        string bsuPath = path;


        SceneloadParameters parameters = SceneloadParameters.FromFilePreset(bsuPath);
        SceneController.instance.LoadScene(parameters);
    }

    public void CloseEditorAvailableForever()
    {
        prefsManager.prefs.showedEditorAvailableWindow = true;
        prefsManager.Save();
    }
    public void OpenWebsite()
    {
        //Application.OpenURL("https://really-big-server.herokuapp.com/download.php");
        Application.OpenURL("https://beat-slayer.glitch.me/editor");
    }
    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }

    private static bool TrustCertificate(object sender, X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors){  return true; }


    static void ClearConsole()
    {
        return;
        //var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");

        //var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        //clearMethod.Invoke(null, null);
    }

    public void CheckAchievement()
    {
        //if (!prefsManager.prefs.hasAchiv_ShoppingSpree) return;
        // Если открыты все карты
        if (prefsManager.prefs.mapUnlocked0 && prefsManager.prefs.mapUnlocked1 && prefsManager.prefs.mapUnlocked2 && prefsManager.prefs.mapUnlocked3)
        {
            int len = prefsManager.prefs.boughtSabers.Length;
            bool allSabersBought = true;

            for (int i = 0; i < len; i++)
            {
                if (!prefsManager.prefs.boughtSabers[i])
                {
                    allSabersBought = false;
                }
            }

            // Если куплены все мечи
            if (allSabersBought)
            {
                // Если куплены все ускорители
                if (prefsManager.prefs.boosters.Where(c => c.count > 0).ToList().Count == prefsManager.prefs.boosters.Count)
                {
                    // Если куплены все скилы
                    if (prefsManager.prefs.skills.Where(c => c.count > 0).ToList().Count == prefsManager.prefs.skills.Count)
                    {
                        Social.ReportProgress(GPGamesManager.achiv_shoppingSpree, 100, (bool success) =>
                        {
                            if (!success) Debug.Log("Cant give shopping spree achiv");
                            else
                            {
                                prefsManager.prefs.hasAchiv_ShoppingSpree = true;
                                prefsManager.Save();
                            }
                        });
                    }
                }
            }
        }
    }


    #region Server message

    public GameObject serverMsgAnim;
    public Sprite[] serverMsgSprites;
    bool isServerMsgOpenned;

    void CheckServerMessage()
    {
        WebClient client = new WebClient();
        client.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs args) =>
        {
            string result = args.Result;
            if(result.Contains("|"))
            {
                serverMsgAnim.SetActive(true);

                string type = result.Split('|')[0];
                string msg = result.Split('|')[1];

                Color32 errorClr = new Color32(160, 0, 0, 170);
                Color32 warningClr = new Color32(240, 100, 0, 170);
                Color32 infoClr = new Color32(210, 210, 210, 170);

                serverMsgAnim.GetComponent<Image>().color = type == "Error" ? errorClr : type == "Warning" ? warningClr : infoClr;
                serverMsgAnim.GetComponentsInChildren<Image>()[1].sprite = type == "Error" ? serverMsgSprites[0] : type == "Warning" ? serverMsgSprites[1] : serverMsgSprites[2];
                serverMsgAnim.GetComponentsInChildren<Image>()[2].color = type == "Error" ? new Color32(255,0,0,255) : type == "Warning" ? new Color32(255, 130, 0, 255) : new Color32(255, 255, 255, 255);
                serverMsgAnim.GetComponentInChildren<Text>().color = type == "Info" ? new Color32(32, 32, 32, 255) : new Color32(255,255,255,255);

                serverMsgAnim.GetComponentInChildren<Text>().text = msg;
            }
            else
            {
                serverMsgAnim.SetActive(false);
            }
        };
        client.DownloadStringAsync(new Uri("http://176.107.160.146/Database/GetMessage"));
    }

    public void OnServerMsgClicked()
    {
        if (!isServerMsgOpenned) { serverMsgAnim.GetComponent<Animator>().Play("OpenMsg"); isServerMsgOpenned = true; }
        else { serverMsgAnim.GetComponent<Animator>().Play("CloseMsg"); isServerMsgOpenned = false; }
    }

    #endregion
}