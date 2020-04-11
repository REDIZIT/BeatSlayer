using Newtonsoft.Json;
using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

public class DatabaseScript : MonoBehaviour
{
    public TrackDatabase db;
    public TrackDatabase cachedDb;

    public TracksDatabase data;

    public bool useGoogleDrive = false;

    public string[] googleDriveInfo;

    public GameObject googleDriveUsedMsg;
    bool isFirstCall = true;

    //string databaseUrl = "https://bsserver.tk/Home/Database";
    string databaseUrl = "http://176.107.160.146/Home/Database";

    //string db_groupsUrl = "https://bsserver.tk/Database/GetGroups";
    string db_groupsUrl = "http://176.107.160.146/Database/GetGroups";
    //string db_mapsUrl = "https://bsserver.tk/Database/GetMaps";
    string db_mapsUrl = "http://176.107.160.146/Database/GetMaps";

    //string db_mapStatUrl = "http://www.bsserver.tk/Database/GetShortStatistics?trackname={0}&nick={1}";
    string db_mapUrl = "http://www.bsserver.tk/Database/GetMap?trackname={0}&nick={1}";


    public Sprite defaultTrackSprite;

    // Get all tracks from db (maps groups)
    public IEnumerator LoadDatabaseAsync(bool refresh = false)
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
        GetComponent<MenuScript_v2>().musicLoadingText.color = Color.white;
        GetComponent<MenuScript_v2>().musicLoadingText.text = "Music is loading";

        WebClient client = new WebClient();

        bool isDone = false;
        string response = "";

        client.DownloadStringAsync(new Uri(db_groupsUrl));
        client.DownloadStringCompleted += (object sender, DownloadStringCompletedEventArgs e) =>
        {
            isDone = true;
            response = e.Result;
        };

        int commaInd = 0;
        while (!isDone)
        {
            GetComponent<MenuScript_v2>().musicLoadingText.text = "Music is loading" + (commaInd == 0 ? "." : commaInd == 1 ? ".." : commaInd == 2 ? "..." : "");
            commaInd++;
            if (commaInd >= 3)
            {
                commaInd = 0;
            }
            yield return new WaitForSeconds(0.3f);
        }

        // Handle the response
        if (data == null) data = new TracksDatabase();
        if (data.tracks == null) data.tracks = new List<TrackGroupClass>();
        else data.tracks.Clear();

        List<MapInfo> mapInfos = (List<MapInfo>)(JsonConvert.DeserializeObject(response, typeof(List<MapInfo>)));

        foreach (MapInfo info in mapInfos)
        {
            bool isNew = (DateTime.Now - info.publishTime) <= new TimeSpan(3, 0, 0, 0);

            TrackGroupClass cls = new TrackGroupClass()
            {
                author = info.author,
                name = info.name,
                mapsCount = info.group.mapsCount,
                downloads = info.downloads,
                plays = info.playCount,
                likes = info.likes,
                dislikes = info.dislikes,
                novelty = isNew
            };
            data.tracks.Add(cls);
        }

        TheGreat.SyncRecords(this);
        GetComponent<ListController>().RefreshAuthorList();
    }
    
    public TrackClass[] GetMapsByTrack(TrackGroupClass groupCls)
    {
        WebClient client = new WebClient();
        string url = db_mapsUrl + "?trackname=" + groupCls.author.Replace("&", "%amp%") + "-" + groupCls.name.Replace("&", "%amp%");
        string response = client.DownloadString(url);

        List<MapInfo> mapInfos = (List<MapInfo>)(JsonConvert.DeserializeObject(response, typeof(List<MapInfo>)));

        TrackClass[] arr = new TrackClass[mapInfos.Count];
        for (int i = 0; i < mapInfos.Count; i++)
        {
            TrackClass cls = new TrackClass()
            {
                nick = mapInfos[i].nick,
                downloads = mapInfos[i].downloads,
                plays = mapInfos[i].playCount,
                likes = mapInfos[i].likes,
                dislikes = mapInfos[i].dislikes,
                group = groupCls,
                hasUpdate = HasUpdateForMap(mapInfos[i]),
                difficulty = mapInfos[i].difficultyStars,
                difficultyName = mapInfos[i].difficultyName
            };
            cls.cover = GetComponent<DownloadHelper>().DownloadSprite(cls);
            arr[i] = cls;
        }

        return arr;
    }

    public Task<List<TrackGroupClass>> GetDownloadedMusic()
    {
        string mapsFolder = Application.persistentDataPath + "/maps";
        return Task.Factory.StartNew<List<TrackGroupClass>>(() =>
        {
            List<TrackGroupClass> ls = new List<TrackGroupClass>();

            string[] groups = Directory.GetDirectories(mapsFolder);
            for (int i = 0; i < groups.Length; i++)
            {
                string trackname = new DirectoryInfo(groups[i]).Name;

                TrackGroupClass groupcls = new TrackGroupClass()
                {
                    author = trackname.Split('-')[0],
                    name = trackname.Split('-')[1],
                    mapsCount = Directory.GetDirectories(mapsFolder + "/" + trackname).Length
                };

                ls.Add(groupcls);
            }

            return ls;
        });
    }
    
    public TrackClass[] GetDownloadedMaps(TrackGroupClass group)
    {
        string trackFolder = Application.persistentDataPath + "/maps/" + group.author + "-" + group.name;
        string[] mapsPathes = Directory.GetDirectories(trackFolder);

        TrackClass[] arr = new TrackClass[mapsPathes.Length];
        for (int i = 0; i < mapsPathes.Length; i++)
        {
            arr[i] = new TrackClass() { group = group };
            string coverPath = TheGreat.GetCoverPath(mapsPathes[i], group.author + "-" + group.name);
            arr[i].cover = coverPath == "" ? defaultTrackSprite : TheGreat.LoadSprite(coverPath);
            arr[i].nick = new DirectoryInfo(mapsPathes[i]).Name;

            //int[] mapStat = GetMapStatistics(group.author + "-" + group.name, arr[i].nick);
            MapInfo mapStat = GetMapInfo(group.author + "-" + group.name, arr[i].nick);
            arr[i].downloads = mapStat.downloads;
            arr[i].plays = mapStat.playCount;
            arr[i].likes = mapStat.likes;
            arr[i].dislikes = mapStat.dislikes;
            arr[i].difficultyName = mapStat.difficultyName;
            arr[i].difficulty = mapStat.difficultyStars;


            arr[i].hasUpdate = HasUpdateForMap(group.author + "-" + group.name, arr[i].nick);
            //db.list.Where(c => c.author == group.author && c.name == group.name && c.)
        }

        return arr;
    }

    public TrackClass[] GetCustomMaps(TrackGroupClass group)
    {
        TrackClass[] arr = new TrackClass[1];
        arr[0] = new TrackClass()
        {
            nick = "[LOCAL STORAGE]",
            cover = defaultTrackSprite,
            group = group
        };
        return arr;
    }



    public MapInfo GetMapInfo(string trackname, string nick)
    {
        try
        {
            WebClient c = new WebClient();
            string response = c.DownloadString(string.Format(db_mapUrl, trackname, nick));

            return (MapInfo)JsonConvert.DeserializeObject(response, typeof(MapInfo));
        }
        catch (Exception err)
        {
            Debug.LogError("GetMapStatistics for " + trackname + "   " + nick + "\n" + err.Message);
            return new MapInfo();
        }
    }




    public bool HasUpdateForMap(string trackname, string nick)
    {
        trackname = trackname.Replace("&", "%amp%");
        nick = nick.Replace("&", "%amp%");

        string path = Application.persistentDataPath + "/maps/" + trackname + "/" + nick + "/" + trackname + ".bsu";
        long utcTicks = new FileInfo(path).LastWriteTimeUtc.Ticks;

        string url = "http://176.107.160.146/Database/HasUpdateForMap?trackname=" + trackname + "&nick=" + nick + "&utcTicks=" + utcTicks;
        string response = new WebClient().DownloadString(url);

        Debug.Log("Has Updates " + trackname + " with nick " + nick + "? " + response + "\n" + url);

        return bool.Parse(response);
    }
    public bool HasUpdateForMap(MapInfo info)
    {
        string trackname = info.author + "-" + info.name;
        string nick = info.nick;

        string path = Application.persistentDataPath + "/maps/" + trackname + "/" + nick + "/" + trackname + ".bsu";
        if (!File.Exists(path)) return false;
        long utcTicks = new FileInfo(path).LastWriteTimeUtc.Ticks;

        return info.publishTime.Ticks > utcTicks;
    }
}

[Serializable]
public class TrackDatabase
{
    public List<TrackItemClass> list;
}

[Serializable]
public class TrackItemClass
{
    public string trackname;
    public string author
    {
        get
        {
            if (trackname.Contains("-")) return trackname.Split('-')[0];
            else return "Unknown";
        }
    }
    public string name
    {
        get
        {
            if (trackname.Contains("-")) return trackname.Split('-')[1];
            else return trackname;
        }
    }
    public int mins, secs;
    public int downloads, plays, likes, dislikes;
    public enum Rated { NotSet, Liked, Disliked };
    public Rated rated;
    public bool isNew;
    public string source;
    public string mapCreator;
    

    public TrackItemClass(string trackname, int mins, int secs, int downloads, int plays, int likes, int dislikes, Rated rated, bool isNew, string source, string mapCreator)
    {
        this.trackname = trackname;
        this.mins = mins;
        this.secs = secs;
        this.downloads = downloads;
        this.plays = plays;
        this.likes = likes;
        this.dislikes = dislikes;
        this.rated = rated;
        this.isNew = isNew;
        this.source = source;
        this.mapCreator = mapCreator;
    }
}

[Serializable]
public class UserTrackList
{
    public List<UserTrackClass> list;
}
[Serializable]
public class UserTrackClass
{
    public string path;
    public string trackname;
    public string author
    {
        get
        {
            if (trackname.Contains("-")) return trackname.Split('-')[0];
            else return "Unknown";
        }
    }
    public string name
    {
        get
        {
            if (trackname.Contains("-")) return trackname.Split('-')[1];
            else return trackname;
        }
    }
    public bool isNew;

    public UserTrackClass(string path, string trackname)
    {
        this.path = path;
        this.trackname = trackname;
    }
}



public class TrackInfoPacket
{
    public string fullname;
    public int downloaded, played, likes, dislikes;
}



// Выбрал такое название потому что он великий и он первый в выдаче по поиску "The"
public static class TheGreat
{
    public static void SendStatistics(string trackname, string nick, string key)
    {
        WebClient client = new WebClient();

        string url = "http://176.107.160.146/Database/SetStatistics?trackname=" + UrlEncode(trackname) + "&nick=" + UrlEncode(nick) + "&key=" + UrlEncode(key) + "&value=1";

        client.DownloadStringAsync(new Uri(url));
    }

    public static Texture2D LoadTexure(byte[] bytes)
    {
        Texture2D tex = new Texture2D(0, 0);
        tex.LoadImage(bytes);
        return tex;
    }
    public static Texture2D LoadTexure(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(0, 0);
        tex.LoadImage(bytes);
        return tex;
    }
    public static Sprite LoadSprite(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(0, 0);
        tex.LoadImage(bytes);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }
    public static Sprite LoadSprite(byte[] bytes)
    {
        Texture2D tex = new Texture2D(0, 0);
        tex.LoadImage(bytes);
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    public static string GetCoverPath(string mapPath, string trackname)
    {
        string jpgPath = mapPath + "/" + trackname + ".jpg";
        string pngPath = mapPath + "/" + trackname + ".png";
        if (File.Exists(jpgPath)) return jpgPath;
        else if (File.Exists(pngPath)) return pngPath;

        return "";
    }

    public static string UrlEncode(string url)
    {
        return url.Replace("&", "%amp%");
    }
    public static string UrlDecode(string url)
    {
        return url.Replace("%amp%", "$");
    }


    public static TrackRecordGroup GetRecords()
    {
        XmlSerializer xml = new XmlSerializer(typeof(TrackRecordGroup));
        var stream = File.OpenRead(Application.persistentDataPath + "/rsave.bsf");
        var group = xml.Deserialize(stream);
        stream.Close();
        return (TrackRecordGroup)group;
    }

    public static void SaveRecords(TrackRecordGroup group)
    {
        XmlSerializer xml = new XmlSerializer(typeof(TrackRecordGroup));
        var stream = File.Create(Application.persistentDataPath + "/rsave.bsf");
        xml.Serialize(stream, group);
        stream.Close();
    }

    public static void UpdateRecord(TrackRecordGroup group, string author, string name, string nick, string score, string multiplier)
    {
        TrackRecord existsRecord = group.ls.Find(c => c.author == author && c.name == name && c.nick == nick);
        if (existsRecord != null)
        {
            if(int.Parse(score) > int.Parse(existsRecord.score))
            {
                existsRecord.score = SSytem.instance.Encrypt(score);
            }
        }
        else
        {
            TrackRecord record = new TrackRecord()
            {
                author = author,
                name = name,
                nick = nick,
                score = SSytem.instance.Encrypt(score),
                multiplier = SSytem.instance.Encrypt(multiplier)
            };
            group.ls.Add(record);
        }

        SaveRecords(group);
    }

    public static TrackRecord GetRecord(TrackRecordGroup group, string author, string name, string nick)
    {
        TrackRecord record = group.ls.Find(c => c.author == author && c.name == name && c.nick == nick);
        if (record == null) return null;
        else
        {
            record.score = SSytem.instance.Decrypt(record.score);
            return record;
        }
    }

    public static void SyncRecords(DatabaseScript db)
    {
        if (!Social.localUser.authenticated) return;

        TrackRecordGroup group = GetRecords();
        float sum = 0;
        IEnumerable<TrackRecord> records = group.ls.Where(c => c.nick != "[LOCAL STORAGE]");
        foreach (var record in records)
        {
            sum += float.Parse(SSytem.instance.Decrypt(record.score));
        }

        Social.ReportScore(Mathf.RoundToInt(sum), GPGamesManager.leaderboard, (bool result) => { Debug.Log("ChangedBoard result: " + result); });
    }
}