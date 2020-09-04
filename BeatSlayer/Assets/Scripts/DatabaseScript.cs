using GameNet;
using Newtonsoft.Json;
using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class DatabaseScript : MonoBehaviour
{
    public TrackDatabase db;
    public TrackDatabase cachedDb;

    public TracksDatabase data;
    
    #region urls
    public static string url_getGroups => NetCore.Url_Server + "/Database/GetGroupsExtended";
    public static string url_getMapsWithResult => NetCore.Url_Server + "/Database/GetMapsWithResult?trackname={0}";
    public static string url_getMap => NetCore.Url_Server + "/Database/GetMap?trackname={0}&nick={1}";
    public static string url_doesMapExist => NetCore.Url_Server + "/Database/DoesMapExist?trackname={0}&nick={1}";
    public static string url_hasMapUpdate => NetCore.Url_Server + "/Database/HasUpdateForMap?trackname={0}&nick={1}&utcTicks={2}";
    public static string url_setDifficultyStatistics => NetCore.Url_Server + "/Database/SetDifficultyStatistics?trackname={0}&nick={1}&difficultyId={2}&key={3}";
    public static string url_getPrelistenFile => NetCore.Url_Server + "/Maps/GetPrelistenFile?trackname={0}";
    public static string url_hasPrelistenFile => NetCore.Url_Server + "/Maps/HasPrelistenFile?trackname={0}";
    #endregion
    

    #region Groups and maps (Server loading)
    // Get all tracks from db (maps groups)
    public IEnumerator LoadDatabaseAsync(bool refresh = false)
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (message, cert, chain, sslPolicyErrors) => true;
        GetComponent<MenuScript_v2>().musicLoadingText.color = Color.white;
        GetComponent<MenuScript_v2>().musicLoadingText.text = "Music is loading";

        WebClient client = new WebClient();

        bool isDone = false;
        string response = "";

        client.DownloadStringAsync(new Uri(url_getGroups));
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

        //TheGreat.SyncRecords(this);
        //GetComponent<ListController>().RefreshAuthorList();
    }
    
    
    
    public List<MapInfo> GetMapsByTrack(GroupInfoExtended groupInfo)
    { 
        List<MapInfo> mapInfos;

        if(Application.internetReachability != NetworkReachability.NotReachable)
        {
            WebClient client = new WebClient();
            
            string trackname = groupInfo.author.Replace("&", "%amp%") + "-" + groupInfo.name.Replace("&", "%amp%");
            string url = string.Format(url_getMapsWithResult, trackname);
            string response = client.DownloadString(url);

            mapInfos = (List<MapInfo>)(JsonConvert.DeserializeObject(response, typeof(List<MapInfo>)));
        }
        else
        {
            mapInfos = new List<MapInfo>();
            string trackname = groupInfo.author + "-" + groupInfo.name;
            string groupFolder = Application.persistentDataPath + "/maps/" + trackname;
            foreach(string mapFolder in Directory.GetDirectories(groupFolder))
            {
                MapInfo info = new MapInfo()
                {
                    group = groupInfo,
                    nick = new DirectoryInfo(mapFolder).Name
                };
                mapInfos.Add(info);
            }
        }
        

        return mapInfos;
    }
    public void GetMapsByTrackAsync(GroupInfoExtended groupInfo, Action<List<MapInfo>> callback, Action<string> error)
    {
        List<MapInfo> mapInfos = null;

        if(Application.internetReachability != NetworkReachability.NotReachable)
        {
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (s, a) =>
            {
                if (!a.Cancelled && a.Error == null)
                {
                    if (a.Result.StartsWith("[ERR]"))
                    {
                        string err = a.Result.Replace("[ERR] ", "");
                        error(err);
                        if (err == "Group has been deleted")
                        {
                            List<MapInfo> ls = LoadMapInfosFromLocal(groupInfo);
                            ls.ForEach((info) =>
                            {
                                info.isMapDeleted = true;
                            });
                            callback(ls);   
                        }
                    }
                    else
                    {
                        mapInfos = (List<MapInfo>) (JsonConvert.DeserializeObject(a.Result, typeof(List<MapInfo>)));
                        callback(mapInfos);   
                    }
                }
                else if(!a.Cancelled)
                {
                    error(a.Error.Message);
                }
            };

            string trackname = groupInfo.author.Replace("&", "%amp%") + "-" + groupInfo.name.Replace("&", "%amp%");
            string url = string.Format(url_getMapsWithResult, trackname);
            client.DownloadStringAsync(new Uri(url));
        }
        else
        {
            error("No internet connection");
            callback(LoadMapInfosFromLocal(groupInfo));
        }
    }

    
    
    List<MapInfo> LoadMapInfosFromLocal(GroupInfo groupInfo)
    {
        List<MapInfo> mapInfos = new List<MapInfo>();
        string trackname = groupInfo.author + "-" + groupInfo.name;
        string groupFolder = Application.persistentDataPath + "/maps/" + trackname;
        foreach(string mapFolder in Directory.GetDirectories(groupFolder))
        {
            MapInfo info = new MapInfo()
            {
                group = groupInfo,
                nick = new DirectoryInfo(mapFolder).Name,
                difficulties = new List<DifficultyInfo>()
            };
            mapInfos.Add(info);
        }

        return mapInfos;
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
    public List<MapInfo> GetDownloadedMaps(GroupInfo group)
    {
        string trackFolder = Application.persistentDataPath + "/maps/" + group.author + "-" + group.name;
        string[] mapsPathes = Directory.GetDirectories(trackFolder);

        List<MapInfo> mapInfos = new List<MapInfo>();
        for (int i = 0; i < mapsPathes.Length; i++)
        {
            string nick = new DirectoryInfo(mapsPathes[i]).Name;
            MapInfo info = GetMapInfo(group.author + "-" + group.name, new DirectoryInfo(mapsPathes[i]).Name);
            mapInfos.Add(info);

            //arr[i].hasUpdate = HasUpdateForMap(group.author + "-" + group.name, arr[i].nick);
        }

        return mapInfos;
    }
    public List<MapInfo> GetCustomMaps(GroupInfoExtended group)
    {
        List<MapInfo> ls = new List<MapInfo>();
        ls.Add(new MapInfo(group)
        {
            nick = "[LOCAL STORAGE]",
            filepath = group.filepath,
            difficultyName = "Standard",
            difficultyStars = 4,
            difficulties = new List<DifficultyInfo>()
            {
                new DifficultyInfo()
                {
                    name = "Standard",
                    stars = 4
                }
            }
        });
        return ls;
    }


    public static bool DoesMapExist(string trackname, string nick)
    {
        string url = string.Format(url_doesMapExist, trackname.Replace("&", "%26"), nick);
        WebClient c = new WebClient();
        string response = c.DownloadString(url);
        bool b = bool.Parse(response);

        Debug.Log("Does map " + trackname + " by " + nick + " exists? " + b);
        return b;
    }
    

    #endregion

    public MapInfo GetMapInfo(string trackname, string nick)
    {
        try
        {
            WebClient c = new WebClient();
            string response = c.DownloadString(string.Format(url_getMap, trackname, nick));

            return (MapInfo)JsonConvert.DeserializeObject(response, typeof(MapInfo));
        }
        catch (Exception err)
        {
            Debug.LogError("GetMapStatistics for " + trackname + "   " + nick + "\n" + err.Message);
            return new MapInfo();
        }
    }
    

    public static bool HasUpdateForMap(string trackname, string nick)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) return false;
    
        string path = Application.persistentDataPath + "/maps/" + trackname + "/" + nick + "/" + trackname + ".bsu";
        long utcTicks = new FileInfo(path).LastWriteTimeUtc.Ticks;

        trackname = trackname.Replace("&", "%amp%");
        nick = nick.Replace("&", "%amp%");

        string url = string.Format(url_hasMapUpdate, trackname, nick, utcTicks);
        string response = new WebClient().DownloadString(url);

        return bool.Parse(response);
    }



    #region Prelisten

    public void HasPrelistenFile(string trackname, Action<bool> callback)
    {
        WebClient c = new WebClient();

        c.DownloadStringCompleted += (sender, args) =>
        {
            callback(bool.Parse(args.Result));
        };

        string url = string.Format(url_hasPrelistenFile, trackname);
        c.DownloadStringAsync(new Uri(url));
    }
    public void LoadPrelistenFile(string trackname, Action<AudioClip> callback)
    {
        StartCoroutine(ILoadPrelistenFile(trackname, callback));
    }
    IEnumerator ILoadPrelistenFile(string trackname, Action<AudioClip> callback)
    {
        string url = string.Format(url_getPrelistenFile, trackname);
        using (WWW www = new WWW(url))
        {
            yield return www;
            callback(www.GetAudioClip(false, false, AudioType.MPEG));
        }
        /*using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.Send();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
                callback(null);
            }
            else
            {
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                callback(myClip);
            }
        }        */
        /*
        
        WWW web = new WWW(url);
        yield return web;

        if (web.bytes.Length != 0)
        {
            AudioClip clip = web.GetAudioClip(false, false, AudioType);
            
        }
        else
        {
           
        }*/
    }

    #endregion    
    
    
    


    public enum StatisticsKeyType
    {
        Download, Play, Like, Dislike
    }
    public static void SendStatistics(string trackname, string nick, int difficultyId, StatisticsKeyType key)
    {
        string keyStr = key.ToString().ToLower();

        trackname = trackname.Replace("&", "%amp%");

        string url = string.Format(url_setDifficultyStatistics, trackname, nick, difficultyId, keyStr);
        
        WebClient c = new WebClient();
        c.DownloadStringAsync(new Uri(url));
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



// Выбрал такое название потому что он великий ( этого в нем немного :D ) и он первый в выдаче по поиску "The"
public static class TheGreat
{
    public static string UrlEncode(string url)
    {
        return url.Replace("&", "%amp%");
    }
    public static string UrlDecode(string url)
    {
        return url.Replace("%amp%", "$");
    }
}