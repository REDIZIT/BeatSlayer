using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using System.ComponentModel;
using UnityEngine.UI;
using Pixelplacement;
using UnityEngine.UI.Extensions;
using Assets.SimpleLocalization;
using UnityEngine.Video;
using InGame.SceneManagement;
using DatabaseManagement;
using GameNet;
using ProjectManagement;
using Testing;
using Debug = UnityEngine.Debug;
using Web;
using InGame.UI.Overlays;
using InGame.Settings;
using InGame.Game.Menu;
using InGame.UI.Menu;

public class MenuScript_v2 : MonoBehaviour
{
    public DatabaseScript database;
    public DailyRewarder dailyRewarder;
    public BeatmapUI beatmapUI;
    
    public TrackListUI TrackListUI { get { return GetComponent<TrackListUI>(); } }
    public AdvancedSaveManager PrefsManager { get { return GetComponent<AdvancedSaveManager>(); } }
    public AccountManager accountManager;
    public TutorialManager tutorialManager;
    public OwnMusicUI ownMusicUI;
    public SettingsUI settingsUI;

    public Language language;
    public enum Language
    {
        English, Russian, French
    }


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

    [Header("Settings")]
    public ColumnGridLayout[] twoColumnGrids;

    [Header("Misc")]
    public GameObject newTracksImg;

    

    public void Configuration()
    {
        NetCore.OnLogIn += () =>
        {
            RefreshCoinsTexts();
        };
    }
    //private static bool TrustCertificate(object sender, X509Certificate x509Certificate, X509Chain x509Chain, SslPolicyErrors sslPolicyErrors) { return true; }
    public void Awake()
    {
        Application.targetFrameRate = 60;
        if(Time.timeScale != 1)
        {
            Debug.LogWarning("Time scale in menu is not 1!");
            Time.timeScale = 1;
        }
        debugConsole.SetActive(true);

        TranslateStart();
        
        GetComponent<SceneController>().Init(GetComponent<SceneControllerUI>());

        Database.Init(ownMusicUI);
        ProjectManager.defaultTrackTexture = defaultTrackTexture;
    }

    private void Start()
    {
        versionText.text = Application.version.ToString() + " in " + Application.installMode.ToString() + " mode";

        UnlockMapsTranslate();

        CheckFolders();
        HandleSettings();

        TestManager.Setup(this);

        TrackListUI.RefreshDownloadedList();

        #region Loading prefs


        mapHss.StartingScreen = PrefsManager.prefs.selectedMapId;
        mapLockers[0].SetActive(!PrefsManager.prefs.mapUnlocked0);
        mapLockers[1].SetActive(!PrefsManager.prefs.mapUnlocked1);
        mapLockers[2].SetActive(!PrefsManager.prefs.mapUnlocked2);
        mapLockers[3].SetActive(!PrefsManager.prefs.mapUnlocked3);

        if(File.Exists(Application.persistentDataPath + "/Money.txt"))
        {
            PrefsManager.prefs.coins = 9999999;
            PrefsManager.Save();
        }

        if(Payload.Account != null) RefreshCoinsTexts();
       
        

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

        CheckAchievement();

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += delegate { videoPlayer.Play(); };

        if(LoadingData.sceneLoadCount == 0)
        {
            WebAPI.OnGameLaunch();
        }
        LoadingData.sceneLoadCount++;
    }

    private void Update()
    {
        WebHandlers_Handle();

        TestManager.CheckUpdates();
        TestManager.CheckModerationUpdates();
    }


    public void HandleSettings()
    {
        if(debugConsole != null)
        {
            debugConsole.SetActive(SettingsManager.Settings.Dev.ConsoleEnabled);
            debugConsole.transform.GetChild(3).gameObject.SetActive(SettingsManager.Settings.Dev.ShowFpsEnabled);
        }

        settingsUI.Subscribe("TwoColumnList", () =>
        {
            foreach (var grid in twoColumnGrids)
            {
                grid.allowTwoColumns = SettingsManager.Settings.Menu.TwoColumnList;
                grid.Build();
            }
        });
    }

    void TranslateStart()
    {
        if(Application.isEditor)
        {
            LocalizationManager._language = language.ToString();
            LocalizationManager.Read();
            return;
        }
        if (Application.systemLanguage == SystemLanguage.Russian || Application.systemLanguage == SystemLanguage.Ukrainian) LocalizationManager.Language = "Russian";
        else if (Application.systemLanguage == SystemLanguage.French) LocalizationManager.Language = "French";
        else LocalizationManager.Language = "English";
        
        LocalizationManager.Read();
    }

    void CheckFolders()
    {
        bool showTutorial = false;


        if (!Directory.Exists(Application.persistentDataPath + "/maps"))
        {
            showTutorial = true;
            Directory.CreateDirectory(Application.persistentDataPath + "/maps");
        }

        if (!Directory.Exists(Application.persistentDataPath + "/data"))
        {
            showTutorial = true;
            Directory.CreateDirectory(Application.persistentDataPath + "/data");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/data/account"))
        {
            showTutorial = true;
            Directory.CreateDirectory(Application.persistentDataPath + "/data/account");
        }

        if (showTutorial) tutorialManager.ShowOverlay();
    }

    

    #region Music lists

    #region Music lists: Refresh

    public void OnRefreshAuthorBtnClick()
    {
        StartCoroutine(database.LoadDatabaseAsync(true));
    }
    

    #endregion

   

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


        trackRecordText.text = LocalizationManager.Localize("Your record is") + " " + (PrefsManager.prefs.GetRecord(selectedTrack.fullname) != 0 ? PrefsManager.prefs.GetRecord(selectedTrack.fullname).ToString() : LocalizationManager.Localize("NotSetYet"));

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
    }
   
    public void OnExtendSearchClicked(Animator anim)
    {
        bool isExtendSearchShowed = anim.transform.GetChild(1).GetComponent<CanvasGroup>().alpha == 1f;
        anim.Play(isExtendSearchShowed ? "HideExtend" : "ShowExtend");
    }

    #endregion




    // == Rescale
    // ============================================================================================================================
    public Transform[] settingsRescalingObjs;
    public float settingsRescaleOffset;

    public StateMachine Main_UI;
    public RectTransform selectMapRect;
    public GameObject achievementBtn, leaderboardBtn;



    public HorizontalScrollSnap mapHss;
    public void SelectMap()
    {
        PrefsManager.prefs.selectedMapId = mapHss._currentPage;
        PrefsManager.Save();
    }
    public void ScrollMap(int dir)
    {
        if (mapHss._currentPage + dir != 0)
        {
            int mapIndex = mapHss._currentPage + dir;
            selectMapBtn.interactable = mapIndex == 0 ? true : mapIndex == 1 ? PrefsManager.prefs.mapUnlocked0 : mapIndex == 2 ? PrefsManager.prefs.mapUnlocked1 : mapIndex == 3 ? PrefsManager.prefs.mapUnlocked2 : mapIndex == 4 ? PrefsManager.prefs.mapUnlocked3 : false;
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
            WWW www = new WWW(NetCore.Url_Server + "/Builds/GetGameVersion");

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
        Application.OpenURL("https://docs.google.com/document/d/1EF1LaPpdGQ5a73chgJB1caqIRbaX6rjObgWdXeBux7Y/edit?usp=sharing");
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
        int coins = Payload.Account.Coins;
        int mapIndex = int.Parse(btn.name.Replace("Locker", ""));
        int cost = mapsCosts[mapIndex];
        if (coins >= cost)
        {
            Payload.Account.Coins -= cost;
            NetCore.ServerActions.Shop.SendCoins(Payload.Account.Nick, -cost);
            if (mapIndex == 0) PrefsManager.prefs.mapUnlocked0 = true;
            else if (mapIndex == 1) PrefsManager.prefs.mapUnlocked1 = true;
            else if (mapIndex == 2) PrefsManager.prefs.mapUnlocked2 = true;
            else if (mapIndex == 3) PrefsManager.prefs.mapUnlocked3 = true;
            //btn.gameObject.SetActive(false);
            RefreshCoinsTexts();
            selectMapBtn.interactable = true;

            //if (!prefsManager.prefs.hasAchiv_NewMapNewLife)
            //{
                
            //}
            Social.ReportProgress(GPGamesManager.achievement_NewMap, 100, (bool success) =>
            {
                if (!success) Debug.LogError("Achiv error");
                if (success)
                {
                    PrefsManager.prefs.hasAchiv_NewMapNewLife = true;
                }
            });

            CheckAchievement();

            // Animation
            StartCoroutine(UnlockMapAnimator(btn.gameObject, cost));
            PrefsManager.Save();
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

    public void RefreshCoinsTexts()
    {
        foreach (var t in coinsTexts) t.text = Payload.Account.Coins.ToString();
    }
    
    public void CloseEditorAvailableForever()
    {
        PrefsManager.prefs.showedEditorAvailableWindow = true;
        PrefsManager.Save();
    }
    public void OpenWebsite()
    {
        Application.OpenURL("https://bsserver.tk/Builds");
    }
    public void OpenUrl(string url)
    {
        Application.OpenURL(url);
    }


    public void CheckAchievement()
    {
        //if (!prefsManager.prefs.hasAchiv_ShoppingSpree) return;
        // Если открыты все карты
        if (PrefsManager.prefs.mapUnlocked0 && PrefsManager.prefs.mapUnlocked1 && PrefsManager.prefs.mapUnlocked2 && PrefsManager.prefs.mapUnlocked3)
        {
            int len = PrefsManager.prefs.boughtSabers.Length;
            bool allSabersBought = true;

            for (int i = 0; i < len; i++)
            {
                if (!PrefsManager.prefs.boughtSabers[i])
                {
                    allSabersBought = false;
                }
            }

            // Если куплены все мечи
            if (allSabersBought)
            {
                // Если куплены все ускорители
                if (PrefsManager.prefs.boosters.Where(c => c.count > 0).ToList().Count == PrefsManager.prefs.boosters.Count)
                {
                    // Если куплены все скилы
                    if (PrefsManager.prefs.skills.Where(c => c.count > 0).ToList().Count == PrefsManager.prefs.skills.Count)
                    {
                        Social.ReportProgress(GPGamesManager.achiv_shoppingSpree, 100, (bool success) =>
                        {
                            if (!success) Debug.Log("Cant give shopping spree achiv");
                            else
                            {
                                PrefsManager.prefs.hasAchiv_ShoppingSpree = true;
                                PrefsManager.Save();
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
        client.DownloadStringAsync(new Uri(NetCore.Url_Server + "/Database/GetMessage"));
    }

    public void OnServerMsgClicked()
    {
        if (!isServerMsgOpenned) { serverMsgAnim.GetComponent<Animator>().Play("OpenMsg"); isServerMsgOpenned = true; }
        else { serverMsgAnim.GetComponent<Animator>().Play("CloseMsg"); isServerMsgOpenned = false; }
    }

    #endregion
}