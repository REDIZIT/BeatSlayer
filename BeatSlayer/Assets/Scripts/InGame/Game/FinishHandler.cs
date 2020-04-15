using Assets.SimpleLocalization;
using InGame.Game;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class FinishHandler : MonoBehaviour
{
    public GameManager gm { get { return GetComponent<GameManager>(); } }
    public AudioManager audioManager { get { return GetComponent<AudioManager>(); } }
    public AdvancedSaveManager prefsManager { get { return GetComponent<AdvancedSaveManager>(); } }

    public const string url_leaderboard = "http://www.bsserver.tk/Account/GetMapGlobalLeaderboard?trackname={0}&nick={1}";


    [Header("UI")]
    public GameObject finishOverlay;
    public Image coverImage;
    public Text authorText, nameText, creatorText;
    public Text scoreText, missedText, accuracyText, RPText;
    public GameObject uploadingText, recordText;
    public GameObject heartIcon;

    [Header("Difficulties")]
    public Text cubesSpeedText;
    public Text musicSpeedText;
    public Toggle noArrowsToggle, noLinesToggle;

    [Header("Leaderboard")]
    public Transform leaderboardContent;
    public Text leaderboardLoadingText;



    public void CheckLevelFinish()
    {
        if (!gm.gameStarting && audioManager.asource.time == 0 && gm.beats.ToArray().Length == 0 && !audioManager.asource.isPlaying)
        {
            OnLevelFinished();
        }
        else
        {
            float trackPer = (audioManager.asource.time < audioManager.asource.clip.length ?
                audioManager.asource.time / audioManager.asource.clip.length : 1) * 100;
            trackPer = Mathf.Round(trackPer * 10) / 10;
            gm.trackTimeSlider.value = trackPer;
            gm.trackTextSliderText.text = trackPer + "%";
        }
    }
    void OnLevelFinished()
    {
        if (!gm.gameCompleted)
        {
            gm.gameCompleted = true;

            FinishServerActions();


            int coins = prefsManager.prefs.coins;
            int addCoins = Mathf.RoundToInt(gm.replay.score / 16f * gm.maxCombo / 2f * gm.scoreMultiplier);

;
            prefsManager.prefs.coins = coins + addCoins;
            prefsManager.Save();

            HandleFinishUI();

            gm.RateBtnUpdate();

            FinishAchievements();
        }
    }




    void HandleFinishUI()
    {
        finishOverlay.SetActive(true);
        gm.trackText.gameObject.SetActive(false);
        gm.trackTimeSlider.value = 100;
        gm.trackTextSliderText.text = "100%";

        string coverPath = TheGreat.GetCoverPath(Application.persistentDataPath + "/maps/" + gm.project.author + "-" + gm.project.name + "/" + gm.project.creatorNick, gm.fullTrackName);
        coverImage.sprite = coverPath == "" ? gm.defaultTrackSprite : TheGreat.LoadSprite(coverPath);

        authorText.text = gm.project.author;
        nameText.text = gm.project.name;
        creatorText.text = LocalizationManager.Localize("by") + " " + gm.project.creatorNick;

        scoreText.text = Mathf.RoundToInt(gm.replay.score * 10f) / 10f + "";
        missedText.text = gm.replay.missed.ToString();
        accuracyText.text = Mathf.RoundToInt(gm.replay.Accuracy * 1000) / 10f + "%";

        cubesSpeedText.text = LocalizationManager.Localize("CubesSpeed") + ": " + (gm.cubesspeed == 1 ? "1.0x" : gm.cubesspeed + "x");
        musicSpeedText.text = LocalizationManager.Localize("MusicSpeed") + ": " + (gm.pitch == 1 ? "1.0x" : gm.pitch + "x");
        noLinesToggle.isOn = gm.nolines;
        noArrowsToggle.isOn = gm.noarrows;


    }
    void FinishAchievements()
    {
        (Social.Active as GooglePlayGames.PlayGamesPlatform).IncrementAchievement(GPGamesManager.youArePlayer, 1, (bool s) => { });
        (Social.Active as GooglePlayGames.PlayGamesPlatform).IncrementAchievement(GPGamesManager.dj, 1, (bool s) => { });
        (Social.Active as GooglePlayGames.PlayGamesPlatform).IncrementAchievement(GPGamesManager.musicKing, 1, (bool s) => { });

        if (!prefsManager.prefs.hasAchiv_Uff)
        {
            Social.ReportProgress(GPGamesManager.achievement_UffEnded, 100, (bool success) =>
            {
                if (!success) Debug.LogError("Achiv error");
                if (success)
                {
                    prefsManager.prefs.hasAchiv_Uff = true;
                    prefsManager.Save();
                }
            });
        }

        if (gm.replay.missed == 0 && gm.replay.score >= 4000 && gm.cubesspeed == 1.5f && !gm.nolines && !gm.noarrows)
        {
            if (!prefsManager.prefs.hasAchiv_Hardcore)
            {
                Social.ReportProgress(GPGamesManager.achievement_Hardcore, 100, (bool success) =>
                {
                    if (!success) Debug.LogError("Achiv error");
                    if (success)
                    {
                        prefsManager.prefs.hasAchiv_ThatsMy = true;
                        prefsManager.Save();
                    }
                });
            }
        }

        if (gm.replay.missed == gm.replay.AllCubes && gm.replay.AllCubes >= 10 && !prefsManager.prefs.hasAchiv_Terrible) // 10 is random value :D
        {
            Social.ReportProgress(GPGamesManager.terrible, 100, (bool success) => { if (!success) Debug.LogError("Achiv error: terrible"); });
            prefsManager.prefs.hasAchiv_Terrible = true;
            prefsManager.Save();
        }
    }
    void FinishServerActions()
    {
        StartCoroutine(IEFinishServerActions());
    }
    IEnumerator IEFinishServerActions()
    {
        Debug.Log("IEFinishServerActions()");

        Debug.Log("LoadingData.loadparams.Type: " + LoadingData.loadparams.Type);

        if (Application.internetReachability == NetworkReachability.NotReachable) yield break;
        if (LoadingData.loadparams.Type != SceneloadParameters.LoadType.Author) yield break;

        RPText.text = ".";
        Debug.Log("Is account null? " + (AccountManager.account == null));

        if (AccountManager.account == null) yield break;

        uploadingText.SetActive(true);

        RPText.text = "..";

        Replay bestReplay = null;
        AccountManager.GetBestReplay(AccountManager.account.nick, gm.project.author + "-" + gm.project.name, gm.project.creatorNick, (Replay replay) =>
        {
            bestReplay = replay;
        });

        yield return bestReplay;

        if (bestReplay != null && bestReplay.score > gm.replay.score)
        {
            recordText.SetActive(true);
        }

        AccountManager.SendReplay(gm.replay, (double RP) =>
        {
            RPText.text = Mathf.RoundToInt((float)RP).ToString();
            LoadLeaderboard();
        });

        gm.accountManager.UpdatePlayedMap(gm.project.author, gm.project.name, gm.project.creatorNick);
        gm.accountManager.UpdateSessionTime();

        // Refresh leaderboard again
    }


    void LoadLeaderboard()
    {
        leaderboardLoadingText.text = "Loading..";
        WebClient c = new WebClient();
        c.DownloadStringCompleted += OnLeaderboardLoaded;
        string url = string.Format(url_leaderboard, gm.project.author + "-" + gm.project.name, gm.project.creatorNick);
        Debug.Log(url);
        c.DownloadStringAsync(new System.Uri(url));
    }

    void OnLeaderboardLoaded(object sender, DownloadStringCompletedEventArgs e)
    {
        if (e.Cancelled) { leaderboardLoadingText.text = "Canceled"; return; }
        if (e.Error != null) { leaderboardLoadingText.text = "Error: " + e.Error.Message; return; }


        List<Replay> leaderboardReplays = JsonConvert.DeserializeObject<List<Replay>>(e.Result);



        foreach (Transform child in leaderboardContent) if (child.name != "Item") Destroy(child.gameObject);
        GameObject prefab = leaderboardContent.GetChild(0).gameObject;
        prefab.SetActive(true);

        float height = 0;
        int place = 0;
        foreach (var item in leaderboardReplays)
        {
            place++;
            CreateLeaderboardItem(item, prefab, place, item.player == AccountManager.account.nick);
            height += 80 + 4;
        }

        leaderboardContent.GetComponent<RectTransform>().sizeDelta = new Vector2(leaderboardContent.GetComponent<RectTransform>().sizeDelta.x, height);
        prefab.SetActive(false);
    }


    void CreateLeaderboardItem(Replay replay, GameObject prefab, int place, bool isCurrentPlayer)
    {
        Transform itemgo = Instantiate(prefab, leaderboardContent).transform;
        LeaderboardUIItem item = itemgo.GetComponent<LeaderboardUIItem>();
        item.replay = replay;
        item.Refresh(place, isCurrentPlayer);
    }
}