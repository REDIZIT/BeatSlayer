using CoversManagement;
using ProjectManagement;
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

    public MapInfo mapInfo;

    public Text nickText;

    public Text likesText, dislikesText, downloadsText, playsText;
    //public Image coverImage;
    public RawImage coverImage;

    public GameObject downloadIndicator;
    public GameObject downloadBtn, playBtn, deleteBtn;
    public Slider progressBar;
    public Text progressText;
    public GameObject isPassedImage;

    public Text recordText;

    public GameObject updateBtn;

    public Text difficultyText;
    public Transform difficultyStarsContnet;

    public void Setup(MenuScript_v2 menu, bool isPassed, MapInfo mapInfo)
    {
        this.menu = menu;
        this.mapInfo = mapInfo;

        nickText.text = mapInfo.nick;
        likesText.text = mapInfo.likes.ToString();
        dislikesText.text = mapInfo.dislikes.ToString();
        downloadsText.text = mapInfo.downloads.ToString();
        CoversManager.AddPackages(new List<CoverRequestPackage>()
        {
            new CoverRequestPackage(coverImage, mapInfo.author + "-" + mapInfo.name, mapInfo.nick, true)
        });


        string filepath = Application.persistentDataPath + "/maps/" + mapInfo.group.author + "-" + mapInfo.group.name + "/" + mapInfo.nick + "/" + mapInfo.group.author + "-" + mapInfo.group.name + ".bsu";
        bool exists = File.Exists(filepath);

        downloadIndicator.SetActive(exists);
        playBtn.SetActive(exists);
        downloadBtn.SetActive(!exists);
        progressBar.gameObject.SetActive(false);
        deleteBtn.SetActive(exists);

        isPassedImage.SetActive(isPassed);

        if (exists)
        {
            
            //updateBtn.SetActive(mapInfo.HasUpdates());
        }

        //difficultyStarsContnet implement
        UpdateDifficulty();

    }
    public void SetupForLocalFile(MenuScript_v2 menu, MapInfo mapInfo)
    {
        this.menu = menu;
        this.mapInfo = mapInfo;

        nickText.text = mapInfo.nick;
        likesText.transform.parent.gameObject.SetActive(false);
        dislikesText.transform.parent.gameObject.SetActive(false);
        downloadsText.transform.parent.gameObject.SetActive(false);
        playsText.transform.parent.gameObject.SetActive(false);
        //coverImage.sprite = mapInfo.cover;

        downloadIndicator.SetActive(true);
        playBtn.SetActive(true);
        downloadBtn.SetActive(false);
        progressBar.gameObject.SetActive(false);
        deleteBtn.SetActive(false);

        difficultyText.gameObject.SetActive(false);
        difficultyStarsContnet.gameObject.SetActive(false);
    }

    public void UpdateDifficulty()
    {
        int difficulty = mapInfo.difficultyStars;
        string diffName = mapInfo.difficultyName;

        difficultyText.text = diffName;
        float xOffset = difficultyText.preferredWidth + 20;


        foreach (Transform child in difficultyStarsContnet) if (child.name != "Item") Destroy(child.gameObject);
        GameObject prefab = difficultyStarsContnet.GetChild(0).gameObject;
        prefab.SetActive(true);

        for (int i = 1; i <= 10; i++)
        {
            Color clr = i <= difficulty ? Color.white : new Color(0.18f, 0.18f, 0.18f);

            GameObject item = Instantiate(prefab, difficultyStarsContnet);
            item.GetComponent<Image>().color = clr;
        }

        prefab.SetActive(false);

        difficultyStarsContnet.GetComponent<RectTransform>().anchoredPosition = new Vector2(xOffset, 0);
    }




    #region Downloading track

    WebClient downloadClient;
    public void OnDownloadClick()
    {
        Debug.Log("OnDownloadClick()");
        //menu.downloadHelper.DownloadTrack(item, OnDownloadComplete, OnDownloadProgress);



        downloadClient = new WebClient();

        string downloadUrl = "http://176.107.160.146/Home/DownloadProject?trackname=" + (mapInfo.group.author + "-" + mapInfo.group.name).Replace("&", "%amp%") + "&nickname=" + mapInfo.nick.Replace("&", "%amp%");
        Debug.LogWarning("Download url: " + downloadUrl);
        Uri uri = new Uri(downloadUrl);

        if (!Directory.Exists(Application.persistentDataPath + "/temp")) Directory.CreateDirectory(Application.persistentDataPath + "/temp");

        string tempPath = Application.persistentDataPath + "/temp/" + (mapInfo.group.author.Trim() + "-" + mapInfo.group.name.Trim()) + ".bsz";
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
        string tempPath = Application.persistentDataPath + "/temp/" + (mapInfo.group.author.Trim() + "-" + mapInfo.group.name.Trim()) + ".bsz";
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

            tempPath = Application.persistentDataPath + "/temp/" + (mapInfo.group.author.Trim() + "-" + mapInfo.group.name.Trim()) + ".bsz";

            TimeSpan t1 = DateTime.Now.TimeOfDay;
            Debug.Log("Unpacking... " + t1);
            menu.UnpackBspFile(tempPath);
            Debug.Log("Unpacked in " + (DateTime.Now.TimeOfDay - t1).TotalMilliseconds);

            TheGreat.SendStatistics(mapInfo.group.author + "-" + mapInfo.group.name, mapInfo.nick, "download");
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
        SceneloadParameters load;
        if (mapInfo.filepath == "")
        {
            // Load author music
            load = SceneloadParameters.AuthorMusicPreset(mapInfo);

            TheGreat.SendStatistics(mapInfo.group.author + "-" + mapInfo.group.name, mapInfo.nick, "play");
        }
        else
        {
            // Load own music
            load = SceneloadParameters.OwnMusicPreset(mapInfo.filepath);
        }

        InGame.SceneManagement.SceneController.instance.LoadScene(load);
    }

    public void OnDeleteClick()
    {
        string trackPath = Application.persistentDataPath + "/maps/" + mapInfo.group.author + "-" + mapInfo.group.name;
        string mapPath = trackPath + "/" + mapInfo.nick;

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
