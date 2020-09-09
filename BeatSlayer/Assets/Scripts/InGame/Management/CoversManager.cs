using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using GameNet;
using InGame.Utils;
using ProjectManagement;
using UnityEngine;
using UnityEngine.UI;

namespace CoversManagement
{
    public static class CoversManager
    {
        public static List<BasicRequestPackage> requests = new List<BasicRequestPackage>();


        private static string apibase = NetCore.Url_Server;
        private static Texture2D DefaultTexture { get { return TrackListUI.defaultIcon; } }


        private static Dictionary<string, CachedRequestPackage> playersAvatarsCache = new Dictionary<string, CachedRequestPackage>();
        private static Dictionary<string, CachedRequestPackage> mapsCoversCache = new Dictionary<string, CachedRequestPackage>();


        private static bool isInited;
        private static bool isDownloading;


        static readonly WebClient client = new WebClient();

        private static void Init()
        {
            if (isInited) return;

            client.DownloadDataCompleted += OnDataDownloaded;

            isInited = true;
        }

      

        public static void AddPackages(List<CoverRequestPackage> ls)
        {
            requests.AddRange(ls);
            OnRequestsListUpdate();
        }
        public static void AddPackage(CoverRequestPackage package)
        {
            requests.Add(package);
            OnRequestsListUpdate();
        }
        public static void AddAvatarPackage(RawImage targetImage, string playerNick, bool priority = false)
        {
            requests.Add(new AvatarRequestPackage(targetImage, playerNick, priority));
            OnRequestsListUpdate();
        }
        public static void ClearPackages(RawImage[] images)
        {
            if (requests.Count == 0) return;

            List<BasicRequestPackage> toRemove = new List<BasicRequestPackage>();
            foreach (RawImage img in images)
            {
                if (requests[0].targetImage == img)
                {
                    client.CancelAsync();
                    toRemove.Add(requests[0]);
                    continue;
                }

                BasicRequestPackage package = requests.Find(c => c.targetImage == img);
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




        private static void OnRequestsListUpdate()
        {
            if (requests.Count == 0 || isDownloading) return;

            if (!isInited) Init();

            requests = requests.OrderByDescending(c => c.priority).ToList();

            string url = string.Format(apibase + requests[0].GetUrl());

            Texture2D tex = null;

            // Load from cache
            // If not cache -> try load from downloads
            CachedRequestPackage cache = GetCachedTextureOrNull(requests[0]);
            if (cache == null)
            {
                if (requests[0] is CoverRequestPackage)
                {
                    // file path for downloaded map cover
                    tex = ProjectManager.LoadCoverOrNull((requests[0] as CoverRequestPackage).trackname);
                }
            }
            else
            {
                tex = cache.texture;
            }
            


            if (tex != null)
            {
                //Debug.Log("Cover: << Load downloaded texture");
                requests[0].Apply(tex);

                AddTextureToCache(requests[0], tex);

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
                    requests[0].Apply(DefaultTexture);
                    requests.RemoveAt(0);

                    OnRequestsListUpdate();
                }
            }
        }
        private static void OnDataDownloaded(object sender, DownloadDataCompletedEventArgs e)
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
                    requests[0].Apply(DefaultTexture);
                    AddTextureToCache(requests[0], null);
                }
                else
                {
                    Texture2D tex = ProjectManager.LoadTexture(bytes);
                    requests[0].Apply(tex);
                    AddTextureToCache(requests[0], tex);
                }

                requests.RemoveAt(0);

                OnRequestsListUpdate();
            }
        }


        private static void AddTextureToCache(BasicRequestPackage package, Texture2D tex)
        {
            if (package is CoverRequestPackage)
            {
                mapsCoversCache[(package as CoverRequestPackage).trackname] = new CachedRequestPackage(tex);
            }
            else if (package is AvatarRequestPackage)
            {
                playersAvatarsCache[(package as AvatarRequestPackage).nick] = new CachedRequestPackage(tex);
            }
        }
        private static CachedRequestPackage GetCachedTextureOrNull(BasicRequestPackage package)
        {
            if (package is CoverRequestPackage)
            {
                CoverRequestPackage pack = package as CoverRequestPackage;

                if (mapsCoversCache.ContainsKey(pack.trackname)) return mapsCoversCache[pack.trackname];
                else return null;
            }
            else if (package is AvatarRequestPackage)
            {
                AvatarRequestPackage pack = package as AvatarRequestPackage;

                if (playersAvatarsCache.ContainsKey(pack.nick)) return playersAvatarsCache[pack.nick];
                else return null;
            }

            return null;
        }
    }


    public class CachedRequestPackage
    {
        public bool isDefault;
        public Texture2D texture;

        /// <param name="texture">Set null if need to use default texture</param>
        public CachedRequestPackage(Texture2D texture)
        {
            isDefault = texture == null;
            this.texture = texture;
        }
    }





    public abstract class BasicRequestPackage
    {
        public RawImage targetImage;
        public Action<Texture2D> callback;
        public bool priority;

        public BasicRequestPackage(RawImage targetImage, bool priority = false, Action<Texture2D> callback = null)
        {
            this.targetImage = targetImage;
            this.priority = priority;
            this.callback = callback;
        }
        public abstract string GetUrl();
        public void Apply(Texture2D tex)
        {
            if(targetImage != null)
            {
                targetImage.texture = tex;
            }
            callback?.Invoke(tex);
        }
    }
    public class CoverRequestPackage : BasicRequestPackage
    {
        public string trackname, nick;

        public CoverRequestPackage(RawImage targetImage, string trackname, string nick = "", bool priority = false, Action<Texture2D> callback = null) : base(targetImage, priority, callback)
        {
            this.targetImage = targetImage;
            this.priority = priority;
            this.callback = callback;

            this.trackname = trackname;
            this.nick = nick;
        }

        public override string GetUrl()
        {
            return $"/Database/GetCover?trackname={UrlEncoder.Encode(trackname)}&nick={UrlEncoder.Encode(nick)}";
        }
    }

    public class AvatarRequestPackage : BasicRequestPackage
    {
        public string nick;


        public AvatarRequestPackage(RawImage targetImage, string playerNick, bool priority = false, Action<Texture2D> callback = null) : base(targetImage, priority, callback)
        {
            this.targetImage = targetImage;
            this.priority = priority;
            this.callback = callback;

            this.nick = playerNick;
        }

        public override string GetUrl()
        {
            return "/WebAPI/GetAvatar?nick=" + UrlEncoder.Encode(nick);
        }
    }
}