using GameNet;
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

        public static string Apibase => NetCore.Url_Server;
        public static string url_approved = Apibase + "/Moderation/GetApprovedGroups";
        public static string url_groups = Apibase + "/Database/GetGroupsExtended";
        public static string url_getAllMaps = Apibase + "/Database/GetMapsWithResult?trackname={0}";
        public static string url_getMap = Apibase + "/Database/GetMap?trackname={0}&nick={1}";
        public static string url_doesMapExist = Apibase + "/Database/DoesMapExist?trackname={0}&nick={1}";
        public static string url_hasMapUpdate = Apibase + "/Database/HasUpdateForMap?trackname={0}&nick={1}&utcTicks={2}";
        public static string url_setDifficultyStatistics = Apibase + "/Database/SetDifficultyStatistics?trackname={0}&nick={1}&difficultyId={2}&key={3}";


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

            List<GroupInfoExtended> groups = new List<GroupInfoExtended>();
            string[] groupFolders = Directory.GetDirectories(Application.persistentDataPath + "/maps");
            foreach (string groupFolder in groupFolders)
            {
                int mapsCount = Directory.GetDirectories(groupFolder).Count();
                string trackname = new DirectoryInfo(groupFolder).Name;

                GroupInfoExtended info = new GroupInfoExtended()
                {
                    author = trackname.Split('-')[0],
                    name = trackname.Split('-')[1],
                    mapsCount = mapsCount
                };

                string[] mapFolders = Directory.GetDirectories(groupFolder);
                info.nicks = new List<string>();
                info.nicks.AddRange(mapFolders.Select(c => new DirectoryInfo(c).Name));

                groups.Add(info);
            }
            container.DownloadedGroups = groups;

            callback();
        }
        public static void LoadOwnGroups(Action callback)
        {
            ownMusicUI.OnOwnBtnClicked(callback);
        }



        public static void GetMapsByTrackAsync(GroupInfoExtended groupInfo, Action<List<ProjectMapInfo>> callback, Action<string> error)
        {
            Debug.Log("GetMapsByTrackAsync");
            List<ProjectMapInfo> mapInfos = null;

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
                                List<ProjectMapInfo> ls = LoadMapDataFromStorage(groupInfo);
                                callback(ls);
                            }
                        }
                        else
                        {
                            mapInfos = (List<ProjectMapInfo>)(JsonConvert.DeserializeObject(a.Result, typeof(List<ProjectMapInfo>)));
                            callback(mapInfos);
                        }
                    }
                    else if (!a.Cancelled)
                    {
                        error(a.Error.Message);
                    }
                };

                string trackname = groupInfo.author.Replace("&", "%amp%") + "-" + groupInfo.name.Replace("&", "%amp%");
                string url = string.Format(url_getAllMaps, trackname);
                client.DownloadStringAsync(new Uri(url));
            }
            else
            {
                error("No internet connection");
                callback(LoadMapDataFromStorage(groupInfo));
            }
        }

        /// <summary>Get already downloaded maps</summary>
        public static List<ProjectMapInfo> GetDownloadedMaps(GroupInfo group)
        {
            string trackFolder = Application.persistentDataPath + "/maps/" + group.author + "-" + group.name;
            string[] mapsPathes = Directory.GetDirectories(trackFolder);

            List<ProjectMapInfo> mapInfos = new List<ProjectMapInfo>();
            for (int i = 0; i < mapsPathes.Length; i++)
            {
                ProjectMapInfo info = GetMapInfo(group.author + "-" + group.name, new DirectoryInfo(mapsPathes[i]).Name);
                mapInfos.Add(info);
            }

            return mapInfos;
        }
        public static ProjectMapInfo GetMapInfo(string trackname, string nick)
        {
            try
            {
                WebClient c = new WebClient();
                string response = c.DownloadString(string.Format(url_getMap, trackname, nick));

                return (ProjectMapInfo)JsonConvert.DeserializeObject(response, typeof(ProjectMapInfo));
            }
            catch (Exception err)
            {
                Debug.LogError("GetMapStatistics for " + trackname + "   " + nick + "\n" + err.Message);
                return new ProjectMapInfo();
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
        private static List<ProjectMapInfo> LoadMapDataFromStorage(GroupInfo groupInfo)
        {
            List<ProjectMapInfo> mapInfos = new List<ProjectMapInfo>();
            string trackname = groupInfo.author + "-" + groupInfo.name;
            string groupFolder = Application.persistentDataPath + "/maps/" + trackname;
            foreach (string mapFolder in Directory.GetDirectories(groupFolder))
            {
                ProjectMapInfo info = new ProjectMapInfo()
                {
                    group = groupInfo,
                    nick = new DirectoryInfo(mapFolder).Name,
                    difficulties = new List<DifficultyInfo>()
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
                List<GroupInfoExtended> maps = JsonConvert.DeserializeObject<List<GroupInfoExtended>>(json);
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
                List<GroupInfoExtended> maps = JsonConvert.DeserializeObject<List<GroupInfoExtended>>(json);
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
        public List<GroupInfoExtended> approvedGroups = new List<GroupInfoExtended>();
        public List<GroupInfoExtended> allGroups = new List<GroupInfoExtended>();
        public List<GroupInfoExtended> DownloadedGroups { get; set; } = new List<GroupInfoExtended>();
        public List<GroupInfoExtended> OwnGroups { get; set; } = new List<GroupInfoExtended>();
    }
}