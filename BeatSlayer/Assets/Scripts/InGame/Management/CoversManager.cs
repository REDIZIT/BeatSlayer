using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;

namespace CoversManagement
{
    public static class CoversManager
    {
        public static List<CoverRequestPackage> requests = new List<CoverRequestPackage>();
        static bool isInited;
        static bool isDownloading;

        static string url_cover = "http://176.107.160.146/Database/GetCover?trackname={0}&nick={1}";
        static WebClient client = new WebClient();

        static Texture2D DefaultTexture { get { return TrackListUI.defaultIcon; } }

        public static void Init()
        {
            if (isInited) return;

            client.DownloadDataCompleted += OnDataDownloaded;

            isInited = true;
        }

        static void OnDataDownloaded(object sender, DownloadDataCompletedEventArgs e)
        {
            isDownloading = false;
            if (e.Cancelled)
            {
                Debug.Log("Downloading canceled");
            }
            else if (e.Error != null)
            {
                Debug.LogError(e.Error);
                requests.RemoveAt(0);

                OnRequestsListUpdate(); 
            }
            else
            {
                byte[] bytes = e.Result;
                if (bytes.Length == 0)
                {
                    requests[0].image.texture = DefaultTexture;
                }
                else
                {
                    Texture2D tex = TheGreat.LoadTexure(bytes);
                    requests[0].image.texture = tex;
                }

                Debug.Log("Completed " + requests[0].trackname);
                requests.RemoveAt(0);

                OnRequestsListUpdate();
            }
        }

        public static void OnRequestsListUpdate()
        {
            if (requests.Count == 0 || isDownloading) return;

            if (!isInited) Init();

            string url = string.Format(url_cover, requests[0].trackname, requests[0].nick);

            Uri uri = new Uri(url);
            client.DownloadDataAsync(uri);
            isDownloading = true;
        }

        public static void AddPackages(List<CoverRequestPackage> ls)
        {
            requests.AddRange(ls);
            OnRequestsListUpdate();
        }
        public static void ClearPackages(RawImage[] images)
        {
            if (requests.Count == 0) return;

            List<CoverRequestPackage> toRemove = new List<CoverRequestPackage>();
            foreach (RawImage img in images)
            {
                if (requests[0].image == img)
                {
                    client.CancelAsync();
                    toRemove.Add(requests[0]);
                    Debug.Log("Removed zero index");
                    continue;
                }

                CoverRequestPackage package = requests.Find(c => c.image == img);
                if(package != null)
                {
                    Debug.Log("Removed " + package.trackname);
                    toRemove.Add(package);
                }
            }

            Debug.Log("Removed " + toRemove.Count + "/" + requests.Count);
            requests = requests.Except(toRemove).ToList();
            Debug.Log(".. Requests count " + requests.Count);
        }
    }
    
    public class CoverRequestPackage
    {
        public RawImage image;

        public string trackname, nick;

        public CoverRequestPackage(RawImage image, string trackname, string nick = "")
        {
            this.image = image;
            this.trackname = trackname;
            this.nick = nick;
        }
    }
}