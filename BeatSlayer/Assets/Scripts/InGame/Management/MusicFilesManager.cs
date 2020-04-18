using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using UnityEngine;

namespace MusicFilesManagement
{
    public static class MusicFilesManager
    {
        public static string CacheFilePath { get; } = Application.persistentDataPath + "/data/musicfiles.xml";

        public static MusicFilesData data;

        public static void LoadData(Action callback)
        {
            if (File.Exists(CacheFilePath))
            {
                Debug.Log("MusicFilesManager.LoadData Loading from file");
                XmlSerializer xml = new XmlSerializer(typeof(MusicFilesData));
                using(var stream = File.OpenRead(CacheFilePath))
                {
                    data = (MusicFilesData)xml.Deserialize(stream);
                }
                callback();
            }
            else
            {
                Debug.LogError("MusicFilesManager.LoadData Loading from new");
                data = new MusicFilesData();

                data.folders = new List<string>()
                {
                    "/storage/emulated/0/Music", "/storage/emulated/0/Download", "/storage/emulated/0/VK", "/storage/emulated/0/Telegram", GetSDPath(), Application.persistentDataPath
                };

                SaveData();
                Search(callback);
            }
        }
        public static void SaveData()
        {
            XmlSerializer xml = new XmlSerializer(typeof(MusicFilesData));
            using (var stream = File.Create(CacheFilePath))
            {
                xml.Serialize(stream, data);
            }
        }


        /// <summary>
        /// Async search music files in folders
        /// </summary>
        /// <param name="callback"></param>
        public static async void Search(Action callback)
        {
            DateTime dt1 = DateTime.Now;

            await SearchTask();

            Debug.LogError("MusicFilesManager.Search() end with " + (DateTime.Now - dt1).TotalMilliseconds + "ms");

            callback();
        }

        static Task SearchTask()
        {
            return Task.Factory.StartNew(() =>
            {
                DateTime dt1 = DateTime.Now;

                data.files.Clear();
                foreach (string folder in data.folders)
                {
                    if (!Directory.Exists(folder)) continue;

                    string[] allFiles = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
                    IEnumerable<string> files = allFiles.Where(c => Path.GetExtension(c) == ".mp3" || Path.GetExtension(c) == ".ogg");

                    data.files.AddRange(files.Where(c => !data.files.Contains(c)));
                }
                Debug.LogError("Data files count is " + data.files.Count);

                Debug.LogError("SearchTask() end with " + (DateTime.Now - dt1).TotalMilliseconds + "ms");

                SaveData();
            });
        }


        /// <summary>
        /// Check MusicFilesData folders. If exists, leave, else remove from folders list
        /// </summary>
        public static void CheckFolders()
        {
            List<string> foldersToRemove = data.folders.Where(c => !Directory.Exists(c)).ToList();
            for(int i = 0; i < foldersToRemove.Count; i++)
            {
                data.folders.Remove(foldersToRemove[i]);
            }
        }
        static string GetSDPath()
        {
            var removableDives = System.IO.DriveInfo.GetDrives()
                //Take only removable drives into consideration as a SD card candidates
                .Where(drive => drive.DriveType == DriveType.Removable)
                .Where(drive => drive.IsReady)
                //If volume label of SD card is always the same, you can identify
                //SD card by uncommenting following line
                //.Where(drive => drive.VolumeLabel == "MySdCardVolumeLabel")
                .ToList();

            if (removableDives.Count == 0)
                return "";

            string sdCardRootDirectory;
            return sdCardRootDirectory = removableDives[0].RootDirectory.FullName;
        }
    }

    public class MusicFilesData
    {
        /// <summary>
        /// Audio files list
        /// </summary>
        public List<string> files = new List<string>();

        /// <summary>
        /// Folders where you should search
        /// </summary>
        public List<string> folders = new List<string>();
    }
}