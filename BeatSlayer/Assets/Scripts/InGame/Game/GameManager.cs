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
#if UNITYEDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    #region Unity components

    //public SettingsManager settingsManager { get { return GetComponent<SettingsManager>(); } }
    public AccountManager accountManager;
    public AudioManager audioManager { get { return GetComponent<AudioManager>(); } }
    public FinishHandler finishHandler { get { return GetComponent<FinishHandler>(); } }
    public AdvancedSaveManager prefsManager { get { return GetComponent<AdvancedSaveManager>(); } }
    public GameUIManager UIManager { get { return GetComponent<GameUIManager>(); } }
    public BeatManager beatManager { get { return GetComponent<BeatManager>(); } }

    public ScoringManager scoringManager;
    public MissAnim missAnim;
    public CheatEngine cheatEngine;

    #endregion


    private Camera cam;


    [HideInInspector] public Project project;
    [HideInInspector] public DifficultyInfo difficultyInfo;
    [HideInInspector] public Difficulty difficulty;
    [HideInInspector] public TestRequest testRequest;

    private float Pitch { get; set; }



    public Analyzer a;

    public PostProcessVolume PostProcessing;
    public Canvas canvas;

    public GameObject[] scenes;

    public TextMeshPro scoreText, missedText, perText, comboText, comboMultiplierText;
    [SerializeField] TextMeshPro gradeText;
    public Text trackText;



    [Header("Beat cubes stuff")]
    public bool displayColor;
    public SpawnPointScript[] spawnPoints;
    [HideInInspector] public float comboMultiplierAnimValue;

    
    public GameObject BeatCubePrefab, BeatLinePrefab;

    public SaberController rightSaber, leftSaber;
    public Material orangeMaterial, blueMaterial;


    [Header("Skills")]
    public GameObject timeTravelPanel;
    public GameObject skillImg;
    public Sprite[] skillSprites;

    [Header("UI")]
    public GameObject pausePanel;
    public GameObject lvlfinishPanel, pauseButton;
    public Text coinsText;
    public Text fpsText;
    public Slider trackTimeSlider;
    public Text trackTextSliderText, timeInSecondsText;
    public GameObject finishRecordPanel;
    public Image comboValueImg;
    public Animator skipBtnAnimator;
    bool isSkipAnimatorPlaying;

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
    public GameState State { get; set; }
    public enum GameState
    {
        Starting, Started
    }

    private string fullTrackName;
    private SettingsModel Settings => SettingsManager.Settings;


    // Settings
    float sliceeffectVolume = 0.8f;








    public void InitProject()
    {
         project = LoadingData.project;


        if(project.difficulties == null || project.difficulties.Count == 0)
        {
            project = ProjectUpgrader.UpgradeToDifficulty(project);
        }
        
        difficulty = project.difficulties.Find(c => c.id == difficultyInfo.id);
        Debug.Log("Id: " + difficultyInfo.id + "\nIs DIFF null? " + (difficulty == null) + "\nCount is: " + project.difficulties.Count);
        
        IEnumerable<BeatCubeClass> ls = difficulty == null ? project.beatCubeList.OrderBy(c => c.time) : difficulty.beatCubeList.OrderBy(c => c.time);

        if (LoadingData.loadparams.IsPracticeMode)
        {
            ls = ls.Where(c => c.time >= LoadingData.loadparams.StartTime);
        }


        beatManager.beats.AddRange(ls);
    }
    public void ProcessBeatTime()
    {
        if (LoadingData.loadparams.Type != SceneloadParameters.LoadType.AudioFile)
        {
            beatManager.ProcessSpawnCycle();
            //float beatAudioTime = IsGameStartingMap ? asReplacer : audioManager.asource.time + beatManager.fieldCrossTime;
            //if (beats.Count > 0 && beatAudioTime >= beats[0].time)
            //{
            //    beatManager.SpawnBeatCube(beats[0]);
            //    beats.RemoveAt(0);
            //    ProcessBeatTime();
            //}
        }
        else
        {
            a.AnalyzerUpdate();
        }
    }
    public AudioClip CloneAudioClip(AudioClip audioClip, string newName)
    {
        AudioClip newAudioClip = AudioClip.Create(newName, audioClip.samples, audioClip.channels, audioClip.frequency, false);
        float[] copyData = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(copyData, 0);
        newAudioClip.SetData(copyData, 0);
        return newAudioClip;
    }



    void Awake()
    {
        if (!enabled) return;
        
        #region Redirect to menu

        if (LoadingData.loadparams == null || (LoadingData.loadparams.Type == SceneloadParameters.LoadType.Menu && this.enabled))
        {
            Debug.Log("Redirect to menu");
            SceneManager.LoadScene("Menu");
            return;
        }

        #endregion

        GetComponent<SceneController>().Init(GetComponent<SceneControllerUI>());
        cam = GetComponent<Camera>();

        //scoringManager.scoreMultiplier = SettingsManager.GetScoreMultiplier();
        scoringManager.scoreMultiplier = 1;

        accountManager = GetComponent<AccountManager>();

        fullTrackName = LoadingData.project.author + "-" + LoadingData.project.name;

        GameObject sc = Instantiate(scenes[prefsManager.prefs.selectedMapId]);
        sc.transform.position = new Vector3(0, 0, 0);

        difficultyInfo = LoadingData.loadparams.difficultyInfo;
        if (LoadingData.loadparams.Type != SceneloadParameters.LoadType.AudioFile)
        {
            InitProject();
        }

        scoringManager.OnGameStart(LoadingData.loadparams.Map, LoadingData.loadparams.difficultyInfo, difficulty);


        InitSettings();

        beatManager.Setup(this, noarrows, nolines, difficulty.speed);


        InitGraphics();

        StartForUI(); 
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
            audioManager.spectrumAsource.pitch = Pitch;
            audioManager.spectrumAsource.time = time;
        }




        if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile)
        {
            AudioClip sClip = CloneAudioClip(LoadingData.aclip, "SUKAAAAA");
            audioManager.spectrumAsource.clip = sClip;
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



        Color leftSaberColor = SSytem.instance.leftColor * (1 + SSytem.instance.GlowPowerSaberLeft / 25f);
        Color rightSaberColor = SSytem.instance.rightColor * (1 + SSytem.instance.GlowPowerSaberRight / 25f);

        float trailLifeTime = SSytem.instance.TrailLength / 100f * 0.4f;

        leftSaber.Init(leftSaberColor, prefsManager.prefs.selectedLeftSaberId, prefsManager.prefs.selectedSaberEffect, trailLifeTime);
        rightSaber.Init(rightSaberColor, prefsManager.prefs.selectedRightSaberId, prefsManager.prefs.selectedSaberEffect, trailLifeTime);
    }
    void InitSettings()
    {
        sliceeffectVolume = SettingsManager.Settings.Sound.SliceEffectVolume / 100f * 0.3f;


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

            //trackText.transform.GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 8.3f);
            //trackText.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition =
            //    new Vector2(trackText.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition.x, 8.3f);
        }
    }
    void Start()
    {
        trackText.text = fullTrackName;

        AlignToSide();

        InitAudio();

        IsGameStartingMap = true;

        StartCoroutine(beatManager.IOnStart());

        skipBtnAnimator.gameObject.SetActive(beatManager.CanSkip());

    }

    //ScreenOrientation screenOrientation = ScreenOrientation.Landscape;
    private void Update()
    {
        CheckFingerPause();

        UpdateSkipButton();

        /* Пэтя: Да нахуй воно мэне надо xD
        try
        {
            // Настройка UI
            
            if (Screen.orientation != screenOrientation)
            {
                screenOrientation = Screen.orientation;
                //Debug.Log("New orin: " + screenOrientation.ToString());
                bool isPortrait = Screen.orientation == ScreenOrientation.Portrait;
                if (Application.isEditor) isPortrait = Screen.width < Screen.height;

                //CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                //// Инвентируем исходное разрешение (как будто переворачиваем)
                //scaler.referenceResolution = isPortrait ? new Vector2(600, 800) : new Vector2(800, 600);
                //scaler.matchWidthOrHeight = isPortrait ? 1 : 0;

                cam.fieldOfView = isPortrait ? 90 : 60;

                missedText.transform.position = isPortrait ? new Vector3(-6.49f, -10.74f, 1.9f) : new Vector3(-14.54f, -0.83f, 1.9f);
                perText.transform.position = isPortrait ? new Vector3(-6.49f, -13.66f, 1.9f) : new Vector3(-14.54f, -4.1f, 1.9f);

                scoreText.transform.position = isPortrait ? new Vector3(6.49f, -10.74f, 1.9f) : new Vector3(14.54f, -0.83f, 1.9f);
                comboText.transform.position = isPortrait ? new Vector3(6.49f, -13.66f, 1.9f) : new Vector3(14.54f, -4.1f, 1.9f);

                comboValueImg.transform.parent.localPosition = isPortrait ? new Vector3(333.8f, -292, -61.2f) : new Vector3(388f, -292f, 0);
                comboValueImg.transform.parent.eulerAngles = isPortrait ? new Vector3(145, 195f, 0) : new Vector3(19.388f, 31.207f, 9.7f);
            }
        }
        catch (Exception err) { Debug.LogError("Catched error::UI: " + err.Message); }*/


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
        gradeText.text = GetRuntimeGrade(scoringManager.Replay.Accuracy, scoringManager.Replay.Missed).ToString();

        #endregion

        beatManager.SabersUpdate();
        
    }
    Dictionary<int, Vector3> inputTouchesStartPoses = new Dictionary<int, Vector3>();

    // Этот метод нужен для распознования ошибочных тачей
    // (Если очень быстро тапать по одному месту, то будет не один, появляющийся и исчезающий, тач, а несколко
    // Этот баг может привести к вызову меню, а т.к. игрок не успевает на меню отреагировать, то зачастую попадает по кнопки Меню
    /*public bool isRealTouch(Touch t)
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.touches[i].position == t.position) // Если это один и тот же тач
            {
                continue;
            }
            float curDistX = Mathf.Abs(Input.touches[i].position.x - t.position.x);
            float curDistY = Mathf.Abs(Input.touches[i].position.y - t.position.y);
            
            if(curDistX <= 100 || curDistY <= 100)
            {
                return false;
            }
        }
        return true;
    }*/


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
        scoringManager.OnCubeMiss();

        missAnim.OnMiss();

        if (!prefsManager.prefs.hasAchiv_Blinked)
        {
            /*Social.ReportProgress(GPGamesManager.achievement_Blinked, 100, (bool success) =>
            {
                if (!success) Debug.LogError("Blinked error");
                if (success)
                {
                    prefsManager.prefs.hasAchiv_Blinked = true;
                    prefsManager.Save();
                }
            });*/
        }

        UseSkill();
        cheatEngine.RemoveCube(beat);
    }


    public void BeatCubeSliced(IBeat beat)
    {
        try
        {
            if (SettingsManager.Settings.Sound.SliceEffectEnabled)
            {
                AndroidNativeAudio.play(LCData.hitsIds[Random.Range(0, LCData.hitsIds.Length - 1)], sliceeffectVolume, sliceeffectVolume, 1, 0, 1.2f);
            }
        }
        catch (System.Exception err)
        {
            Debug.LogWarning("Slice sound err: " + err);
        }

        scoringManager.OnCubeHit();
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
        int state = prefsManager.prefs.GetRateState(fullTrackName);

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
        int state = prefsManager.prefs.GetRateState(fullTrackName);
        

        bool liked = btn.name == "LikeBtn";

        if (state == 0)
        {
            prefsManager.prefs.SetRateState(fullTrackName, liked ? 1 : -1);
            
            string trackname = project.author + "-" + project.name;
            DatabaseScript.SendStatistics(trackname, project.creatorNick, difficultyInfo.id, liked ? DatabaseScript.StatisticsKeyType.Like : DatabaseScript.StatisticsKeyType.Dislike);

            likeBtnImg.color = new Color32(0, (byte)(liked ? 145 : 34), 0, 255);
            likeBtnImg.transform.GetChild(0).GetComponent<Image>().color = new Color32(0, (byte)(liked ? 145 : 34), 0, 255);

            dislikeBtnImg.color = new Color32((byte)(!liked ? 145 : 34), 0, 0, 255);
            dislikeBtnImg.transform.GetChild(0).GetComponent<Image>().color = new Color32((byte)(!liked ? 145 : 34), 0, 0, 255);
        }

        prefsManager.Save();
    }



    // Просто для удобства и красоты (какая нахуй красота дебил?! в каком месте он красивый???) кода
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
