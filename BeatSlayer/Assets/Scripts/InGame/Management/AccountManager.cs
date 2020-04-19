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
using Newtonsoft.Json;
using GooglePlayGames.BasicApi.Multiplayer;

public class AccountManager : MonoBehaviour
{
    public static Account account;
    public Account viewedAccount;

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

    public const string url_sendReplay = "http://www.bsserver.tk/Account/AddReplay?nick={0}&json={1}";
    public const string url_getBestReplay = "http://www.bsserver.tk/Account/GetBestReplay?player={0}&trackname={1}&nick={2}";
    public const string url_playTime = "http://www.bsserver.tk/Account/UpdateInGameTime?nick={0}&seconds={1}";



    private void Awake()
    {
        //if (instance != null)
        //{
        //    Destroy(gameObject);
        //    return;
        //}
        //DontDestroyOnLoad(gameObject);
        //instance = this;
    }
    private void Update()
    {
        //if (instance != null) return;

        if (account == null) return;

        lastUploadedPlayTime += Time.unscaledDeltaTime;
        account.playTime = account.playTime.Add(TimeSpan.FromSeconds(Time.unscaledDeltaTime));

        if (accountPlayTime != null && viewedAccount == null)
        {
            string days = account.playTime.ToString("dd") + LocalizationManager.Localize("dd");
            string hours = account.playTime.ToString("hh") + LocalizationManager.Localize("hh");
            string minutes = account.playTime.ToString("mm") + LocalizationManager.Localize("mm");
            string secs = account.playTime.ToString("ss") + LocalizationManager.Localize("ss");
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
            string authUrl = "http://176.107.160.146/Account/Login?";

            WebClient c = new WebClient();
            string authResult = c.DownloadString(authUrl + "nick=" + login + "&password=" + password);

            if (authResult.Contains("[ERR]")) Debug.LogError("Auth error: " + authResult);
            else
            {
                account = JsonConvert.DeserializeObject<Account>(authResult);
                // !!!!!!!!!!!!
            }
        });
    }

    public async void LogIn(string login, string password, AuthManager manager)
    {
        if (!(Application.internetReachability != NetworkReachability.NotReachable)) return;
        

        await Task.Factory.StartNew(() =>
        {
            string authUrl = "http://176.107.160.146/Account/Login?";

            WebClient c = new WebClient();
            string authResult = c.DownloadString(authUrl + "nick=" + login + "&password=" + password);

            if (authResult.Contains("[ERR]")) Debug.LogError("Auth error: " + authResult);
            else
            {
                account = JsonConvert.DeserializeObject<Account>(authResult);
                // !!!!!!!!!!!!
            }

            manager.response = authResult;
        });
    }
    public void AccountUpload()
    {
        if (!(Application.internetReachability != NetworkReachability.NotReachable)) return;

        string uploadUrl = "http://176.107.160.146/Account/Update";

        WebClient c = new WebClient();

        //string json = "";
        string json = JsonConvert.SerializeObject(account);
        // !!!!!!!!!!!!

        CI.HttpClient.HttpClient client = new CI.HttpClient.HttpClient();

        var httpContent = new CI.HttpClient.MultipartFormDataContent();
        httpContent.Add(new CI.HttpClient.StringContent(json), "accountJson");


        client.Post(new System.Uri(uploadUrl), httpContent, CI.HttpClient.HttpCompletionOption.AllResponseContent, (r) =>
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
            string authUrl = "http://176.107.160.146/Account/Register?";

            WebClient c = new WebClient();
            string result = c.DownloadString(authUrl + "nick=" + login + "&password=" + password);

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
        if (account == null) { loadingCircle.SetActive(false); return; }
        loadingCircle.SetActive(false);

        viewedAccount = null;

        accountPageScreen.ChangeState(accountPageScreen.gameObject);

        accountNick.text = account.nick;
        accountEmail.text = account.email;

        
        playedTimes.text = $"{LocalizationManager.Localize("Played")} {account.playedMaps.Count} {LocalizationManager.Localize(account.playedMaps.Count <= 1 ? "map(one)" : "map(many)")} {account.playedMaps.Sum(c => c.playTimes)} {LocalizationManager.Localize("times")}";

        string days = account.playTime.ToString("dd") + LocalizationManager.Localize("dd");
        string hours = account.playTime.ToString("hh") + LocalizationManager.Localize("hh");
        string minutes = account.playTime.ToString("mm") + LocalizationManager.Localize("mm");
        string secs = account.playTime.ToString("ss") + LocalizationManager.Localize("ss");
        accountPlayTime.text = LocalizationManager.Localize("PlayTime") + " " + days + " " + hours + " " + minutes + " " + secs;

        accountRegTime.text = LocalizationManager.Localize("Registered") + " " + account.regTime.ToString($"dd.MM.yyyy '{LocalizationManager.Localize("in")}' HH:mm");

        if (account.score != 0) ratingText.text = LocalizationManager.Localize("RankingPlace") + " #" + account.ratingPlace + $"\n<size=32>{account.score} {LocalizationManager.Localize("RankingScore")}</size>";
        else ratingText.text = "<size=32>" + LocalizationManager.Localize("RankingPlaceUnknown") + "</size>";

        LoadAvatar(account.nick);

        RefreshPlayedMaps(account);

        RefreshLeaderboard(account);
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

        viewedAccount = JsonConvert.DeserializeObject<Account>(response);
        // !!!!!!!!!!!!


        accountPageScreen.ChangeState(accountPageScreen.gameObject);

        accountNick.text = viewedAccount.nick;
        accountEmail.text = "";

        playedTimes.text = "Сыграл " + viewedAccount.playedMaps.Count + " карт(-ы) " + viewedAccount.playedMaps.Sum(c => c.playTimes) + " раз(-а)";

        string days = viewedAccount.playTime.ToString("dd");
        string hours = viewedAccount.playTime.ToString("hh");
        string minutes = viewedAccount.playTime.ToString("mm");
        accountPlayTime.text = "В игре " + days + "д " + hours + "ч " + minutes + "мин";

        accountRegTime.text = "Зарегался " + viewedAccount.regTime.ToString("dd.MM.yyyy 'в' HH:mm");

        if (viewedAccount.score != 0) ratingText.text = "Место в рейтинге #" + viewedAccount.ratingPlace + $"\n<size=32>{viewedAccount.score} очков</size>";
        else ratingText.text = "Место в рейтинге ещё не определено";

        LoadAvatar(viewedAccount.nick);

        RefreshPlayedMaps(viewedAccount);

        RefreshLeaderboard(viewedAccount);
    }
    Task<string> LoadAnotherAccount(string nick)
    {
        return Task.Factory.StartNew(() =>
        {
            string url = "http://176.107.160.146/Account/ViewAccount?nick=" + nick;
            WebClient c = new WebClient();
            return c.DownloadString(url);
        });
    }

    #endregion

    async void RefreshLeaderboard(Account acc)
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
            string url = "http://176.107.160.146/Account/getshortleaderboard?";

            WebClient c = new WebClient();
            string result = c.DownloadString(url + "nick=" + nick);

            return result;
        });
    }

    void RefreshPlayedMaps(Account acc)
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
        if (account == null) return;

        if(account.records.Exists(c => c.author == author && c.name == name && c.nick == nick))
        {
            AccountTrackRecord record = account.records.Find(c => c.author == author && c.name == name && c.nick == nick);
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
            account.records.Add(record);
        }
    }
    public AccountTrackRecord GetRecord(string author, string name, string nick)
    {
        if (account == null) return null;
        AccountTrackRecord record = account.records.Find(c => c.author == author && c.name == name && c.nick == nick);
        return record;
    }


    #endregion

    #region Сыгранные карты

    public void UpdatePlayedMap(string author, string name, string nick)
    {
        if (account == null) return;

        if (account.playedMaps.Exists(c => c.author == author && c.name == name && c.nick == nick))
        {
            account.playedMaps.Find(c => c.author == author && c.name == name && c.nick == nick).playTimes++;
        }
        else
        {
            account.playedMaps.Add(new AccountMapInfo()
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
        if (account == null) return false;

        return account.playedMaps.Exists(c => c.author == author && c.name == name && c.nick == nick);
    }
    public bool IsPassed(string author, string name)
    {
        if (account == null) return false;

        return account.playedMaps.Exists(c => c.author == author && c.name == name);
    }

    #endregion

    #region Время сессии и игры

    public void UpdateSessionTime()
    {
        if (account == null) return;

        Debug.Log("UpdateSessionTime add " + lastUploadedPlayTime + " seconds");

        int secondsToAdd = Mathf.RoundToInt(lastUploadedPlayTime);
        lastUploadedPlayTime = 0;

        SendRequestAsync((string response) => { }, url_playTime, account.nick, secondsToAdd);
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
        if (viewedAccount != null) return;
        NativeGallery.GetImageFromGallery(new NativeGallery.MediaPickCallback(OnAvatarSelected), "Select avatar");
    }

    public void OnAvatarSelected(string filepath)
    {
        Debug.Log("[SPRITE] " + filepath);

        Sprite sprite = TheGreat.LoadSprite(filepath);
        avatar.sprite = sprite;

        SendAvatarFile(filepath);
    }

    public async void SendAvatarFile(string filepath)
    {
        if (account == null) return;


        string url_publish = "http://176.107.160.146/Account/SetAvatar";

        CI.HttpClient.HttpClient client = new CI.HttpClient.HttpClient();

        byte[] buffer = File.ReadAllBytes(filepath);
        var httpContent = new CI.HttpClient.MultipartFormDataContent();

        httpContent.Add(new CI.HttpClient.StringContent(account.nick), "nick");
        httpContent.Add(new CI.HttpClient.StringContent(account.password), "password");

        CI.HttpClient.ByteArrayContent content = new CI.HttpClient.ByteArrayContent(buffer, "multipart/form-data");
        httpContent.Add(content, "file", Path.GetFileName(filepath));

        httpContent.Add(new CI.HttpClient.StringContent(Path.GetExtension(filepath)), "extension");

        await Task.Factory.StartNew(() =>
        {
            client.Post(new System.Uri(url_publish), httpContent, CI.HttpClient.HttpCompletionOption.AllResponseContent, (r) =>
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
        if(account.nick == nick && File.Exists(imgPath))
        {
            bytes = File.ReadAllBytes(imgPath);
        }
        else
        {
            await Task.Factory.StartNew(() =>
            {
                WebClient c = new WebClient();
                bytes = c.DownloadData("http://176.107.160.146/Account/GetAvatar?nick=" + nick);
                if(account.nick == nick) File.WriteAllBytes(imgPath, bytes);
            });

        }

        sprite = TheGreat.LoadSprite(bytes);
        avatar.sprite = sprite;
    }

    #endregion

    #region Рейтинг

    public async void OpenLeaderboard()
    {
        if (account == null) return;

        bigLeaderboardLocker.gameObject.SetActive(true);

        foreach (Transform child in bigLeaderboardContent) if (child.name != "Item") Destroy(child.gameObject);

        List<string> nicks = new List<string>();
        List<string> scores = new List<string>();

        await Task.Factory.StartNew(() =>
        {
            WebClient c = new WebClient();
            string response = c.DownloadString("http://176.107.160.146/Account/GetLeaderboard");

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

            if(nick == account.nick)
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

    public static void SendReplay(Replay replay, Action<double> callback)
    {
        replay.player = account.nick;

        WebClient c = new WebClient();
        //c.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
        //{
        //    if(e.Error != null) Debug.LogError("SendReplay error: " + e.Error);
        //    Debug.Log("SendReplay response is " + e.Result);
        //    //if (!e.Cancelled && e.Error != null) Debug.LogError("SendReplay\n" + e.Error);

        //    callback(double.Parse(e.Result, System.Globalization.CultureInfo.InvariantCulture));
        //};

        string url = string.Format(url_sendReplay, account.nick, JsonConvert.SerializeObject(replay));
        //c.DownloadStringAsync(new Uri(url));
        string RPstr = c.DownloadString(url);
        Debug.Log(url + "\n" + RPstr);
        callback(double.Parse(RPstr, System.Globalization.CultureInfo.InvariantCulture));
    }
    public static void GetBestReplay(string player, string trackname, string nick, Action<Replay> callback)
    {
        WebClient c = new WebClient();
        string url = string.Format(url_getBestReplay, player, trackname, nick);

        c.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
        {
            if (!e.Cancelled && e.Error != null) { Debug.LogError("GetBestReplay\n" + e.Error); callback(null); return; }
            else Debug.Log("GetBestReplay response is " + e.Result);

            Replay replay = JsonConvert.DeserializeObject<Replay>(e.Result);
            callback(replay);
        };
    }


    #endregion

    public void SendRequestAsync(Action<string> callback, string url, params object[] strings)
    {
        WebClient c = new WebClient();
        c.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
        {
            callback(e.Result);
        };
        string _url = string.Format(url, strings);
        Debug.Log("Send async request : " + _url);
        c.DownloadStringAsync(new Uri(_url));
    }
}



public class Account
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