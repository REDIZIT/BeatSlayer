using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
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

public class MenuScript_v2 : MonoBehaviour
{
    public BeatmapUI beatmapUI;
    
    public TrackListUI TrackListUI { get { return GetComponent<TrackListUI>(); } }
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

        DatabaseManager.Init(ownMusicUI);
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


        mapHss.StartingScreen = AdvancedSaveManager.prefs.selectedMapId;
        mapLockers[0].SetActive(!AdvancedSaveManager.prefs.mapUnlocked0);
        mapLockers[1].SetActive(!AdvancedSaveManager.prefs.mapUnlocked1);
        mapLockers[2].SetActive(!AdvancedSaveManager.prefs.mapUnlocked2);
        mapLockers[3].SetActive(!AdvancedSaveManager.prefs.mapUnlocked3);

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

   

    public State configScreen;
    public Text trackName, trackAuthor, configMapCreatorText;
    public Image trackImg;
    public Sprite defaultTrackSprite;
    public Texture2D defaultTrackTexture;

    [Header("Track info")]
    public GameObject trackInfoLocker;
    public Text trackInfoAuthorText, trackInfoNameText, trackInfoMapsCountText;
    public GameObject trackInfoMapPrefab;
    public Transform trackInfoMapContent;
    public RawImage trackInfoCoverImage;

    public void OnTrackItemClicked(GroupPresenter listItem)
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
        AdvancedSaveManager.prefs.selectedMapId = mapHss._currentPage;
        AdvancedSaveManager.Save();
    }
    public void ScrollMap(int dir)
    {
        if (mapHss._currentPage + dir != 0)
        {
            int mapIndex = mapHss._currentPage + dir;
            selectMapBtn.interactable = mapIndex == 0 ? true : 
                mapIndex == 1 ? AdvancedSaveManager.prefs.mapUnlocked0 :
                mapIndex == 2 ? AdvancedSaveManager.prefs.mapUnlocked1 :
                mapIndex == 3 ? AdvancedSaveManager.prefs.mapUnlocked2 : 
                mapIndex == 4 ? AdvancedSaveManager.prefs.mapUnlocked3 : false;
        }
        else
        {
            selectMapBtn.interactable = true;
        }
    }


    //public MenuTrackButton selectedTrack;
    public GameObject trackDownloadBtn, trackDownloadCancelBtn, trackChangeMapBtn, trackPlayBtn, trackDeleteBtn, difficultPanel;
    public Button homeBtn;
    public float downloadingTimeout = 15;
    [HideInInspector] public float handler_downloadProgressPercentage;
    public Text trackRecordText, errorDownloadText, downloadTrackSliderText, sourceText;
    public Slider downloadTrackSlider;



    IEnumerator CheckForUpdatesAsync()
    {
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
            if (mapIndex == 0) AdvancedSaveManager.prefs.mapUnlocked0 = true;
            else if (mapIndex == 1) AdvancedSaveManager.prefs.mapUnlocked1 = true;
            else if (mapIndex == 2) AdvancedSaveManager.prefs.mapUnlocked2 = true;
            else if (mapIndex == 3) AdvancedSaveManager.prefs.mapUnlocked3 = true;
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
                    AdvancedSaveManager.prefs.hasAchiv_NewMapNewLife = true;
                }
            });

            CheckAchievement();

            // Animation
            StartCoroutine(UnlockMapAnimator(btn.gameObject, cost));
            AdvancedSaveManager.Save();
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
        AdvancedSaveManager.prefs.showedEditorAvailableWindow = true;
        AdvancedSaveManager.Save();
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
        if (AdvancedSaveManager.prefs.mapUnlocked0 && AdvancedSaveManager.prefs.mapUnlocked1 && AdvancedSaveManager.prefs.mapUnlocked2 && AdvancedSaveManager.prefs.mapUnlocked3)
        {
            int len = AdvancedSaveManager.prefs.boughtSabers.Length;
            bool allSabersBought = true;

            for (int i = 0; i < len; i++)
            {
                if (!AdvancedSaveManager.prefs.boughtSabers[i])
                {
                    allSabersBought = false;
                }
            }

            // Если куплены все мечи
            if (allSabersBought)
            {
                // Если куплены все ускорители
                if (AdvancedSaveManager.prefs.boosters.Where(c => c.count > 0).ToList().Count == AdvancedSaveManager.prefs.boosters.Count)
                {
                    // Если куплены все скилы
                    if (AdvancedSaveManager.prefs.skills.Where(c => c.count > 0).ToList().Count == AdvancedSaveManager.prefs.skills.Count)
                    {
                        Social.ReportProgress(GPGamesManager.achiv_shoppingSpree, 100, (bool success) =>
                        {
                            if (!success) Debug.Log("Cant give shopping spree achiv");
                            else
                            {
                                AdvancedSaveManager.prefs.hasAchiv_ShoppingSpree = true;
                                AdvancedSaveManager.Save();
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