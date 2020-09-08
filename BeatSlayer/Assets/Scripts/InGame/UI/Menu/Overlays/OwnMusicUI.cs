using Assets.SimpleLocalization;
using DatabaseManagement;
using GameNet;
using InGame.Helpers;
using Newtonsoft.Json;
using ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace InGame.UI.Overlays
{
    public static class OwnMusicPayload
    {
        public static string Folder { get; set; }
        public static IEnumerable<string> AllFiles { get; set; }
        public static bool ForceUpdate { get; set; }
        public static Action callback { get; set; }
        public static string SyncState { get; set; }
    }
    public class OwnMusicUI : MonoBehaviour
    {
        public TrackListUI trackListUI;

        public Text syncText;
        public Text stateText;

        public Button syncBtn;

        public string DataFolder { get; set; }


        private void Awake()
        {
            DataFolder = Application.persistentDataPath;
        }

        private void Update()
        {
            syncText.text = OwnMusicPayload.SyncState;
        }

        public void OnOwnBtnClicked(Action callback)
        {
            SearchAllMusic(() =>
            {
                Debug.Log("OnOwnBtnClicked");
                callback();
            }, false);
        }
        public void OnSyncBtnClick()
        {
            Stopwatch w = new Stopwatch();
            w.Start();

            //syncBtn.interactable = false;
            SearchAllMusic(() => trackListUI.RefreshOwnList(), true);

            w.Stop();
            Debug.Log("Unity thread slept " + w.ElapsedMilliseconds + "ms");
        }


        /// <summary>
        /// Search music in all folder and subfolder on phone. Will take a lot of time
        /// </summary>
        /// <param name="callback">Callback with searching result</param>
        void SearchAllMusic(Action callback, bool forceupdate = false)
        {
            // For example
            string folder = @"C:\Users\REDIZIT\AppData\LocalLow\REDIZIT\Beat Slayer\test";
            //string folder = @"C:\# Data #\GitHub Projects\BeatSlayer";

            Stopwatch w = new Stopwatch();
            w.Start();

            OwnMusicPayload.SyncState = "";
            OwnMusicPayload.Folder = folder;
            OwnMusicPayload.ForceUpdate = forceupdate;
            OwnMusicPayload.callback = () =>
            {
                w.Stop();
                UnityMainThreadDispatcher.Instance().Enqueue(callback);
            };

            Thread t = new Thread(SearchAllMusicInThread);
            t.Start();
        }
        void SearchAllMusicInThread()
        {
            try
            {
                if (OwnMusicPayload.ForceUpdate)
                {
                    RefreshAllMusicFiles();
                }
                else
                {
                    if (File.Exists(DataFolder + "/data/musicfiles.json"))
                    {
                        LoadFromFile();
                        OwnMusicPayload.callback();
                    }
                    else SearchAllMusic(OwnMusicPayload.callback, true);

                    
                }
            }
            catch (Exception err)
            {
                OwnMusicPayload.SyncState = LocalizationManager.Localize("Error!") + " " + err.Message;
            }
        }

        void RefreshAllMusicFiles()
        {
            string mainFolder = "/storage/emulated/0";
            if (Application.isEditor)
            {
                mainFolder = @"C:\Users\REDIZIT\AppData\LocalLow\REDIZIT\Beat Slayer\test";
                //mainFolder = @"C:\# Data #\GitHub Projects";
            }


            OwnMusicPayload.SyncState = LocalizationManager.Localize("GettingAllFiles", mainFolder);

            IEnumerable<string> allFiles = Directory.GetFiles(mainFolder, "*", SearchOption.AllDirectories);

            /// Select only music files
            OwnMusicPayload.SyncState = LocalizationManager.Localize("GettingMusicFiles");

            allFiles = allFiles.Where(c => PredicateMusicFile(c));
            OwnMusicPayload.AllFiles = allFiles;


            // Adding files to groups container
            Database.container.OwnGroups.Clear();

            Database.container.OwnGroups = LoadGroupsFromFilepathes(allFiles);

            SaveToFile();

            OwnMusicPayload.SyncState = LocalizationManager.Localize("Refreshed");

            OwnMusicPayload.callback();
        }
        void LoadFromFile()
        {
            string path = DataFolder + "/data/musicfiles.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                List<string> files = JsonConvert.DeserializeObject<List<string>>(json);
                Database.container.OwnGroups = LoadGroupsFromFilepathes(files);
            }
        }
        void SaveToFile()
        {
            string json = JsonConvert.SerializeObject(Database.container.OwnGroups.Select(c => c.filepath));
            string path = DataFolder + "/data/musicfiles.json";
            File.WriteAllText(path, json);
        }
        List<GroupInfoExtended> LoadGroupsFromFilepathes(IEnumerable<string> files)
        {
            List<GroupInfoExtended> ls = new List<GroupInfoExtended>();
            foreach (var file in files)
            {
                string filename = Path.GetFileNameWithoutExtension(file);

                string[] split = filename.Split('-');
                string author;
                string name;

                if (split.Length > 1)
                {
                    author = split[0].Trim();
                    name = split[1].Trim();
                }
                else
                {
                    author = "Unknown";
                    name = split[0].Trim();
                }

                GroupInfoExtended group = new GroupInfoExtended()
                {
                    author = author,
                    name = name,
                    mapsCount = 1,
                    filepath = file,
                    groupType = GroupType.Own
                };
                ls.Add(group);
            }

            return ls;
        }


        bool PredicateMusicFile(string path)
        {
            string extension = Path.GetExtension(path);

            return extension == ".mp3" || extension == ".ogg";
        }
    }
}