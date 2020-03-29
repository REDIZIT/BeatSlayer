using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DownloadHelper : MonoBehaviour
{
    public Sprite defaultSprite;
    public Texture2D defaultIcon;

    MenuScript_v2 menu { get { return GetComponent<MenuScript_v2>(); } }

    public void DownloadTrack(TrackClass item, Action<object, AsyncCompletedEventArgs> completeCallback, Action<object, DownloadProgressChangedEventArgs> progressCallback)
    {
        WebClient downloadClient = new WebClient();

        string downloadUrl = "http://176.107.160.146/Home/DownloadProject?trackname=" + (item.group.author + "-" + item.group.name).Replace("&", "%amp%") + "&nickname=" + item.nick.Replace("&", "%amp%");
        Debug.LogWarning("Download url: " + downloadUrl);
        Uri uri = new Uri(downloadUrl);

        if (!Directory.Exists(Application.persistentDataPath + "/temp")) Directory.CreateDirectory(Application.persistentDataPath + "/temp");

        string tempPath = Application.persistentDataPath + "/temp/" + (item.group.author.Trim() + "-" + item.group.name.Trim()) + ".bsz";
        downloadClient.DownloadFileCompleted += new AsyncCompletedEventHandler(completeCallback);
        downloadClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progressCallback);
        downloadClient.DownloadFileAsync(uri, tempPath);
    }

    public Sprite DownloadSprite(TrackGroupClass group)
    {
        string trackname = group.author + "-" + group.name;
        string nick = "";

        WebClient client = new WebClient();

        string url = "http://176.107.160.146/Database/GetCover?trackname=" + trackname.Replace("&", "%amp%") + "&nick=" + nick.Replace("&", "%amp%");
        byte[] arr = client.DownloadData(url);

        if (arr.Length == 0) return defaultSprite;

        //Texture2D texture = new Texture2D(2, 2);\
        texture.LoadImage(arr);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
    public void DownloadSpriteWithCallback(TrackGroupClass group, Action<Sprite> callback)
    {
        string trackname = group.author + "-" + group.name;
        string nick = "";

        WebClient client = new WebClient();

        string url = "http://176.107.160.146/Database/GetCover?trackname=" + trackname.Replace("&", "%amp%") + "&nick=" + nick.Replace("&", "%amp%");

        client.DownloadDataCompleted += (object sender, DownloadDataCompletedEventArgs args) =>
        {
            if (args.Result.Length == 0) callback(defaultSprite);
            else
            {
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(args.Result);
                callback(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
            }
        };

        client.DownloadDataAsync(new Uri(url));
    }
    public void DownloadSpriteWithCallbackSmart(TrackGroupClass group, Action<Texture2D> callback, string filepath)
    {
        string trackname = group.author + "-" + group.name;
        string nick = "";

        WebClient client = new WebClient();

        string url = "http://176.107.160.146/Database/GetCover?trackname=" + trackname.Replace("&", "%amp%") + "&nick=" + nick.Replace("&", "%amp%");

        client.DownloadDataCompleted += (object sender, DownloadDataCompletedEventArgs args) =>
        {
            if (args.Result.Length == 0) callback(defaultIcon);
            else
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
                texture.LoadImage(args.Result);

                if (filepath != "") File.WriteAllBytes(filepath, args.Result);

                callback(texture);
                //callback(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
            }
        };

        client.DownloadDataAsync(new Uri(url));
    }






    #region Groups preview loading

    bool loadingPreview = true;
    public List<TrackListItem> trackListItems = new List<TrackListItem>();
    public Texture2D texture;

    //public void LoadPreview(TrackListItem item)
    //{
    //    loadingPreviewsThread = new Thread(LoadPreviewThreadMain);
    //    texture = new Texture2D(2,2);
    //    loadingPreviewsThread.Start();
    //}
    //public void LoadPreview(TrackListItem item)
    //{
    //    StartCoroutine(LoadPreviewCor(item));
    //}
    //public IEnumerator LoadPreviewCor(TrackListItem item)
    //{
    //    yield return new WaitForEndOfFrame();
    //    if (loadingPreview)
    //    {
    //        DownloadSpriteWithCallback(item.group, (Sprite sprite) =>
    //        {
    //            if (trackListItems.Count > 0) // If 0 -> thats all OR list was forced to clear
    //            {
    //                if (!item.coverImage.IsDestroyed())
    //                {
    //                    item.coverImage.sprite = sprite;
    //                    trackListItems.Remove(item);
    //                    if (trackListItems.Count > 0) LoadPreview(trackListItems[0]);
    //                }
    //            }
    //        });
    //    }
    //}

    public List<TrackListItem> smartQueue = new List<TrackListItem>();
    public List<TrackListItem> smartLoadingQueue = new List<TrackListItem>();

    //public async void LoadPreviewSmart(TrackListItem item)
    //{

    //}
    public void LoadSmartQueue()
    {
        StartCoroutine(IELoadSmartQueue());
    }
    IEnumerator IELoadSmartQueue()
    {
        DateTime t1 = DateTime.Now;
        

        foreach (var item in smartLoadingQueue)
        {
            if (!Directory.Exists(Application.persistentDataPath + "/dbmaps")) Directory.CreateDirectory(Application.persistentDataPath + "/dbmaps");
            // Saved preview file path
            string filepath = Application.persistentDataPath + "/dbmaps/" + item.group.author + "-" + item.group.name;
            string extension = File.Exists(filepath + ".jpg") ? ".jpg" : ".png";
            filepath += extension;

            if (File.Exists(filepath))
            {
                
                byte[] bytes = File.ReadAllBytes(filepath);

                Texture2D tex = TheGreat.LoadTexure(bytes);

                DateTime t2 = DateTime.Now;

                item.coverImage.texture = tex;
                //item.coverImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f)); ;
                
            }
            else
            {
                item.coverImage.texture = defaultIcon;
                smartQueue.Add(item);
            }

            yield return new WaitForEndOfFrame();
        }

        if(smartQueue.Count > 0)
        {
            DownloadPreviewsSmart(smartQueue[0]);
        }

        Debug.Log("[LOADSMARTQUEUE] " + (DateTime.Now - t1).TotalMilliseconds);
    }

    public void DownloadPreviewsSmart(TrackListItem item)
    {
        DownloadSpriteWithCallbackSmart(item.group, (Texture2D tex) =>
        {
            if (smartQueue.Count > 0) // If 0 -> thats all OR list was forced to clear
            {
                if (!item.coverImage.IsDestroyed())
                {
                    //item.coverImage.sprite = sprite;
                    item.coverImage.texture = tex;

                    smartQueue.Remove(item);
                    if (smartQueue.Count > 0) DownloadPreviewsSmart(smartQueue[0]);
                }
            }
        }, Application.persistentDataPath + "/dbmaps/" + item.group.author + "-" + item.group.name + ".jpg");
    }

    #endregion


    public Sprite DownloadSprite(TrackClass cls)
    {
        string trackname = cls.group.author + "-" + cls.group.name;
        string nick = cls.nick;

        WebClient client = new WebClient();

        string url = "http://176.107.160.146/Database/GetCover?trackname=" + TheGreat.UrlEncode(trackname) + "&nick=" + TheGreat.UrlEncode(nick);
        byte[] arr = client.DownloadData(url);

        if (arr.Length == 0) return defaultSprite;

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(arr);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }



}
