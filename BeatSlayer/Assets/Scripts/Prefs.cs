using System;
using System.Collections.Generic;

/// <summary>
/// Один и ооочень большой костыль размером с игру
/// </summary>
[Serializable]
public class Prefs
{
    // Achievements
    public bool hasAchiv_Blinked, hasAchiv_Uff, hasAchiv_NewMapNewLife, hasAchiv_ThatsMy, hasAchiv_Hardcore;
    public bool hasAchiv_ShoppingSpree = false;
    public bool hasAchiv_MadeInChina = false;
    public bool hasAchiv_Terrible = false;

    // Game settings
    public bool postProcessing = true;
    public bool bloom = true;
    public int bloomQuality = 2;
    public int bloomPower = 1;
    public bool sliceSound = true;
    public float sliceSoundVolume = 0.5f;
    public bool sliceEffect = true;
    public bool menuMusic = true;
    public int trackTextSide = 0;
    public int sortTracks = 2; // 0 - Popular, 1 - Likes, 2 - Index (Date)
    public int sortUsers = 0; // All, Players, Dev
    public bool enableFingerPause = true; // Pause if 3 touches detected
    public int vibrationTime = 50; // Вибрация: 0-выкл, 50-низк,100-сред,200-долгое

    // Track difficult
    public float musicSpeed = 1;
    public float cubeSpeed = 1;
    public bool noArrows = false;
    public bool noLines = false;

    // Other
    public int selectedMapId = 0;
    public bool mapUnlocked0, mapUnlocked1, mapUnlocked2, mapUnlocked3;
    public int coins = 0;
    Dictionary<string, int> _records = new Dictionary<string, int>();




    public Prefs()
    {
        if (sabers == null || sabers.Count == 0)
        {
            // Upgrade
            sabers = new List<Saber>();
            for (int i = 0; i < boughtSabers.Length; i++)
            {
                sabers.Add(new Saber() { id = i, isBought = boughtSabers[i] });
            }
        }
    }



    [Obsolete]
    public int GetRecord(string trackname)
    {
        if (_records != null && _records.ContainsKey(trackname)) return _records[trackname];
        return 0;
    }
    //public void SetRecord(string trackname, int value)
    //{
    //    if (_records != null && _records.ContainsKey(trackname)) _records[trackname] = value;
    //    else _records.Add(trackname, value);
    //}
    public Dictionary<string, int> _states = new Dictionary<string, int>();
    public int GetRateState(string trackname)
    {
        if (_states != null && _states.ContainsKey(trackname)) return _states[trackname];
        return 0;
    }
    public void SetRateState(string trackname, int value)
    {
        if (_states != null && _states.ContainsKey(trackname)) _states[trackname] = value;
        else _states.Add(trackname, value);
    }
    public bool showLeaderboardTip = true;

    // DailyRewarder
    public DateTime prevPlay;
    public int daysPlayed;

    // Poll
    public int lastPollId;
    public string lastPollAnswer;


    // Shop
    public bool shopTutorial = false;

    // Skills
    public int skillSelected = 0;
    public List<Skill> skills = new List<Skill>()
    {
        new Skill() { name = "Time travel", description = "TimeTravelDescription", count = 0, cost = 600 },
        new Skill() { name = "Explosion", description = "ExplosionDescription", count = 0, cost = 400 }
    };

    // Boosters
    public int selectedBooster;
    public List<Booster> boosters = new List<Booster>()
    {
        new Booster() { name = "Coins booster (x2)", description = "DoubleCoins", count = 0, cost = 1000 },
        new Booster() { name = "Coins decelerator (/2)", description = "DivideCoins", count = 0, cost = 1500 }
    };

    // Sabers
    public int selectedSaber;

    public int selectedLeftSaberId, selectedRightSaberId;

    public bool[] boughtSabers = new bool[7] { true, false, false, false, false, false, false };
    public List<Saber> sabers;

    public float colorPower = 2;

    // Sabers effects
    public int selectedSaberEffect;
    public bool[] boughtSaberEffects = new bool[3] { true, false, false };
    public int[] saberEffectsCosts = new int[3] { 0, 200000, 15999 };

    // Editor link
    public bool showedEditorAvailableWindow = false;
}