using GameNet;
using InGame.Models;
using InGame.UI.Overlays;
using Newtonsoft.Json;
using ProjectManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace DatabaseManagement
{
    public static class DatabaseManager
    {
        public static DatabaseContainer container;
        public static OwnMusicUI ownMusicUI;

        public static string url_approved => NetCore.Url_Server + "/Moderation/GetApprovedGroups";
        public static string url_groups => NetCore.Url_Server + "/Database/GetGroupsExtended";
        public static string url_getAllMaps => NetCore.Url_Server + "/Database/GetMapsWithResult?trackname={0}";
        public static string url_getMap => NetCore.Url_Server + "/Database/GetMap?trackname={0}&nick={1}";
        public static string url_doesMapExist => NetCore.Url_Server + "/Database/DoesMapExist?trackname={0}&nick={1}";
        public static string url_hasMapUpdate => NetCore.Url_Server + "/Database/HasUpdateForMap?trackname={0}&nick={1}&utcTicks={2}";
        public static string url_setDifficultyStatistics => NetCore.Url_Server + "/Database/SetDifficultyStatistics?trackname={0}&nick={1}&difficultyId={2}&key={3}";


        public static Action onApprovedLoadedCallback, onAllMusicLoadedCallback, onDownloadedMusicCallback;


        public static void Init(OwnMusicUI ui)
        {
            container = new DatabaseContainer();
            ownMusicUI = ui;
        }
        public static void LoadApproved(Action callback)
        {
            onApprovedLoadedCallback = callback;

            WebClient c = new WebClient();
            c.DownloadStringCompleted += OnLoadedApproved;
            c.DownloadStringAsync(new Uri(url_approved));
        }
        public static void LoadAllGroups(Action callback)
        {
            onAllMusicLoadedCallback = callback;

            WebClient c = new WebClient();
            c.DownloadStringCompleted += OnLoadedAllMusic;
            c.DownloadStringAsync(new Uri(url_groups));
        }
        public static void LoadDownloadedGroups(Action callback)
        {
            onDownloadedMusicCallback = callback;

            List<MapsData> groups = new List<MapsData>();
            string[] groupFolders = Directory.GetDirectories(Application.persistentDataPath + "/maps");
            foreach (string groupFolder in groupFolders)
            {
                int mapsCount = Directory.GetDirectories(groupFolder).Count();
                string trackname = new DirectoryInfo(groupFolder).Name;
                string[] mapFolders = Directory.GetDirectories(groupFolder);

                MapsData info = new MapsData()
                {
                    Author = trackname.Split('-')[0],
                    Name = trackname.Split('-')[1],
                    MappersNicks = mapFolders.Select(c => new DirectoryInfo(c).Name).ToList()
                };

                groups.Add(info);
            }
            container.DownloadedGroups = groups;

            callback();
        }
        public static void LoadOwnGroups(Action callback)
        {
            ownMusicUI.OnOwnBtnClicked(callback);
        }



        public static void GetMapsByTrackAsync(MapsData groupInfo, Action<List<FullMapData>> callback, Action<string> error)
        {
            List<FullMapData> mapInfos = null;

            if (Application.internetReachability != NetworkReachability.NotReachable)
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
                                List<FullMapData> ls = LoadMapDataFromStorage(groupInfo);
                                callback(ls);
                            }
                        }
                        else
                        {
                            mapInfos = JsonConvert.DeserializeObject<List<FullMapData>>(a.Result);
                            callback(mapInfos);
                        }
                    }
                    else if (!a.Cancelled)
                    {
                        error(a.Error.Message);
                    }
                };

                string trackname = groupInfo.Trackname.Replace("&", "%amp%");
                string url = string.Format(url_getAllMaps, trackname);
                Debug.Log("Url: " + url);
                client.DownloadStringAsync(new Uri(url));
            }
            else
            {
                error("No internet connection");
                callback(LoadMapDataFromStorage(groupInfo));
            }
        }

        /// <summary>Get already downloaded maps</summary>
        public static List<FullMapData> GetDownloadedMaps(MapsData group)
        {
            string trackFolder = Application.persistentDataPath + "/maps/" + group.Trackname;
            string[] mapsPathes = Directory.GetDirectories(trackFolder);

            List<FullMapData> mapInfos = new List<FullMapData>();
            for (int i = 0; i < mapsPathes.Length; i++)
            {
                FullMapData info = GetMapInfo(group.Trackname, new DirectoryInfo(mapsPathes[i]).Name);
                mapInfos.Add(info);
            }

            return mapInfos;
        }
        public static FullMapData GetMapInfo(string trackname, string nick)
        {
            try
            {
                WebClient c = new WebClient();
                string response = c.DownloadString(string.Format(url_getMap, trackname, nick));

                return JsonConvert.DeserializeObject<FullMapData>(response);
            }
            catch (Exception err)
            {
                Debug.LogError("GetMapStatistics for " + trackname + "   " + nick + "\n" + err.Message);
                throw err;
            }
        }
       
        public static void SendStatistics(string trackname, string nick, int difficultyId, StatisticsKeyType key)
        {
            string keyStr = key.ToString().ToLower();

            trackname = trackname.Replace("&", "%amp%");

            string url = string.Format(url_setDifficultyStatistics, trackname, nick, difficultyId, keyStr);

            WebClient c = new WebClient();
            c.DownloadStringAsync(new Uri(url));
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




        /// <summary>Used when no internet and needed to play</summary>
        private static List<FullMapData> LoadMapDataFromStorage(MapsData groupInfo)
        {
            var mapInfos = new List<FullMapData>();

            string groupFolder = Application.persistentDataPath + "/maps/" + groupInfo.Trackname;

            foreach (string mapFolder in Directory.GetDirectories(groupFolder))
            {
                FullMapData info = new FullMapData(groupInfo)
                {
                    MapperNick = new DirectoryInfo(mapFolder).Name,
                    Difficulties = new List<DifficultyInfo>()
                };
                mapInfos.Add(info);
            }

            return mapInfos;
        }








        #region OnLoadedData events

        private static void OnLoadedApproved(object sender, DownloadStringCompletedEventArgs e)
        {
            string json = e.Result;
            
            try
            {
                List<MapsData> maps = JsonConvert.DeserializeObject<List<MapsData>>(json);
                container.approvedGroups = maps;
            }
            catch (Exception err)
            {
                Debug.LogError("OnLoadedApproved error " + err);
            }
            
            onApprovedLoadedCallback();
        }
        private static void OnLoadedAllMusic(object sender, DownloadStringCompletedEventArgs e)
        {
            string json = e.Result;

            try
            {
                List<MapsData> maps = JsonConvert.DeserializeObject<List<MapsData>>(json);
                container.allGroups = maps;
            }
            catch (Exception err)
            {
                Debug.LogError("OnLoadedAllMusic error " + err);
            }

            onAllMusicLoadedCallback();
        }

        #endregion
    }


    public enum StatisticsKeyType
    {
        Download, Play, Like, Dislike
    }

    public class DatabaseContainer
    {
        public List<MapsData> approvedGroups = new List<MapsData>();
        public List<MapsData> allGroups = new List<MapsData>();
        public List<MapsData> DownloadedGroups { get; set; } = new List<MapsData>();
        public List<MapsData> OwnGroups { get; set; } = new List<MapsData>();
    }
}