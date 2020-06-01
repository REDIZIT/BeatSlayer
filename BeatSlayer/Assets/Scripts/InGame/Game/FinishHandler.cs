using Assets.SimpleLocalization;
using InGame.Game;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using ProjectManagement;
using Ranking;
using UnityEngine;
using UnityEngine.UI;

public class FinishHandler : MonoBehaviour
{
    public GameManager gm { get { return GetComponent<GameManager>(); } }
    public AudioManager audioManager { get { return GetComponent<AudioManager>(); } }
    public AdvancedSaveManager prefsManager { get { return GetComponent<AdvancedSaveManager>(); } }


    [Header("UI")]
    public GameObject finishOverlay;
    public RawImage coverImage;
    public Text authorText, nameText, creatorText;
    public Text scoreText, missedText, accuracyText, RPText, coinsText;
    public GameObject uploadingText, recordText;
    public GameObject heartIcon;
    public GameObject goToEditorBtn;

    [Header("Difficulties")]
    public Text cubesSpeedText;
    public Text musicSpeedText;
    public Toggle noArrowsToggle, noLinesToggle;
    public Transform difficultyContent;
    public Text difficultyText;

    [Header("Leaderboard")]
    public Transform leaderboardContent;
    public Text leaderboardLoadingText;


    
    

    public void CheckLevelFinish()
    {
        if (!gm.gameStarting && gm.gameStarted && audioManager.asource.time == 0 && gm.beats.ToArray().Length == 0 && !audioManager.asource.isPlaying)
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


            int coins = NetCorePayload.CurrentAccount.Coins;
            int addCoins = Mathf.RoundToInt(gm.replay.score / 16f * gm.maxCombo / 2f * gm.scoreMultiplier);
            
            NetCorePayload.CurrentAccount.Coins = coins + addCoins;
            prefsManager.Save();

            HandleFinishUI();

            gm.RateBtnUpdate();

            //FinishAchievements();
            
            if(LoadingData.loadparams.Type == SceneloadParameters.LoadType.Moderation)
            {
                string filepath = Application.persistentDataPath + "/data/moderation/" + LoadingData.loadparams.Map.author + "-" + LoadingData.loadparams.Map.name + ".bsz";
                File.Delete(filepath);
            }
        }
    }




    void HandleFinishUI()
    {
        finishOverlay.SetActive(true);
        gm.trackText.gameObject.SetActive(false);
        gm.trackTimeSlider.value = 100;
        gm.trackTextSliderText.text = "100%";

        string coverPath = ProjectManager.GetCoverPath(gm.project.author + "-" + gm.project.name, gm.project.creatorNick);
        coverImage.texture = coverPath == "" ? gm.defaultTrackTexture : ProjectManager.LoadTexture(coverPath);

        authorText.text = LoadingData.loadparams.Map.author;
        nameText.text = LoadingData.loadparams.Map.name;
        creatorText.text = LocalizationManager.Localize("by") + " " + LoadingData.loadparams.Map.nick;

        scoreText.text = Mathf.RoundToInt(gm.replay.score * 10f) / 10f + "";
        missedText.text = gm.replay.missed.ToString();
        accuracyText.text = Mathf.RoundToInt(gm.replay.Accuracy * 1000) / 10f + "%";

        cubesSpeedText.text = (gm.replay.cubesSpeed == 1 ? "1.0x" : gm.replay.cubesSpeed.ToString().Replace(",", ".") + "x");
        musicSpeedText.text = (gm.replay.musicSpeed == 1 ? "1.0x" : gm.replay.musicSpeed.ToString().Replace(",", ".") + "x");
        noLinesToggle.isOn = gm.nolines;
        noArrowsToggle.isOn = gm.noarrows;

        heartIcon.SetActive(LoadingData.loadparams.Map.approved);

        UpdateDifficulty();


        goToEditorBtn.SetActive(LoadingData.loadparams.Type == SceneloadParameters.LoadType.Moderation);
    }
    public void UpdateDifficulty()
    {
        int difficulty = LoadingData.loadparams.Map.difficultyStars;
        string diffName = LoadingData.loadparams.Map.difficultyName;

        difficultyText.text = diffName;
        float xOffset = difficultyText.preferredWidth + 20;


        foreach (Transform child in difficultyContent) if (child.name != "Item") Destroy(child.gameObject);
        GameObject prefab = difficultyContent.GetChild(0).gameObject;
        prefab.SetActive(true);

        for (int i = 1; i <= 10; i++)
        {
            Color clr = i <= difficulty ? Color.white : new Color(0.18f, 0.18f, 0.18f);

            GameObject item = Instantiate(prefab, difficultyContent);
            item.GetComponent<Image>().color = clr;
        }

        prefab.SetActive(false);

        difficultyContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset, 0);
    }

    void FinishAchievements()
    {/*
        (Social.Active as GooglePlayGames.PlayGamesPlatform).IncrementAchievement(GPGamesManager.youArePlayer, 1, (bool s) => { });
        (Social.Active as GooglePlayGames.PlayGamesPlatform).IncrementAchievement(GPGamesManager.dj, 1, (bool s) => { });
        (Social.Active as GooglePlayGames.PlayGamesPlatform).IncrementAchievement(GPGamesManager.musicKing, 1, (bool s) => { });*/

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

        if (gm.replay.missed == 0 && gm.replay.score >= 4000 && gm.replay.cubesSpeed == 1.5f && !gm.nolines && !gm.noarrows)
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
        string trackname = gm.project.author + "-" + gm.project.name;
        
        if (!LoadingData.loadparams.Map.approved)
        {
            leaderboardLoadingText.text = "Not approved map";
            yield break;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable) yield break;
        if (LoadingData.loadparams.Type != SceneloadParameters.LoadType.Author) yield break;

        if (!DatabaseScript.DoesMapExist(trackname, gm.project.creatorNick))
        {
            leaderboardLoadingText.text = "Map has been deleted";
            yield break;
        }
        
        DatabaseScript.SendStatistics(trackname, gm.project.creatorNick, LoadingData.loadparams.difficultyInfo.id, DatabaseScript.StatisticsKeyType.Play);

        RPText.text = ".";

        if (NetCorePayload.CurrentAccount == null) yield break;

        uploadingText.SetActive(true);

        RPText.text = "..";

        ReplayData bestReplay = null;
        bool bestReplayGot = false;
        /*AccountManager.GetBestReplay(NetCorePayload.CurrentAccount.Nick, trackname, gm.project.creatorNick, (ReplayInfo replay) =>
        {
            bestReplay = replay;
            bestReplayGot = true;
            Debug.Log("Best replay got");
        });*/

        NetCore.Subs.Accounts_OnGetBestReplay += info =>
        {
            Debug.Log("Best replay got!");
            if (bestReplay != null && bestReplay.Score > gm.replay.score)
            {
                recordText.SetActive(true);
            }


            AccountManager.SendReplay(gm.replay, (ReplaySendData data) =>
            {
                Debug.Log("OnSendReplay");
                
                RPText.text = Mathf.RoundToInt((float) 1).ToString();
                coinsText.text = "+" + data.Coins;
                NetCorePayload.CurrentAccount.Coins += data.Coins;
                
                LoadLeaderboard();
                uploadingText.SetActive(false);
            });

            gm.accountManager.UpdateSessionTime();
        };
        //Debug.Log("Before getbestreplay: " + NetCore.State);
        NetCore.ServerActions.Account.GetBestReplay(NetCorePayload.CurrentAccount.Nick, trackname, gm.project.creatorNick);
        //Debug.Log("After getbestreplay: " + NetCore.State);
    }


    void LoadLeaderboard()
    {
        leaderboardLoadingText.text = "Loading..";
        WebClient c = new WebClient();

        string url = string.Format(AccountManager.url_leaderboard, gm.project.author + "-" + gm.project.name, gm.project.creatorNick);

        string response = c.DownloadString(url);
        OnLeaderboardLoaded(response);
    }

    void OnLeaderboardLoaded(/*object sender, DownloadStringCompletedEventArgs e*/string response)
    {
        //if (e.Cancelled) { leaderboardLoadingText.text = "Canceled"; return; }
        //if (e.Error != null) { leaderboardLoadingText.text = "Error: " + e.Error.Message; return; }


        List<Replay> leaderboardReplays = JsonConvert.DeserializeObject<List<Replay>>(response);



        foreach (Transform child in leaderboardContent) if (child.name != "Item") Destroy(child.gameObject);
        GameObject prefab = leaderboardContent.GetChild(0).gameObject;
        prefab.SetActive(true);

        float height = 0;
        int place = 0;
        foreach (var item in leaderboardReplays)
        {
            place++;
            CreateLeaderboardItem(item, prefab, place, item.player == NetCorePayload.CurrentAccount.Nick);
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