using InGame.Game;
using InGame.Game.Spawn;
using InGame.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProjectManagement;
using Testing;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using InGame.Animations;
using InGame.Settings;
using Ranking;
using GameNet;
using InGame.Game.Tutorial;
using InGame.Extensions.Objects;
#if UNITYEDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    #region Unity components

    [Header("Components")]
    public AccountManager accountManager;
    public AudioManager audioManager;
    public FinishHandler finishHandler;
    public AdvancedSaveManager prefsManager;
    public GameUIManager UIManager;
    public BeatManager beatManager;
    public ScoringManager scoringManager;
    public CheatEngine cheatEngine;
    public MapTutorial tutorial;
    public MissAnim missAnim;
    public Analyzer analyzer;


    #endregion


    [HideInInspector] public Project project;
    [HideInInspector] public DifficultyInfo difficultyInfo;
    [HideInInspector] public Difficulty difficulty;
    [HideInInspector] public TestRequest testRequest;

    [Header("Environment")]
    public PostProcessVolume PostProcessing;
    public GameObject[] scenes;
    public Text trackText;

    [SerializeField] private TextMeshPro scoreText, missedText, perText, comboText, comboMultiplierText;
    [SerializeField] private TextMeshPro gradeText;



    [Header("Sabers")]
    public SaberController rightSaber;
    public SaberController leftSaber;


    [Header("Skills")]
    public GameObject timeTravelPanel;
    public GameObject skillImg;
    public Sprite[] skillSprites;


    [Header("UI")]
    public Slider trackTimeSlider;
    public Text trackTextSliderText, timeInSecondsText;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Image comboValueImg;
    [SerializeField] private Animator skipBtnAnimator;
    [SerializeField] private Text fpsText;
    private float comboMultiplierAnimValue;
    private bool isSkipAnimatorPlaying;


    public Texture2D defaultTrackTexture;

    [Header("Statistics UI")]
    public Image likeBtnImg;
    public Image dislikeBtnImg;


    [HideInInspector] public bool paused;
    [HideInInspector] public bool noarrows;
    [HideInInspector] public bool nolines;
    [HideInInspector] public bool gameCompleted;

    /// <summary>
    /// Is game in period when asource isn't playing but cubes are spawning
    /// </summary>
    public bool IsGameStartingMap { get; set; }

    private SettingsModel Settings => SettingsManager.Settings;

    private float Pitch { get; set; }
    private Camera cam;




    public void ProcessBeatTime()
    {
        if (LoadingData.loadparams.Type != SceneloadParameters.LoadType.AudioFile)
        {
            beatManager.ProcessSpawnCycle();
        }
        else
        {
            analyzer.AnalyzerUpdate();
        }
    }


    void Awake()
    {
        if (!enabled) return;
        
        #region Redirect to menu

        if (LoadingData.loadparams == null || (LoadingData.loadparams.Type == SceneloadParameters.LoadType.Menu && enabled))
        {
            SceneManager.LoadScene("Menu");
            return;
        }

        #endregion

        GetComponent<SceneController>().Init(GetComponent<SceneControllerUI>());
        cam = GetComponent<Camera>();

        CreateScene();

        if (LoadingData.loadparams.Type != SceneloadParameters.LoadType.AudioFile)
        {
            InitProject();
        }
        tutorial.IsEnabled = LoadingData.loadparams.Type == SceneloadParameters.LoadType.Tutorial;

        scoringManager.OnGameStart(LoadingData.loadparams.Map, LoadingData.loadparams.difficultyInfo, difficulty, LoadingData.loadparams.Type);


        InitSettings();

        beatManager.Setup(this, noarrows, nolines, difficulty.speed);


        InitGraphics();

        StartForUI(); 
    }
    void Start()
    {
        trackText.text = project.Trackname;

        AlignToSide();

        InitAudio();

        IsGameStartingMap = true;

        StartCoroutine(beatManager.IOnStart());

        skipBtnAnimator.gameObject.SetActive(beatManager.CanSkip());

    }
    void Update()
    {
        CheckFingerPause();

        UpdateSkipButton();

        if (paused) return;


        if (paused)
        {
            if (audioManager.asource.isPlaying)
            {
                audioManager.asource.Pause();
                if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) audioManager.spectrumAsource.Pause();
            }
            return;
        }

        ProcessBeatTime();

        fpsText.text = 1f / Time.smoothDeltaTime + " FPS";


        if (audioManager.asource != null)
        {
            if (audioManager.asource.clip != null)
            {
                if (audioManager.asource.time != 0)
                {
                    timeInSecondsText.text = SecondsToString(audioManager.asource.time) + " / " + SecondsToString(audioManager.asource.clip.length);
                }
            }
            else Debug.LogError("asource clip is null");
        }
        else Debug.LogError("asource is null");

        finishHandler.CheckLevelFinish();

        #region Score and combo

        if (!gameCompleted)
        {
            scoreText.text = Mathf.RoundToInt(scoringManager.Replay.Score * 10f) / 10f + "";
            missedText.text = scoringManager.Replay.Missed.ToString();
            perText.text = Mathf.RoundToInt(scoringManager.Replay.Accuracy * 1000f) / 10f + "%";

            comboValueImg.fillAmount = scoringManager.comboValue / scoringManager.comboValueMax;
            comboMultiplierText.text = "x" + scoringManager.comboMultiplier;

            comboMultiplierText.transform.localScale += new Vector3(comboMultiplierAnimValue, comboMultiplierAnimValue, comboMultiplierAnimValue);
        }

        // Runtime grade calculation
        // Using server conditions (might be legacy, needed to update)
        if (gradeText.gameObject.activeSelf)
        {
            gradeText.text = GetRuntimeGrade(scoringManager.Replay.Accuracy, scoringManager.Replay.Missed).ToString();
        }

        #endregion

        beatManager.SabersUpdate();

    }





    private void CreateScene()
    {
        GameObject sc = Instantiate(scenes[prefsManager.prefs.selectedMapId]);
        sc.transform.position = new Vector3(0, 0, 0);
    }
    private void InitProject()
    {
        project = LoadingData.project;
        difficultyInfo = LoadingData.loadparams.difficultyInfo;

        if (project.difficulties == null || project.difficulties.Count == 0)
        {
            project = ProjectUpgrader.UpgradeToDifficulty(project);
        }

        difficulty = project.difficulties.Find(c => c.id == difficultyInfo.id);
        Debug.Log("Id: " + difficultyInfo.id + "\nIs DIFF null? " + (difficulty == null) + "\nCount is: " + project.difficulties.Count);

        IEnumerable<BeatCubeClass> ls = difficulty == null ? project.beatCubeList : difficulty.beatCubeList;

        if (LoadingData.loadparams.IsPracticeMode)
        {
            ls = ls.Where(c => c.time >= LoadingData.loadparams.StartTime);
        }
       
        beatManager.SetBeats(ls);
    }


    void InitAudio()
    {
        audioManager.asource.clip = LoadingData.aclip;

        Pitch = LoadingData.loadparams.IsPracticeMode ? LoadingData.loadparams.MusicSpeed : 1;
        float time = LoadingData.loadparams.IsPracticeMode ? LoadingData.loadparams.StartTime : 0;


        audioManager.asource.pitch = Pitch;
        audioManager.asource.time = time;

        if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile)
        {
            AudioClip sClip = LoadingData.aclip.Clone();
            audioManager.spectrumAsource.clip = sClip;

            audioManager.spectrumAsource.pitch = Pitch;
            audioManager.spectrumAsource.time = time;
        }
    }
    void InitGraphics()
    {
        bool usePostProcessing = Settings.Graphics.IsGlowEnabled;

        PostProcessing.gameObject.SetActive(usePostProcessing);

        if (usePostProcessing)
        {
            int clamp = 0;
            int bloomPower = 0;

            switch(Settings.Graphics.GlowQuality)
            {
                case GlowQuality.Disabled: clamp = 0; break;
                case GlowQuality.Low: clamp = 1; break;
                case GlowQuality.High: clamp = 2; break;
            }

            switch (Settings.Graphics.GlowPower)
            {
                case GlowPower.Low: bloomPower = 1; break;
                case GlowPower.Middle: bloomPower = 2; break;
                case GlowPower.High: bloomPower = 3; break;
            }

            cam.allowHDR = Settings.Graphics.GlowQuality == GlowQuality.High;

            Bloom bloom = PostProcessing.profile.GetSetting<Bloom>();
            bloom.active = true;
            bloom.clamp.value = clamp;
            bloom.intensity.value = bloomPower;
        }


        fpsText.gameObject.SetActive(false);

        likeBtnImg.gameObject.SetActive(LoadingData.loadparams.Type != SceneloadParameters.LoadType.AudioFile);
        dislikeBtnImg.gameObject.SetActive(LoadingData.loadparams.Type != SceneloadParameters.LoadType.AudioFile);

        gradeText.gameObject.SetActive(Settings.Gameplay.ShowGrade);


        Color leftSaberColor = SSytem.instance.leftColor * (1 + SSytem.instance.GlowPowerSaberLeft / 25f);
        Color rightSaberColor = SSytem.instance.rightColor * (1 + SSytem.instance.GlowPowerSaberRight / 25f);

        float trailLifeTime = SSytem.instance.TrailLength / 100f * 0.4f;

        leftSaber.Init(leftSaberColor, prefsManager.prefs.selectedLeftSaberId, prefsManager.prefs.selectedSaberEffect, trailLifeTime);
        rightSaber.Init(rightSaberColor, prefsManager.prefs.selectedRightSaberId, prefsManager.prefs.selectedSaberEffect, trailLifeTime);
    }
    void InitSettings()
    {
        if (!LoadingData.loadparams.Map.approved)
        {
            noarrows = SSytem.instance.GetBool("NoArrows");
            nolines = SSytem.instance.GetBool("NoLines");
        }

        SettingsModel settings = SettingsManager.Settings;

        cam.fieldOfView = settings.Gameplay.FOV;
        transform.parent.position += new Vector3(0, settings.Gameplay.CameraHeight, settings.Gameplay.CameraOffset);
        transform.parent.localEulerAngles += new Vector3(settings.Gameplay.CameraAngle, 0);

    }


    void StartForUI()
    {
        if (prefsManager.prefs.skillSelected == -1)
        {
            skillImg.SetActive(false);
            return;
        }
        skillImg.transform.GetChild(0).GetComponent<Image>().sprite = skillSprites[prefsManager.prefs.skillSelected];
        skillImg.transform.GetChild(2).GetComponent<Text>().text = "x" + prefsManager.prefs.skills[prefsManager.prefs.skillSelected].count;
    }



    void AlignToSide()
    {
        if (SettingsManager.Settings.Graphics.TracknameTextPosition == TracknameTextPosition.Bottom)
        {
            trackText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            trackText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            trackText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 170);
        }
        else if (SettingsManager.Settings.Graphics.TracknameTextPosition == TracknameTextPosition.BottomReverse)
        {
            trackText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            trackText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            trackText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 90);

            trackText.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 110);
        }
    }
    



    #region Combo

    public IEnumerator comboMultiplierAnim()
    {
        comboMultiplierAnimValue = 0.2f;
        yield return new WaitForSeconds(0.3f);
        comboMultiplierAnimValue = -0.22f;
        yield return new WaitForSeconds(0.3f);
        comboMultiplierAnimValue = 0;
        comboMultiplierText.transform.localScale = new Vector3(1, 1, 1);
    }

    #endregion


    #region Pause menu buttons voids

    public void CheckFingerPause()
    {
        // ========================================================================================================
        //  Пауза 3 тачами
        if (SettingsManager.Settings.Gameplay.FingerPauseEnabled)
        {
            if (Input.touchCount > 2
                && Input.touches[0].phase != TouchPhase.Began
                && Input.touches[1].phase != TouchPhase.Began
                && Input.touches[2].phase != TouchPhase.Began)
            {
                if (!paused)
                {
                    bool isAllIn = true;
                    for (int i = 0; i < Input.touches.Length; i++)
                    {
                        Vector2 pos = Input.touches[i].position;
                        if (!(pos.x > 50 && pos.x < Screen.width - 50 &&
                              pos.y > 50 && pos.y < Screen.height - 50)) { isAllIn = false; break; }
                    }
                    if (isAllIn)
                    {
                        Pause();
                    }
                }
            }

        }
    }
    public void Pause()
    {
        paused = true;
        pausePanel.SetActive(true);
        audioManager.PauseSource();
        UIManager.OnPause();
        Time.timeScale = 0;
        if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) audioManager.spectrumAsource.Pause();
    }
    public void Unpause()
    {
        pausePanel.SetActive(false);
        UIManager.OnResume();
        StartCoroutine(Unpauing());
    }
    public IEnumerator Unpauing()
    {
        paused = false;
        audioManager.PlaySource();
        if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) audioManager.spectrumAsource.Play();

        float timespeed = 0;
        float encrease = 0.02f;
        while (timespeed < 1)
        {
            if(paused) break;
            
            encrease *= 1.001f;
            timespeed += encrease;
            if (timespeed > 1)
            {
                timespeed = 1;
            }

            audioManager.asource.pitch = timespeed * Pitch;

            if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) audioManager.spectrumAsource.pitch = timespeed * Pitch;

            Time.timeScale = timespeed;
            yield return new WaitForEndOfFrame();
        }
    }


    // =================================================================================================
    //  Game End Stuff
    public void Exit()
    {
        accountManager.UpdateSessionTime();

        SceneloadParameters parameters = SceneloadParameters.GoToMenuPreset();
        SceneController.instance.LoadScene(parameters);
    }
    public void Restart()
    {
        Time.timeScale = 1;
        SceneController.instance.LoadScene(LoadingData.loadparams);
    }
    #endregion

    #region Beat cubes voids

    public List<IBeat> activeCubes = new List<IBeat>();
    
    public void MissedBeatCube(IBeat beat)
    {
        scoringManager.OnCubeMiss(beat);

        if(beat.GetClass().type != BeatCubeClass.Type.Bomb)
        {
            missAnim.OnMiss();
            UseSkill();
        }

        cheatEngine.RemoveCube(beat);
    }


    public void BeatCubeSliced(IBeat beat)
    {
        if (Settings.Sound.SliceEffectEnabled)
        {
            AndroidNativeAudio.play(Payload.HitSoundIds[Random.Range(0, Payload.HitSoundIds.Count)], Settings.Sound.SliceEffectVolume / 100f);
        }

        if(beat.GetClass().type == BeatCubeClass.Type.Bomb)
        {
            missAnim.OnMiss();
        }

        scoringManager.OnCubeHit(beat);
        cheatEngine.RemoveCube(beat);
    }
    public void BeatLineSliced(IBeat beat)
    {
        scoringManager.OnLineHit();
        cheatEngine.RemoveCube(beat);
    }
    public void BeatLineHold()
    {
        scoringManager.OnLineHold();
    }

    

    #endregion



    #region Skills
    void UseSkill()
    {
        if (prefsManager.prefs.skillSelected == -1) return;
        if (prefsManager.prefs.skills[prefsManager.prefs.skillSelected].count > 0)
        {
            if (prefsManager.prefs.skillSelected == 0) TimeTravel();
            else if (prefsManager.prefs.skillSelected == 1) Explode();

            skillImg.transform.GetChild(2).GetComponent<Text>().text = "x" + prefsManager.prefs.skills[prefsManager.prefs.skillSelected].count;
        }
    }

    void TimeTravel()
    {
        prefsManager.prefs.skills[prefsManager.prefs.skillSelected].count--;
        prefsManager.Save();
        StartCoroutine(TimeTravelling());
    }
    IEnumerator TimeTravelling()
    {
        timeTravelPanel.gameObject.SetActive(true);
        skillImg.transform.GetChild(1).gameObject.SetActive(true);
        skillImg.transform.GetChild(1).GetComponent<Image>().fillAmount = 1;

        float currentTime = audioManager.asource.time;
        float timeToTravel = currentTime - 2;

        if (timeToTravel < 0) timeToTravel = 0;

        audioManager.PauseSource();
        for (int i = 0; i < activeCubes.Count; i++)
        {
            activeCubes[i].SpeedMultiplier = 0;
        }
        yield return new WaitForSeconds(0.5f);


        for (int i = 0; i < activeCubes.Count; i++)
        {
            activeCubes[i].SpeedMultiplier = -1;
        }
        while (currentTime > timeToTravel)
        {
            audioManager.asource.time -= Time.deltaTime;
            currentTime -= Time.deltaTime;
            skillImg.transform.GetChild(1).GetComponent<Image>().fillAmount -= Time.deltaTime / 2f;
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < activeCubes.Count; i++) activeCubes[i].SpeedMultiplier = 0;
        //timeTravelImg.transform.GetChild(1).GetComponent<Image>().fillAmount = 0;
        skillImg.transform.GetChild(1).gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        audioManager.PlaySource();
        for (int i = 0; i < activeCubes.Count; i++) activeCubes[i].SpeedMultiplier = 1;

        timeTravelPanel.gameObject.SetActive(false);
    }

    void Explode()
    {
        foreach (IBeat cube in activeCubes)
        {
            cube.Destroy();
        }
        prefsManager.prefs.skills[prefsManager.prefs.skillSelected].count--;
        prefsManager.Save();
    }
    #endregion


    #region High quality code (cringe but okay)

    public void GoToEditor()
    {
        TestManager.OpenEditor();
    }

    void UpdateSkipButton()
    {
        if (!isSkipAnimatorPlaying)
        {
            if (beatManager.CanSkip())
            {
                isSkipAnimatorPlaying = true;
                skipBtnAnimator.Play("Show");
            }
        }
        else
        {
            if (!beatManager.CanSkip())
            {
                skipBtnAnimator.Play("Hide");
            }
        }
    }
    public void OnSkipBtnClick()
    {
        audioManager.OnKeyPress();
        beatManager.Skip();
    }

    /// <summary>
    /// Copy of server side grade
    /// </summary>
    public Grade GetRuntimeGrade(float accuracy, int misses)
    {
        if (accuracy == 1)
        {
            return Grade.SS;
        }

        if (accuracy >= 0.96f && misses <= 2)
        {
            return Grade.S;
        }

        if (accuracy >= 0.93f)
        {
            return Grade.A;
        }

        if (accuracy >= 0.7f)
        {
            return Grade.B;
        }

        if (accuracy >= 0.5f)
        {
            return Grade.C;
        }

        return Grade.D;
    }


    #endregion





    public void RateBtnUpdate()
    {
        int state = prefsManager.prefs.GetRateState(project.Trackname);

        if (state != 0)
        {
            bool liked = state == 1;

            likeBtnImg.GetComponentsInChildren<Image>()[0].color = new Color32(0, (byte)(liked ? 145 : 34), 0, 255);
            likeBtnImg.GetComponentsInChildren<Image>()[1].color = new Color32(0, (byte)(liked ? 145 : 34), 0, 255);

            dislikeBtnImg.GetComponentsInChildren<Image>()[0].color = new Color32((byte)(!liked ? 145 : 34), 0, 0, 255);
            dislikeBtnImg.GetComponentsInChildren<Image>()[1].color = new Color32((byte)(!liked ? 145 : 34), 0, 0, 255);
        }
    }
    public void OnRateTrackBtnClicked(GameObject btn)
    {
        int state = prefsManager.prefs.GetRateState(project.Trackname);
        

        bool liked = btn.name == "LikeBtn";

        if (state == 0)
        {
            prefsManager.prefs.SetRateState(project.Trackname, liked ? 1 : -1);
            
            string trackname = project.author + "-" + project.name;
            DatabaseScript.SendStatistics(trackname, project.creatorNick, difficultyInfo.id, liked ? DatabaseScript.StatisticsKeyType.Like : DatabaseScript.StatisticsKeyType.Dislike);

            likeBtnImg.color = new Color32(0, (byte)(liked ? 145 : 34), 0, 255);
            likeBtnImg.transform.GetChild(0).GetComponent<Image>().color = new Color32(0, (byte)(liked ? 145 : 34), 0, 255);

            dislikeBtnImg.color = new Color32((byte)(!liked ? 145 : 34), 0, 0, 255);
            dislikeBtnImg.transform.GetChild(0).GetComponent<Image>().color = new Color32((byte)(!liked ? 145 : 34), 0, 0, 255);
        }

        prefsManager.Save();
    }



    // Просто для удобства и красоты
    public string SecondsToString(float allTimeInSeconds)
    {
        int mins = Mathf.FloorToInt(allTimeInSeconds / 60f);
        int secs = Mathf.FloorToInt(allTimeInSeconds - mins * 60);

        return mins + ":" + (secs < 10 ? "0" + secs : secs.ToString());
    }
    public int[] SplitTime(float allTime)
    {
        int mins = Mathf.FloorToInt(allTime / 60f);
        int secs = Mathf.FloorToInt(allTime - mins * 60);
        return new int[2] { mins, secs };
    }
}
