using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapListItem : MonoBehaviour
{
    MenuScript_v2 menu;
    public AdvancedSaveManager prefsManager
    {
        get { return menu.transform.GetComponent<AdvancedSaveManager>(); }
    }

    public TrackClass item;

    public Text nickText;

    public Text likesText, dislikesText, downloadsText, playsText;
    public Image coverImage;

    public GameObject downloadIndicator;
    public GameObject downloadBtn, playBtn, deleteBtn;
    public Slider progressBar;
    public Text progressText;
    public GameObject isPassedImage;

    public Text recordText;

    public GameObject updateBtn;

    public void Setup(MenuScript_v2 menu, TrackClass item, bool isPassed)
    {
        this.menu = menu;
        this.item = item;

        nickText.text = item.nick;
        likesText.text = item.likes.ToString();
        dislikesText.text = item.dislikes.ToString();
        downloadsText.text = item.downloads.ToString();
        playsText.text = item.plays.ToString();
        coverImage.sprite = item.cover;

        string filepath = Application.persistentDataPath + "/maps/" + item.group.author + "-" + item.group.name + "/" + item.nick + "/" + item.group.author + "-" + item.group.name + ".bsu";
        bool exists = File.Exists(filepath);

        downloadIndicator.SetActive(exists);
        playBtn.SetActive(exists);
        downloadBtn.SetActive(!exists);
        progressBar.gameObject.SetActive(false);
        deleteBtn.SetActive(exists);

        isPassedImage.SetActive(isPassed);

        if (exists)
        {
            updateBtn.SetActive(item.hasUpdate);
        }
    }
    public void SetupForLocalFile(MenuScript_v2 menu, TrackClass item)
    {
        Debug.Log("[SETUP LOCAL]");
        this.menu = menu;
        this.item = item;

        nickText.text = item.nick;
        likesText.transform.parent.gameObject.SetActive(false);
        dislikesText.transform.parent.gameObject.SetActive(false);
        downloadsText.transform.parent.gameObject.SetActive(false);
        playsText.transform.parent.gameObject.SetActive(false);
        coverImage.sprite = item.cover;

        downloadIndicator.SetActive(true);
        playBtn.SetActive(true);
        downloadBtn.SetActive(false);
        progressBar.gameObject.SetActive(false);
        deleteBtn.SetActive(false);
    }


    #region Downloading track

    WebClient downloadClient;
    public void OnDownloadClick()
    {
        Debug.Log("OnDownloadClick()");
        //menu.downloadHelper.DownloadTrack(item, OnDownloadComplete, OnDownloadProgress);



        downloadClient = new WebClient();

        string downloadUrl = "http://176.107.160.146/Home/DownloadProject?trackname=" + (item.group.author + "-" + item.group.name).Replace("&", "%amp%") + "&nickname=" + item.nick.Replace("&", "%amp%");
        Debug.LogWarning("Download url: " + downloadUrl);
        Uri uri = new Uri(downloadUrl);

        if (!Directory.Exists(Application.persistentDataPath + "/temp")) Directory.CreateDirectory(Application.persistentDataPath + "/temp");

        string tempPath = Application.persistentDataPath + "/temp/" + (item.group.author.Trim() + "-" + item.group.name.Trim()) + ".bsz";
        //downloadClient.DownloadFileCompleted += new AsyncCompletedEventHandler(OnDownloadComplete);
        //downloadClient.DownloadFileAsync(uri, tempPath);

        //downloadClient.DownloadFile(uri, tempPath);

        downloadClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnDownloadProgress);
        downloadClient.DownloadDataCompleted += OnDownloadComplete;
        downloadClient.DownloadDataAsync(uri);
        //OnDownloadComplete(null)


        progressText.text = "Waiting...";
        progressBar.gameObject.SetActive(true);
        progressBar.value = 0;
        downloadBtn.SetActive(false);
    }

    public void OnDownloadComplete(object sender, DownloadDataCompletedEventArgs e)
    {
        TimeSpan t2 = DateTime.Now.TimeOfDay;
        string tempPath = Application.persistentDataPath + "/temp/" + (item.group.author.Trim() + "-" + item.group.name.Trim()) + ".bsz";
        File.WriteAllBytes(tempPath, e.Result);
        Debug.Log("WriteAllBytes time is " + (DateTime.Now.TimeOfDay - t2).TotalMilliseconds);

        Debug.Log("Download is completed. Error? " + e.Error);
        downloadClient.Dispose();
        if (e.Error == null)
        {
            

            progressBar.value = 0;
            progressBar.gameObject.SetActive(false);
            playBtn.SetActive(true);
            downloadBtn.SetActive(false);
            downloadIndicator.SetActive(true);
            deleteBtn.SetActive(true);

            tempPath = Application.persistentDataPath + "/temp/" + (item.group.author.Trim() + "-" + item.group.name.Trim()) + ".bsz";

            TimeSpan t1 = DateTime.Now.TimeOfDay;
            Debug.Log("Unpacking... " + t1);
            menu.UnpackBspFile(tempPath);
            Debug.Log("Unpacked in " + (DateTime.Now.TimeOfDay - t1).TotalMilliseconds);

            TheGreat.SendStatistics(item.group.author + "-" + item.group.name, item.nick, "download");
        }
        else
        {
            Debug.Log("Something went wrong " + e.Error);
            progressBar.gameObject.SetActive(false);
            progressBar.value = 0;
            downloadBtn.SetActive(true);
            deleteBtn.SetActive(false);
        }
    }
    public void OnDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
    {
        progressBar.value = e.ProgressPercentage;
        progressText.text = "Downloading.. " + e.ProgressPercentage + "%";
    }

    #endregion


    public void OnPlayClick()
    {
        LCData.track = item;
        LCData.loadingType = item.group.filepath != "" ? LCData.LoadingType.AudioFile : LCData.LoadingType.Standard;

        //if (LCData.loadingType == LCData.LoadingType.Standard)
        //{
        //    if (!prefsManager.prefs.hasAchiv_ThatsMy)
        //    {
        //        Social.ReportProgress(GPGamesManager.achievement_ThatsMy, 100, (bool success) =>
        //        {
        //            if (!success) Debug.LogError("Achiv error");
        //            if (success)
        //            {
        //                prefsManager.prefs.hasAchiv_ThatsMy = true;
        //                prefsManager.Save();
        //            }
        //        });
        //    }
        //    if (item.nick != "")
        //    {
        //        if (!prefsManager.prefs.hasAchiv_MadeInChina)
        //        {
        //            Social.ReportProgress(GPGamesManager.achiv_madeInChina, 100, (bool success) => { if (!success) Debug.LogError("Achiv error: madeInChina"); });
        //            prefsManager.prefs.hasAchiv_MadeInChina = true;
        //            prefsManager.Save();
        //        }
        //    }

        //}
        
        Debug.Log("[LOADING] " + item.group.filepath);

        Debug.Log("[LOADING] Is Custom(AudioFile) ? " + (LCData.loadingType == LCData.LoadingType.AudioFile));

        if (LCData.loadingType == LCData.LoadingType.Standard)
        {
            TheGreat.SendStatistics(LCData.track.group.author + "-" + LCData.track.group.name, LCData.track.nick, "play");
        }

        //LCData.sceneLoading = "ServerLevel";
        LCData.sceneLoading = "Game";
        SceneManager.LoadScene("LoadScene");
    }

    public void OnDeleteClick()
    {
        string trackPath = Application.persistentDataPath + "/maps/" + item.group.author + "-" + item.group.name;
        string mapPath = trackPath + "/" + item.nick;

        Directory.Delete(mapPath, true);

        if(Directory.GetDirectories(trackPath).Length == 0)
        {
            Directory.Delete(trackPath);
        }

        menu.GetComponent<ListController>().RefreshDownloadList();

        downloadIndicator.SetActive(false);
        playBtn.SetActive(false);
        downloadBtn.SetActive(true);
        progressBar.gameObject.SetActive(false);
        deleteBtn.SetActive(false);
    }


    public void OnUpdateBtnClick()
    {
        updateBtn.SetActive(false);

        OnDeleteClick();

        OnDownloadClick();
    }
}
