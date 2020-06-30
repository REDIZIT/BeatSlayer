using Assets.SimpleLocalization;
using InGame.Game;
using System.IO;
using GameNet;
using ProjectManagement;
using Ranking;
using UnityEngine;
using UnityEngine.UI;
using Web;
using BeatSlayerServer.Dtos.Mapping;
using InGame.Leaderboard;
using InGame.Animations;
using InGame.Game.Spawn;

public class FinishHandler : MonoBehaviour
{
    public GameManager gm { get { return GetComponent<GameManager>(); } }
    public BeatManager bm;

    public AudioManager audioManager { get { return GetComponent<AudioManager>(); } }
    public AdvancedSaveManager prefsManager { get { return GetComponent<AdvancedSaveManager>(); } }
    public AccountManager accountManager;
    public CompactLeaderboard leaderboard;
    public FireworksSystem fireworksSystem;



    [Header("UI")]
    public GameObject finishOverlay;
    public RawImage coverImage;
    public Text authorText, nameText, creatorText;
    public Text scoreText, missedText, accuracyText, RPText, coinsText;
    public GameObject uploadingText, recordText;
    public GameObject heartIcon;
    public GameObject goToEditorBtn;
    public GameObject RPTextsContainer;
    

    [Header("Grade")]
    public Animator rankAnimator; // Anitmator for ranks (SS,S,A,B,C,D)
    public Text rankText;
    public Color[] gradeColors;

    [Header("Difficulties")]
    public Text cubesSpeedText;
    public Text musicSpeedText;
    public Toggle noArrowsToggle, noLinesToggle;
    public Transform difficultyContent;
    public Text difficultyText;

    [Header("Leaderboard")]
    public Transform leaderboardContent;
    public Text leaderboardLoadingText;

    [Header("Finish conditions")]
    public bool isNotStarting;
    public bool isAudioTimeZero;
    public bool isArrayEmpty;
    public bool isAudioStopped;

    public float audioTime;

    public void CheckLevelFinish()
    {
        // Debuggin' finish end
        isNotStarting = !gm.IsGameStartingMap;
        isAudioTimeZero = audioManager.asource.time == 0;
        isArrayEmpty = bm.beats.ToArray().Length == 0;
        isAudioStopped = !audioManager.asource.isPlaying;

        audioTime = audioManager.asource.time;


        if (FinishConditions())
        {
            OnLevelFinished(gm.scoringManager.Replay);
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


    private bool FinishConditions()
    {
        return
            !gm.IsGameStartingMap
            && audioManager.asource.time == 0
            //&& gm.beats.ToArray().Length == 0
            && !audioManager.asource.isPlaying;
    }




    void OnLevelFinished(ReplayData replay)
    {
        if (!gm.gameCompleted)
        {
            gm.gameCompleted = true;

            FinishServerActions(replay);

            if(Payload.CurrentAccount != null)
            {
                int coins = Payload.CurrentAccount.Coins;
                int addCoins = Mathf.RoundToInt(replay.Score / 16f * gm.scoringManager.maxCombo / 2f * gm.scoringManager.scoreMultiplier);

                Payload.CurrentAccount.Coins = coins + addCoins;
                prefsManager.Save();
            }


            HandleFinishUI(replay);

            gm.RateBtnUpdate();

            if(LoadingData.loadparams.Type == SceneloadParameters.LoadType.Moderation)
            {
                string filepath = Application.persistentDataPath + "/data/moderation/" + LoadingData.loadparams.Map.author + "-" + LoadingData.loadparams.Map.name + ".bsz";
                File.Delete(filepath);
            }
        }
    }




    void HandleFinishUI(ReplayData replay)
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

        ShowScoring(replay.Score, replay.Missed, replay.Accuracy);


        cubesSpeedText.text = replay.Difficulty.CubesSpeed == 1 ? "1.0x" : replay.Difficulty.CubesSpeed.ToString().Replace(",", ".") + "x";
        //musicSpeedText.text = (.replay.musicSpeed == 1 ? "1.0x" : gm.replay.musicSpeed.ToString().Replace(",", ".") + "x");
        noLinesToggle.isOn = gm.nolines;
        noArrowsToggle.isOn = gm.noarrows;

        heartIcon.SetActive(LoadingData.loadparams.Map.approved);

        UpdateDifficulty();


        goToEditorBtn.SetActive(LoadingData.loadparams.Type == SceneloadParameters.LoadType.Moderation);
    }
    public void UpdateDifficulty()
    {
        int difficulty = LoadingData.loadparams.difficultyInfo.stars;
        string diffName = LoadingData.loadparams.difficultyInfo.name;//LoadingData.loadparams.Map.difficultyName;

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

    private async void FinishServerActions(ReplayData replay)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) return;


        string trackname = gm.project.author.Trim() + "-" + gm.project.name.Trim();

        DatabaseScript.SendStatistics(trackname, gm.project.creatorNick, LoadingData.loadparams.difficultyInfo.id, DatabaseScript.StatisticsKeyType.Play);
        WebAPI.OnMapPlayed(LoadingData.loadparams.Map.approved);


        if (Payload.CurrentAccount == null)
        {
            leaderboard.SetStatus(LocalizationManager.Localize("NotLoggedIn"));
            return;
        }

        if (LoadingData.loadparams.IsPracticeMode)
        {
            leaderboard.SetStatus(LocalizationManager.Localize("PracticeMode"));
            return;
        }

        if (LoadingData.loadparams.Type != SceneloadParameters.LoadType.Author)
        {
            leaderboard.SetStatus(LocalizationManager.Localize("NotAuthorMap"));
            return;
        }

        if (!DatabaseScript.DoesMapExist(trackname, gm.project.creatorNick))
        {
            leaderboard.SetStatus(LocalizationManager.Localize("MapHasBeenDeleted"));
            return;
        }



        // Show or hide rp container
        if (LoadingData.loadparams.Map.approved) ShowRPLoading();
        else HideRP();


        uploadingText.SetActive(true);
        ReplayData bestReplay = await NetCore.ServerActions.Account.GetBestReplay(Payload.CurrentAccount.Nick, trackname, gm.project.creatorNick);


        if (bestReplay != null && replay.Score > bestReplay.Score)
        {
            fireworksSystem.StartEmitting();
            recordText.SetActive(true);
        }


        ReplaySendData data = await NetCore.ServerActions.Account.SendReplay(replay);
        uploadingText.SetActive(false);


        coinsText.text = "+" + data.Coins;
        Payload.CurrentAccount.Coins += data.Coins;

        if (LoadingData.loadparams.Map.approved)
        {
            ShowRP(data.RP);
        }

        ShowGrade(data.Grade);

        if(data.Grade == Grade.SS || data.Grade == Grade.S)
        {
            fireworksSystem.StartEmitting();
        }

        

        gm.accountManager.UpdateSessionTime();

       

        // Load map leaderboard validation

        if (!LoadingData.loadparams.Map.approved)
        {
            leaderboard.SetStatus(LocalizationManager.Localize("NotApprovedMap"));
            return;
        }

        await leaderboard.LoadLeaderboard(replay.Map.Trackname, replay.Map.Nick);

    }


    private void ShowGrade(Grade grade)
    {
        rankAnimator.Play("Show");
        rankText.text = grade.ToString();
        rankText.color = grade == Grade.Unknown ? gradeColors[0] : gradeColors[(int)grade];
    }
    private void ShowScoring(float score, int missed, float accuracy)
    {
        scoreText.text = Mathf.RoundToInt(score * 10f) / 10f + "";
        missedText.text = missed.ToString();
        accuracyText.text = Mathf.FloorToInt(accuracy * 1000) / 10f + "%";
    }
    private void ShowRP(float RP)
    {
        RPTextsContainer.SetActive(true);
        RPText.text = Mathf.FloorToInt(RP * 10) / 10f + "";
    }
    private void ShowRPLoading()
    {
        RPTextsContainer.SetActive(true);
        RPText.text = "";
    }
    private void HideRP()
    {
        RPTextsContainer.SetActive(false);
    }
}