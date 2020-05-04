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

                try
                {
                    if (filepath != "") File.WriteAllBytes(filepath, args.Result);
                }
                catch (Exception err)
                {
                    Debug.LogError($"On cover downloaded for {filepath} error: " + err.Message);
                }

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
