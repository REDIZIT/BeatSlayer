using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using ProjectManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CoversManagement
{
    public static class CoversManager
    {
        public static List<CoverRequestPackage> requests = new List<CoverRequestPackage>();

        static bool isInited;
        static bool isDownloading;

        public const string url_cover = "http://176.107.160.146/Database/GetCover?trackname={0}&nick={1}";
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
            if (e.Cancelled) return;

            if (e.Error != null)
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
                    Texture2D tex = ProjectManager.LoadTexture(bytes);
                    requests[0].image.texture = tex;
                }

                requests.RemoveAt(0);

                OnRequestsListUpdate();
            }
        }

        public static void OnRequestsListUpdate()
        {
            if (requests.Count == 0 || isDownloading) return;

            if (!isInited) Init();

            requests = requests.OrderByDescending(c => c.priority).ToList();

            string url = string.Format(url_cover, TheGreat.UrlEncode(requests[0].trackname), TheGreat.UrlEncode(requests[0].nick));

            // file path for downloaded map cover
            Texture2D tex = ProjectManager.LoadCoverOrNull(requests[0].trackname);
            //Debug.Log("Cover: > Handling " + requests[0].trackname);
            if (tex != null)
            {
                //Debug.Log("Cover: << Load downloaded texture");
                requests[0].image.texture = tex;
                requests.RemoveAt(0);
                OnRequestsListUpdate();
            }
            else
            {
                //Debug.Log("Cover: << No downloaded texture");
                if (Application.internetReachability != NetworkReachability.NotReachable)
                {
                    //Debug.Log("Cover: >> Download cover");
                    Uri uri = new Uri(url);
                    client.DownloadDataAsync(uri);
                    isDownloading = true;
                }
                else
                {
                    //Debug.Log("Cover: >> No inet, set default");
                    requests[0].image.texture = DefaultTexture;
                    requests.RemoveAt(0);
                    OnRequestsListUpdate();
                }
            }
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
                    continue;
                }

                CoverRequestPackage package = requests.Find(c => c.image == img);
                if(package != null)
                {
                    toRemove.Add(package);
                }
            }
            requests = requests.Except(toRemove).ToList();
        }
        public static void ClearAll()
        {
            if(requests.Count > 0) client.CancelAsync();
            requests.Clear();
        }
    }
    
    public class CoverRequestPackage
    {
        public RawImage image;

        public string trackname, nick;
        public bool priority;

        public CoverRequestPackage(RawImage image, string trackname, string nick = "", bool priority = false)
        {
            this.image = image;
            this.trackname = trackname;
            this.nick = nick;
            this.priority = priority;
        }
    }
}