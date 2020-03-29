using Assets.SimpleLocalization;
using InGame.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITYEDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public SettingsManager settingsManager { get { return GetComponent<SettingsManager>(); } }
    AccountManager accountManager;
    public AudioManager audioManager { get { return GetComponent<AudioManager>(); } }




    public Analyzer a;

    public PostProcessVolume PostProcessing;
    public Canvas canvas;

    public GameObject[] scenes;

    public Text DEBUG_TEXT;

    [Header("Text Mesh Pro")]
    public Text missedTextMsg;
    public TextMeshPro scoreText, missedText, perText, comboText, comboMultiplierText;
    public Text trackText;



    [Header("Beat cubes stuff")]
    public bool displayColor;
    public SpawnPointScript[] spawnPoints;
    List<BeatCubeClass> beats = new List<BeatCubeClass>();

    float comboValue = 0, comboValueMax = 16, comboMultiplierAnimValue;
    float comboMultiplier = 1, score;
    [HideInInspector] public float earnedScore;

    List<SpawnPointClass> spawnPointClasses = new List<SpawnPointClass>();
    public GameObject BeatCubePrefab, BeatLinePrefab;
    Project project;
    int cubesMissed, cubesSliced, cubesSpawned;

    public SaberController rightSaber, leftSaber;
    public Material orangeMaterial, blueMaterial;

    float scoreMultiplier;

    [Header("Skills")]
    public GameObject timeTravelPanel;
    public GameObject skillImg;
    public Sprite[] skillSprites;

    [Header("UI")]
    public GameObject pausePanel;
    public GameObject lvlfinishPanel, pauseButton;
    public Text coinsText;
    public Animator missEffectAnimator;
    public Text fpsText;
    public Slider trackTimeSlider;
    public Text trackTextSliderText, timeInSecondsText;
    public GameObject finishRecordPanel;
    public Slider pitchSlider;
    public Image comboValueImg;

    [Header("Game end")]
    public Image endCoverImage;
    public Text endNameText, endAuthorText, endCreatorText, endScoreText, endRecordText, endDifficultText;
    public Sprite defaultTrackSprite;

    [Header("Statistics UI")]
    public Image likeBtnImg;
    public Image dislikeBtnImg;


    [HideInInspector] public bool paused;
    [HideInInspector] public float pitch = 1;
    [HideInInspector] public float cubesspeed = 1;
    [HideInInspector] public bool noarrows;
    [HideInInspector] public bool nolines;
    string fullTrackName;
    bool gameCompleted, gameStarted, gameStarting;
    float maxCombo = 1;
    int[] clipTime;

    //PrefsManager prefsManager
    //{
    //    get { return GetComponent<PrefsManager>(); }
    //}
    AdvancedSaveManager prefsManager
    {
        get { return GetComponent<AdvancedSaveManager>(); }
    }

    // Settings
    float sliceeffectVolume = 0.8f;

    float bitCubeEndTime = 0; // Изначально 0.7 т.к. позиция спавнера 42, а куб доходит до 0 координаты (скорость куба 60) за 0.7 сек (получили из 42/60)
    // 70 - Нормальная дистанция для разрезания
    // 60 - Скорость куба
    // bitCubeEndTime = 70 / 60 = ~1.16


    public void InitProject()
    {
        project = LCData.project;

        beats.AddRange(project.beatCubeList.OrderBy(c => c.time).ToArray());
    }
    public void ProcessBeatTime()
    {
        if (LCData.loadingType != LCData.LoadingType.AudioFile)
        {
            float asTime = gameStarting ? asReplacer : audioManager.asource.time + bitCubeEndTime / pitch;
            if (beats.ToArray().Length > 0 && asTime >= beats[0].time)
            {
                SpawnBeatCube(beats[0]);

                beats.RemoveAt(0);
                ProcessBeatTime();
            }
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

        if (LCData.sceneLoading == "" && this.enabled)
        {
            SceneManager.LoadScene("LoadScene");
            return;
        }

        #endregion

        scoreMultiplier = SettingsManager.GetScoreMultiplier();

        accountManager = GetComponent<AccountManager>();

        try
        {

            fullTrackName = LCData.project.author + "-" + LCData.project.name;

            GameObject sc = Instantiate(scenes[prefsManager.prefs.selectedMapId]);
            sc.transform.position = new Vector3(0, 0, 0);

            if (LCData.loadingType != LCData.LoadingType.AudioFile)
            {
                InitProject();
            }
        }
        catch (System.Exception err)
        {
            trackText.text = "Error Setup: " + err.Message;
        }

        try { InitSettings(); }
        catch (System.Exception err) { trackText.text = "Error InitSettings: " + err.Message; }

        try { InitGraphics(); }
        catch (System.Exception err) { trackText.text = "Error InitGraphics: " + err.Message; }

        try { InitAudio(); }
        catch (System.Exception err) { trackText.text = "Error InitAudio: " + err.Message; }

        try { StartForUI(); }
        catch (System.Exception err) { trackText.text = "Error StartForUI: " + err.Message; }
    }
    void InitAudio()
    {
        audioManager.asource.clip = LCData.aclip;

        pitch = SSytem.instance.GetFloat("MusicSpeed") / 10f;
        audioManager.asource.pitch = pitch;
        if (LCData.loadingType == LCData.LoadingType.AudioFile) audioManager.spectrumAsource.pitch = pitch;
        pitchSlider.value = (pitch - 1) * 10;
        OnPitchChanged();




        if (LCData.loadingType == LCData.LoadingType.AudioFile)
        {
            AudioClip sClip = CloneAudioClip(LCData.aclip, "SUKAAAAA");
            audioManager.spectrumAsource.clip = sClip;
        }


        clipTime = SplitTime(LCData.aclip.length);

        Debug.Log("Audio Clip is null? " + (LCData.aclip == null));
        Debug.Log("Audio Extension is " + (LCData.project.audioExtension == Project.AudioExtension.Mp3 ? "mp3" : "ogg"));
    }
    void InitGraphics()
    {
        bool usePostProcessing = SSytem.instance.GetInt("GlowQuality") != 0;
        PostProcessing.gameObject.SetActive(usePostProcessing);
        if (usePostProcessing)
        {
            //int bloomQuality = prefsManager.prefs.bloomQuality;
            int bloomQuality = SSytem.instance.bloomQuality;
            int bloomPower = SSytem.instance.GetInt("GlowPower") + 1;

            bool useBloom = bloomQuality != 0;
            GetComponent<Camera>().allowHDR = bloomQuality >= 2;
            int clamp = bloomQuality == 1 ? 1 : bloomQuality == 2 ? 3 : 0;

            Bloom bloom = PostProcessing.profile.GetSetting<Bloom>();
            bloom.active = useBloom;
            bloom.clamp.value = clamp;
            bloom.intensity.value = bloomPower;
        }
        fpsText.gameObject.SetActive(false);

        likeBtnImg.gameObject.SetActive(LCData.loadingType != LCData.LoadingType.AudioFile);
        //likeBtnImg.gameObject.SetActive(true);
        dislikeBtnImg.gameObject.SetActive(LCData.loadingType != LCData.LoadingType.AudioFile);
        //dislikeBtnImg.gameObject.SetActive(true);

        rightSaber.Init(SSytem.instance.rightColor, prefsManager.prefs.selectedSaber);
        leftSaber.Init(SSytem.instance.leftColor, prefsManager.prefs.selectedSaber);

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            spawnPointClasses.Add(new SpawnPointClass() { index = i });
        }
    }
    void InitSettings()
    {
        sliceeffectVolume = SSytem.instance.GetFloat("SliceVolume") * 0.3f;
        cubesspeed = SSytem.instance.GetFloat("CubesSpeed") / 10f;
        pitch = SSytem.instance.GetFloat("MusicSpeed") / 10f;
        noarrows = SSytem.instance.GetBool("NoArrows");
        nolines = SSytem.instance.GetBool("NoLines");


        float distanceToSpawn = spawnPoints[0].transform.position.z;
        float localOffset = 10;
        bitCubeEndTime = (distanceToSpawn + localOffset) / (cubesspeed * 60) * pitch;
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



    float asReplacer = 0;
    void AlignToSide()
    {
        int side = SSytem.instance.GetInt("TrackTextSide");
        if (side == 1)
        {
            trackText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            trackText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            trackText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 36);
        }
        else if (side == 2)
        {
            trackText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0);
            trackText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0);
            trackText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 22);

            trackText.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 10f);

            trackText.transform.GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 8.3f);
            trackText.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition =
                new Vector2(trackText.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition.x, 8.3f);
        }
    }
    IEnumerator Start()
    {
        DEBUG_TEXT.gameObject.SetActive(File.Exists(Application.persistentDataPath + "/Dev.txt"));
        trackText.text = fullTrackName + (pitch == 1 ? "" : " x" + pitch);

        AlignToSide();

        gameStarting = true;
        gameStarted = true;
        if (LCData.loadingType == LCData.LoadingType.AudioFile) { audioManager.PlaySpectrumSource(); }
        yield return new WaitForSeconds(bitCubeEndTime / pitch / cubesspeed);
        audioManager.PlaySource();
        if (paused)
        {
            audioManager.asource.Pause();
            if (LCData.loadingType == LCData.LoadingType.AudioFile) audioManager.spectrumAsource.Pause();
        }
        gameStarting = false;
    }

    ScreenOrientation screenOrientation = ScreenOrientation.Landscape;
    private void Update()
    {
        // ========================================================================================================
        //  Пауза 3 тачами
        try
        {
            if (SSytem.instance.GetInt("FingerPause") == 0)
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
        catch (Exception err) { Debug.LogError("Catched error::Touch: " + err.Message); }
        
        try
        {
            // Настройка UI
            if (Screen.orientation != screenOrientation)
            {
                screenOrientation = Screen.orientation;
                //Debug.Log("New orin: " + screenOrientation.ToString());
                bool isPortrait = Screen.orientation == ScreenOrientation.Portrait;
                if (Application.isEditor) isPortrait = Screen.width < Screen.height;

                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                // Инвентируем исходное разрешение (как будто переворачиваем)
                scaler.referenceResolution = isPortrait ? new Vector2(600, 800) : new Vector2(800, 600);
                scaler.matchWidthOrHeight = isPortrait ? 1 : 0;

                GetComponent<Camera>().fieldOfView = isPortrait ? 90 : 60;

                missedText.transform.position = isPortrait ? new Vector3(-6.49f, -10.74f, 1.9f) : new Vector3(-14.54f, -0.83f, 1.9f);
                perText.transform.position = isPortrait ? new Vector3(-6.49f, -13.66f, 1.9f) : new Vector3(-14.54f, -4.1f, 1.9f);

                scoreText.transform.position = isPortrait ? new Vector3(6.49f, -10.74f, 1.9f) : new Vector3(14.54f, -0.83f, 1.9f);
                comboText.transform.position = isPortrait ? new Vector3(6.49f, -13.66f, 1.9f) : new Vector3(14.54f, -4.1f, 1.9f);

                comboValueImg.transform.parent.localPosition = isPortrait ? new Vector3(333.8f, -292, -61.2f) : new Vector3(388f, -292f, 0);
                comboValueImg.transform.parent.eulerAngles = isPortrait ? new Vector3(145, 195f, 0) : new Vector3(19.388f, 31.207f, 9.7f);
            }
        }
        catch (Exception err) { Debug.LogError("Catched error::UI: " + err.Message); }
        

        if (paused) return;


        try
        {
            if (earnedScore >= 1)
            {
                float rounded = Mathf.FloorToInt(earnedScore) * scoreMultiplier;
                earnedScore -= rounded;
                score += rounded;
            }

            if (!paused && gameStarted)
            {
                if (gameStarting)
                {
                    asReplacer += Time.deltaTime;
                }
                if (paused)
                {
                    if (audioManager.asource.isPlaying)
                    {
                        audioManager.asource.Pause();
                        if (LCData.loadingType == LCData.LoadingType.AudioFile) audioManager.spectrumAsource.Pause();
                    }
                    return;
                }

                ProcessBeatTime();
            }


            if (trackTimeSlider.value >= 60)
            {
                if (trackTextSliderText.transform.parent != trackTimeSlider.transform)
                {
                    trackTextSliderText.transform.SetParent(trackTimeSlider.transform);
                }
            }
        }
        catch (Exception err) { Debug.LogError("Catched error::Checks: " + err.Message); }
        fpsText.text = 1f / Time.smoothDeltaTime + " FPS";

        
        try
        {
            if (audioManager.asource != null)
            {
                if (audioManager.asource.clip != null)
                {
                    if (audioManager.asource.time != 0)
                    {
                        int[] curTime = new int[2];
                        try
                        {
                            curTime = SplitTime(audioManager.asource.time);
                        }
                        catch (Exception err) { Debug.LogError("Catched error::Time::Split: " + err.Message); }
                        
                        try
                        {
                            if (timeInSecondsText == null) Debug.LogError("TimeInSecondsText is null");
                            timeInSecondsText.text =
                            curTime[0] + ":" + (curTime[1] < 10 ? "0" + curTime[1] : curTime[1] + "") + " / " +
                            clipTime[0] + ":" + (clipTime[1] < 10 ? "0" + clipTime[1] : clipTime[1] + "");
                        }
                        catch (Exception err) { Debug.LogError("Catched error::Time::Text: " + err.Message); }
                        
                    }
                    else Debug.LogError("asource clip time is zero");
                }
                else Debug.LogError("asource clip is null");
            }
            else Debug.LogError("asource is null");
        }
        catch (Exception err) { Debug.LogError("Catched error::Time: " + err.Message); }
        


        // =================================
        //  Игровые процессы
        if (!gameStarted) return;

        try
        {
            #region Level Finished

            //if (!gameStarting && !paused && !asource.isPlaying && (beats.ToArray().Length == 0 || (beats.ToArray().Length < 10 && asource.time == 0)))
            if (!gameStarting && audioManager.asource.time == 0 && beats.ToArray().Length == 0 && !audioManager.asource.isPlaying
                /*&& (LCData.loadingType == LCData.LoadingType.AudioFile ? spectrumAsource.isPlaying : true)*/)
            {
                if (!gameCompleted)
                {
                    gameCompleted = true;
                    lvlfinishPanel.SetActive(true);
                    trackTimeSlider.value = 100;
                    trackTextSliderText.text = "100%";

                    if (LCData.loadingType != LCData.LoadingType.ProjectFile)
                    {
                        if (Application.internetReachability != NetworkReachability.NotReachable)
                        {
                            AccountTrackRecord atRecord = accountManager.GetRecord(LCData.author, LCData.name, LCData.track.nick);
                            AccountTrackRecord newRecord = new AccountTrackRecord()
                            {
                                author = LCData.author,
                                name = LCData.name,
                                nick = LCData.track.nick,
                                score = float.Parse(scoreText.text),
                                missed = int.Parse(missedText.text),
                                percent = float.Parse(perText.text.Replace("%", ""))
                            };

                            if (atRecord == null)
                            {
                                endRecordText.text = "";
                            }
                            else
                            {
                                if (newRecord.score > atRecord.score)
                                {
                                    endRecordText.text = "Новый рекорд!";
                                }
                                else
                                {
                                    endRecordText.text = $"Рекорд: <color=#fff>{atRecord.score}</color> <color=#f40>{atRecord.percent}%</color> <color=#f00>{atRecord.missed}</color>";
                                }
                            }

                            accountManager.UpdateRecord(LCData.author, LCData.name, LCData.track.nick, newRecord);
                            accountManager.UpdatePlayedMap(LCData.author, LCData.name, LCData.track.nick);
                            accountManager.UpdateSessionTime();
                        }
                    }

                    int coins = prefsManager.prefs.coins;
                    int addCoins = Mathf.RoundToInt(score / 16f * maxCombo / 2f * scoreMultiplier);


                    StartCoroutine(coinsAnimator(coins, addCoins));
                    prefsManager.prefs.coins = coins + addCoins;
                    prefsManager.Save();



                    string coverPath = TheGreat.GetCoverPath(Application.persistentDataPath + "/maps/" + LCData.track.group.author + "-" + LCData.track.group.name + "/" + LCData.track.nick, fullTrackName);
                    endCoverImage.sprite = coverPath == "" ? defaultTrackSprite : TheGreat.LoadSprite(coverPath);

                    endAuthorText.text = LCData.track.group.author;
                    endNameText.text = LCData.track.group.name;
                    endCreatorText.text = LocalizationManager.Localize("by") + " " + LCData.track.nick;

                    endScoreText.text = $"<color=#fff>{scoreText.text}</color> <color=#f40>{perText.text}</color> <color=#f00>{missedText.text}</color>";

                    string cubeSpeed = LocalizationManager.Localize("CubesSpeed") + ": " + (cubesspeed == 1 ? "1.0x" : cubesspeed + "x");
                    string musicSpeed = LocalizationManager.Localize("MusicSpeed") + ": " + (pitch == 1 ? "1.0x" : pitch + "x");
                    string noLines = LocalizationManager.Localize("NoLines") + "? " + (nolines ? LocalizationManager.Localize("Yes") : LocalizationManager.Localize("No"));
                    string noArrows = LocalizationManager.Localize("NoArrows") + "? " + (noarrows ? LocalizationManager.Localize("Yes") : LocalizationManager.Localize("No"));
                    endDifficultText.text = "<size=18>" + LocalizationManager.Localize("Difficult") + @"</size>
" + cubeSpeed + @"
" + musicSpeed + @"
" + noLines + @"
" + noArrows;


                    RateBtnUpdate();

                    trackText.gameObject.SetActive(false);

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

                    if (cubesMissed == 0 && score >= 4000 && cubesspeed == 1.5f && !nolines && !noarrows)
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

                    (Social.Active as GooglePlayGames.PlayGamesPlatform).IncrementAchievement(GPGamesManager.youArePlayer, 1, (bool s) => { });
                    (Social.Active as GooglePlayGames.PlayGamesPlatform).IncrementAchievement(GPGamesManager.dj, 1, (bool s) => { });
                    (Social.Active as GooglePlayGames.PlayGamesPlatform).IncrementAchievement(GPGamesManager.musicKing, 1, (bool s) => { });

                    if (cubesMissed == cubesSpawned && cubesSpawned >= 10 && !prefsManager.prefs.hasAchiv_Terrible) // 10 is random value :D
                    {
                        Social.ReportProgress(GPGamesManager.terrible, 100, (bool success) => { if (!success) Debug.LogError("Achiv error: terrible"); });
                        prefsManager.prefs.hasAchiv_Terrible = true;
                        prefsManager.Save();
                    }
                }
            }
            else
            {
                float trackPer = (audioManager.asource.time < audioManager.asource.clip.length ?
                    audioManager.asource.time / audioManager.asource.clip.length : 1) * 100;
                trackPer = Mathf.Round(trackPer * 10) / 10;
                trackTimeSlider.value = trackPer;
                trackTextSliderText.text = trackPer + "%";
            }

            #endregion
        }
        catch (Exception err) { Debug.LogError("Catched error::Level Finished: " + err.Message); }
        
        try
        {
            #region Score and combo

            if (!gameCompleted)
            {
                scoreText.text = score.ToString();
                missedText.text = cubesMissed.ToString();
                float per = cubesSpawned > 0 ? ((float)cubesMissed / (float)cubesSliced) : 1;
                float perRounded = (Mathf.RoundToInt((100 - per * 100f) * 100f) / 100f);
                perText.text = (perRounded < 0 ? 0 : perRounded) + "%";
                if (comboValue >= comboValueMax && comboMultiplier < 16)
                {
                    comboValue = 2;
                    comboMultiplier *= 2;
                    comboValueMax = 8 * comboMultiplier;
                    StartCoroutine(comboMultiplierAnim());
                }
                else if (comboValue <= 0)
                {
                    if (comboMultiplier != 1)
                    {
                        comboMultiplier /= 2;
                        comboValue = comboValueMax - 5;
                    }
                    else
                    {
                        comboValue = 0;
                    }
                }
                if (comboValue > 0)
                {
                    //comboValue -= Time.deltaTime * comboMultiplier * 0.4f;
                }
                comboValueImg.fillAmount = comboValue / comboValueMax;
                comboMultiplierText.text = "x" + comboMultiplier;
                if (comboMultiplier > maxCombo) maxCombo = comboMultiplier;
                comboMultiplierText.transform.localScale += new Vector3(comboMultiplierAnimValue, comboMultiplierAnimValue, comboMultiplierAnimValue);
            }

            #endregion
        }
        catch (Exception err) { Debug.LogError("Catched error::Score and combo: " + err.Message); }
        

        try
        {
            #region Slicing

            if (Application.isEditor)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                bool r = false, l = false;
                if (Input.GetMouseButtonDown(0))
                {
                    if (!inputTouchesStartPoses.ContainsKey(0))
                    {
                        inputTouchesStartPoses.Add(0, Input.mousePosition);
                    }
                    else
                    {
                        inputTouchesStartPoses[0] = Input.mousePosition;
                    }

                }
                else if (Input.GetMouseButton(0))
                {
                    if (inputTouchesStartPoses[0].x >= Screen.width / 2f)
                    {
                        r = true;
                        rightSaber.SetSword(Input.mousePosition);
                    }
                    else
                    {
                        l = true;
                        leftSaber.SetSword(Input.mousePosition);
                    }
                }
                rightSaber.SetEnabled(r);
                leftSaber.SetEnabled(l);

                RaycastHit[] hits = Physics.RaycastAll(ray, 100);
                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.name == "LineCollider")
                    {
                        hit.transform.parent.GetComponent<Bit>().Spec(hit.point);
                    }
                    else if (hit.transform.GetComponent<Bit>() != null && !hit.transform.GetComponent<Bit>().isDead)
                    {
                        Bit bit = hit.transform.GetComponent<Bit>();
                        bit.Sliced();
                    }
                }

            }
            else
            {
                bool r = false, l = false; // Включение и выключение мечей
                for (int i = 0; i < Input.touches.Length; i++)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.touches[i].position);
                    //RaycastHit hit;

                    int saberSide = 0;
                    if (Input.touches[i].phase == TouchPhase.Began)
                    {
                        if (!inputTouchesStartPoses.ContainsKey(Input.touches[i].fingerId))
                        {
                            inputTouchesStartPoses.Add(Input.touches[i].fingerId, Input.touches[i].position);
                        }
                        else
                        {
                            inputTouchesStartPoses[Input.touches[i].fingerId] = Input.touches[i].position;
                        }

                    }
                    else if (Input.touches[i].phase != TouchPhase.Canceled && Input.touches[i].phase != TouchPhase.Ended)
                    {
                        if (inputTouchesStartPoses[Input.touches[i].fingerId].x >= Screen.width / 2f)
                        {
                            r = true;
                            rightSaber.SetSword(Input.touches[i].position);
                            saberSide = 1;
                        }
                        else
                        {
                            l = true;
                            leftSaber.SetSword(Input.touches[i].position);
                            saberSide = -1;
                        }
                    }

                    RaycastHit[] hits;
                    hits = Physics.RaycastAll(ray, 100);

                    for (int rayI = 0; rayI < hits.Length; rayI++)
                    {
                        RaycastHit hit = hits[rayI];

                        if (hit.transform.name == "LineCollider")
                        {
                            hit.transform.parent.GetComponent<Bit>().Spec(hit.point);
                        }
                        else if (hit.transform.GetComponent<Bit>() != null && !hit.transform.GetComponent<Bit>().isDead)
                        {
                            if (hit.transform.GetComponent<Bit>().saberType == 0 || hit.transform.GetComponent<Bit>().saberType == saberSide)
                            {
                                hit.transform.GetComponent<Bit>().Sliced(i);
                            }
                        }
                    }
                    //if (Physics.Raycast(ray, out hit, 100))
                    //{
                    //    if (hit.transform.name == "LineCollider")
                    //    {
                    //        hit.transform.parent.GetComponent<Bit>().Spec(hit.point);
                    //    }
                    //    else if (hit.transform.GetComponent<Bit>() != null && !hit.transform.GetComponent<Bit>().isDead)
                    //    {
                    //        if(hit.transform.GetComponent<Bit>().saberType == 0 || hit.transform.GetComponent<Bit>().saberType == saberSide)
                    //        {
                    //            hit.transform.GetComponent<Bit>().Sliced(i);
                    //        }
                    //    }
                    //}
                }
                //rightSaber.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = r;
                //leftSaber.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = l;
                rightSaber.SetEnabled(r);
                leftSaber.SetEnabled(l);
            }

            foreach (SpawnPointClass c in spawnPointClasses)
            {
                if (c.cooldown > 0)
                {
                    c.cooldown -= Time.deltaTime;
                }
            }

            #endregion
        }
        catch (Exception err) { Debug.LogError("Catched error::Slicing: " + err.Message); }
        
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

    public void Pause()
    {
        paused = true;
        pausePanel.SetActive(true);
        audioManager.PauseSource();
        if (LCData.loadingType == LCData.LoadingType.AudioFile) audioManager.spectrumAsource.Pause();
    }
    public void Unpause()
    {
        pausePanel.SetActive(false);
        StartCoroutine(Unpauing());
    }
    public IEnumerator Unpauing()
    {
        paused = false;
        audioManager.PlaySource();
        if (LCData.loadingType == LCData.LoadingType.AudioFile) audioManager.spectrumAsource.Play();

        float timespeed = 0;
        float encrease = 0.02f;
        while (timespeed < 1)
        {
            encrease *= 1.001f;
            timespeed += encrease;
            if (timespeed > 1)
            {
                timespeed = 1;
            }
            audioManager.asource.pitch = timespeed * pitch;
            if (LCData.loadingType == LCData.LoadingType.AudioFile) audioManager.spectrumAsource.pitch = timespeed * pitch;
            Time.timeScale = timespeed;
            yield return new WaitForEndOfFrame();
        }
    }


    // =================================================================================================
    //  Game End Stuff
    public void Exit()
    {
        accountManager.UpdateSessionTime();

        LCData.sceneLoading = "Menu";
        SceneManager.LoadScene("LoadScene");
    }
    public void Restart()
    {
        //LCData.sceneLoading = "ServerLevel";
        LCData.sceneLoading = "Game";
        SceneManager.LoadScene("LoadScene");
    }
    public void OnPitchChanged()
    {
        Text sliderText = pitchSlider.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<Text>();
        float pitch = 1 + pitchSlider.value / 10f;
        sliderText.text = "x" + (pitch == 1 ? "1.0" : pitch.ToString());
        //prefsManager.prefs.musicSpeed = pitch;
        prefsManager.Save();
    }
    #endregion

    #region Beat cubes voids

    public List<GameObject> activeCubes = new List<GameObject>();
    public void SpawnBeatCube(BeatCubeClass beat)
    {
        BeatCubeClass.Type type = beat.type;
        if (type == BeatCubeClass.Type.Dir && noarrows)
        {
            type = BeatCubeClass.Type.Point;
        }
        else if (type == BeatCubeClass.Type.Line && nolines)
        {
            return;
        }

        GameObject c = Instantiate(beat.type == BeatCubeClass.Type.Line ? BeatLinePrefab : BeatCubePrefab);
        activeCubes.Add(c);

        c.GetComponent<Bit>().gs = this;
        c.GetComponent<Bit>().speed *= cubesspeed;
        c.GetComponent<Bit>().useSoundEffect = SSytem.instance.GetBool("SliceSound");

        c.GetComponent<Bit>().type = type;
        c.GetComponent<Bit>().subType = beat.subType;

        c.GetComponent<Bit>().saberType = beat.saberType;

        c.GetComponent<Bit>().leftSaberColor = SSytem.instance.leftColor * prefsManager.prefs.colorPower * new Color(2f, 0.5f, 0.5f);
        c.GetComponent<Bit>().rightSaberColor = SSytem.instance.rightColor * prefsManager.prefs.colorPower * new Color(2f, 0.5f, 0.5f);

        c.GetComponent<Bit>().leftArrowColor = SSytem.instance.leftDirColor * prefsManager.prefs.colorPower;
        c.GetComponent<Bit>().rightArrowColor = SSytem.instance.rightDirColor * prefsManager.prefs.colorPower;

        c.GetComponent<Bit>().vibrationTime = SSytem.instance.GetInt("Vibration") * 50;

        if (beat.type == BeatCubeClass.Type.Line)
        {
            List<Vector3> points = new List<Vector3>();
            for (int i = 0; i < beat.linePoints.Count; i++)
            {
                Vector3 v3 = beat.linePoints[i];
                points.Add(v3);
            }
            c.GetComponent<Bit>().SpecSpawn(points.ToArray());
        }

        //int road = bcc[0].road == -1 ? GetBestSpawnPoint(c.GetComponent<Bit>().sliceDir) : bcc[0].road;
        //int road = bcc[0].road;
        int road = beat.road == -1 ? GetBestSpawnPoint(beat) : beat.road;

        if (road >= spawnPoints.Length) road = UnityEngine.Random.Range(0, 3);
        Transform selectedSpawnPoint = spawnPoints[road].transform;
        c.transform.position = new Vector3(selectedSpawnPoint.position.x, beat.level == 0 ? 1 : 5f, selectedSpawnPoint.position.z);
        spawnPoints[road].Spawn(beat.type);
        c.transform.name = "BeatCube";
        cubesSpawned++;

        c.GetComponent<Bit>().Start();
    }
    string missedBeatCubeMsg = "";
    public void MissedBeatCube()
    {
        if (missedBeatCubeMsg == "") missedBeatCubeMsg = LocalizationManager.Localize("Miss");

        int alreadyMissed = 0;
        missEffectAnimator.Play("MissEffectAnim");
        if (missedTextMsg.color.a == 0)
        {
            missedTextMsg.text = missedBeatCubeMsg;
        }
        else
        {
            if (missedTextMsg.text == missedBeatCubeMsg)
            {
                alreadyMissed = 2;
                missedTextMsg.text = missedBeatCubeMsg + " x2";
            }
            else if (missedTextMsg.text.Contains(missedBeatCubeMsg + " x"))
            {
                alreadyMissed = int.Parse(missedTextMsg.text.Replace(missedBeatCubeMsg + " x", ""));
                alreadyMissed++;
                missedTextMsg.text = missedBeatCubeMsg + " x" + alreadyMissed;
            }
        }

        missedTextMsg.transform.GetComponent<Animator>().Play(0);
        comboValue -= 10;
        score -= 5 * scoreMultiplier;
        if (score < 0) score = 0;
        cubesMissed++;

        if (!prefsManager.prefs.hasAchiv_Blinked)
        {
            Social.ReportProgress(GPGamesManager.achievement_Blinked, 100, (bool success) =>
            {
                if (!success) Debug.LogError("Blinked error");
                if (success)
                {
                    prefsManager.prefs.hasAchiv_Blinked = true;
                    prefsManager.Save();
                }
            });
        }

        UseSkill();
    }

    int[] audio_hitsSoundId = new int[10];
    public void BeatCubeSliced()
    {
        try
        {
            if (SSytem.instance.GetBool("SliceSound"))
            {
                AndroidNativeAudio.play(LCData.hitsIds[UnityEngine.Random.Range(0, LCData.hitsIds.Length - 1)], sliceeffectVolume, sliceeffectVolume, 1, 0, 1.2f);
            }
        }
        catch (System.Exception err)
        {
            Debug.LogWarning("Slice sound err: " + err);
        }

        try
        {
            score += comboMultiplier * scoreMultiplier;
            cubesSliced++;
            comboValue += 1;
        }
        catch (System.Exception err)
        {
            Debug.LogWarning("Slice sound err #2: " + err);
        }
    }
    public void BeatLineSliced()
    {
        score += comboMultiplier * scoreMultiplier;
        cubesSliced++;
        comboValue += 1;
    }

    public int GetBestSpawnPoint(BeatCubeClass beat)
    {
        List<SpawnPointClass> spawnPoints = spawnPointClasses.OrderBy(o => o.cooldown).ToList();

        if (beat.type == BeatCubeClass.Type.Dir)
        {
            List<int> available = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                if (spawnPoints[i].cooldown <= 0)
                {
                    available.Add(i);
                }
            }
            int rnd = UnityEngine.Random.Range(0, available.ToArray().Length);
            spawnPoints[rnd].cooldown = 0.3f;
            return spawnPoints[rnd].index;
        }
        else
        {
            int rnd = UnityEngine.Random.Range(0, spawnPoints.ToArray().Length);
            spawnPoints[rnd].cooldown = 0.3f;
            return spawnPointClasses[rnd].index;
        }

        //// Список тех точек которые полностью свободены
        //List<int> freePoints = new List<int>();

        //// Поиск наилучшего варианта (Если freepoints пустой)
        //float min = 999;
        //int bestIndex = Random.Range(0,4);

        //for (int i = 0; i < spawnPoints.Length; i++)
        //{
        //    if(spawnPoints[i].cooldown < min)
        //    {
        //        min = spawnPoints[i].cooldown;
        //        bestIndex = i;
        //    }


        //    // Если точка полностью свободна, то добавляем её в список
        //    if (spawnPoints[i].cooldown <= 0)
        //    {
        //        freePoints.Add(i);
        //    }
        //}

        //// Если послностью свободных точек нет, то возвращаем лучший вариант 
        //if(freePoints.ToArray().Length == 0)
        //{
        //    return bestIndex;
        //}
        //else
        //{
        //    // Если есть полностью свободные точки, то вернуть одну из них
        //    return freePoints[Random.Range(0, freePoints.ToArray().Length)];
        //}
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
            activeCubes[i].GetComponent<Bit>().speed = 0;
        }
        yield return new WaitForSeconds(0.5f);


        for (int i = 0; i < activeCubes.Count; i++)
        {
            activeCubes[i].GetComponent<Bit>().speed = -activeCubes[i].GetComponent<Bit>()._speed;
        }
        while (currentTime > timeToTravel)
        {
            audioManager.asource.time -= Time.deltaTime;
            currentTime -= Time.deltaTime;
            skillImg.transform.GetChild(1).GetComponent<Image>().fillAmount -= Time.deltaTime / 2f;
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < activeCubes.Count; i++) activeCubes[i].GetComponent<Bit>().speed = 0;
        //timeTravelImg.transform.GetChild(1).GetComponent<Image>().fillAmount = 0;
        skillImg.transform.GetChild(1).gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        audioManager.PlaySource();
        for (int i = 0; i < activeCubes.Count; i++) activeCubes[i].GetComponent<Bit>().speed = activeCubes[i].GetComponent<Bit>()._speed;

        timeTravelPanel.gameObject.SetActive(false);
    }

    void Explode()
    {
        foreach (GameObject cube in activeCubes)
        {
            cube.GetComponent<Bit>().SendBitSliced(Vector2.zero);
        }
        prefsManager.prefs.skills[prefsManager.prefs.skillSelected].count--;
        prefsManager.Save();
    }
    #endregion



    void RateBtnUpdate()
    {
        int state = prefsManager.prefs.GetRateState(fullTrackName);
        Debug.Log("RateBtnUpdate: state is " + state);

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

            Debug.Log("RateBtnSET: state is " + (liked ? 1 : -1));

            //File.AppendAllText(Application.persistentDataPath + "/order.ls", (liked ? "Liked:" : "Disliked:") + fullTrackName + "\n");
            TheGreat.SendStatistics(fullTrackName, LCData.track.nick, liked ? "like" : "dislike");

            likeBtnImg.color = new Color32(0, (byte)(liked ? 145 : 34), 0, 255);
            likeBtnImg.transform.GetChild(0).GetComponent<Image>().color = new Color32(0, (byte)(liked ? 145 : 34), 0, 255);

            dislikeBtnImg.color = new Color32((byte)(!liked ? 145 : 34), 0, 0, 255);
            dislikeBtnImg.transform.GetChild(0).GetComponent<Image>().color = new Color32((byte)(!liked ? 145 : 34), 0, 0, 255);
        }

        prefsManager.Save();
    }



    // Просто для удобства и красоты кода
    public int[] SplitTime(float allTime)
    {
        int mins = Mathf.FloorToInt(allTime / 60f);
        int secs = Mathf.FloorToInt(allTime - mins * 60);
        return new int[2] { mins, secs };
    }

    public string dbgStr;
    public void Log(string str)
    {
        dbgStr += str + @"
";
    }

    IEnumerator coinsAnimator(float _coins, int toadd)
    {
        float boosting = 1;
        if (prefsManager.prefs.selectedBooster != -1)
        {
            Booster booster = prefsManager.prefs.boosters[prefsManager.prefs.selectedBooster];
            if (booster.count > 0)
            {
                boosting = booster.name.Contains("x2") ? 2 : 0.5f;
                toadd = Mathf.RoundToInt(toadd * boosting);
                booster.count--;
            }
        }

        prefsManager.Save();


        float coins = _coins;
        float target = coins + toadd;


        coinsText.text = coins + "<color=white>+" + toadd + (boosting == 1 ? "" : " <b>x" + boosting + "</b>") + "</color>";


        yield return new WaitForSeconds(0.6f);
        while (coins < target)
        {
            coins += 0.1f + (target - coins) / 50f;

            if (coins >= target) coins = target;

            coinsText.text = Mathf.RoundToInt(coins) + "<color=white>+" + Mathf.RoundToInt(target - coins) + "</color>";
            yield return new WaitForEndOfFrame();
        }

        coinsText.text = Mathf.RoundToInt(target) + "";
    }
}