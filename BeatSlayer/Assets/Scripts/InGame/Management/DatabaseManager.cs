using Newtonsoft.Json;
using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DatabaseManagement
{
    public static class Database
    {
        public static DatabaseContainer container;

        public static string url_approved = "http://www.bsserver.tk/Moderation/GetApprovedGroups";
        public static string url_groups = "http://176.107.160.146/Database/GetGroupsExtended";


        public static Action onApprovedLoadedCallback, onAllMusicLoadedCallback;


        public static void Init()
        {
            container = new DatabaseContainer();
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
    }

    public class DatabaseContainer
    {
        public List<GroupInfoExtended> approvedGroups = new List<GroupInfoExtended>();
        public List<GroupInfoExtended> allGroups = new List<GroupInfoExtended>();
    }
}