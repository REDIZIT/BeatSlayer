using Pixelplacement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Assets.SimpleLocalization;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using Newtonsoft.Json;
using GooglePlayGames.BasicApi.Multiplayer;
using ProjectManagement;
using Ranking;
using UnityEngine.Serialization;

public class AccountManager : MonoBehaviour
{
    public static LegacyAccount LegacyAccount;
    [FormerlySerializedAs("viewedAccount")] public LegacyAccount viewedLegacyAccount;

    [Header("Account page")]
    public ScrollRect accountPageRect;
    public State accountPageScreen;
    public Text accountNick, accountEmail;
    public Text playedTimes;
    public Text accountPlayTime, accountRegTime;
    public Text ratingText;
    public GameObject loadingCircle;
    public Image avatar;

    [Header("Played maps")]
    public Transform playedMapsContent;

    [Header("Leaderboard")]
    public Transform leaderboardContent;
    public Transform bigLeaderboardLocker, bigLeaderboardContent;
    public Transform youPanel;

    float lastUploadedPlayTime;

    [Header("Action buttons")]
    public GameObject actionLocker;
    public Transform actionContnet;

    #region urls
    public const string url_sendReplay = "http://www.bsserver.tk/Account/AddReplay?nick={0}&json={1}";
    public const string url_getBestReplay = "http://www.bsserver.tk/Account/GetBestReplay?player={0}&trackname={1}&nick={2}";
    public const string url_playTime = "http://www.bsserver.tk/Account/UpdateInGameTime?nick={0}&seconds={1}";
    public const string url_getMapLeaderboardPlace = "http://www.bsserver.tk/Account/GetMapLeaderboardPlace?player={0}&trackname={1}&nick={2}";
    public const string url_leaderboard = "http://www.bsserver.tk/Account/GetMapGlobalLeaderboard?trackname={0}&nick={1}";
    public const string url_login = "http://176.107.160.146/Account/Login?";
    public const string url_signup = "http://176.107.160.146/Account/Register?";
    public const string url_upload = "http://176.107.160.146/Account/Update";
    public const string url_viewAccount = "http://176.107.160.146/Account/ViewAccount?nick=";
    public const string url_getshortleaderboard = "http://176.107.160.146/Account/getshortleaderboard?";
    public const string url_getLeaderboard = "http://176.107.160.146/Account/GetLeaderboard";
    public const string url_setAvatar = "http://176.107.160.146/Account/SetAvatar";
    public const string url_getAvatar = "http://176.107.160.146/Account/GetAvatar?nick=";
    #endregion

    
    private void Update()
    {
        //if (instance != null) return;

        if (LegacyAccount == null) return;

        lastUploadedPlayTime += Time.unscaledDeltaTime;
        LegacyAccount.playTime = LegacyAccount.playTime.Add(TimeSpan.FromSeconds(Time.unscaledDeltaTime));

        if (accountPlayTime != null && viewedLegacyAccount == null)
        {
            string days = LegacyAccount.playTime.ToString("dd") + LocalizationManager.Localize("dd");
            string hours = LegacyAccount.playTime.ToString("hh") + LocalizationManager.Localize("hh");
            string minutes = LegacyAccount.playTime.ToString("mm") + LocalizationManager.Localize("mm");
            string secs = LegacyAccount.playTime.ToString("ss") + LocalizationManager.Localize("ss");
            accountPlayTime.text = LocalizationManager.Localize("PlayTime") + " " + days + " " + hours + " " + minutes + " " + secs;

            if (lastUploadedPlayTime >= 30)
            {
                UpdateSessionTime();
            }
        }
    }


    #region Авторизация и обновление

    public Task Auth(string login, string password)
    {
        return Task.Factory.StartNew(() =>
        {
            WebClient c = new WebClient();
            string authResult = c.DownloadString(url_login + "nick=" + login + "&password=" + password);

            if (authResult.Contains("[ERR]")) Debug.LogError("Auth error: " + authResult);
            else
            {
                LegacyAccount = JsonConvert.DeserializeObject<LegacyAccount>(authResult);
                // !!!!!!!!!!!!
            }
        });
    }

    public async void LogIn(string login, string password, AuthManager manager)
    {
        if (!(Application.internetReachability != NetworkReachability.NotReachable)) return;
        

        await Task.Factory.StartNew(() =>
        {
            WebClient c = new WebClient();
            string authResult = c.DownloadString(url_login + "nick=" + login + "&password=" + password);

            if (authResult.Contains("[ERR]")) Debug.LogError("Auth error: " + authResult);
            else
            {
                LegacyAccount = JsonConvert.DeserializeObject<LegacyAccount>(authResult);
                // !!!!!!!!!!!!
            }

            manager.response = authResult;
        });
    }
    public void AccountUpload()
    {
        if (!(Application.internetReachability != NetworkReachability.NotReachable)) return;


        WebClient c = new WebClient();

        //string json = "";
        string json = JsonConvert.SerializeObject(LegacyAccount);
        // !!!!!!!!!!!!

        CI.HttpClient.HttpClient client = new CI.HttpClient.HttpClient();

        var httpContent = new CI.HttpClient.MultipartFormDataContent();
        httpContent.Add(new CI.HttpClient.StringContent(json), "accountJson");


        client.Post(new System.Uri(url_upload), httpContent, CI.HttpClient.HttpCompletionOption.AllResponseContent, (r) =>
        {
            if (r.ContentLength != 0)
            {
                string response = r.ReadAsString();
            }
            else Debug.Log("[ACCOUNT:U] Content size is zero");
        });
    }


    public async void SignUp(string login, string password, AuthManager manager)
    {
        await Task.Factory.StartNew(() =>
        { 
            WebClient c = new WebClient();
            string result = c.DownloadString(url_signup + "nick=" + login + "&password=" + password);

            if (result.Contains("[ERR]"))
            {
                Debug.LogError("SignUp error: " + result);
                
            }
            else if (result == "Registered")
            {
                
            }

            manager.response = result;
        });
    }

    #endregion



    #region Отображение страницы профиля

    #region Отображение своего профиля

    public async void LoadAccountPage()
    {
        if (!(Application.internetReachability != NetworkReachability.NotReachable)) return;

        loadingCircle.SetActive(true);
        if (LegacyAccount == null) { loadingCircle.SetActive(false); return; }
        loadingCircle.SetActive(false);

        viewedLegacyAccount = null;

        accountPageScreen.ChangeState(accountPageScreen.gameObject);

        accountNick.text = LegacyAccount.nick;
        accountEmail.text = LegacyAccount.email;

        
        playedTimes.text = $"{LocalizationManager.Localize("Played")} {LegacyAccount.playedMaps.Count} {LocalizationManager.Localize(LegacyAccount.playedMaps.Count <= 1 ? "map(one)" : "map(many)")} {LegacyAccount.playedMaps.Sum(c => c.playTimes)} {LocalizationManager.Localize("times")}";

        string days = LegacyAccount.playTime.ToString("dd") + LocalizationManager.Localize("dd");
        string hours = LegacyAccount.playTime.ToString("hh") + LocalizationManager.Localize("hh");
        string minutes = LegacyAccount.playTime.ToString("mm") + LocalizationManager.Localize("mm");
        string secs = LegacyAccount.playTime.ToString("ss") + LocalizationManager.Localize("ss");
        accountPlayTime.text = LocalizationManager.Localize("PlayTime") + " " + days + " " + hours + " " + minutes + " " + secs;

        accountRegTime.text = LocalizationManager.Localize("Registered") + " " + LegacyAccount.regTime.ToString($"dd.MM.yyyy '{LocalizationManager.Localize("in")}' HH:mm");

        if (LegacyAccount.score != 0) ratingText.text = LocalizationManager.Localize("RankingPlace") + " #" + LegacyAccount.ratingPlace + $"\n<size=32>{LegacyAccount.score} {LocalizationManager.Localize("RankingScore")}</size>";
        else ratingText.text = "<size=32>" + LocalizationManager.Localize("RankingPlaceUnknown") + "</size>";

        LoadAvatar(LegacyAccount.nick);

        RefreshPlayedMaps(LegacyAccount);

        RefreshLeaderboard(LegacyAccount);
    }

    #endregion

    #region Отображение чужого профиля

    public async void LoadAnotherAccountPage(string nick)
    {
        if (!(Application.internetReachability != NetworkReachability.NotReachable)) return;

        loadingCircle.gameObject.SetActive(true);
        string response = await LoadAnotherAccount(nick);
        loadingCircle.gameObject.SetActive(false);

        if (response.Contains("[ERR]")) { Debug.LogError("Loading another account err: " + response); return; }

        viewedLegacyAccount = JsonConvert.DeserializeObject<LegacyAccount>(response);
        // !!!!!!!!!!!!


        accountPageScreen.ChangeState(accountPageScreen.gameObject);

        accountNick.text = viewedLegacyAccount.nick;
        accountEmail.text = "";

        playedTimes.text = "Сыграл " + viewedLegacyAccount.playedMaps.Count + " карт(-ы) " + viewedLegacyAccount.playedMaps.Sum(c => c.playTimes) + " раз(-а)";

        string days = viewedLegacyAccount.playTime.ToString("dd");
        string hours = viewedLegacyAccount.playTime.ToString("hh");
        string minutes = viewedLegacyAccount.playTime.ToString("mm");
        accountPlayTime.text = "В игре " + days + "д " + hours + "ч " + minutes + "мин";

        accountRegTime.text = "Зарегался " + viewedLegacyAccount.regTime.ToString("dd.MM.yyyy 'в' HH:mm");

        if (viewedLegacyAccount.score != 0) ratingText.text = "Место в рейтинге #" + viewedLegacyAccount.ratingPlace + $"\n<size=32>{viewedLegacyAccount.score} очков</size>";
        else ratingText.text = "Место в рейтинге ещё не определено";

        LoadAvatar(viewedLegacyAccount.nick);

        RefreshPlayedMaps(viewedLegacyAccount);

        RefreshLeaderboard(viewedLegacyAccount);
    }
    Task<string> LoadAnotherAccount(string nick)
    {
        return Task.Factory.StartNew(() =>
        {
            string url = url_viewAccount + nick;
            WebClient c = new WebClient();
            return c.DownloadString(url);
        });
    }

    #endregion

    async void RefreshLeaderboard(LegacyAccount acc)
    {
        bigLeaderboardContent.parent.GetChild(1).gameObject.SetActive(true);

        Task<string> leaderboardTask = GetLeaderboard(acc.nick);
        await leaderboardTask;

        bigLeaderboardContent.parent.GetChild(1).gameObject.SetActive(false);

        if (leaderboardTask.Result.Contains("[ERR]")) { Debug.LogError("Leaderboard error: " + leaderboardTask.Result); return; }

        for (int i = 0; i < 4; i++)
        {
            string line = leaderboardTask.Result.Split('\n')[i];
            string[] split = line.Split('|');

            leaderboardContent.GetChild(i).GetComponent<Image>().color = split[1] == acc.nick ? new Color32(170, 70, 0, 160) : new Color32(22, 22, 22, 160);

            leaderboardContent.GetChild(i).GetChild(0).GetComponent<Text>().text = split[0] != "0" ? "#" + split[0] : "-";
            leaderboardContent.GetChild(i).GetChild(1).GetComponent<Text>().text = split[1];
            leaderboardContent.GetChild(i).GetChild(2).GetComponent<Text>().text = split[2];

            leaderboardContent.GetChild(i).GetComponent<AccountItemButton>().Setup(this, split[1]);
        }
    }

    Task<string> GetLeaderboard(string nick)
    {
        return Task<string>.Factory.StartNew(() =>
        {
            WebClient c = new WebClient();
            string result = c.DownloadString(url_getshortleaderboard + "nick=" + nick);

            return result;
        });
    }

    void RefreshPlayedMaps(LegacyAccount acc)
    {
        foreach (Transform child in playedMapsContent) if (child.name != "Item") Destroy(child.gameObject);

        GameObject prefab = playedMapsContent.GetChild(0).gameObject;
        prefab.SetActive(true);

        float contentHeight = 0;
        for (int i = 0; i < acc.playedMaps.Count; i++)
        {
            GameObject item = Instantiate(prefab, playedMapsContent);

            Text[] texts = item.GetComponentsInChildren<Text>();
            texts[0].text = acc.playedMaps[i].name;
            texts[1].text = acc.playedMaps[i].author + " <color=#07f>" + LocalizationManager.Localize("by") + " " + acc.playedMaps[i].nick + "</color>";

            AccountTrackRecord accountRecord = acc.records.Find(c => c.author == acc.playedMaps[i].author && c.name == acc.playedMaps[i].name && c.nick == acc.playedMaps[i].nick);
            if(accountRecord != null)
            {
                string record = LocalizationManager.Localize("record") + " " + accountRecord.score;
                texts[2].text = LocalizationManager.Localize("played") + " " + acc.playedMaps[i].playTimes + " " + LocalizationManager.Localize("times") + " <color=#f90>" + record + "</color>";
            }
            

            contentHeight += 82.62f + 2;
        }

        prefab.SetActive(false);
        playedMapsContent.GetComponent<RectTransform>().sizeDelta = new Vector2(playedMapsContent.GetComponent<RectTransform>().sizeDelta.x, contentHeight);
    }

    public void OnMoreBtnClicked()
    {
        accountPageRect.velocity = new Vector2(0, 4000);
    }

    #endregion



    #region Рекорды

    public void UpdateRecord(string author, string name, string nick, AccountTrackRecord newRecord)
    {
        if (LegacyAccount == null) return;

        if(LegacyAccount.records.Exists(c => c.author == author && c.name == name && c.nick == nick))
        {
            AccountTrackRecord record = LegacyAccount.records.Find(c => c.author == author && c.name == name && c.nick == nick);
            if(newRecord.score > record.score)
            {
                record.score = newRecord.score;
                record.missed = newRecord.missed;
                record.accuracy = newRecord.accuracy;
            }
        }
        else
        {
            AccountTrackRecord record = new AccountTrackRecord()
            {
                author = author,
                name = name,
                nick = nick,
                score = newRecord.score,
                missed = newRecord.missed,
                accuracy = newRecord.accuracy
            };
            LegacyAccount.records.Add(record);
        }
    }
    public AccountTrackRecord GetRecord(string author, string name, string nick)
    {
        if (LegacyAccount == null) return null;
        AccountTrackRecord record = LegacyAccount.records.Find(c => c.author == author && c.name == name && c.nick == nick);
        return record;
    }


    #endregion

    #region Сыгранные карты

    public void UpdatePlayedMap(string author, string name, string nick)
    {
        if (LegacyAccount == null) return;

        if (LegacyAccount.playedMaps.Exists(c => c.author == author && c.name == name && c.nick == nick))
        {
            LegacyAccount.playedMaps.Find(c => c.author == author && c.name == name && c.nick == nick).playTimes++;
        }
        else
        {
            LegacyAccount.playedMaps.Add(new AccountMapInfo()
            {
                author = author,
                name = name,
                nick = nick,
                playTimes = 1
            });
        }
    }

    public bool IsPassed(string author, string name, string nick)
    {
        if (LegacyAccount == null) return false;

        return LegacyAccount.playedMaps.Exists(c => c.author == author && c.name == name && c.nick == nick);
    }
    public static bool IsPassed(string author, string name)
    {
        if (LegacyAccount == null) return false;

        return LegacyAccount.playedMaps.Exists(c => c.author == author && c.name == name);
    }

    #endregion

    #region Время сессии и игры

    public void UpdateSessionTime()
    {
        if (LegacyAccount == null) return;

        int secondsToAdd = Mathf.RoundToInt(lastUploadedPlayTime);
        lastUploadedPlayTime = 0;

        SendRequestAsync((string response) => { }, url_playTime, LegacyAccount.nick, secondsToAdd);
    }
    //public float lastUpdatedSession;
    //public void UpdateSessionTime()
    //{
    //    UpdateSessionTimeAsync(Time.realtimeSinceStartup);
    //}
    //async void UpdateSessionTimeAsync(float time)
    //{
    //    await Task.Factory.StartNew(() =>
    //    {
    //        float delta = time - lastUpdatedSession;
    //        lastUpdatedSession = time;

    //        account.playTime = account.playTime.Add(TimeSpan.FromSeconds(delta));


    //        Debug.Log("Now play time is " + account.playTime);
    //    });

    //    AccountUpload();
    //}


    #endregion



    #region Аватарка

    public void OnAvatarClick()
    {
        if (viewedLegacyAccount != null) return;
        NativeGallery.GetImageFromGallery(new NativeGallery.MediaPickCallback(OnAvatarSelected), "Select avatar");
    }

    public void OnAvatarSelected(string filepath)
    {
        Sprite sprite = ProjectManager.LoadSprite(filepath);
        avatar.sprite = sprite;

        SendAvatarFile(filepath);
    }

    public async void SendAvatarFile(string filepath)
    {
        if (LegacyAccount == null) return;
        

        CI.HttpClient.HttpClient client = new CI.HttpClient.HttpClient();

        byte[] buffer = File.ReadAllBytes(filepath);
        var httpContent = new CI.HttpClient.MultipartFormDataContent();

        httpContent.Add(new CI.HttpClient.StringContent(LegacyAccount.nick), "nick");
        httpContent.Add(new CI.HttpClient.StringContent(LegacyAccount.password), "password");

        CI.HttpClient.ByteArrayContent content = new CI.HttpClient.ByteArrayContent(buffer, "multipart/form-data");
        httpContent.Add(content, "file", Path.GetFileName(filepath));

        httpContent.Add(new CI.HttpClient.StringContent(Path.GetExtension(filepath)), "extension");

        await Task.Factory.StartNew(() =>
        {
            client.Post(new System.Uri(url_setAvatar), httpContent, CI.HttpClient.HttpCompletionOption.AllResponseContent, (r) =>
            {
                string response = r.ReadAsString();
                Debug.LogWarning("[SET AVATAR REPONSE] " + response);

                if (response.ToLower().Contains("success"))
                {
                    File.WriteAllBytes(Application.persistentDataPath + "/data/account/avatar.pic", buffer);
                }
            });
        });
    }

    public async void LoadAvatar(string nick)
    {
        avatar.sprite = null;

        Sprite sprite = null;
        byte[] bytes = null;

        string imgPath = Application.persistentDataPath + "/data/account/avatar.pic";
        if(LegacyAccount.nick == nick && File.Exists(imgPath))
        {
            bytes = File.ReadAllBytes(imgPath);
        }
        else
        {
            await Task.Factory.StartNew(() =>
            {
                WebClient c = new WebClient();
                bytes = c.DownloadData(url_getAvatar + nick);
                if(LegacyAccount.nick == nick) File.WriteAllBytes(imgPath, bytes);
            });

        }

        sprite = ProjectManager.LoadSprite(bytes);
        avatar.sprite = sprite;
    }

    #endregion

    #region Рейтинг

    public async void OpenLeaderboard()
    {
        if (LegacyAccount == null) return;

        bigLeaderboardLocker.gameObject.SetActive(true);

        foreach (Transform child in bigLeaderboardContent) if (child.name != "Item") Destroy(child.gameObject);

        List<string> nicks = new List<string>();
        List<string> scores = new List<string>();

        await Task.Factory.StartNew(() =>
        {
            WebClient c = new WebClient();
            string response = c.DownloadString(url_getLeaderboard);

            if (response.Contains("[ERR]")) { Debug.LogError("[OPEN LEADERBOARD] " + response); return; }
            else
            {
                foreach (string line in response.Split('\n'))
                {
                    nicks.Add(line.Split('|')[0]);
                    scores.Add(line.Split('|')[1]);
                }
            }
        });

        bigLeaderboardContent.GetChild(0).gameObject.SetActive(true);

        int myPlace = -1;
        int contentSize = 0;
        for (int i = 0; i < nicks.Count; i++)
        {
            contentSize += 72;
            if (i != 0) contentSize += 2;

            string nick = nicks[i];
            string score = scores[i];

            GameObject item = Instantiate(bigLeaderboardContent.GetChild(0).gameObject, bigLeaderboardContent);
            item.GetComponentsInChildren<Text>()[0].text = "#" + (i + 1);
            item.GetComponentsInChildren<Text>()[1].text = nick;
            item.GetComponentsInChildren<Text>()[2].text = score;
            item.GetComponent<AccountItemButton>().Setup(this, nick);

            if(nick == LegacyAccount.nick)
            {
                item.GetComponent<Image>().color = new Color32(255, 70, 0, 255);
                item.GetComponentsInChildren<Text>()[2].color = Color.white;
                myPlace = i;
            }
        }

        bigLeaderboardContent.GetComponent<RectTransform>().sizeDelta = new Vector2(
            bigLeaderboardContent.GetComponent<RectTransform>().sizeDelta.x,
            contentSize);

        float posRatio = (float)myPlace / (float)nicks.Count;
        float sliderHeight = youPanel.parent.GetComponent<RectTransform>().rect.height;
        float youPanY = sliderHeight * posRatio;
        youPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(40, -youPanY);

        bigLeaderboardContent.GetChild(0).gameObject.SetActive(false);
    }
    public void LeaderboardScrollToMe()
    {
        float y = youPanel.GetComponent<RectTransform>().anchoredPosition.y;
        float ratio = y / youPanel.parent.GetComponent<RectTransform>().rect.height;
        float contentY = bigLeaderboardContent.GetComponent<RectTransform>().rect.height * ratio;
        contentY += 72 * 5;
        bigLeaderboardContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -contentY);
    }

    
    
    
    
    public static void SendReplay(Replay replay, Action<ReplaySendData> callback)
    {
        replay.player = NetCorePayload.CurrentAccount.Nick;
        string json = JsonConvert.SerializeObject(replay);

        NetCore.Subs.Accounts_OnSendReplay += data =>
        {
            Debug.Log("Accounts_OnSendReplay " + data.Coins);
            callback(data);
        };
        
        NetCore.ServerActions.Account.SendReplay(json);
    }
    public static void GetBestReplay(string player, string trackname, string nick, Action<ReplayData> callback)
    {
        NetCore.Subs.Accounts_OnGetBestReplay += callback;
        NetCore.ServerActions.Account.GetBestReplay(player, trackname, nick);
    }

    
    
    
    
    
    public static void GetMapLeaderboardPlace(string player, string trackname, string nick, Action<int> callback)
    {
        string url = string.Format(url_getMapLeaderboardPlace, player, trackname, nick);
        
        SendRequestAsync(args =>
        {
            if (args.Cancelled || args.Error != null) callback(-1);
            else callback(int.Parse(args.Result));
        }, url);
    }

    #endregion

    public static void SendRequestAsync(Action<string> callback, string url, params object[] strings)
    {
        WebClient c = new WebClient();
        c.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
        {
            callback(e.Result);
        };
        string _url = string.Format(url, strings);
        c.DownloadStringAsync(new Uri(_url));
    }
    public static void SendRequestAsync(Action<DownloadStringCompletedEventArgs> callback, string url)
    {
        WebClient c = new WebClient();
        c.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
        {
            callback(e);
        };
        c.DownloadStringAsync(new Uri(url));
    }
}


public class LegacyAccount
{
    public string nick;
    public string email;
    public string password;
    public string role;
    public AccountRole Role
    {
        get
        {
            return role == null || role == "" ? AccountRole.Player : (AccountRole)Enum.Parse(typeof(AccountRole), role);
        }
    }

    public TimeSpan playTime;

    public DateTime regTime;
    public DateTime activeTime;


    public int ratingPlace;
    public float score;

    public List<AccountMapInfo> playedMaps = new List<AccountMapInfo>();
    public List<AccountTrackRecord> records = new List<AccountTrackRecord>();

    public double RP
    {
        get
        {
            return replays.OrderByDescending(c => c.score).GroupBy(c => c.author + "-" + c.name).Select(c => c.First()).Sum(c => c.RP);
        }
    }
    public double TotalRP { get { return replays.Sum(c => c.RP); } }
    public List<Replay> replays = new List<Replay>();
}
public enum AccountRole
{
    Player = 0,
    Developer = 1,
    Moderator = 2
}
public class AccountTrackRecord
{
    public string author, name, nick;

    public float score = 0, accuracy = 0; // Accuracy in 1.0
    public int missed = 0, sliced = 0;
}

public class AccountMapInfo
{
    public string author, name, nick;
    public int playTimes;
}